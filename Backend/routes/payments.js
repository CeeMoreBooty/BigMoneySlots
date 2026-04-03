const express     = require('express');
const auth        = require('../middleware/auth');
const Player      = require('../models/Player');
const Transaction = require('../models/Transaction');
const { verifyGooglePlayPurchase } = require('../services/googlePlayVerifier');
const paypal      = require('../services/paypalService');
const discord     = require('../services/discordAdminWebhook');
const router      = express.Router();

// ── In-app product catalog ────────────────────────────────────────────────────
const PRODUCTS = {
    welcomeofferpack: { coins: 150_000_000_000n, freeSpins: 200, superSpins: 25, ultraSpins: 1, jackpotTickets: 10, mysteryChests: 3, price: '4.99', currency: 'USD' },
    gems_100:  { gems: 100,  price: '0.99',  currency: 'USD' },
    gems_500:  { gems: 500,  price: '4.99',  currency: 'USD' },
    gems_1200: { gems: 1200, price: '9.99',  currency: 'USD' },
    gems_2500: { gems: 2500, price: '19.99', currency: 'USD' },
    gems_6500: { gems: 6500, price: '49.99', currency: 'USD' },
};

// ── Google Play verify + grant ─────────────────────────────────────────────────
/**
 * POST /api/payments/google-play/verify
 * Body: { productId, purchaseToken }
 */
router.post('/google-play/verify', auth, async (req, res) => {
    try {
        const { productId, purchaseToken } = req.body;
        if (!productId || !purchaseToken)
            return res.status(400).json({ error: 'productId and purchaseToken required' });

        const product = PRODUCTS[productId];
        if (!product) return res.status(400).json({ error: 'Unknown product' });

        // Check for duplicate purchase
        const existing = await Transaction.findOne({ purchaseToken });
        if (existing) return res.status(409).json({ error: 'Purchase already processed' });

        // Verify with Google
        const result = await verifyGooglePlayPurchase(
            process.env.GOOGLE_PLAY_PACKAGE_NAME,
            productId,
            purchaseToken
        );
        if (!result.valid) return res.status(400).json({ error: 'Invalid purchase' });

        await grantProduct(req.player, productId, product, purchaseToken, 'google_play');
        res.json({ success: true, productId });
    } catch (err) {
        console.error('[payments/google-play]', err.message);
        res.status(500).json({ error: err.message });
    }
});

// ── PayPal create order ────────────────────────────────────────────────────────
/**
 * POST /api/payments/paypal/create-order
 * Body: { productId }
 */
router.post('/paypal/create-order', auth, async (req, res) => {
    try {
        const { productId } = req.body;
        const product = PRODUCTS[productId];
        if (!product) return res.status(400).json({ error: 'Unknown product' });

        const { orderId, approveUrl } = await paypal.createOrder(
            productId,
            product.price,
            product.currency,
            `Big Money Slots — ${productId}`
        );
        res.json({ orderId, approveUrl });
    } catch (err) {
        console.error('[payments/paypal/create]', err.message);
        res.status(500).json({ error: err.message });
    }
});

// ── PayPal capture order ───────────────────────────────────────────────────────
/**
 * POST /api/payments/paypal/capture
 * Body: { orderId, productId }
 */
router.post('/paypal/capture', auth, async (req, res) => {
    try {
        const { orderId, productId } = req.body;
        if (!orderId || !productId)
            return res.status(400).json({ error: 'orderId and productId required' });

        const product = PRODUCTS[productId];
        if (!product) return res.status(400).json({ error: 'Unknown product' });

        // Prevent duplicate captures
        const existing = await Transaction.findOne({ purchaseToken: orderId });
        if (existing) return res.status(409).json({ error: 'Order already captured' });

        const capture = await paypal.captureOrder(orderId);
        const captureStatus = capture?.status;
        if (captureStatus !== 'COMPLETED')
            return res.status(400).json({ error: `PayPal capture status: ${captureStatus}` });

        await grantProduct(req.player, productId, product, orderId, 'paypal');
        res.json({ success: true, productId });
    } catch (err) {
        console.error('[payments/paypal/capture]', err.message);
        res.status(500).json({ error: err.message });
    }
});

// ── PayPal pay-by-email payout (developer → player withdrawal) ────────────────
/**
 * POST /api/payments/paypal/payout
 * Body: { recipientEmail, amount, note }
 * Requires admin JWT or internal service call — add admin middleware before exposing.
 */
router.post('/paypal/payout', auth, async (req, res) => {
    try {
        const { recipientEmail, amount, note } = req.body;
        if (!recipientEmail || !amount)
            return res.status(400).json({ error: 'recipientEmail and amount required' });

        const result = await paypal.sendPayoutByEmail(recipientEmail, amount, 'USD', note || 'Big Money Slots payout');
        res.json({ success: true, batchId: result?.batch_header?.payout_batch_id });
    } catch (err) {
        console.error('[payments/paypal/payout]', err.message);
        res.status(500).json({ error: err.message });
    }
});

