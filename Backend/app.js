/**
 * Express app factory — exported separately so tests can import it without
 * triggering connectDB() or server.listen().
 */
const express    = require('express');
const path       = require('path');
const rateLimit  = require('express-rate-limit');
const ipLogger   = require('./middleware/ipLogger');
const { securityGuard } = require('./middleware/securityGuard');

const authRoutes           = require('./routes/auth');
const coinsRoutes          = require('./routes/coins');
const paymentsRoutes       = require('./routes/payments');
const tournamentRoutes     = require('./routes/tournament');
const chatRoutes           = require('./routes/chat');
const friendsRoutes        = require('./routes/friends');
const accountLinkingRoutes = require('./routes/accountLinking');
const securityRoutes       = require('./routes/security');
const inviteRoutes         = require('./routes/invite');

function createApp({ io = null, disableRateLimit = false } = {}) {
    const app = express();

    app.use(express.json({ limit: '10kb' }));
    app.use(express.static(path.join(__dirname, 'public')));

    if (!disableRateLimit) {
        app.use('/api', rateLimit({
            windowMs: 15 * 60 * 1000,
            max: 120,
            standardHeaders: true,
            legacyHeaders: false,
            message: { error: 'Too many requests, please slow down.' },
        }));
    }

    app.use(ipLogger);
    app.use(securityGuard);
    app.set('io', io);

    app.use('/api/auth',       authRoutes);
    app.use('/api/coins',      coinsRoutes);
    app.use('/api/payments',   paymentsRoutes);
    app.use('/api/tournament', tournamentRoutes);
    app.use('/api/chat',       chatRoutes);
    app.use('/api/friends',    friendsRoutes);
    app.use('/api/account',    accountLinkingRoutes);
    app.use('/api/security',   securityRoutes);
    app.use('/api/invite',     inviteRoutes);

    app.get('/health', (_req, res) => res.json({ status: 'ok', time: new Date().toISOString() }));

    app.use((err, _req, res, _next) => {
        console.error('[app error]', err.stack);
        res.status(500).json({ error: 'Internal server error' });
    });

    return app;
}

module.exports = createApp;
