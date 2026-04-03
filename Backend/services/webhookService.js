/**
 * Security Webhook Service
 *
 * Forwards BigMoneySlots security events and ban notifications to a
 * company-configured HTTP/HTTPS endpoint so your team can monitor threats
 * in real time from a personal server, Slack incoming webhook, Discord,
 * or any custom dashboard.
 *
 * ── Authentication ────────────────────────────────────────────────────────────
 * Every POST is signed with HMAC-SHA256 over the raw JSON body using
 * SECURITY_WEBHOOK_SECRET.  Your receiving server should verify:
 *
 *   const sig = req.headers['x-bms-signature'];         // "sha256=<hex>"
 *   const expected = 'sha256=' + crypto
 *       .createHmac('sha256', YOUR_SECRET)
 *       .update(req.body)          // raw Buffer / string — NOT parsed JSON
 *       .digest('hex');
 *   if (!crypto.timingSafeEqual(Buffer.from(sig), Buffer.from(expected))) {
 *       return res.status(401).send('Bad signature');
 *   }
 *
 * ── Configuration (.env) ──────────────────────────────────────────────────────
 *   SECURITY_WEBHOOK_ENABLED=true
 *   SECURITY_WEBHOOK_URL=https://your-company-server.com/hooks/bigmoneyslots
 *   SECURITY_WEBHOOK_SECRET=change_this_to_a_long_random_string
 *   SECURITY_WEBHOOK_MIN_SEVERITY=medium   # low | medium | high | critical
 *
 * ── Reliability ───────────────────────────────────────────────────────────────
 * - Retries up to 3 times with exponential back-off (1 s, 2 s, 4 s).
 * - 5-second per-attempt timeout so a slow receiver never stalls the server.
 * - Failed deliveries after all retries are queued in memory (max 500 items)
 *   and replayed on the next successful delivery.
 * - All errors are caught — this service NEVER throws to its callers.
 */

const crypto = require('crypto');
const https  = require('https');
const http   = require('http');
const { URL } = require('url');

// ── Severity ordering ─────────────────────────────────────────────────────────
const SEVERITY_ORDER = { low: 0, medium: 1, high: 2, critical: 3 };

function _meetsMinSeverity(severity) {
    const min = process.env.SECURITY_WEBHOOK_MIN_SEVERITY || 'medium';
    return (SEVERITY_ORDER[severity] ?? 0) >= (SEVERITY_ORDER[min] ?? 1);
}

// ── In-memory retry queue (survives transient receiver downtime) ──────────────
const MAX_QUEUE = 500;
const _retryQueue = [];   // [{ payload, attempts }]

// ── Low-level HTTP POST ───────────────────────────────────────────────────────
function _post(webhookUrl, bodyStr, signature) {
    return new Promise((resolve, reject) => {
        let parsed;
        try { parsed = new URL(webhookUrl); } catch (e) { return reject(e); }

        const transport = parsed.protocol === 'https:' ? https : http;
        const bodyBuf   = Buffer.from(bodyStr, 'utf8');

        const options = {
            hostname: parsed.hostname,
            port:     parsed.port || (parsed.protocol === 'https:' ? 443 : 80),
            path:     parsed.pathname + parsed.search,
            method:   'POST',
            headers: {
                'Content-Type':   'application/json',
                'Content-Length': bodyBuf.length,
                'X-BMS-Signature': `sha256=${signature}`,
                'X-BMS-Source':    'BigMoneySlots',
            },
            timeout: 5000,
        };

        const req = transport.request(options, (res) => {
            res.resume();   // drain
            res.statusCode >= 200 && res.statusCode < 300
                ? resolve(res.statusCode)
                : reject(new Error(`HTTP ${res.statusCode}`));
        });

        req.on('timeout', () => { req.destroy(); reject(new Error('timeout')); });
        req.on('error',   reject);
        req.write(bodyBuf);
        req.end();
    });
}

