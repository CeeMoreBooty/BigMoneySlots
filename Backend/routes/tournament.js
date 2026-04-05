const express         = require('express');
const auth            = require('../middleware/auth');
const Tournament      = require('../models/Tournament');
const TournamentEntry = require('../models/TournamentEntry');
const Player          = require('../models/Player');
const Transaction     = require('../models/Transaction');
const router          = express.Router();

/**
 * GET /api/tournament/current  — active tournament
 */
router.get('/current', auth, async (req, res) => {
    try {
        const t = await Tournament.findOne({ status: 'active' }).sort({ startTime: -1 });
        if (!t) return res.json({ tournament: null });

        const entry = await TournamentEntry.findOne({ tournamentId: t._id, playerId: req.player._id });
        res.json({ tournament: t, myEntry: entry });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/tournament/join  — join active tournament
 */
router.post('/join', auth, async (req, res) => {
    try {
        const t = await Tournament.findOne({ status: 'active' });
        if (!t) return res.status(404).json({ error: 'No active tournament' });

        const existing = await TournamentEntry.findOne({ tournamentId: t._id, playerId: req.player._id });
        if (existing) return res.json({ entry: existing, alreadyJoined: true });

        const entry = await TournamentEntry.create({
            tournamentId: t._id,
            playerId:     req.player._id,
            displayName:  req.player.displayName,
        });
        await Tournament.findByIdAndUpdate(t._id, { $inc: { totalPlayers: 1 } });
        res.status(201).json({ entry });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/tournament/score  — submit spin result score
 * Body: { coinsWon, biggestWin }
 */
router.post('/score', auth, async (req, res) => {
    try {
        const coinsWon  = Math.max(0, Math.min(50_000_000_000, Number(req.body.coinsWon)  || 0));
        const biggestWin = Math.max(0, Math.min(50_000_000_000, Number(req.body.biggestWin) || 0));

        const t = await Tournament.findOne({ status: 'active' });
        if (!t) return res.status(404).json({ error: 'No active tournament' });

        // Require player to have joined first (no upsert)
        const entry = await TournamentEntry.findOneAndUpdate(
            { tournamentId: t._id, playerId: req.player._id },
            {
                $inc: { score: coinsWon, spinsPlayed: 1 },
                $max: { biggestWin },
            },
            { new: true }
        );
        if (!entry) return res.status(400).json({ error: 'You must join the tournament first (POST /api/tournament/join)' });
        res.json({ entry });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * GET /api/tournament/leaderboard?limit=20
 */
router.get('/leaderboard', auth, async (req, res) => {
    try {
        const t = await Tournament.findOne({ status: { $in: ['active', 'ended'] } }).sort({ startTime: -1 });
        if (!t) return res.json({ leaderboard: [] });

        const limit = Math.min(100, parseInt(req.query.limit) || 20);
        const entries = await TournamentEntry
            .find({ tournamentId: t._id })
            .sort({ score: -1 })
            .limit(limit)
            .select('displayName score biggestWin spinsPlayed rank prizeAwarded');

        res.json({ tournament: t, leaderboard: entries });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * GET /api/tournament/history?page=1
 */
router.get('/history', auth, async (req, res) => {
    try {
        const page   = Math.max(1, parseInt(req.query.page) || 1);
        const limit  = 10;
        const past   = await Tournament.find({ status: 'ended' })
            .sort({ endTime: -1 })
            .skip((page - 1) * limit)
            .limit(limit);
        res.json({ tournaments: past, page });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
