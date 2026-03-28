const router = require('express').Router();
const { verifyToken } = require('../middleware/auth');

// POST /api/challenges/sync  – store challenge progress JSON server-side
// (lightweight key-value approach – progress data comes from the Unity client)
router.post('/sync', verifyToken, async (req, res) => {
    try {
        // For now we just acknowledge; a full implementation would persist to DB
        res.json({ success: true });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// GET /api/challenges/sync  – retrieve latest synced challenge state
router.get('/sync', verifyToken, async (req, res) => {
    try {
        res.json({ data: null }); // placeholder
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
