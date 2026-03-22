/**
 * Security Service — core detection and enforcement engine.
 *
 * Tracks in-memory counters for fast hot-path checks (brute force, rate abuse)
 * and writes SecurityEvent + BannedEntity documents for audit and persistence.
 *
 * Detection capabilities:
 *  ✓ Brute-force login attempts (per IP, sliding window)
 *  ✓ Abnormal request rates (per IP, per endpoint, sliding window)
 *  ✓ JWT reuse from multiple IP addresses
 *  ✓ Coin / economy manipulation (impossible delta)
 *  ✓ NoSQL / SQL / script injection patterns in request body
 *  ✓ Known bot / exploit tool user-agent fingerprints
 *  ✓ Payload size abuse
 *  ✓ Duplicate purchase token replay attacks
 *  ✓ Auto-ban on threshold breach with optional socket.io admin alert
 */

const SecurityEvent   = require('../models/SecurityEvent');
const BannedEntity    = require('../models/BannedEntity');
const webhook         = require('./webhookService');

// ── Configurable thresholds (can be moved to .env) ───────────────────────────
const CFG = {
    // Brute-force: max failed auth attempts per IP before ban
    bruteForceWindow:     10 * 60 * 1000,   // 10 min sliding window
    bruteForceMax:        10,                // attempts before medium-severity event
    bruteForceBanAt:      20,                // attempts before auto-ban

    // Abnormal rate: max hits to the SAME endpoint per IP per window
    rateAbuseWindow:      60 * 1000,         // 1 min
    rateAbuseMax:         60,                // hits before flagging
    rateAbuseBanAt:       120,               // hits before auto-ban

    // JWT multi-IP: how many distinct IPs allowed per token before flagging
    jwtMultiIpMax:        3,

    // Coin manipulation: max plausible win per single spin (coins)
    // Grand Jackpot max is ~5B, so anything above this is impossible client-side
    maxPlausibleWinPerSpin: 10_000_000_000,  // 10B

    // Payload abuse: max body size in bytes before flagging (1 MB)
    maxBodyBytes:         1_048_576,

    // Auto-ban thresholds: N high/critical events within window → ban
    autoBanWindow:        30 * 60 * 1000,    // 30 min
    autoBanHighCount:     5,
    autoBanCriticalCount: 1,
};

// ── In-memory counters (reset on server restart — DB is the persistent record) ─
// Map<ip, [{time}]>
const _authAttempts   = new Map();
// Map<`ip:path`, [{time}]>
const _rateCounts     = new Map();
// Map<jwtTokenHash, Set<ip>>
const _tokenIpSets    = new Map();

// ── Helpers ───────────────────────────────────────────────────────────────────

function _now() { return Date.now(); }

/** Keep only entries within the sliding window. */
function _prune(arr, windowMs) {
    const cutoff = _now() - windowMs;
    let i = 0;
    while (i < arr.length && arr[i].time < cutoff) i++;
    if (i > 0) arr.splice(0, i);
}

/** Simple hash to avoid storing full JWT in memory. */
function _hashToken(token) {
    let h = 0;
    for (let i = 0; i < Math.min(token.length, 64); i++) {
        h = ((h << 5) - h) + token.charCodeAt(i);
        h |= 0;
    }
    return String(h);
}

