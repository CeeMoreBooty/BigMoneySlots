/**
 * Security Admin Routes — /api/security/*
 *
 * All endpoints require the ADMIN_SECRET header:
 *   x-admin-secret: <value of ADMIN_SECRET env var>
 *
 * These are internal/admin-only endpoints.
 * Do NOT expose them publicly without additional authentication.
 */

const express        = require('express');
const SecurityEvent  = require('../models/SecurityEvent');
const BannedEntity   = require('../models/BannedEntity');
const security       = require('../services/securityService');
const webhook        = require('../services/webhookService');
const discord        = require('../services/discordAdminWebhook');
const router         = express.Router();

// ── Admin auth guard ──────────────────────────────────────────────────────────
function adminGuard(req, res, next) {
    const secret = req.headers['x-admin-secret'];
    if (!secret || secret !== process.env.ADMIN_SECRET) {
        return res.status(403).json({ error: 'Forbidden' });
    }
    next();
}
router.use(adminGuard);

// ── GET /api/security/events ──────────────────────────────────────────────────
// List recent security events. Query params: severity, eventType, ip, limit (max 200).
router.get('/events', async (req, res) => {
    try {
        const { severity, eventType, ip, limit = 50, resolved } = req.query;
        const filter = {};
        if (severity)  filter.severity  = severity;
        if (eventType) filter.eventType = eventType;
        if (ip)        filter.ip        = ip;
        if (resolved !== undefined) filter.resolved = resolved === 'true';

        const events = await SecurityEvent.find(filter)
            .sort({ createdAt: -1 })
            .limit(Math.min(Number(limit), 200))
            .lean();

        res.json({ count: events.length, events });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── GET /api/security/stats ───────────────────────────────────────────────────
// Dashboard summary: counts by severity and event type in the last 24 h.
router.get('/stats', async (req, res) => {
    try {
        const since = new Date(Date.now() - 24 * 60 * 60 * 1000);

        const [bySeverity, byType, bans] = await Promise.all([
            SecurityEvent.aggregate([
                { $match: { createdAt: { $gte: since } } },
                { $group: { _id: '$severity', count: { $sum: 1 } } },
            ]),
            SecurityEvent.aggregate([
                { $match: { createdAt: { $gte: since } } },
                { $group: { _id: '$eventType', count: { $sum: 1 } } },
                { $sort: { count: -1 } },
            ]),
            BannedEntity.countDocuments({ active: true }),
        ]);

        res.json({ period: '24h', bySeverity, byType, activeBans: bans });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── GET /api/security/bans ────────────────────────────────────────────────────
// List active bans.
router.get('/bans', async (req, res) => {
    try {
        const bans = await BannedEntity.find({ active: true })
            .sort({ createdAt: -1 })
            .limit(200)
            .lean();
        res.json({ count: bans.length, bans });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── POST /api/security/ban ────────────────────────────────────────────────────
// Manually ban an IP, player, or device.
// Body: { type: 'ip'|'player'|'device', value, reason, expiresAt? }
router.post('/ban', async (req, res) => {
    try {
        const { type, value, reason, expiresAt } = req.body;
        if (!type || !value) return res.status(400).json({ error: 'type and value required' });

        await security.manualBan(type, value, reason || 'Manual admin ban', expiresAt ? new Date(expiresAt) : null);

        // Notify Discord
        discord.ban(type, value, reason || 'Manual admin ban', 'admin');

        res.json({ success: true, type, value });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── POST /api/security/unban ──────────────────────────────────────────────────
// Remove a ban. Body: { type, value }
router.post('/unban', async (req, res) => {
    try {
        const { type, value } = req.body;
        if (!type || !value) return res.status(400).json({ error: 'type and value required' });

        await security.unban(type, value);
        res.json({ success: true, type, value });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── PATCH /api/security/events/:id/resolve ────────────────────────────────────
// Mark a security event as resolved (investigated / false positive).
router.patch('/events/:id/resolve', async (req, res) => {
    try {
        const event = await SecurityEvent.findByIdAndUpdate(
            req.params.id,
            { resolved: true },
            { new: true }
        );
        if (!event) return res.status(404).json({ error: 'Event not found' });
        res.json({ success: true, event });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── GET /api/security/webhook/status ─────────────────────────────────────────
// Returns the current webhook configuration (URL is partially masked).
router.get('/webhook/status', (_req, res) => {
    res.json(webhook.status());
});

// ── POST /api/security/webhook/test ──────────────────────────────────────────
// Sends a test ping to the configured webhook URL to verify connectivity.
router.post('/webhook/test', async (_req, res) => {
    try {
        const result = await webhook.test();
        if (result.ok) {
            res.json({ success: true, statusCode: result.statusCode });
        } else {
            res.status(502).json({ success: false, error: result.error });
        }
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── POST /api/security/discord/test ──────────────────────────────────────────
// Sends a test ping to the Discord admin webhook to verify it is reachable.
router.post('/discord/test', async (_req, res) => {
    try {
        const result = await discord.testWebhook();
        if (result.ok) {
            res.json({ success: true, message: 'Discord webhook test sent successfully.' });
        } else {
            res.status(502).json({ success: false, error: result.error });
        }
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── GET /api/security/discord/status ─────────────────────────────────────────
// Returns whether the Discord webhook is enabled and configured.
router.get('/discord/status', (_req, res) => {
    const url = process.env.ADMIN_DISCORD_WEBHOOK_URL || '';
    const masked = url.length > 30
        ? url.slice(0, 38) + '***' + url.slice(-4)
        : url ? '(configured)' : '(not set)';
    res.json({
        enabled:     process.env.ADMIN_DISCORD_WEBHOOK_ENABLED === 'true',
        url:         masked,
        minSeverity: process.env.ADMIN_DISCORD_MIN_SEVERITY || 'medium',
    });
});

module.exports = router;
