const mongoose = require('mongoose');

const linkedAccountSchema = new mongoose.Schema({
    playerId:    { type: mongoose.Schema.Types.ObjectId, ref: 'Player', required: true },
    provider:    { type: String, enum: ['facebook', 'google', 'phone', 'discord'], required: true },
    providerUid: { type: String, required: true },       // UID from OAuth provider
    displayName: { type: String, default: '' },          // e.g. email or username
    bonusGranted:{ type: Boolean, default: false },
    linkedAt:    { type: Date, default: Date.now },
}, { timestamps: true });

linkedAccountSchema.index({ playerId: 1, provider: 1 }, { unique: true });
linkedAccountSchema.index({ provider: 1, providerUid: 1 }, { unique: true });

module.exports = mongoose.model('LinkedAccount', linkedAccountSchema);
