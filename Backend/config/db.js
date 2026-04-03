const mongoose = require('mongoose');

/**
 * Connects to MongoDB using the URI from the environment.
 *
 * Supported env vars (checked in order):
 *   MONGO_URI  — standard / local / Atlas
 *   MONGO_URL  — Railway-injected variable name
 *
 * Connection options:
 *   maxPoolSize          — up to 10 concurrent connections
 *   serverSelectionTimeoutMS — give up trying to find a server after 10 s
 *   socketTimeoutMS      — close idle sockets after 45 s
 *   heartbeatFrequencyMS — check replica-set / sharded-cluster health every 10 s
 */
const connectDB = async () => {
    const uri = process.env.MONGO_URI || process.env.MONGO_URL;
    if (!uri) {
        console.error('[DB] MONGO_URI / MONGO_URL env var is not set.');
        process.exit(1);
    }

    try {
        await mongoose.connect(uri, {
            maxPoolSize:               10,
            serverSelectionTimeoutMS:  10_000,
            socketTimeoutMS:           45_000,
            heartbeatFrequencyMS:      10_000,
        });
        console.log('[DB] MongoDB connected:', mongoose.connection.host);
    } catch (err) {
        console.error('[DB] Initial connection failed:', err.message);
        process.exit(1);
    }
};

// ── Connection lifecycle events ────────────────────────────────────────────────
mongoose.connection.on('disconnected', () =>
    console.warn('[DB] MongoDB disconnected — will retry automatically.'));

mongoose.connection.on('reconnected', () =>
    console.log('[DB] MongoDB reconnected.'));

mongoose.connection.on('error', err =>
    console.error('[DB] MongoDB error:', err.message));

module.exports = connectDB;
