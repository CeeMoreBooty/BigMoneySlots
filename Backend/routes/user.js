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

// PATCH /api/user/profile  – update coins/gems/level/xp/profileImageUrl
router.patch('/profile', verifyToken, async (req, res) => {
    try {
        const allowed = ['coins', 'gems', 'level', 'xp', 'profileImageUrl'];
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

// PUT /api/user/profile-image – update profile image URL
router.put('/profile-image', verifyToken, async (req, res) => {
    try {
        const { profileImageUrl } = req.body;
        if (!profileImageUrl) {
            return res.status(400).json({ error: 'profileImageUrl is required' });
        }

        const user = await User.findByIdAndUpdate(
            req.user.id,
            { profileImageUrl },
            { new: true }
        );

        if (!user) return res.status(404).json({ error: 'User not found' });
        res.json({
            success: true,
            profileImageUrl: user.profileImageUrl,
            user: user.toPublicJSON()
        });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
