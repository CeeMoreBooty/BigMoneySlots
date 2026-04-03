const mongoose = require('mongoose');

/**
 * SecurityEvent — one record per detected threat or suspicious action.
 * Severity levels:
 *   low    → logged only (informational)
 *   medium → logged + counted toward auto-ban threshold
 *   high   → logged + immediate auto-ban triggered
 *   critical → logged + banned + admin alert emitted via socket.io
 */
const securityEventSchema = new mongoose.Schema({
    // Who / where
    ip:          { type: String, default: '' },
    playerId:    { type: mongoose.Schema.Types.ObjectId, ref: 'Player', default: null },
    deviceId:    { type: String, default: '' },
    macAddress:  { type: String, default: '' },   // client-reported device MAC (x-device-mac header)

    // What happened
    eventType: {
        type: String,
        required: true,
        enum: [
            'brute_force_auth',       // repeated failed logins
            'rate_limit_exceeded',    // hit Express rate limiter
            'banned_ip_attempt',      // banned IP tried to connect
            'banned_player_attempt',  // banned player tried to use API
            'coin_manipulation',      // client sent impossible coin delta
            'spin_result_tampering',  // client-side spin outcome altered
            'jwt_reuse_multi_ip',     // same JWT token used from 2+ different IPs
            'invalid_jwt',            // malformed / expired / forged token
            'injection_attempt',      // SQL/NoSQL/script injection in body
            'bot_fingerprint',        // known bot/scraper user-agent detected
            'abnormal_request_rate',  // >N identical endpoint hits in short window
            'payload_too_large',      // request body suspiciously oversized
            'forbidden_endpoint',     // attempt to access admin route without privilege
            'account_takeover',       // password reset / link for different device unexpectedly
            'duplicate_purchase',     // same purchaseToken submitted twice (replay attack)
        ]
    },

    severity:   { type: String, enum: ['low', 'medium', 'high', 'critical'], default: 'medium' },
    resolved:   { type: Boolean, default: false },
    autoBanned: { type: Boolean, default: false },

    // Evidence / context
    path:       { type: String, default: '' },
    method:     { type: String, default: '' },
    userAgent:  { type: String, default: '' },
    evidence:   { type: mongoose.Schema.Types.Mixed, default: {} },  // free-form JSON context
    note:       { type: String, default: '' },
}, { timestamps: true });

securityEventSchema.index({ ip:        1, createdAt: -1 });
securityEventSchema.index({ playerId:  1, createdAt: -1 });
securityEventSchema.index({ macAddress: 1, createdAt: -1 });
securityEventSchema.index({ eventType: 1, createdAt: -1 });
securityEventSchema.index({ severity:  1, resolved: 1 });
securityEventSchema.index({ autoBanned: 1 });

module.exports = mongoose.model('SecurityEvent', securityEventSchema);
