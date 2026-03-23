/**
 * Shared test setup:
 *   - Starts an in-memory MongoDB instance before all tests
 *   - Connects Mongoose to it
 *   - Builds the Express app (with rate-limiting disabled)
 *   - Tears everything down after all tests
 */
const mongoose          = require('mongoose');
const { MongoMemoryServer } = require('mongodb-memory-server');
const jwt               = require('jsonwebtoken');
const createApp         = require('../../app');

process.env.JWT_SECRET    = 'test-secret-key';
process.env.JWT_EXPIRES_IN = '1h';
process.env.ADMIN_SECRET  = 'test-admin-secret';

let mongod;

async function connect() {
    mongod = await MongoMemoryServer.create();
    await mongoose.connect(mongod.getUri());
}

async function disconnect() {
    await mongoose.connection.dropDatabase();
    await mongoose.disconnect();
    await mongod.stop();
}

async function clearCollections() {
    const collections = mongoose.connection.collections;
    for (const key in collections) {
        await collections[key].deleteMany({});
    }
}

function buildApp() {
    return createApp({ disableRateLimit: true });
}

function makeToken(playerId) {
    return jwt.sign({ id: playerId }, process.env.JWT_SECRET, { expiresIn: '1h' });
}

module.exports = { connect, disconnect, clearCollections, buildApp, makeToken };
