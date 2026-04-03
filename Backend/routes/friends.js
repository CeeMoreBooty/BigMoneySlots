const express    = require('express');
const auth       = require('../middleware/auth');
const Friendship = require('../models/Friendship');
const Player     = require('../models/Player');
const router     = express.Router();

/**
 * GET /api/friends  — get friends list + pending requests
 */
router.get('/', auth, async (req, res) => {
    try {
        const id = req.player._id;

        const accepted = await Friendship.find({
            status: 'accepted',
            $or: [{ requester: id }, { recipient: id }],
        }).populate('requester recipient', 'displayName profileImageUrl');

        const pending = await Friendship.find({
            recipient: id, status: 'pending'
        }).populate('requester', 'displayName profileImageUrl');

        const friends = accepted.map(f => {
            const other = f.requester._id.equals(id) ? f.recipient : f.requester;
            return {
                playerId: other._id,
                displayName: other.displayName,
                profileImageUrl: other.profileImageUrl || '',
                isOnline: false
            };
        });

        const pendingList = pending.map(f => ({
            playerId: f.requester._id,
            displayName: f.requester.displayName,
            profileImageUrl: f.requester.profileImageUrl || '',
            isPending: true
        }));

        res.json({ friends, pending: pendingList });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * GET /api/friends/search?q=name
 */
router.get('/search', auth, async (req, res) => {
    try {
        const q = (req.query.q || '').trim();
        if (q.length < 2) return res.status(400).json({ error: 'Query too short' });
        const players = await Player.find({
            displayName: { $regex: q, $options: 'i' },
            _id:         { $ne: req.player._id },
        }).limit(20).select('displayName');
        res.json({ results: players });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/friends/request  Body: { targetPlayerId }
 */
router.post('/request', auth, async (req, res) => {
    try {
        const { targetPlayerId } = req.body;
        if (!targetPlayerId) return res.status(400).json({ error: 'targetPlayerId required' });
        if (targetPlayerId === String(req.player._id)) return res.status(400).json({ error: 'Cannot add yourself' });

        const existing = await Friendship.findOne({
            $or: [
                { requester: req.player._id, recipient: targetPlayerId },
                { requester: targetPlayerId, recipient: req.player._id },
            ]
        });
        if (existing) return res.status(409).json({ error: 'Friend request already exists' });

        const friendship = await Friendship.create({ requester: req.player._id, recipient: targetPlayerId });

        const io = req.app.get('io');
        if (io) io.to(`player:${targetPlayerId}`).emit('friend_request', { from: req.player.displayName });

        res.status(201).json({ friendship });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/friends/accept  Body: { targetPlayerId }
 */
router.post('/accept', auth, async (req, res) => {
    try {
        const { targetPlayerId } = req.body;
        const f = await Friendship.findOne({ requester: targetPlayerId, recipient: req.player._id, status: 'pending' });
        if (!f) return res.status(404).json({ error: 'No pending request from that player' });
        f.status = 'accepted';
        await f.save();
        res.json({ friendship: f });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/friends/decline  Body: { targetPlayerId }
 */
router.post('/decline', auth, async (req, res) => {
    try {
        const { targetPlayerId } = req.body;
        await Friendship.findOneAndDelete({ requester: targetPlayerId, recipient: req.player._id, status: 'pending' });
        res.json({ ok: true });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/friends/remove  Body: { targetPlayerId }
 */
router.post('/remove', auth, async (req, res) => {
    try {
        const { targetPlayerId } = req.body;
        await Friendship.findOneAndDelete({
            status: 'accepted',
            $or: [
                { requester: req.player._id, recipient: targetPlayerId },
                { requester: targetPlayerId,  recipient: req.player._id },
            ]
        });
        res.json({ ok: true });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
