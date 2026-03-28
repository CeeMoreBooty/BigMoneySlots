const express       = require('express');
const auth          = require('../middleware/auth');
const LinkedAccount = require('../models/LinkedAccount');
const Player        = require('../models/Player');
const Transaction   = require('../models/Transaction');
const router        = express.Router();

// Bonus amounts per provider (in coins)
const BONUSES = {
    facebook: 2_500_000_000_000,
    google:   2_500_000_000_000,
    phone:    2_500_000_000_000,
    discord:  5_000_000_000_000,
};

const VALID_PROVIDERS = Object.keys(BONUSES);

/**
 * GET /api/account/links  — get all linked accounts for current player
 */
router.get('/links', auth, async (req, res) => {
    try {
        const links = await LinkedAccount.find({ playerId: req.player._id })
            .select('provider displayName bonusGranted linkedAt');
        res.json({ links });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

/**
 * POST /api/account/link
 * Body: { provider, token }
 *
 * In production:
 *   - Facebook: validate token via https://graph.facebook.com/me?access_token=TOKEN
 *   - Google:   validate via Google OAuth2 tokeninfo endpoint
 *   - Discord:  validate via https://discord.com/api/users/@me with Bearer token
 *   - Phone:    token = verified OTP session ID from your SMS provider (Twilio etc.)
 *
 * For this scaffold the token is accepted as-is with a TODO marker for each provider.
 */
router.post('/link', auth, async (req, res) => {
    try {
        const { provider, token } = req.body;
        if (!provider || !token)
            return res.status(400).json({ error: 'provider and token required' });

        const providerKey = provider.toLowerCase();
        if (!VALID_PROVIDERS.includes(providerKey))
            return res.status(400).json({ error: `Invalid provider. Must be one of: ${VALID_PROVIDERS.join(', ')}` });

        // TODO: validate OAuth token with the provider's API
        // const providerUid = await validateProviderToken(providerKey, token);
        const providerUid   = token;    // STUB — replace with real provider validation
        const displayName   = token;    // STUB — provider returns real display name

        // Prevent the same provider account being linked to two players
        const conflict = await LinkedAccount.findOne({ provider: providerKey, providerUid });
        if (conflict && !conflict.playerId.equals(req.player._id))
            return res.status(409).json({ error: 'This account is already linked to another player' });

        // Upsert the link
        let link = await LinkedAccount.findOne({ playerId: req.player._id, provider: providerKey });
        let bonusGranted = false;

        if (!link) {
            link = new LinkedAccount({ playerId: req.player._id, provider: providerKey, providerUid, displayName });
        } else {
            link.providerUid = providerUid;
            link.displayName = displayName;
        }

        // Grant bonus once only
        if (!link.bonusGranted) {
            const bonus = BONUSES[providerKey];
            req.player.coins += bonus;
            await req.player.save();

            await Transaction.create({
                playerId:     req.player._id,
                type:         'admin_grant',
                amount:       bonus,
                currency:     'coins',
                description:  `Account link bonus: ${providerKey}`,
                balanceAfter: req.player.coins,
            });

            link.bonusGranted = true;
            bonusGranted = true;
        }

        await link.save();

        res.json({ success: true, provider: providerKey, displayName, bonusGranted, newBalance: req.player.coins });
    } catch (err) {
        console.error('[accountLinking]', err.message);
        res.status(500).json({ error: err.message });
    }
});

/**
 * DELETE /api/account/link/:provider  — unlink a provider (no bonus reversal)
 */
router.delete('/link/:provider', auth, async (req, res) => {
    try {
        const providerKey = req.params.provider.toLowerCase();
        await LinkedAccount.findOneAndDelete({ playerId: req.player._id, provider: providerKey });
        res.json({ ok: true });
    } catch (err) {
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
