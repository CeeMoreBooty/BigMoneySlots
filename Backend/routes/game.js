const router = require('express').Router();
const Spin   = require('../models/Spin');
const User   = require('../models/User');
const { verifyToken } = require('../middleware/auth');

// Maximum coins a single spin can award (anti-cheat ceiling)
const MAX_COINS_PER_SPIN = 50_000_000_000; // 50B – generous ceiling for jackpots

// POST /api/game/spin  – record a spin result and update player coins
router.post('/spin', verifyToken, async (req, res) => {
    try {
        const { coinsWon, winLevel } = req.body;

        // Validate coinsWon is a finite, non-negative number within bounds
        const won = Number(coinsWon) || 0;
        if (!Number.isFinite(won) || won < 0 || won > MAX_COINS_PER_SPIN) {
            return res.status(400).json({ error: 'Invalid coinsWon value' });
        }
        const safeWinLevel = Math.max(0, Math.min(10, Number(winLevel) || 0));

        // Record the spin
        await Spin.create({
            user:     req.user.id,
            coinsWon: String(won),
            winLevel: safeWinLevel
        });

        // Update user totals
        const user = await User.findById(req.user.id);
        if (!user) return res.status(404).json({ error: 'User not found' });

        const wonBig   = BigInt(Math.floor(won));
        const current  = BigInt(user.coins || 0);
        user.coins     = String(current + wonBig);
        user.totalSpins += 1;
        await user.save();

        res.json({ success: true, coins: user.coins, totalSpins: user.totalSpins });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
