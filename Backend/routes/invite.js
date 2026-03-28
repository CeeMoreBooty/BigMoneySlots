/**
 * Invite Reward Routes — /api/invite/*
 *
 * Players earn gems by sharing their personal invite code with friends.
 * When a new player redeems the code, both parties receive a gem bonus.
 *
 * Gem rewards (configurable via constants below):
 *   INVITER_GEM_REWARD  : 100 gems to the code owner per successful redemption
 *   REDEEMER_GEM_REWARD : 50  gems to the new player using the code (one-time)
 *
 * Rules:
 *   - A player cannot redeem their own code
 *   - A player may only redeem one invite code ever (lifetime, first-time bonus)
 *   - The same redeemer cannot appear twice under the same invite
 */

const express      = require('express');
const crypto       = require('crypto');
const auth         = require('../middleware/auth');
const InviteReward = require('../models/InviteReward');
const Player       = require('../models/Player');
const Transaction  = require('../models/Transaction');
const router       = express.Router();

const INVITER_GEM_REWARD  = 100;   // gems awarded to the code owner
const REDEEMER_GEM_REWARD = 50;    // gems awarded to the player using the code

// ── Helpers ───────────────────────────────────────────────────────────────────

/** Generate a unique 8-char alphanumeric invite code prefixed with BMS. */
function generateCode() {
    return 'BMS' + crypto.randomBytes(4).toString('hex').toUpperCase();
}

// ── GET /api/invite/code ──────────────────────────────────────────────────────
/**
 * Returns the player's personal invite code, creating one if none exists yet.
 * Response: { code, totalInvites, gemsEarned, gemRewardPerInvite, gemRewardForNew }
 */
router.get('/code', auth, async (req, res) => {
    try {
        let invite = await InviteReward.findOne({ inviterId: req.player._id });

        if (!invite) {
            // Guard against duplicate-code collision on concurrent first requests
            let code;
            let attempts = 0;
            do {
                code = generateCode();
                attempts++;
            } while (attempts < 10 && await InviteReward.exists({ inviteCode: code }));

            invite = await InviteReward.create({
                inviterId:  req.player._id,
                inviteCode: code,
            });
        }

        res.json({
            code:               invite.inviteCode,
            totalInvites:       invite.redemptions.length,
            gemsEarned:         invite.redemptions.length * INVITER_GEM_REWARD,
            gemRewardPerInvite: INVITER_GEM_REWARD,
            gemRewardForNew:    REDEEMER_GEM_REWARD,
        });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

// ── POST /api/invite/redeem ───────────────────────────────────────────────────
/**
 * Redeem another player's invite code.
 * Body: { code }
 * Response: { success, gemsAwarded, newGemBalance }
 */
router.post('/redeem', auth, async (req, res) => {
    try {
        const { code } = req.body;
        if (!code || typeof code !== 'string')
            return res.status(400).json({ error: 'code required' });

        const normalised = code.trim().toUpperCase();
        const invite = await InviteReward.findOne({ inviteCode: normalised });
        if (!invite)
            return res.status(404).json({ error: 'Invalid invite code' });

        // Cannot use own code
        if (invite.inviterId.equals(req.player._id))
            return res.status(400).json({ error: 'You cannot redeem your own invite code' });

        // Already redeemed any code (lifetime one-time bonus)
        const alreadyRedeemed = await InviteReward.exists({
            'redemptions.redeemerId': req.player._id,
        });
        if (alreadyRedeemed)
            return res.status(409).json({ error: 'You have already redeemed an invite code' });

        // ── Grant gems to the new player (redeemer) ───────────────────────────
        req.player.gems = (req.player.gems || 0) + REDEEMER_GEM_REWARD;
        await req.player.save();

        await Transaction.create({
            playerId:    req.player._id,
            type:        'invite_reward',
            amount:      REDEEMER_GEM_REWARD,
            currency:    'gems',
            description: `Invite code redeemed: ${normalised}`,
            balanceAfter: req.player.coins,
        });

        // ── Grant gems to the inviter ─────────────────────────────────────────
        const inviter = await Player.findById(invite.inviterId);
        if (inviter) {
            inviter.gems = (inviter.gems || 0) + INVITER_GEM_REWARD;
            await inviter.save();

            await Transaction.create({
                playerId:    inviter._id,
                type:        'invite_reward',
                amount:      INVITER_GEM_REWARD,
                currency:    'gems',
                description: `Friend invited: ${req.player.displayName}`,
                balanceAfter: inviter.coins,
            });

            // Real-time notification to inviter via socket.io
            const io = req.app.get('io');
            if (io) {
                io.to(`player:${inviter._id}`).emit('invite_redeemed', {
                    redeemerName: req.player.displayName,
                    gemsEarned:   INVITER_GEM_REWARD,
                });
            }
        }

        // ── Record redemption ─────────────────────────────────────────────────
        invite.redemptions.push({ redeemerId: req.player._id });
        await invite.save();

        res.json({
            success:       true,
            gemsAwarded:   REDEEMER_GEM_REWARD,
            newGemBalance: req.player.gems,
        });
    } catch (err) {
        console.error('[invite/redeem]', err.message);
        res.status(500).json({ error: err.message });
    }
});

// ── GET /api/invite/stats ─────────────────────────────────────────────────────
/**
 * Returns invite statistics for the current player.
 * Response: { totalInvites, gemsEarned, gemRewardPerInvite, gemRewardForNew }
 */
router.get('/stats', auth, async (req, res) => {
    try {
        const invite = await InviteReward.findOne({ inviterId: req.player._id });
        res.json({
            totalInvites:       invite ? invite.redemptions.length : 0,
            gemsEarned:         invite ? invite.redemptions.length * INVITER_GEM_REWARD : 0,
            gemRewardPerInvite: INVITER_GEM_REWARD,
            gemRewardForNew:    REDEEMER_GEM_REWARD,
        });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
