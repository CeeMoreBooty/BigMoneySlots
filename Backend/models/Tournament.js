const mongoose = require('mongoose');

// Describes a tournament prize tier
const prizeTierSchema = new mongoose.Schema({
    rank:        { type: Number, required: true },  // 1 = 1st place
    coinsReward: { type: Number, required: true },
    label:       { type: String, default: '' },     // e.g. "🥇 1st Place"
}, { _id: false });

const tournamentSchema = new mongoose.Schema({
    // Scheduling
    startTime:   { type: Date, required: true },
    endTime:     { type: Date, required: true },    // startTime + 30 minutes
    status:      { type: String, enum: ['pending', 'active', 'ended'], default: 'pending' },

    // Selected slot game for this tournament
    selectedGame: {
        gameId:   { type: String, required: true },
        gameName: { type: String, required: true },
    },

    // Prize pool
    prizeTiers: { type: [prizeTierSchema], default: () => defaultPrizeTiers() },

    // Totals (denormalized for fast reads)
    totalPlayers: { type: Number, default: 0 },
    winnersAwarded: { type: Boolean, default: false },
}, { timestamps: true });

tournamentSchema.index({ status: 1 });
tournamentSchema.index({ startTime: -1 });

function defaultPrizeTiers() {
    return [
        { rank: 1, coinsReward: 500_000_000, label: '🥇 1st Place' },
        { rank: 2, coinsReward: 250_000_000, label: '🥈 2nd Place' },
        { rank: 3, coinsReward: 100_000_000, label: '🥉 3rd Place' },
        { rank: 4, coinsReward:  50_000_000, label: '4th Place' },
        { rank: 5, coinsReward:  25_000_000, label: '5th Place' },
    ];
}

module.exports = mongoose.model('Tournament', tournamentSchema);
