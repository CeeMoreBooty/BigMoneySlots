const express     = require('express');
const auth        = require('../middleware/auth');
const ChatMessage = require('../models/ChatMessage');
const router      = express.Router();

/**
 * GET /api/chat/history?channel=global&limit=50
 */
router.get('/history', auth, async (req, res) => {
    try {
        const { channel = 'global', roomId, limit = 50 } = req.query;
        const query = { channel, deleted: false };
        if (channel === 'room' && roomId) query.roomId = roomId;
        if (channel === 'friends') {
            query.$or = [
                { senderId: req.player._id, targetId: req.query.targetId },
                { senderId: req.query.targetId, targetId: req.player._id },
            ];
        }
        const messages = await ChatMessage.find(query)
            .sort({ createdAt: -1 })
            .limit(Math.min(100, parseInt(limit)))
            .lean();
        res.json({ messages: messages.reverse() });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/chat/send
 * Body: { text, channel, roomId?, targetId? }
 */
router.post('/send', auth, async (req, res) => {
    try {
        const { text, channel = 'global', roomId, targetId } = req.body;
        if (!text || text.trim().length === 0)
            return res.status(400).json({ error: 'Text required' });

        const sanitized = text.trim().substring(0, 200);
        const msg = await ChatMessage.create({
            senderId:   req.player._id,
            senderName: req.player.displayName,
            text:       sanitized,
            channel,
            roomId:     roomId  || null,
            targetId:   targetId || null,
        });

        // Emit via socket.io (injected by server.js)
        const io = req.app.get('io');
        if (io) io.to(channel === 'room' ? `room:${roomId}` : channel).emit('chat_message', msg);

        res.status(201).json({ message: msg });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/chat/voice/join  — signal backend that player joined voice room
 */
router.post('/voice/join', auth, async (req, res) => {
    const { roomId } = req.body;
    const io = req.app.get('io');
    if (io && roomId) io.to(`room:${roomId}`).emit('voice_joined', { playerId: req.player._id, displayName: req.player.displayName });
    res.json({ ok: true });
});

/**
 * POST /api/chat/voice/leave
 */
router.post('/voice/leave', auth, async (req, res) => {
    const { roomId } = req.body;
    const io = req.app.get('io');
    if (io && roomId) io.to(`room:${roomId}`).emit('voice_left', { playerId: req.player._id });
    res.json({ ok: true });
});

module.exports = router;
