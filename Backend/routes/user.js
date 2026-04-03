const router = require('express').Router();
const User   = require('../models/User');
const { verifyToken } = require('../middleware/auth');

// GET /api/user/profile
router.get('/profile', verifyToken, async (req, res) => {
    try {
        const user = await User.findById(req.user.id);
        if (!user) return res.status(404).json({ error: 'User not found' });
        res.json(user.toPublicJSON());
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// PATCH /api/user/profile  – update coins/gems/level/xp
router.patch('/profile', verifyToken, async (req, res) => {
    try {
        const allowed = ['coins', 'gems', 'level', 'xp'];
        const updates = {};
        for (const key of allowed)
            if (req.body[key] !== undefined) updates[key] = req.body[key];

        const user = await User.findByIdAndUpdate(
            req.user.id, updates, { new: true });
        if (!user) return res.status(404).json({ error: 'User not found' });
        res.json(user.toPublicJSON());
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
