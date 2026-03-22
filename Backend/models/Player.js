const mongoose = require('mongoose');
const bcrypt = require('bcryptjs');

const playerSchema = new mongoose.Schema({
    deviceId:     { type: String, required: true, unique: true },
    displayName:  { type: String, default: 'Player' },
    passwordHash: { type: String },

    // Coin ledger (authoritative server-side balance)
    coins:        { type: Number, default: 0, min: 0 },
    freeSpins:    { type: Number, default: 0, min: 0 },
    superSpins:   { type: Number, default: 0, min: 0 },
    ultraSpins:   { type: Number, default: 0, min: 0 },
    jackpotTickets: { type: Number, default: 0, min: 0 },
    mysteryChests:  { type: Number, default: 0, min: 0 },

    // Cosmetics
    badgeId:     { type: String, default: '' },
    reelSkinId:  { type: String, default: '' },

    // Win boost
    winBoostMultiplier: { type: Number, default: 1 },
    winBoostExpiry:     { type: Date, default: null },

    // Flags
    welcomeOfferPurchased: { type: Boolean, default: false },
    newPlayerPackClaimed:  { type: Boolean, default: false },
}, { timestamps: true });

// Hash password before save
playerSchema.pre('save', async function (next) {
    if (this.isModified('passwordHash') && this.passwordHash) {
        this.passwordHash = await bcrypt.hash(this.passwordHash, 12);
    }
    next();
});

playerSchema.methods.verifyPassword = function (plain) {
    return bcrypt.compare(plain, this.passwordHash);
};

module.exports = mongoose.model('Player', playerSchema);
