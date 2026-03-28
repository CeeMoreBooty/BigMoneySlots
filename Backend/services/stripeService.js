/**
 * Stripe Payments Service
 * Uses the Stripe Node SDK to create PaymentIntents and verify webhook events.
 *
 * The SECRET key (sk_test_...) is what lets the server receive money.
 * The PUBLISHABLE key (pk_test_...) is sent to the checkout page so Stripe.js
 * can safely tokenize card details without them touching our server.
 *
 * Environment variables required (in .env — never in source code):
 *   STRIPE_SECRET_KEY      — sk_test_... or sk_live_...
 *   STRIPE_PUBLISHABLE_KEY — pk_test_... or pk_live_...
 *   STRIPE_WEBHOOK_SECRET  — whsec_... (from Stripe Dashboard → Webhooks)
 */

const Stripe = require('stripe');

const stripe = Stripe(process.env.STRIPE_SECRET_KEY);

// ── Create a PaymentIntent ────────────────────────────────────────────────────
/**
 * Creates a PaymentIntent for the given product.
 * Returns the clientSecret used by the checkout page to confirm payment.
 *
 * @param {string} productId   — e.g. "gems_500"
 * @param {string} amountStr   — e.g. "4.99"
 * @param {string} currency    — e.g. "USD"
 * @param {string} description — shown on Stripe receipt
 * @param {string} playerId    — stored in metadata for webhook reconciliation
 * @returns {{ clientSecret: string, paymentIntentId: string }}
 */
async function createPaymentIntent(productId, amountStr, currency, description, playerId) {
    // Stripe amounts are in the smallest currency unit (cents for USD)
    const amount = Math.round(parseFloat(amountStr) * 100);

    const intent = await stripe.paymentIntents.create({
        amount,
        currency:    currency.toLowerCase(),
        description,
        metadata: {
            productId,
            playerId: String(playerId),
            game:     'BigMoneySlots',
        },
        // Automatically send a receipt email via Stripe (optional — remove if not wanted)
        // receipt_email: playerEmail,
    });

    return {
        clientSecret:    intent.client_secret,
        paymentIntentId: intent.id,
    };
}

// ── Retrieve a PaymentIntent ──────────────────────────────────────────────────
async function getPaymentIntent(paymentIntentId) {
    return stripe.paymentIntents.retrieve(paymentIntentId);
}

// ── Construct and verify a webhook event ─────────────────────────────────────
/**
 * Verifies the Stripe-Signature header so we know the event is genuinely
 * from Stripe and not a spoofed POST from anyone else.
 *
 * IMPORTANT: The route that calls this must use express.raw() middleware,
 * NOT express.json(), because Stripe signs the raw bytes.
 *
 * @param {Buffer} rawBody        — req.body when using express.raw()
 * @param {string} signatureHeader — req.headers['stripe-signature']
 * @returns {object} Stripe event object
 */
function constructWebhookEvent(rawBody, signatureHeader) {
    return stripe.webhooks.constructEvent(
        rawBody,
        signatureHeader,
        process.env.STRIPE_WEBHOOK_SECRET
    );
}

// ── Expose publishable key to checkout page ───────────────────────────────────
function getPublishableKey() {
    return process.env.STRIPE_PUBLISHABLE_KEY;
}

module.exports = {
    createPaymentIntent,
    getPaymentIntent,
    constructWebhookEvent,
    getPublishableKey,
};
