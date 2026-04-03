const mongoose = require('mongoose');

const spinSchema = new mongoose.Schema({
    user:      { type: mongoose.Schema.Types.ObjectId, ref: 'User', required: true },
    coinsWon:  { type: String, default: '0' },
    winLevel:  { type: Number, default: 0 },   // 0=None,1=Normal,2=Big,3=Mega,4=Epic
    createdAt: { type: Date, default: Date.now }
}, { versionKey: false });

module.exports = mongoose.model('Spin', spinSchema);
