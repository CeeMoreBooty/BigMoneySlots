const router  = require('express').Router();
const User    = require('../models/User');
const Player  = require('../models/Player');
const auth    = require('../middleware/auth');
const { signToken } = auth;

// ── Player-based registration (deviceId) ─────────────────────────────────────

// POST /api/auth/register  — supports both { deviceId } and { username, password }
router.post('/register', async (req, res) => {
    try {
        const { deviceId, displayName, username, password } = req.body;

        // ── Device-based registration (Player model) ─────────────────
        if (deviceId) {
            let player = await Player.findOne({ deviceId });
            if (player) {
                const token = signToken(player._id);
                return res.json({ token, isNew: false, playerId: player._id });
            }
            player = await Player.create({
                deviceId,
                displayName: displayName || 'Player',
                coins: 10_000_000,
            });
            const token = signToken(player._id);
            return res.status(201).json({ token, isNew: true, playerId: player._id });
        }

        // ── Username/password registration (User model) ──────────────
        if (!username || !password)
            return res.status(400).json({ error: 'deviceId or (username + password) is required' });

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

// POST /api/auth/login  — username/password
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

// ── Player profile endpoints ─────────────────────────────────────────────────

// GET /api/auth/me  — returns authenticated player's profile
router.get('/me', auth, async (req, res) => {
    try {
        res.json({
            playerId:    req.player._id,
            displayName: req.player.displayName,
            coins:       req.player.coins,
            gems:        req.player.gems || 0,
            freeSpins:   req.player.freeSpins,
            deviceId:    req.player.deviceId,
        });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// PATCH /api/auth/display-name  — update display name
router.patch('/display-name', auth, async (req, res) => {
    try {
        let { displayName } = req.body;
        if (!displayName || typeof displayName !== 'string')
            return res.status(400).json({ error: 'displayName is required' });

        displayName = displayName.trim();
        if (displayName.length < 2)
            return res.status(400).json({ error: 'Display name must be at least 2 characters' });
        if (displayName.length > 24)
            displayName = displayName.substring(0, 24);

        req.player.displayName = displayName;
        await req.player.save();
        res.json({ displayName: req.player.displayName });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
