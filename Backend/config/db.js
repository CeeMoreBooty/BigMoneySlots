const mongoose = require('mongoose');

const connectDB = async () => {
    // Railway sets MONGO_URL; fall back to MONGO_URI or MONGODB_URI
    const uri = process.env.MONGO_URL
             || process.env.MONGO_URI
             || process.env.MONGODB_URI;

    if (!uri) {
        console.error('No MongoDB connection URI set. Define MONGO_URL, MONGO_URI, or MONGODB_URI.');
        process.exit(1);
    }

    try {
        await mongoose.connect(uri);
        console.log('MongoDB connected:', mongoose.connection.host);
    } catch (err) {
        console.error('MongoDB connection error:', err.message);
        process.exit(1);
    }
};

module.exports = connectDB;