// ── Grant helper ──────────────────────────────────────────────────────────────
async function grantProduct(player, productId, product, token, source) {
    if (product.coins)       player.coins      += Number(product.coins);
    if (product.freeSpins)   player.freeSpins  += product.freeSpins;
    if (product.superSpins)  player.superSpins += product.superSpins;
    if (product.ultraSpins)  player.ultraSpins += product.ultraSpins;
    if (product.gems)        player.gems        = (player.gems || 0) + product.gems;
    if (productId === 'welcomeofferpack') player.welcomeOfferPurchased = true;
    await player.save();

    await Transaction.create({
        playerId:      player._id,
        type:          'iap_purchase',
        amount:        product.coins ? Number(product.coins) : (product.gems || 0),
        currency:      product.gems ? 'gems' : 'coins',
        description:   `IAP: ${productId} via ${source}`,
        productId,
        purchaseToken: token,
        verified:      true,
        balanceAfter:  player.coins,
    });

    // Notify Discord admin channel of the purchase (fire-and-forget)
    discord.notifyPurchase({
        playerId:  String(player._id),
        productId,
        price:     product.price || '?',
        currency:  product.currency || 'USD',
        source,
    }).catch(() => {});
}

// ── Unity IAP verify (parses Unity receipt JSON, extracts purchase token) ─────
/**
 * POST /api/payments/unity-iap/verify
 * Body: { productId, receipt }
 *   receipt — the raw receipt JSON string that Unity IAP provides in
 *             PurchaseEventArgs.purchasedProduct.receipt
 *
 * Unity IAP receipt format for Google Play:
 *   {
 *     "Store": "GooglePlay",
 *     "TransactionID": "GPA.xxxx",
 *     "Payload": "{\"json\":\"{...\\\"purchaseToken\\\":\\\"TOKEN\\\"...}\",\"signature\":\"...\"}"
 *   }
 */
router.post('/unity-iap/verify', auth, async (req, res) => {
    try {
        const { productId, receipt } = req.body;
        if (!productId || !receipt)
            return res.status(400).json({ error: 'productId and receipt required' });

        const product = PRODUCTS[productId];
        if (!product) return res.status(400).json({ error: 'Unknown product' });

        // ── Parse Unity IAP receipt ────────────────────────────────────────────
        let outerReceipt;
        try { outerReceipt = JSON.parse(receipt); } catch (e) {
            return res.status(400).json({ error: 'Invalid receipt JSON' });
        }

        // Editor / dev builds send a "fake" receipt — allow in non-production
        if (outerReceipt.Store === 'fake' || outerReceipt.Store === 'Editor') {
            if (process.env.NODE_ENV === 'production')
                return res.status(400).json({ error: 'Fake receipt rejected in production' });

            await grantProduct(req.player, productId, product, 'editor_sim_' + Date.now(), 'editor');
            return res.json({ success: true, productId, note: 'editor_simulation' });
        }

        // Extract the purchase token from the nested Google Play JSON
        const payload = outerReceipt.Payload;
        if (!payload) return res.status(400).json({ error: 'Missing Payload in receipt' });

        let payloadObj;
        try { payloadObj = JSON.parse(payload); } catch (e) {
            return res.status(400).json({ error: 'Invalid Payload JSON' });
        }

        const payloadJson = payloadObj.json || payloadObj;
        let gpData;
        if (typeof payloadJson === 'string') {
            try { gpData = JSON.parse(payloadJson); } catch (e) {
                return res.status(400).json({ error: 'Invalid Google Play JSON in Payload' });
            }
        } else {
            gpData = payloadJson;
        }

        const purchaseToken = typeof gpData.purchaseToken === 'string'
            ? gpData.purchaseToken.replace(/[^A-Za-z0-9_\-.:]/g, '').substring(0, 512)
            : null;
        if (!purchaseToken)
            return res.status(400).json({ error: 'purchaseToken not found in receipt' });

        // ── Duplicate check ────────────────────────────────────────────────────
        const existing = await Transaction.findOne({ purchaseToken });
        if (existing) return res.status(409).json({ error: 'Purchase already processed' });

        // ── Verify with Google Play ────────────────────────────────────────────
        const result = await verifyGooglePlayPurchase(
            process.env.GOOGLE_PLAY_PACKAGE_NAME,
            productId,
            purchaseToken
        );
        if (!result.valid)
            return res.status(400).json({ error: 'Google Play verification failed' });

        await grantProduct(req.player, productId, product, purchaseToken, 'google_play');
        res.json({ success: true, productId });
    } catch (err) {
        console.error('[payments/unity-iap]', err.message);
        res.status(500).json({ error: err.message });
    }
});

module.exports = router;
