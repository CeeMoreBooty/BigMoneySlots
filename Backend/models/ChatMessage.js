const mongoose = require('mongoose');

const chatMessageSchema = new mongoose.Schema({
    senderId:   { type: mongoose.Schema.Types.ObjectId, ref: 'Player', required: true },
    senderName: { type: String, required: true },
    text:       { type: String, required: true, maxlength: 200 },
    channel:    { type: String, enum: ['global', 'room', 'friends'], required: true },
    roomId:     { type: String, default: null },    // for room channel
    targetId:   { type: mongoose.Schema.Types.ObjectId, ref: 'Player', default: null }, // for DM
    deleted:    { type: Boolean, default: false },  // soft delete for moderation
}, { timestamps: true });

chatMessageSchema.index({ channel: 1, createdAt: -1 });
chatMessageSchema.index({ roomId:   1, createdAt: -1 });
chatMessageSchema.index({ senderId: 1, targetId:  1 });

module.exports = mongoose.model('ChatMessage', chatMessageSchema);
