const jwt          = require('jsonwebtoken');
const Player       = require('../models/Player');
const BannedEntity = require('../models/BannedEntity');

const JWT_SECRET  = process.env.JWT_SECRET  || 'changeme_use_env_var';
const JWT_EXPIRES = process.env.JWT_EXPIRES_IN || process.env.JWT_EXPIRES || '7d';

/**
 * Sign a JWT containing { id }.
 */
const signToken = (userId) =>
    jwt.sign({ id: userId }, JWT_SECRET, { expiresIn: JWT_EXPIRES });

/**
 * Lightweight token verifier — sets req.user from the JWT payload.
 * Used by User-model routes (game, leaderboard, user, challenges).
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

/**
 * Player-loading auth middleware — verifies JWT then loads the full Player
 * document from MongoDB and attaches it as req.player.
 * Also checks if the player is banned.
 * Used by coins, chat, friends, payments, tournament, invite, accountLinking routes.
 */
const authenticatePlayer = async (req, res, next) => {
    const header = req.headers.authorization;
    if (!header || !header.startsWith('Bearer '))
        return res.status(401).json({ error: 'No token provided' });

    const token = header.split(' ')[1];
    try {
        const decoded = jwt.verify(token, JWT_SECRET);
        const player  = await Player.findById(decoded.id);
        if (!player) return res.status(401).json({ error: 'Player not found' });

        // Check if this player is banned
        const ban = await BannedEntity.findOne({
            type: 'player', value: String(player._id), active: true,
            $or: [{ expiresAt: null }, { expiresAt: { $gt: new Date() } }]
        });
        if (ban) return res.status(403).json({ error: 'Account suspended' });

        req.player = player;
        req.user   = decoded;   // keep for compatibility
        next();
    } catch {
        res.status(401).json({ error: 'Invalid or expired token' });
    }
};

// Default export = Player-loading middleware (used by: const auth = require('./auth'))
// Named exports  = signToken, verifyToken
module.exports              = authenticatePlayer;
module.exports.signToken    = signToken;
module.exports.verifyToken  = verifyToken;
