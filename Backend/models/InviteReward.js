const mongoose = require('mongoose');

/**
 * InviteReward — one document per player (their invite code + redemption log).
 *
 * Gem rewards:
 *   Inviter (code owner) : INVITER_GEM_REWARD gems per unique new player who redeems
 *   Redeemer (new player): REDEEMER_GEM_REWARD gems one-time for entering any valid code
 *
 * Rules enforced at route level:
 *   - A player cannot redeem their own code
 *   - A player can only redeem one invite code in total (first-time only)
 *   - The same redeemer can only appear once in any given invite's redemptions list
 */

const redemptionSchema = new mongoose.Schema({
    redeemerId: { type: mongoose.Schema.Types.ObjectId, ref: 'Player', required: true },
    redeemedAt: { type: Date, default: Date.now },
}, { _id: false });

const inviteRewardSchema = new mongoose.Schema({
    inviterId:  { type: mongoose.Schema.Types.ObjectId, ref: 'Player', required: true, unique: true },
    inviteCode: { type: String, required: true, unique: true, uppercase: true, trim: true },
    redemptions: { type: [redemptionSchema], default: [] },
}, { timestamps: true });

inviteRewardSchema.index({ inviteCode: 1 });
inviteRewardSchema.index({ 'redemptions.redeemerId': 1 });

module.exports = mongoose.model('InviteReward', inviteRewardSchema);
