require('dotenv').config();
const http      = require('http');
const jwt       = require('jsonwebtoken');
const { Server } = require('socket.io');
const connectDB  = require('./config/db');
const createApp  = require('./app');

const PORT       = process.env.PORT || 3000;
const JWT_SECRET = process.env.JWT_SECRET || 'changeme_use_env_var';

// ── Database ──────────────────────────────────────────────────────────────────
connectDB();

// ── HTTP + Socket.IO ──────────────────────────────────────────────────────────
const httpServer = http.createServer();
const io = new Server(httpServer, {
    cors: { origin: '*' },
});

// Authenticate socket connections and join the player's personal room
io.use((socket, next) => {
    const token = socket.handshake.auth?.token;
    if (!token) return next(new Error('Missing token'));
    try {
        const payload = jwt.verify(token, JWT_SECRET);
        socket.playerId = payload.id;
        next();
    } catch {
        next(new Error('Invalid token'));
    }
});

io.on('connection', (socket) => {
    socket.join(`player:${socket.playerId}`);
    socket.on('join_room', (roomId) => socket.join(`room:${roomId}`));
    socket.on('leave_room', (roomId) => socket.leave(`room:${roomId}`));
});

// ── Express app ───────────────────────────────────────────────────────────────
const app = createApp({ io });
httpServer.on('request', app);

httpServer.listen(PORT, () =>
    console.log(`BigMoneySlots API running on port ${PORT}`)
);
