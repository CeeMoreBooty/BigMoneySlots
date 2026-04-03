const IpLog = require('../models/IpLog');

/**
 * Middleware: logs every API request IP for security and fraud detection.
 * Attach after express.json() in server.js.
 * NOTE: Ensure your Privacy Policy discloses IP collection (GDPR / CCPA requirement).
 */
const ipLogger = async (req, res, next) => {
    if (process.env.IP_LOGGING_ENABLED !== 'true') return next();

    // Respect X-Forwarded-For when running behind a proxy / load balancer
    const ip = (req.headers['x-forwarded-for'] || req.socket.remoteAddress || '')
        .split(',')[0]
        .trim();

    const playerId = req.player?._id ?? null;

    // Fire-and-forget — don't block the request on DB write
    IpLog.create({
        ip,
        playerId,
        path:      req.path,
        method:    req.method,
        userAgent: req.headers['user-agent'] || '',
    }).catch(err => console.error('[ipLogger] Failed to write log:', err.message));

    next();
};

module.exports = ipLogger;
