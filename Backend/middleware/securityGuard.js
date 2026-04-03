/**
 * Security Guard — runs on every API request before route handlers.
 *
 * Order of checks:
 *  1. Expire any temporary bans that have lapsed
 *  2. Check if IP is banned           → 403
 *  3. Check user-agent fingerprint    → 403 if exploit tool
 *  4. Check payload size              → 413 if too large
 *  5. Check request body for injection patterns → 400
 *  6. Track per-IP per-endpoint rate  → 429 if abusive
 *  7. If JWT present: track multi-IP token reuse
 *
 * Player-level ban (step 2b) is enforced in auth middleware after JWT decode.
 * This guard runs *before* auth so it covers unauthenticated probes too.
 */

const security = require('../services/securityService');

let _io = null;   // socket.io instance, injected via setIo()

function setIo(io) { _io = io; }

const securityGuard = async (req, res, next) => {
    const ip = (req.headers['x-forwarded-for'] || req.socket?.remoteAddress || '')
        .split(',')[0].trim();

    const userAgent     = req.headers['user-agent'] || '';
    const contentLength = req.headers['content-length'] || '0';
    const path          = req.path;
    const method        = req.method;

    // ── 1. Expire lapsed temporary bans (lightweight — uses index) ────────────
    //    Fire-and-forget; don't await to keep latency low.
    require('../models/BannedEntity')
        .updateMany({ active: true, expiresAt: { $lte: new Date() } }, { active: false })
        .catch(() => {});

    // ── 2a. Check banned IP ───────────────────────────────────────────────────
    const banned = await security.isBanned({ ip });
    if (banned) {
        await security.logEvent({
            eventType: 'banned_ip_attempt', severity: 'medium',
            ip, path, method, userAgent, io: _io,
        });
        return res.status(403).json({ error: 'Access denied.' });
    }

    // ── 3. User-agent fingerprint check ──────────────────────────────────────
    const uaCheck = await security.checkUserAgent(ip, userAgent, path, _io);
    if (uaCheck.flagged) {
        return res.status(403).json({ error: 'Access denied.' });
    }

    // ── 4. Payload size check ─────────────────────────────────────────────────
    await security.checkPayloadSize(ip, null, path, contentLength, _io);
    // (Soft flag only — we don't block on size alone; Express will 413 on its own)

    // ── 5. Injection check on request body ───────────────────────────────────
    if (req.body && typeof req.body === 'object') {
        const inj = await security.checkInjection(ip, null, path, method, req.body, userAgent, _io);
        if (inj.flagged) {
            return res.status(400).json({ error: 'Invalid request.' });
        }
    }

    // ── 6. Per-IP per-endpoint rate tracking ─────────────────────────────────
    const rate = await security.trackRequestRate(ip, path, _io);
    if (rate.banned) {
        return res.status(429).json({ error: 'Too many requests. You have been temporarily suspended.' });
    }

    // ── 7. JWT multi-IP tracking (if Authorization header present) ────────────
    const authHeader = req.headers.authorization;
    if (authHeader && authHeader.startsWith('Bearer ')) {
        const token = authHeader.split(' ')[1];
        // playerId not yet decoded here; pass null — auth middleware will fill it
        await security.trackTokenIp(token, ip, null, _io);
    }

    next();
};

module.exports = { securityGuard, setIo };
