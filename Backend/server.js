require('dotenv').config();
const express = require('express');
const rateLimit = require('express-rate-limit');
const connectDB = require('./config/db');

const authRoutes     = require('./routes/auth');
const coinsRoutes    = require('./routes/coins');
const paymentsRoutes = require('./routes/payments');

const app = express();

// ─── DB ───────────────────────────────────────────────────
connectDB();

// ─── Middleware ───────────────────────────────────────────
app.use(express.json());

// Rate limit all API routes (100 req / 15 min per IP)
app.use('/api', rateLimit({
    windowMs: 15 * 60 * 1000,
    max: 100,
    standardHeaders: true,
    legacyHeaders: false,
    message: { error: 'Too many requests, please try again later.' }
}));

// ─── Routes ───────────────────────────────────────────────
app.use('/api/auth',     authRoutes);
app.use('/api/coins',    coinsRoutes);
app.use('/api/payments', paymentsRoutes);

// ─── Health check ─────────────────────────────────────────
app.get('/health', (_req, res) => res.json({ status: 'ok' }));

// ─── Global error handler ─────────────────────────────────
app.use((err, _req, res, _next) => {
    console.error(err.stack);
    res.status(500).json({ error: 'Internal server error' });
});

// ─── Start ────────────────────────────────────────────────
const PORT = process.env.PORT || 3000;
app.listen(PORT, () => console.log(`BigMoneySlots backend running on port ${PORT}`));
