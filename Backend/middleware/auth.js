const jwt    = require('jsonwebtoken');
const Player = require('../models/Player');

const JWT_SECRET  = process.env.JWT_SECRET  || 'changeme_use_env_var';
const JWT_EXPIRES = process.env.JWT_EXPIRES || '7d';

const signToken = (userId) =>
    jwt.sign({ id: userId }, JWT_SECRET, { expiresIn: JWT_EXPIRES });

/**
 * Default export: Player-model auth middleware.
 * Verifies the Bearer JWT, loads the Player document, and sets req.player.
 * Used by routes that operate on the Player model (coins, payments, chat, etc.)
 */
const auth = async (req, res, next) => {
    const header = req.headers.authorization;
    if (!header || !header.startsWith('Bearer '))
        return res.status(401).json({ error: 'No token provided' });

    const token = header.split(' ')[1];
    try {
        const payload = jwt.verify(token, JWT_SECRET);
        const player  = await Player.findById(payload.id);
        if (!player) return res.status(401).json({ error: 'Player not found' });
        req.player = player;
        next();
    } catch {
        res.status(401).json({ error: 'Invalid or expired token' });
    }
};

/**
 * Named export: User-model auth middleware.
 * Verifies the Bearer JWT and sets req.user (id only, no DB lookup).
 * Used by routes that operate on the User model (game, leaderboard, user, challenges).
 */
const verifyToken = (req, res, next) => {
    const header = req.headers.authorization;
    if (!header || !header.startsWith('Bearer '))
        return res.status(401).json({ error: 'No token provided' });

    const token = header.split(' ')[1];
    try {
        req.user = jwt.verify(token, JWT_SECRET);
        next();
    } catch {
        res.status(401).json({ error: 'Invalid or expired token' });
    }
};

// Attach named helpers so both import styles continue to work without change:
//   const auth            = require('../middleware/auth');  → Player middleware fn
//   const { signToken }   = require('../middleware/auth');  → token creator
//   const { verifyToken } = require('../middleware/auth');  → User middleware fn
auth.signToken   = signToken;
auth.verifyToken = verifyToken;

module.exports = auth;
