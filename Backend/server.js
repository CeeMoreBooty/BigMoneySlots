require('dotenv').config();
const express  = require('express');
const cors     = require('cors');
const connectDB = require('./config/db');

const authRoutes       = require('./routes/auth');
const userRoutes       = require('./routes/user');
const gameRoutes       = require('./routes/game');
const challengeRoutes  = require('./routes/challenges');
const leaderboardRoutes = require('./routes/leaderboard');

const app  = express();
const PORT = process.env.PORT || 3000;

// ── Middleware ────────────────────────────────────────────────────────────────
app.use(cors());
app.use(express.json());

// ── Database ──────────────────────────────────────────────────────────────────
connectDB();

// ── Routes ────────────────────────────────────────────────────────────────────
app.use('/api/auth',        authRoutes);
app.use('/api/user',        userRoutes);
app.use('/api/game',        gameRoutes);
app.use('/api/challenges',  challengeRoutes);
app.use('/api/leaderboard', leaderboardRoutes);

// ── Health check ──────────────────────────────────────────────────────────────
app.get('/health', (_req, res) => res.json({ status: 'ok' }));

// ── 404 handler ───────────────────────────────────────────────────────────────
app.use((_req, res) => res.status(404).json({ error: 'Not found' }));

// ── Error handler ─────────────────────────────────────────────────────────────
app.use((err, _req, res, _next) => {
    console.error(err.stack);
    res.status(500).json({ error: 'Internal server error' });
});

app.listen(PORT, () => console.log(`BigMoneySlots API running on port ${PORT}`));
