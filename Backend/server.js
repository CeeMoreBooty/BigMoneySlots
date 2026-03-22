require('dotenv').config();
const express    = require('express');
const http       = require('http');
const { Server } = require('socket.io');
const rateLimit  = require('express-rate-limit');
const connectDB  = require('./config/db');
const ipLogger   = require('./middleware/ipLogger');

// ── Routes ────────────────────────────────────────────────────────────────────
const authRoutes           = require('./routes/auth');
const coinsRoutes          = require('./routes/coins');
const paymentsRoutes       = require('./routes/payments');
const tournamentRoutes     = require('./routes/tournament');
const chatRoutes           = require('./routes/chat');
const friendsRoutes        = require('./routes/friends');
const accountLinkingRoutes = require('./routes/accountLinking');

// ── Services ──────────────────────────────────────────────────────────────────
const tournamentScheduler  = require('./services/tournamentScheduler');

const app    = express();
const server = http.createServer(app);
const io     = new Server(server, { cors: { origin: '*' } });

// ── DB ────────────────────────────────────────────────────────────────────────
connectDB().then(() => tournamentScheduler.start());

// ── Middleware ────────────────────────────────────────────────────────────────
app.use(express.json());
app.use(ipLogger);
app.set('io', io);   // make socket.io available in routes via req.app.get('io')

// Rate limit: 120 req / 15 min per IP
app.use('/api', rateLimit({
    windowMs: 15 * 60 * 1000,
    max: 120,
    standardHeaders: true,
    legacyHeaders: false,
    message: { error: 'Too many requests, please slow down.' },
}));

// ── API Routes ────────────────────────────────────────────────────────────────
app.use('/api/auth',        authRoutes);
app.use('/api/coins',       coinsRoutes);
app.use('/api/payments',    paymentsRoutes);
app.use('/api/tournament',  tournamentRoutes);
app.use('/api/chat',        chatRoutes);
app.use('/api/friends',     friendsRoutes);
app.use('/api/account',     accountLinkingRoutes);

// ── Health ────────────────────────────────────────────────────────────────────
app.get('/health', (_req, res) => res.json({ status: 'ok', time: new Date().toISOString() }));

// ── Socket.IO ─────────────────────────────────────────────────────────────────
io.on('connection', socket => {
    console.log(`[socket.io] Client connected: ${socket.id}`);

    socket.on('join_room', roomId => {
        socket.join(roomId);
        console.log(`[socket.io] ${socket.id} joined room: ${roomId}`);
    });

    socket.on('leave_room', roomId => socket.leave(roomId));

    socket.on('join_player_room', playerId => {
        socket.join(`player:${playerId}`);
    });

    socket.on('chat_message', data => {
        // Broadcast to room or global
        const target = data.roomId ? `room:${data.roomId}` : (data.channel || 'global');
        io.to(target).emit('chat_message', data);
    });

    socket.on('disconnect', () => {
        console.log(`[socket.io] Client disconnected: ${socket.id}`);
    });
});

// ── Global error handler ──────────────────────────────────────────────────────
app.use((err, _req, res, _next) => {
    console.error('[server error]', err.stack);
    res.status(500).json({ error: 'Internal server error' });
});

// ── Start ─────────────────────────────────────────────────────────────────────
const PORT = process.env.PORT || 3000;
server.listen(PORT, () => console.log(`BigMoneySlots backend running on port ${PORT}`));
