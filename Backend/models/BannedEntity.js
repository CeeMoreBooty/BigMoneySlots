const mongoose = require('mongoose');

/**
 * BannedEntity — stores banned IPs, player accounts, and device IDs.
 * Bans can be temporary (expiresAt set) or permanent (expiresAt null).
 */
const bannedEntitySchema = new mongoose.Schema({
    type:      { type: String, enum: ['ip', 'player', 'device', 'mac'], required: true },
    value:     { type: String, required: true },   // the IP, playerId string, or deviceId

    reason:    { type: String, default: 'Automated security ban' },
    bannedBy:  { type: String, default: 'system' }, // 'system' or admin playerId

    // Null = permanent ban
    expiresAt: { type: Date, default: null },

    // Link back to the SecurityEvent that triggered this ban (if automated)
    sourceEventId: { type: mongoose.Schema.Types.ObjectId, ref: 'SecurityEvent', default: null },

    active:    { type: Boolean, default: true },
}, { timestamps: true });

bannedEntitySchema.index({ type: 1, value: 1 }, { unique: true });
bannedEntitySchema.index({ active: 1, expiresAt: 1 });

module.exports = mongoose.model('BannedEntity', bannedEntitySchema);
