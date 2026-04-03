const express  = require('express');
const jwt      = require('jsonwebtoken');
const Player   = require('../models/Player');
const auth     = require('../middleware/auth');
const discord  = require('../services/discordAdminWebhook');
const router   = express.Router();

// ── Register / Login (device-ID based) ───────────────────────────────────────

/**
 * POST /api/auth/register
 * Body: { deviceId, displayName? }
 */
router.post('/register', async (req, res) => {
    try {
        const { deviceId, displayName } = req.body;
        if (!deviceId) return res.status(400).json({ error: 'deviceId required' });

        let player = await Player.findOne({ deviceId });
        if (player) {
            // Already registered — just return a token
            const token = signToken(player._id);
            return res.json({ token, playerId: player._id, isNew: false });
        }

        player = new Player({ deviceId, displayName: displayName || 'Guardian' });
        await player.save();

        // Notify Discord of new registration
        discord.newPlayer(String(player._id), player.displayName, 'Android');

        const token = signToken(player._id);
        res.status(201).json({ token, playerId: player._id, isNew: true });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * GET /api/auth/me  — returns current player profile
 */
router.get('/me', auth, async (req, res) => {
    res.json({
        playerId:    req.player._id,
        displayName: req.player.displayName,
        coins:       req.player.coins,
        freeSpins:   req.player.freeSpins,
        superSpins:  req.player.superSpins,
        ultraSpins:  req.player.ultraSpins,
        gems:        req.player.gems || 0,
        badgeId:     req.player.badgeId,
        reelSkinId:  req.player.reelSkinId,
    });
});

/**
 * PATCH /api/auth/display-name
 */
router.patch('/display-name', auth, async (req, res) => {
    try {
        const { displayName } = req.body;
        if (!displayName || displayName.trim().length < 2)
            return res.status(400).json({ error: 'Invalid display name' });
        req.player.displayName = displayName.trim().substring(0, 24);
        await req.player.save();
        res.json({ displayName: req.player.displayName });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

function signToken(id) {
    return jwt.sign({ id }, process.env.JWT_SECRET, { expiresIn: process.env.JWT_EXPIRES_IN || '30d' });
}

module.exports = router;
