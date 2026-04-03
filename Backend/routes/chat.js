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
    // Remove this speaker's stored chunks so room members stop hearing stale audio
    if (roomId) cleanSpeaker(roomId, String(req.player._id));
    res.json({ ok: true });
});

// ─────────────────────────────────────────────────────────────────────────────
// Voice Audio Relay  (in-memory — MongoDB is too slow for real-time PCM relay)
//
// Stores: roomAudio  Map<roomId, Map<speakerId, { audioData, sampleRate,
//                                                 sampleCount, displayName, ts }>>
// Chunks expire after CHUNK_TTL_MS ms of inactivity per speaker.
// ─────────────────────────────────────────────────────────────────────────────
const roomAudio  = new Map();          // roomId → Map<speakerId, chunk>
const CHUNK_TTL_MS = 800;             // drop stale audio after 800 ms

function cleanSpeaker(roomId, speakerId) {
    const room = roomAudio.get(roomId);
    if (room) room.delete(speakerId);
}

function cleanExpired() {
    const now = Date.now();
    for (const [roomId, speakers] of roomAudio) {
        for (const [speakerId, chunk] of speakers) {
            if (now - chunk.ts > CHUNK_TTL_MS) speakers.delete(speakerId);
        }
        if (speakers.size === 0) roomAudio.delete(roomId);
    }
}
setInterval(cleanExpired, 500);

/**
 * POST /api/chat/voice/audio
 * Body: { roomId, audioData (base64 PCM), sampleRate, sampleCount }
 *
 * Stores the latest audio chunk for this speaker.
 * Also broadcasts via socket.io so clients with a persistent connection
 * receive audio without polling.
 */
router.post('/voice/audio', auth, async (req, res) => {
    const { roomId, audioData, sampleRate, sampleCount } = req.body;
    if (!roomId || !audioData || !sampleCount)
        return res.status(400).json({ error: 'roomId, audioData and sampleCount required' });

    const speakerId = String(req.player._id);
    if (!roomAudio.has(roomId)) roomAudio.set(roomId, new Map());

    const chunk = {
        speakerId,
        displayName: req.player.displayName || 'Player',
        audioData,
        sampleRate:  sampleRate  || 16000,
        sampleCount: sampleCount || 0,
        ts: Date.now(),
    };
    roomAudio.get(roomId).set(speakerId, chunk);

    // Broadcast to socket.io room so low-latency clients skip polling
    const io = req.app.get('io');
    if (io) io.to(`room:${roomId}`).emit('voice_audio', chunk);

    res.json({ ok: true });
});

/**
 * GET /api/chat/voice/audio/:roomId?exclude=<speakerId>
 *
 * Returns the latest audio chunk for every active speaker in the room,
 * excluding the requesting device's own speaker ID.
 */
router.get('/voice/audio/:roomId', auth, (req, res) => {
    const { roomId }  = req.params;
    const excludeId   = req.query.exclude || String(req.player._id);
    const now         = Date.now();

    const speakers = roomAudio.get(roomId);
    if (!speakers || speakers.size === 0) return res.json({ chunks: [] });

    const chunks = [];
    for (const [speakerId, chunk] of speakers) {
        if (speakerId === excludeId) continue;
        if (now - chunk.ts > CHUNK_TTL_MS) continue;  // skip stale
        chunks.push(chunk);
    }
    res.json({ chunks });
});

module.exports = router;
