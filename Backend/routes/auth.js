const router  = require('express').Router();
const User    = require('../models/User');
const { signToken } = require('../middleware/auth');

// POST /api/auth/register
router.post('/register', async (req, res) => {
    try {
        const { username, password } = req.body;
        if (!username || !password)
            return res.status(400).json({ error: 'Username and password are required' });

        const existing = await User.findOne({ username });
        if (existing)
            return res.status(409).json({ error: 'Username already taken' });

        const user  = await User.create({ username, password });
        const token = signToken(user._id);
        res.status(201).json({ token, user: user.toPublicJSON() });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// POST /api/auth/login
router.post('/login', async (req, res) => {
    try {
        const { username, password } = req.body;
        if (!username || !password)
            return res.status(400).json({ error: 'Username and password are required' });

        const user = await User.findOne({ username });
        if (!user || !(await user.comparePassword(password)))
            return res.status(401).json({ error: 'Invalid credentials' });

        user.lastLogin = new Date();
        await user.save();

        const token = signToken(user._id);
        res.json({ token, user: user.toPublicJSON() });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
