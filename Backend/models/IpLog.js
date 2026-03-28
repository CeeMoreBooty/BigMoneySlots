const mongoose = require('mongoose');

/**
 * Stores one record per request for security, fraud detection, and geo-compliance.
 * NOTE: Logging IP addresses requires disclosure in your Privacy Policy (GDPR / CCPA).
 */
const ipLogSchema = new mongoose.Schema({
    ip:         { type: String, required: true },
    playerId:   { type: mongoose.Schema.Types.ObjectId, ref: 'Player', default: null },
    macAddress: { type: String, default: '' },   // client-reported device MAC (x-device-mac header)
    path:       { type: String, default: '' },
    method:     { type: String, default: '' },
    userAgent:  { type: String, default: '' },
    country:    { type: String, default: '' },   // populated by geo lookup if added later
    flagged:    { type: Boolean, default: false }, // set true if suspicious activity detected
}, { timestamps: true });

ipLogSchema.index({ ip: 1, createdAt: -1 });
ipLogSchema.index({ playerId: 1, createdAt: -1 });
ipLogSchema.index({ macAddress: 1, createdAt: -1 });
ipLogSchema.index({ flagged: 1 });

module.exports = mongoose.model('IpLog', ipLogSchema);