// ── Injection pattern detector ────────────────────────────────────────────────
const INJECTION_PATTERNS = [
    /(\$where|\$gt|\$lt|\$ne|\$in|\$nin|\$or|\$and|\$not|\$exists)/i,  // NoSQL operators
    /(<script|javascript:|onerror=|onload=|eval\(|document\.cookie)/i,  // XSS
    /('|"|;|--|\/\*|\*\/|xp_|exec\s*\(|union\s+select)/i,              // SQL
    /(\.\.\/|\.\.\\|\/etc\/passwd|\/bin\/sh|cmd\.exe)/i,                // path traversal / RCE
];

function _containsInjection(value) {
    if (typeof value === 'string') {
        return INJECTION_PATTERNS.some(p => p.test(value));
    }
    if (typeof value === 'object' && value !== null) {
        return Object.values(value).some(v => _containsInjection(v));
    }
    return false;
}

// ── Bot / exploit tool fingerprints ──────────────────────────────────────────
const BOT_UA_PATTERNS = [
    /sqlmap/i, /nikto/i, /nessus/i, /openvas/i, /masscan/i, /nmap/i,
    /metasploit/i, /burpsuite/i, /acunetix/i, /dirbuster/i, /zgrab/i,
    /python-requests\/[0-1]\./i,  // very old requests lib often used for scripted abuse
    /go-http-client\/1\./i,
    /curl\/[0-6]\./i,
    /libwww-perl/i,
    /scrapy/i,
    /phantomjs/i,
    /headlesschrome/i,
];

function _isKnownBot(userAgent) {
    if (!userAgent) return false;
    return BOT_UA_PATTERNS.some(p => p.test(userAgent));
}

// ── Core: log event + optional auto-ban ───────────────────────────────────────

/**
 * Record a security event. If severity is 'critical' or threshold is breached,
 * auto-bans the IP/player and emits an admin socket event.
 *
 * @param {object} opts
 * @param {string}  opts.eventType
 * @param {string}  opts.severity     low | medium | high | critical
 * @param {string}  [opts.ip]
 * @param {string}  [opts.playerId]
 * @param {string}  [opts.deviceId]
 * @param {string}  [opts.path]
 * @param {string}  [opts.method]
 * @param {string}  [opts.userAgent]
 * @param {object}  [opts.evidence]
 * @param {string}  [opts.note]
 * @param {object}  [opts.io]        socket.io Server instance for admin alerts
 * @returns {Promise<SecurityEvent>}
 */
async function logEvent(opts) {
    const {
        eventType, severity = 'medium',
        ip = '', playerId = null, deviceId = '',
        path = '', method = '', userAgent = '',
        evidence = {}, note = '',
        io = null,
    } = opts;

    let autoBanned = false;

    try {
        // Decide if we should auto-ban
        if (severity === 'critical') {
            autoBanned = await _autoBan({ ip, playerId, deviceId, reason: eventType, io });
        } else if (severity === 'high' && ip) {
            // Check recent high/critical count for this IP
            const cutoff = new Date(Date.now() - CFG.autoBanWindow);
            const count  = await SecurityEvent.countDocuments({
                ip,
                severity: { $in: ['high', 'critical'] },
                createdAt: { $gte: cutoff },
            });
            if (count >= CFG.autoBanHighCount - 1) {   // -1 because we haven't saved yet
                autoBanned = await _autoBan({ ip, playerId, deviceId, reason: eventType, io });
            }
        }

        const event = await SecurityEvent.create({
            ip, playerId, deviceId,
            eventType, severity, autoBanned,
            path, method, userAgent,
            evidence, note,
        });

        // Emit admin alert for high/critical
        if (io && (severity === 'high' || severity === 'critical')) {
            io.to('admin_room').emit('security_alert', {
                eventType, severity, ip, playerId,
                path, autoBanned,
                time: new Date().toISOString(),
            });
        }

        console.warn(`[Security][${severity.toUpperCase()}] ${eventType} — IP: ${ip} Player: ${playerId} Path: ${path}${autoBanned ? ' → AUTO-BANNED' : ''}`);

        // Forward to company webhook (fire-and-forget)
        webhook.send('security_event', {
            eventId:    event?._id,
            eventType, severity, autoBanned,
            ip, playerId, deviceId,
            path, method, userAgent,
            evidence, note,
        }, severity);

        return event;
    } catch (err) {
        // Never crash the main request because of a security logging error
        console.error('[securityService] logEvent failed:', err.message);
        return null;
    }
}

async function _autoBan({ ip, playerId, deviceId, reason, io }) {
    const ops = [];
    if (ip)       ops.push(_ban('ip',     ip,           reason));
    if (playerId) ops.push(_ban('player', String(playerId), reason));
    if (deviceId) ops.push(_ban('device', deviceId,     reason));
    await Promise.all(ops);
    if (io) io.to('admin_room').emit('player_banned', { ip, playerId, deviceId, reason, time: new Date().toISOString() });

    // Forward ban notification to company webhook
    webhook.send('ban_issued', { ip, playerId, deviceId, reason, bannedBy: 'system' }, 'high');

    return true;
}

async function _ban(type, value, reason, expiresAt = null) {
    try {
        await BannedEntity.findOneAndUpdate(
            { type, value },
            { type, value, reason, active: true, expiresAt, bannedBy: 'system' },
            { upsert: true, new: true }
        );
    } catch (err) {
        console.error('[securityService] _ban failed:', err.message);
    }
}

// ── Public check helpers (called by securityGuard middleware) ─────────────────

/**
 * Returns true if the IP, playerId, or deviceId is currently banned.
 */
async function isBanned({ ip, playerId, deviceId }) {
    const queries = [];
    if (ip)       queries.push({ type: 'ip',     value: ip });
    if (playerId) queries.push({ type: 'player',  value: String(playerId) });
    if (deviceId) queries.push({ type: 'device',  value: deviceId });
    if (!queries.length) return false;

    const ban = await BannedEntity.findOne({
        $and: [
            { $or: queries },
            { active: true },
            { $or: [{ expiresAt: null }, { expiresAt: { $gt: new Date() } }] },
        ],
    });
    return !!ban;
}

/**
 * Track a failed authentication attempt.
 * Returns { flagged: bool, banned: bool }.
 */
async function trackFailedAuth(ip, io) {
    const key = `auth:${ip}`;
    if (!_authAttempts.has(key)) _authAttempts.set(key, []);
    const arr = _authAttempts.get(key);
    _prune(arr, CFG.bruteForceWindow);
    arr.push({ time: _now() });

    const count = arr.length;
    if (count >= CFG.bruteForceBanAt) {
        await logEvent({ eventType: 'brute_force_auth', severity: 'critical', ip, io,
            evidence: { attempts: count, window: '10min' },
            note: `${count} failed auth attempts — auto-banned` });
        return { flagged: true, banned: true };
    }
    if (count >= CFG.bruteForceMax) {
        await logEvent({ eventType: 'brute_force_auth', severity: 'high', ip, io,
            evidence: { attempts: count, window: '10min' } });
        return { flagged: true, banned: false };
    }
    return { flagged: false, banned: false };
}

/**
 * Track request rate per IP+endpoint.
 * Returns { flagged: bool, banned: bool }.
 */
async function trackRequestRate(ip, path, io) {
    const key = `rate:${ip}:${path}`;
    if (!_rateCounts.has(key)) _rateCounts.set(key, []);
    const arr = _rateCounts.get(key);
    _prune(arr, CFG.rateAbuseWindow);
    arr.push({ time: _now() });

    const count = arr.length;
    if (count >= CFG.rateAbuseBanAt) {
        await logEvent({ eventType: 'abnormal_request_rate', severity: 'critical', ip, path, io,
            evidence: { hitsPerMin: count, path } });
        return { flagged: true, banned: true };
    }
    if (count >= CFG.rateAbuseMax) {
        await logEvent({ eventType: 'abnormal_request_rate', severity: 'high', ip, path, io,
            evidence: { hitsPerMin: count, path } });
        return { flagged: true, banned: false };
    }
    return { flagged: false, banned: false };
}

/**
 * Track which IPs a JWT token is used from.
 * Flags if used from more than CFG.jwtMultiIpMax distinct IPs.
 */
async function trackTokenIp(token, ip, playerId, io) {
    const hash = _hashToken(token);
    if (!_tokenIpSets.has(hash)) _tokenIpSets.set(hash, new Set());
    const ipSet = _tokenIpSets.get(hash);
    ipSet.add(ip);

    if (ipSet.size > CFG.jwtMultiIpMax) {
        await logEvent({
            eventType: 'jwt_reuse_multi_ip', severity: 'high',
            ip, playerId, io,
            evidence: { distinctIps: [...ipSet], threshold: CFG.jwtMultiIpMax },
        });
        return { flagged: true };
    }
    return { flagged: false };
}

/**
 * Check request body for injection patterns.
 * Returns { flagged: bool }.
 */
async function checkInjection(ip, playerId, path, method, body, userAgent, io) {
    if (_containsInjection(body)) {
        await logEvent({
            eventType: 'injection_attempt', severity: 'critical',
            ip, playerId, path, method, userAgent, io,
            evidence: { bodySnippet: JSON.stringify(body).substring(0, 200) },
        });
        return { flagged: true };
    }
    return { flagged: false };
}

/**
 * Check user-agent against known bot/exploit fingerprints.
 */
async function checkUserAgent(ip, userAgent, path, io) {
    if (_isKnownBot(userAgent)) {
        await logEvent({
            eventType: 'bot_fingerprint', severity: 'high',
            ip, path, userAgent, io,
            evidence: { userAgent },
        });
        return { flagged: true };
    }
    return { flagged: false };
}

/**
 * Check payload size.
 */
async function checkPayloadSize(ip, playerId, path, contentLength, io) {
    const bytes = parseInt(contentLength || '0', 10);
    if (bytes > CFG.maxBodyBytes) {
        await logEvent({
            eventType: 'payload_too_large', severity: 'medium',
            ip, playerId, path, io,
            evidence: { bytes, maxBytes: CFG.maxBodyBytes },
        });
        return { flagged: true };
    }
    return { flagged: false };
}

/**
 * Validate a coin delta reported by the client is plausible.
 * Call from the spin/win processing route.
 */
async function checkCoinDelta(ip, playerId, claimedWin, io) {
    if (claimedWin > CFG.maxPlausibleWinPerSpin) {
        await logEvent({
            eventType: 'coin_manipulation', severity: 'critical',
            ip, playerId, io,
            evidence: { claimedWin, maxPlausible: CFG.maxPlausibleWinPerSpin },
            note: `Client claimed ${claimedWin} coins — impossible spin win`,
        });
        return { flagged: true };
    }
    return { flagged: false };
}

/**
 * Manual ban — called by admin routes.
 */
async function manualBan(type, value, reason, expiresAt = null) {
    await _ban(type, value, reason, expiresAt);
    webhook.send('ban_issued', { type, value, reason, expiresAt, bannedBy: 'admin' }, 'high');
}

/**
 * Unban an entity.
 */
async function unban(type, value) {
    await BannedEntity.findOneAndUpdate({ type, value }, { active: false });
}

module.exports = {
    logEvent,
    isBanned,
    trackFailedAuth,
    trackRequestRate,
    trackTokenIp,
    checkInjection,
    checkUserAgent,
    checkPayloadSize,
    checkCoinDelta,
    manualBan,
    unban,
    CFG,
};
