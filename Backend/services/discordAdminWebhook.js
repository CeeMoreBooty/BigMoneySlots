/**
 * Discord Admin Webhook Service — BigMoneySlots
 *
 * Forwards admin-relevant events to your Discord server via an incoming webhook.
 *
 * ── Setup ─────────────────────────────────────────────────────────────────────
 * 1. In Discord: Server Settings → Integrations → Webhooks → New Webhook
 *    Copy the Webhook URL.
 * 2. In your .env file set:
 *      ADMIN_DISCORD_WEBHOOK_URL=<paste webhook URL here>
 *      ADMIN_DISCORD_WEBHOOK_ENABLED=true
 *
 * !! NEVER commit the webhook URL to source code !!
 *
 * ── Events sent ──────────────────────────────────────────────────────────────
 *  • server_start        — server just booted
 *  • ban_issued          — IP/player/device auto-banned or manually banned
 *  • fraud_detected      — high/critical security event flagged
 *  • purchase_completed  — player completed an in-app purchase
 *  • custom             — any ad-hoc admin message
 *
 * ── Reliability ──────────────────────────────────────────────────────────────
 * Fire-and-forget with a 5 s timeout.  Errors are logged but never thrown so
 * a Discord outage can never affect gameplay or the main request path.
 *
 * Copyright (c) 2024 CeeMoreBooty / Big Money Slots. All rights reserved.
 */

'use strict';

const https  = require('https');
const http   = require('http');
const { URL } = require('url');

// ── Colour palette for embed side-bars ───────────────────────────────────────
const COLOUR = {
    INFO:     0x5865F2,   // Discord blurple
    SUCCESS:  0x57F287,   // green
    WARNING:  0xFEE75C,   // yellow
    DANGER:   0xED4245,   // red
    CRITICAL: 0x8B0000,   // dark red
    PURCHASE: 0xFFD700,   // gold
};

// ── Low-level POST ────────────────────────────────────────────────────────────
function _post(webhookUrl, payload) {
    return new Promise((resolve, reject) => {
        let parsed;
        try { parsed = new URL(webhookUrl); } catch (e) { return reject(e); }

        const body    = Buffer.from(JSON.stringify(payload), 'utf8');
        const transport = parsed.protocol === 'https:' ? https : http;

        const req = transport.request({
            hostname: parsed.hostname,
            port:     parsed.port || (parsed.protocol === 'https:' ? 443 : 80),
            path:     parsed.pathname + parsed.search,
            method:   'POST',
            headers: {
                'Content-Type':   'application/json',
                'Content-Length': body.length,
                'User-Agent':     'BigMoneySlots-AdminBot/1.0',
            },
            timeout: 5000,
        }, (res) => {
            res.resume();
            // Discord returns 204 No Content on success
            res.statusCode >= 200 && res.statusCode < 300
                ? resolve(res.statusCode)
                : reject(new Error('Discord HTTP ' + res.statusCode));
        });

        req.on('timeout', () => { req.destroy(); reject(new Error('Discord timeout')); });
        req.on('error',   reject);
        req.write(body);
        req.end();
    });
}

// ── Guard: is the webhook configured and enabled? ─────────────────────────────
function _enabled() {
    return process.env.ADMIN_DISCORD_WEBHOOK_ENABLED === 'true' &&
           !!process.env.ADMIN_DISCORD_WEBHOOK_URL;
}

// ── Core send helper ──────────────────────────────────────────────────────────
async function _send(payload) {
    if (!_enabled()) return;
    const url = process.env.ADMIN_DISCORD_WEBHOOK_URL;
    try {
        await _post(url, payload);
    } catch (err) {
        // Never throw — Discord outage must not affect the game server
        console.error('[discordAdminWebhook] Failed to deliver:', err.message);
    }
}

// ── Shared embed builder ──────────────────────────────────────────────────────
function _embed(title, description, colour, fields = []) {
    return {
        title,
        description,
        color: colour,
        fields: fields.map(([name, value, inline]) => ({
            name, value: String(value), inline: inline ?? true,
        })),
        footer: { text: 'BigMoneySlots Admin' },
        timestamp: new Date().toISOString(),
    };
}

// ── Public API ────────────────────────────────────────────────────────────────

/**
 * Notify Discord that the server has started.
 */
async function notifyServerStart() {
    await _send({
        embeds: [_embed(
            '🟢  Server Started',
            'BigMoneySlots backend is now **online** and accepting connections.',
            COLOUR.SUCCESS,
            [
                ['Port',        process.env.PORT || 3000],
                ['Environment', process.env.NODE_ENV || 'development'],
                ['Time (UTC)',  new Date().toUTCString()],
            ]
        )],
    });
}

/**
 * Notify Discord that a ban was issued.
 *
 * @param {object} info  { type: 'ip'|'player'|'device', value, reason, bannedBy: 'system'|'admin' }
 * @param {string} severity  'medium' | 'high' | 'critical'
 */
async function notifyBan(info, severity = 'high') {
    const colour = severity === 'critical' ? COLOUR.CRITICAL : COLOUR.DANGER;
    const icon   = info.bannedBy === 'admin' ? '🔨' : '🤖';
    await _send({
        embeds: [_embed(
            icon + '  Ban Issued',
            'A **' + info.type + '** has been banned from BigMoneySlots.',
            colour,
            [
                ['Type',       info.type],
                ['Value',      info.value || 'N/A'],
                ['Reason',     info.reason || 'N/A'],
                ['Banned By',  info.bannedBy || 'system'],
                ['Severity',   severity.toUpperCase()],
            ]
        )],
    });
}

/**
 * Notify Discord of a high/critical security / fraud event.
 *
 * @param {object} event  SecurityEvent-like object
 */
async function notifyFraud(event) {
    const colour = event.severity === 'critical' ? COLOUR.CRITICAL : COLOUR.DANGER;
    const fields = [
        ['Event Type', event.eventType],
        ['Severity',   (event.severity || 'unknown').toUpperCase()],
        ['IP',         event.ip        || 'N/A'],
        ['Player',     event.playerId  || 'N/A'],
        ['Path',       event.path      || 'N/A'],
    ];
    if (event.autoBanned) fields.push(['Auto-Banned', '✅ YES', false]);

    await _send({
        embeds: [_embed(
            '⚠️  Security Event',
            '`' + (event.eventType || 'unknown') + '` detected on the game server.',
            colour,
            fields
        )],
    });
}

/**
 * Notify Discord of a completed in-app purchase.
 *
 * @param {object} info  { playerId, productId, price, currency, source }
 */
async function notifyPurchase(info) {
    await _send({
        embeds: [_embed(
            '💰  Purchase Completed',
            'A player completed an in-app purchase.',
            COLOUR.PURCHASE,
            [
                ['Player',    info.playerId  || 'N/A'],
                ['Product',   info.productId || 'N/A'],
                ['Amount',    (info.currency || 'USD') + ' ' + (info.price || '?')],
                ['Source',    info.source    || 'N/A'],
            ]
        )],
    });
}

/**
 * Send a custom admin message.
 *
 * @param {string} title
 * @param {string} description
 * @param {Array}  [fields]   [[name, value, inline?], ...]
 * @param {number} [colour]   Hex int e.g. 0x5865F2
 */
async function notifyCustom(title, description, fields = [], colour = COLOUR.INFO) {
    await _send({ embeds: [_embed(title, description, colour, fields)] });
}

module.exports = {
    notifyServerStart,
    notifyBan,
    notifyFraud,
    notifyPurchase,
    notifyCustom,
    COLOUR,
};
