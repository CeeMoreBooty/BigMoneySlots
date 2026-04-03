const router = require('express').Router();
const User   = require('../models/User');

// GET /api/leaderboard  – top 50 players by coins (public endpoint)
router.get('/', async (req, res) => {
    try {
        const users = await User.find({}, 'username coins level totalSpins')
            .sort({ coins: -1 })
            .limit(50)
            .lean();

        const board = users.map((u, i) => ({
            rank:       i + 1,
            username:   u.username,
            coins:      u.coins,
            level:      u.level,
            totalSpins: u.totalSpins
        }));

        res.json(board);
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
