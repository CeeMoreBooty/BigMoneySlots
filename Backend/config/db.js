const mongoose = require('mongoose');

const connectDB = async () => {
    try {
        // Railway MongoDB plugin uses MONGO_URL, fallback to MONGO_URI for other environments
        const mongoUri = process.env.MONGO_URL || process.env.MONGO_URI;

        if (!mongoUri) {
            throw new Error('MongoDB connection string not found. Set MONGO_URL or MONGO_URI environment variable.');
        }

        await mongoose.connect(mongoUri);
        console.log('MongoDB connected successfully');
    } catch (err) {
        console.error('MongoDB connection error:', err.message);
        process.exit(1);
    }
};

module.exports = connectDB;
