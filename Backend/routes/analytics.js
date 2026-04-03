/**
 * Analytics Route — /api/analytics/*
 *
 * Receives event batches from Unity clients and stores them for
 * admin reporting and fraud correlation.
 *
 * Copyright (c) 2024 CeeMoreBooty / Big Money Slots. All rights reserved.
 */

'use strict';

const express = require('express');
const auth    = require('../middleware/auth');
const router  = express.Router();

// ── In-memory recent-events buffer (last 1000 per process restart) ────────────
// For production, write to MongoDB or a dedicated analytics store.
const _buffer = [];
const MAX_BUFFER = 1000;

function _store(events) {
    for (const e of events) {
        if (_buffer.length >= MAX_BUFFER) _buffer.shift();
        _buffer.push({ ...e, receivedAt: new Date().toISOString() });
    }
}

// ── POST /api/analytics/batch ─────────────────────────────────────────────────
// Unity AnalyticsManager sends event batches here.
// Accepts an array of event objects (no auth required for tracking; fraud filtering
// is done by securityGuard middleware upstream).
router.post('/batch', async (req, res) => {
    try {
        const events = Array.isArray(req.body) ? req.body : [req.body];

        if (events.length === 0)
            return res.status(400).json({ error: 'Empty batch' });

        if (events.length > 200)
            return res.status(400).json({ error: 'Batch too large (max 200)' });

        // Basic sanitise: strip unexpected large fields
        const sanitised = events.map(e => ({
            event:   String(e.event   || 'unknown').slice(0, 64),
            device:  String(e.device  || '').slice(0, 64),
            session: String(e.session || '').slice(0, 32),
            ts:      Number(e.ts) || Date.now(),
            data:    e.data || {},
        }));

        _store(sanitised);

        // Log high-value spin/jackpot events for fraud correlation
        for (const e of sanitised) {
            if (e.event === 'jackpot_win') {
                console.log('[analytics] jackpot_win device=%s amount=%s',
                    e.device, e.data?.win ?? '?');
            }
        }

        res.json({ ok: true, received: sanitised.length });
    } catch (err) {
        console.error('[analytics/batch]', err.message);
        res.status(500).json({ error: 'Internal error' });
    }
});

// ── GET /api/analytics/recent ─────────────────────────────────────────────────
// Admin-only: view recent buffered events.
router.get('/recent', auth, (req, res) => {
    const limit = Math.min(Number(req.query.limit) || 100, MAX_BUFFER);
    const type  = req.query.event;
    const data  = type
        ? _buffer.filter(e => e.event === type).slice(-limit)
        : _buffer.slice(-limit);
    res.json({ count: data.length, events: data });
});

module.exports = router;
