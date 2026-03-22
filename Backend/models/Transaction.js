const mongoose = require('mongoose');

const transactionSchema = new mongoose.Schema({
    playerId:    { type: mongoose.Schema.Types.ObjectId, ref: 'Player', required: true },
    type:        {
        type: String,
        enum: ['iap_purchase', 'spin_win', 'spin_bet', 'reward_hourly', 'reward_daily',
               'new_player_pack', 'tournament_prize', 'admin_grant'],
        required: true
    },
    amount:      { type: Number, required: true }, // positive = credit, negative = debit
    currency:    { type: String, enum: ['coins', 'gems', 'freeSpins', 'superSpins', 'ultraSpins'], default: 'coins' },
    description: { type: String, default: '' },
    // IAP-specific
    productId:       { type: String, default: null },
    purchaseToken:   { type: String, default: null },
    verified:        { type: Boolean, default: false },
    // Reference to related entity
    referenceId:     { type: String, default: null }, // tournamentId, spinId, etc.
    balanceAfter:    { type: Number, required: true },
}, { timestamps: true });

transactionSchema.index({ playerId: 1, createdAt: -1 });
transactionSchema.index({ purchaseToken: 1 }, { sparse: true, unique: true });

module.exports = mongoose.model('Transaction', transactionSchema);
