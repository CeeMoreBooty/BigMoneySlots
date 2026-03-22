const express     = require('express');
const auth        = require('../middleware/auth');
const Player      = require('../models/Player');
const Transaction = require('../models/Transaction');
const router      = express.Router();

/**
 * GET /api/coins/balance
 */
router.get('/balance', auth, async (req, res) => {
    res.json({
        coins:      req.player.coins,
        freeSpins:  req.player.freeSpins,
        superSpins: req.player.superSpins,
        ultraSpins: req.player.ultraSpins,
        gems:       req.player.gems || 0,
    });
});

/**
 * GET /api/coins/transactions?page=1&limit=20
 */
router.get('/transactions', auth, async (req, res) => {
    try {
        const page  = Math.max(1, parseInt(req.query.page)  || 1);
        const limit = Math.min(50, parseInt(req.query.limit) || 20);
        const txs   = await Transaction
            .find({ playerId: req.player._id })
            .sort({ createdAt: -1 })
            .skip((page - 1) * limit)
            .limit(limit);
        res.json({ transactions: txs, page });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/coins/sync  — sync client-side economy to server (authoritative check)
 * Body: { coins, freeSpins, superSpins, ultraSpins }
 * Server always wins on coin balance — client cannot set values higher than server.
 */
router.post('/sync', auth, async (req, res) => {
    try {
        const player = req.player;
        res.json({
            coins:      player.coins,
            freeSpins:  player.freeSpins,
            superSpins: player.superSpins,
            ultraSpins: player.ultraSpins,
            gems:       player.gems || 0,
        });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
