const mongoose = require('mongoose');

const connectDB = async () => {
    // Railway sets MONGO_URL; fall back to MONGODB_URI or a local default
    const uri = process.env.MONGO_URL
             || process.env.MONGODB_URI
             || 'mongodb://localhost:27017/bigmoneyslots';

    try {
        await mongoose.connect(uri);
        console.log('MongoDB connected:', mongoose.connection.host);
    } catch (err) {
        console.error('MongoDB connection error:', err.message);
        process.exit(1);
    }
};

module.exports = connectDB;
