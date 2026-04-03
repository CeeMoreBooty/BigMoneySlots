/**
 * Discord Admin Webhook Service — BigMoneySlots
 * Copyright (c) 2024–2026 PGCN (Pretty Good Casino Network). All rights reserved.
 *
 * Forwards admin-level information (security events, bans, server startup/errors,
 * new registrations, purchase alerts, and tournament events) to a Discord channel
 * via a Discord Incoming Webhook URL.
 *
 * ── Configuration (.env) ─────────────────────────────────────────────────────
 *   ADMIN_DISCORD_WEBHOOK_URL=https://discord.com/api/webhooks/<id>/<token>
 *   ADMIN_DISCORD_WEBHOOK_ENABLED=true
 *   ADMIN_DISCORD_MIN_SEVERITY=medium   # low | medium | high | critical
 *
 * ── Usage ────────────────────────────────────────────────────────────────────
 *   const discord = require('./discordAdminWebhook');
 *
 *   // Security alert
 *   discord.alert('🔒 Banned IP attempted access', { ip: '1.2.3.4', path: '/api/auth' }, 'high');
 *
 *   // Server event
 *   discord.info('🚀 Server started on port 3000');
 *
 *   // Error
 *   discord.error('💥 Unhandled exception', { message: err.message, stack: err.stack });
 *
 * ── Reliability ───────────────────────────────────────────────────────────────
 * - Fire-and-forget; never throws or blocks the request pipeline.
 * - Retries up to 2 times with a 2-second delay on non-4xx failures.
 * - Rate-limited to 1 message per second to respect Discord's 30-req/min limit.
 */

const https  = require('https');
const { URL } = require('url');

// ── Severity colours (Discord embed colour integers) ─────────────────────────
const COLOUR = {
    low:      0x95a5a6,   // grey
    medium:   0xf39c12,   // orange
    high:     0xe74c3c,   // red
    critical: 0x8e44ad,   // purple
    info:     0x3498db,   // blue
    success:  0x2ecc71,   // green
    error:    0xe74c3c,   // red
};

// ── Severity ordering ─────────────────────────────────────────────────────────
const SEVERITY_ORDER = { low: 0, medium: 1, high: 2, critical: 3 };

function _meetsMinSeverity(severity) {
    const min = process.env.ADMIN_DISCORD_MIN_SEVERITY || 'medium';
    return (SEVERITY_ORDER[severity] ?? 0) >= (SEVERITY_ORDER[min] ?? 1);
}

// ── Rate limiter (1 msg/sec) ──────────────────────────────────────────────────
let _lastSent = 0;
const RATE_LIMIT_MS = 1100;

async function _rateWait() {
    const now  = Date.now();
    const wait = _lastSent + RATE_LIMIT_MS - now;
    if (wait > 0) await new Promise(r => setTimeout(r, wait));
    _lastSent = Date.now();
}

// ── Low-level POST to Discord ─────────────────────────────────────────────────
function _postToDiscord(payload) {
    return new Promise((resolve, reject) => {
        const webhookUrl = process.env.ADMIN_DISCORD_WEBHOOK_URL;
        if (!webhookUrl) return reject(new Error('ADMIN_DISCORD_WEBHOOK_URL not set'));

        let parsed;
        try { parsed = new URL(webhookUrl); } catch (e) { return reject(e); }

        const bodyStr = JSON.stringify(payload);
        const bodyBuf = Buffer.from(bodyStr, 'utf8');

        const options = {
            hostname: parsed.hostname,
            port:     parsed.port || 443,
            path:     parsed.pathname + parsed.search,
            method:   'POST',
            headers: {
                'Content-Type':   'application/json',
                'Content-Length': bodyBuf.length,
                'User-Agent':     'BigMoneySlots-AdminBot/1.0',
            },
            timeout: 8000,
        };

        const req = https.request(options, (res) => {
            res.resume();
            res.statusCode >= 200 && res.statusCode < 300
                ? resolve(res.statusCode)
                : reject(new Error(`Discord HTTP ${res.statusCode}`));
        });

        req.on('timeout', () => { req.destroy(); reject(new Error('Discord webhook timeout')); });
        req.on('error',   reject);
        req.write(bodyBuf);
        req.end();
    });
}

// ── Deliver with retry ────────────────────────────────────────────────────────
async function _deliver(payload, maxAttempts = 3) {
    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        try {
            await _rateWait();
            await _postToDiscord(payload);
            return true;
        } catch (err) {
            // Don't retry on 4xx (bad payload) — only on timeouts / 5xx
            const is4xx = err.message.includes('HTTP 4');
            if (is4xx || attempt === maxAttempts) {
                console.error(`[discordAdminWebhook] Delivery failed: ${err.message}`);
                return false;
            }
            await new Promise(r => setTimeout(r, 2000));
        }
    }
    return false;
}

// ── Build Discord embed payload ───────────────────────────────────────────────
function _buildEmbed(title, description, colour, fields) {
    const embed = {
        title,
        description: _truncate(description, 4096),
        color:       colour,
        timestamp:   new Date().toISOString(),
        footer: { text: 'BigMoneySlots Admin • PGCN' },
    };

    if (fields && fields.length > 0) {
        embed.fields = fields.slice(0, 25).map(f => ({
            name:   _truncate(String(f.name),  256),
            value:  _truncate(String(f.value), 1024),
            inline: f.inline ?? true,
        }));
    }

    return { embeds: [embed] };
}

function _truncate(str, max) {
    if (!str) return '—';
    return str.length > max ? str.slice(0, max - 3) + '...' : str;
}