// ── Delivery with retry + exponential back-off ────────────────────────────────
async function _deliver(bodyStr, signature, maxAttempts = 3) {
    const url = process.env.SECURITY_WEBHOOK_URL;
    if (!url) return false;

    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        try {
            await _post(url, bodyStr, signature);
            return true;
        } catch (err) {
            if (attempt < maxAttempts) {
                await new Promise(r => setTimeout(r, 1000 * Math.pow(2, attempt - 1)));
            } else {
                console.error(`[webhookService] Delivery failed after ${maxAttempts} attempts: ${err.message}`);
            }
        }
    }
    return false;
}

// ── Drain the retry queue (called after each successful send) ─────────────────
async function _drainQueue() {
    while (_retryQueue.length > 0) {
        const item = _retryQueue[0];
        const ok   = await _deliver(item.bodyStr, item.signature, 1);
        if (!ok) break;   // receiver still down — stop draining
        _retryQueue.shift();
    }
}

// ── Build and sign a payload ──────────────────────────────────────────────────
function _buildPayload(eventType, data) {
    const payload = {
        source:    'BigMoneySlots',
        eventType,
        timestamp: new Date().toISOString(),
        data,
    };
    const bodyStr  = JSON.stringify(payload);
    const secret   = process.env.SECURITY_WEBHOOK_SECRET || '';
    const signature = crypto.createHmac('sha256', secret).update(bodyStr).digest('hex');
    return { bodyStr, signature };
}

// ── Public API ────────────────────────────────────────────────────────────────

/**
 * Send a security event to the webhook.
 * Fire-and-forget — never throws.
 *
 * @param {string} webhookEventType  e.g. 'security_event' | 'ban_issued' | 'webhook_test'
 * @param {object} data              Free-form payload (SecurityEvent fields, ban info, etc.)
 * @param {string} [severity]        Used for min-severity filtering (skip low-noise events)
 */
async function send(webhookEventType, data, severity = 'medium') {
    try {
        if (process.env.SECURITY_WEBHOOK_ENABLED !== 'true') return;
        if (!process.env.SECURITY_WEBHOOK_URL)               return;
        if (!_meetsMinSeverity(severity))                     return;

        const { bodyStr, signature } = _buildPayload(webhookEventType, data);

        const ok = await _deliver(bodyStr, signature);

        if (ok) {
            // Attempt to drain any previously queued failures
            if (_retryQueue.length > 0) _drainQueue().catch(() => {});
        } else {
            // Queue for later replay
            if (_retryQueue.length < MAX_QUEUE) {
                _retryQueue.push({ bodyStr, signature });
            } else {
                console.error('[webhookService] Retry queue full — oldest item dropped.');
                _retryQueue.shift();
                _retryQueue.push({ bodyStr, signature });
            }
        }
    } catch (err) {
        console.error('[webhookService] Unexpected error:', err.message);
    }
}

/**
 * Send a test ping to verify the webhook is reachable.
 * Returns { ok: bool, statusCode?, error? }.
 */
async function test() {
    const url = process.env.SECURITY_WEBHOOK_URL;
    if (!url) return { ok: false, error: 'SECURITY_WEBHOOK_URL not configured' };

    const { bodyStr, signature } = _buildPayload('webhook_test', {
        message: 'BigMoneySlots security webhook test ping',
        configuredMinSeverity: process.env.SECURITY_WEBHOOK_MIN_SEVERITY || 'medium',
    });

    try {
        const statusCode = await _post(url, bodyStr, signature);
        return { ok: true, statusCode };
    } catch (err) {
        return { ok: false, error: err.message };
    }
}

/**
 * Returns the current webhook configuration (URL is partially masked).
 */
function status() {
    const url = process.env.SECURITY_WEBHOOK_URL || '';
    let maskedUrl = url;
    if (url.length > 20) {
        // Show protocol + first 10 chars + *** + last 6 chars
        const proto = url.startsWith('https') ? 'https://' : 'http://';
        maskedUrl = proto + url.slice(proto.length, proto.length + 10) + '***' + url.slice(-6);
    }
    return {
        enabled:     process.env.SECURITY_WEBHOOK_ENABLED === 'true',
        url:         maskedUrl || '(not set)',
        minSeverity: process.env.SECURITY_WEBHOOK_MIN_SEVERITY || 'medium',
        queuedItems: _retryQueue.length,
    };
}

module.exports = { send, test, status };
