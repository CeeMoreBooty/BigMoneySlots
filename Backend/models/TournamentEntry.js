const mongoose = require('mongoose');

const tournamentEntrySchema = new mongoose.Schema({
    tournamentId: { type: mongoose.Schema.Types.ObjectId, ref: 'Tournament', required: true },
    playerId:     { type: mongoose.Schema.Types.ObjectId, ref: 'Player',     required: true },
    displayName:  { type: String, default: 'Player' },

    // Score = total coins won during the tournament window
    score:        { type: Number, default: 0, min: 0 },
    spinsPlayed:  { type: Number, default: 0, min: 0 },
    biggestWin:   { type: Number, default: 0, min: 0 },

    // Final rank populated when tournament ends
    rank:         { type: Number, default: null },
    prizeAwarded: { type: Number, default: 0 },
}, { timestamps: true });

tournamentEntrySchema.index({ tournamentId: 1, score: -1 });
tournamentEntrySchema.index({ tournamentId: 1, playerId: 1 }, { unique: true });

module.exports = mongoose.model('TournamentEntry', tournamentEntrySchema);