// ── Object → Discord fields ───────────────────────────────────────────────────
function _dataToFields(data) {
    if (!data || typeof data !== 'object') return [];
    return Object.entries(data)
        .filter(([, v]) => v !== undefined && v !== null)
        .slice(0, 20)
        .map(([k, v]) => ({
            name:   k,
            value:  typeof v === 'object' ? '```json\n' + JSON.stringify(v, null, 2).slice(0, 900) + '\n```' : String(v),
            inline: typeof v !== 'object',
        }));
}

// ── Guard: check enabled ──────────────────────────────────────────────────────
function _isEnabled() {
    return process.env.ADMIN_DISCORD_WEBHOOK_ENABLED === 'true' &&
           !!process.env.ADMIN_DISCORD_WEBHOOK_URL;
}

// ── Public API ────────────────────────────────────────────────────────────────

/**
 * Send a security alert embed (with severity filtering).
 * @param {string} title
 * @param {object} data    — key/value fields appended to the embed
 * @param {string} severity — 'low' | 'medium' | 'high' | 'critical'
 */
function alert(title, data = {}, severity = 'medium') {
    if (!_isEnabled()) return;
    if (!_meetsMinSeverity(severity)) return;
    const colour  = COLOUR[severity] ?? COLOUR.medium;
    const fields  = _dataToFields(data);
    const payload = _buildEmbed(`🔒 ${title}`, `**Severity:** ${severity.toUpperCase()}`, colour, fields);
    _deliver(payload).catch(() => {});
}

/**
 * Send a general information embed (not severity-filtered).
 * @param {string} title
 * @param {object|string} [data]
 */
function info(title, data) {
    if (!_isEnabled()) return;
    const fields  = typeof data === 'object' ? _dataToFields(data) : [];
    const desc    = typeof data === 'string' ? data : '';
    const payload = _buildEmbed(`ℹ️ ${title}`, desc, COLOUR.info, fields);
    _deliver(payload).catch(() => {});
}

/**
 * Send a success notification embed.
 */
function success(title, data) {
    if (!_isEnabled()) return;
    const fields  = typeof data === 'object' ? _dataToFields(data) : [];
    const desc    = typeof data === 'string' ? data : '';
    const payload = _buildEmbed(`✅ ${title}`, desc, COLOUR.success, fields);
    _deliver(payload).catch(() => {});
}

/**
 * Send an error embed (always sent regardless of min-severity).
 */
function error(title, data) {
    if (!_isEnabled()) return;
    const fields  = typeof data === 'object' ? _dataToFields(data) : [];
    const desc    = typeof data === 'string' ? data : '';
    const payload = _buildEmbed(`💥 ${title}`, desc, COLOUR.error, fields);
    _deliver(payload).catch(() => {});
}

/**
 * Send a ban notification.
 */
function ban(type, value, reason, adminOrAuto = 'auto') {
    if (!_isEnabled()) return;
    const payload = _buildEmbed(
        `🔨 ${type.toUpperCase()} Banned`,
        `**Reason:** ${reason}`,
        COLOUR.high,
        [
            { name: 'Type',   value: type,        inline: true },
            { name: 'Target', value: value,        inline: true },
            { name: 'By',     value: adminOrAuto,  inline: true },
        ]
    );
    _deliver(payload).catch(() => {});
}

/**
 * Send a purchase/payment notification.
 */
function purchase(playerId, productId, amount, currency, provider) {
    if (!_isEnabled()) return;
    const payload = _buildEmbed(
        `💰 New Purchase`,
        `Player \`${playerId}\` purchased **${productId}**`,
        COLOUR.success,
        [
            { name: 'Amount',   value: `${amount} ${currency}`, inline: true },
            { name: 'Provider', value: provider,                 inline: true },
        ]
    );
    _deliver(payload).catch(() => {});
}

/**
 * Send a new player registration notification.
 */
function newPlayer(playerId, displayName, platform) {
    if (!_isEnabled()) return;
    const payload = _buildEmbed(
        `🎰 New Player Registered`,
        `Welcome **${displayName}**!`,
        COLOUR.info,
        [
            { name: 'Player ID', value: playerId,  inline: true },
            { name: 'Platform',  value: platform,  inline: true },
        ]
    );
    _deliver(payload).catch(() => {});
}

/**
 * Send a server lifecycle notification (startup, shutdown, errors).
 */
function serverEvent(title, details) {
    if (!_isEnabled()) return;
    const fields  = typeof details === 'object' ? _dataToFields(details) : [];
    const desc    = typeof details === 'string' ? details : '';
    const payload = _buildEmbed(`🖥️ ${title}`, desc, COLOUR.info, fields);
    _deliver(payload).catch(() => {});
}

/**
 * Send a test ping to verify the webhook URL is reachable.
 * Returns { ok: bool, error? }.
 */
async function testWebhook() {
    const url = process.env.ADMIN_DISCORD_WEBHOOK_URL;
    if (!url) return { ok: false, error: 'ADMIN_DISCORD_WEBHOOK_URL not set' };

    const payload = _buildEmbed(
        '🧪 Webhook Test',
        'BigMoneySlots admin Discord webhook is configured correctly!',
        COLOUR.success,
        [{ name: 'Status', value: 'Connected ✅', inline: true }]
    );

    try {
        await _rateWait();
        await _postToDiscord(payload);
        return { ok: true };
    } catch (err) {
        return { ok: false, error: err.message };
    }
}

module.exports = { alert, info, success, error, ban, purchase, newPlayer, serverEvent, testWebhook };
