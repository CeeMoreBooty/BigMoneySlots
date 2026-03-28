const router = require('express').Router();
const Spin   = require('../models/Spin');
const User   = require('../models/User');
const { verifyToken } = require('../middleware/auth');

// POST /api/game/spin  – record a spin result and update player coins
router.post('/spin', verifyToken, async (req, res) => {
    try {
        const { coinsWon, winLevel } = req.body;

        // Record the spin
        await Spin.create({
            user:     req.user.id,
            coinsWon: String(coinsWon || 0),
            winLevel: Number(winLevel) || 0
        });

        // Update user totals
        const user = await User.findById(req.user.id);
        if (!user) return res.status(404).json({ error: 'User not found' });

        const won      = BigInt(coinsWon || 0);
        const current  = BigInt(user.coins || 0);
        user.coins     = String(current + won);
        user.totalSpins += 1;
        await user.save();

        res.json({ success: true, coins: user.coins, totalSpins: user.totalSpins });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
