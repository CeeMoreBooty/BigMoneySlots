/**
 * adminOnly — middleware that verifies the x-admin-secret header.
 * Reuses the same ADMIN_SECRET env var as the /api/security/* routes.
 * Apply AFTER the `auth` middleware so req.player is available if needed.
 */
const adminOnly = (req, res, next) => {
    const secret = req.headers['x-admin-secret'];
    if (!secret || secret !== process.env.ADMIN_SECRET) {
        return res.status(403).json({ error: 'Forbidden: admin access required' });
    }
    next();
};

module.exports = adminOnly;
