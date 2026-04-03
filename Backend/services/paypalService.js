/**
 * PayPal REST API v2 — Orders integration.
 * Handles both standard checkout and pay-by-email (PayPal.me / personal transfers).
 * Uses PayPal sandbox in development; set PAYPAL_MODE=live for production.
 */

const https = require('https');

const BASE_URL = process.env.PAYPAL_MODE === 'live'
    ? 'https://api-m.paypal.com'
    : 'https://api-m.sandbox.paypal.com';

// ─── Token cache ─────────────────────────────────────────────────────────────
let _accessToken   = null;
let _tokenExpireAt = 0;

async function getAccessToken() {
    if (_accessToken && Date.now() < _tokenExpireAt) return _accessToken;

    const credentials = Buffer.from(
        `${process.env.PAYPAL_CLIENT_ID}:${process.env.PAYPAL_CLIENT_SECRET}`
    ).toString('base64');

    const data = await _request('POST', '/v1/oauth2/token',
        'grant_type=client_credentials',
        { 'Authorization': `Basic ${credentials}`, 'Content-Type': 'application/x-www-form-urlencoded' }
    );

    _accessToken   = data.access_token;
    _tokenExpireAt = Date.now() + (data.expires_in - 60) * 1000;
    return _accessToken;
}

// ─── Create Order (standard checkout) ────────────────────────────────────────
/**
 * @param {string} productId   - e.g. "welcomeofferpack"
 * @param {string} amount      - e.g. "4.99"
 * @param {string} currency    - e.g. "USD"
 * @param {string} description - shown on PayPal checkout
 * @returns {{ orderId, approveUrl }}
 */
async function createOrder(productId, amount, currency, description) {
    const token = await getAccessToken();

    const body = JSON.stringify({
        intent: 'CAPTURE',
        purchase_units: [{
            reference_id: productId,
            description,
            amount: { currency_code: currency, value: amount },
        }],
        application_context: {
            brand_name:          'Big Money Slots',
            landing_page:        'NO_PREFERENCE',
            user_action:         'PAY_NOW',
            return_url:          process.env.PAYPAL_RETURN_URL,
            cancel_url:          process.env.PAYPAL_CANCEL_URL,
        }
    });

    const order = await _request('POST', '/v2/checkout/orders', body, {
        'Authorization':  `Bearer ${token}`,
        'Content-Type':   'application/json',
    });

    const approveUrl = (order.links || []).find(l => l.rel === 'approve')?.href || null;
    return { orderId: order.id, approveUrl };
}

// ─── Capture Order ────────────────────────────────────────────────────────────
/**
 * Call after payer approves on PayPal; captures the payment.
 * @param {string} orderId
 * @returns {object} capture result
 */
async function captureOrder(orderId) {
    const token = await getAccessToken();
    return _request('POST', `/v2/checkout/orders/${orderId}/capture`, '{}', {
        'Authorization': `Bearer ${token}`,
        'Content-Type':  'application/json',
    });
}

// ─── Get Order details ────────────────────────────────────────────────────────
async function getOrder(orderId) {
    const token = await getAccessToken();
    return _request('GET', `/v2/checkout/orders/${orderId}`, null, {
        'Authorization': `Bearer ${token}`,
    });
}

// ─── Pay-by-Email (Payout to player email) ───────────────────────────────────
/**
 * Send coins prize/withdrawal payout to a player's email via PayPal Payouts API.
 * Requires Payouts feature enabled on your PayPal business account.
 * @param {string} recipientEmail
 * @param {string} amount   - USD amount string e.g. "10.00"
 * @param {string} currency
 * @param {string} note     - message to recipient
 */
async function sendPayoutByEmail(recipientEmail, amount, currency, note) {
    const token = await getAccessToken();

    const body = JSON.stringify({
        sender_batch_header: {
            sender_batch_id: `BMS_${Date.now()}`,
            email_subject:   'You have a payout from Big Money Slots!',
            email_message:   note || 'Congratulations on your Big Money Slots winnings!',
        },
        items: [{
            recipient_type: 'EMAIL',
            receiver:        recipientEmail,
            amount:         { value: amount, currency },
            note,
            sender_item_id: `item_${Date.now()}`,
        }]
    });

    return _request('POST', '/v1/payments/payouts', body, {
        'Authorization': `Bearer ${token}`,
        'Content-Type':  'application/json',
    });
}

// ─── Low-level HTTP helper ────────────────────────────────────────────────────
function _request(method, path, body, headers) {
    return new Promise((resolve, reject) => {
        const url      = new URL(BASE_URL + path);
        const bodyData = body ? Buffer.from(body) : null;

        const req = https.request({
            hostname: url.hostname,
            path:     url.pathname + url.search,
            method,
            headers:  {
                ...headers,
                ...(bodyData ? { 'Content-Length': bodyData.length } : {}),
            }
        }, res => {
            let raw = '';
            res.on('data', chunk => { raw += chunk; });
            res.on('end', () => {
                try {
                    const parsed = JSON.parse(raw);
                    if (res.statusCode >= 400)
                        return reject(new Error(`PayPal ${res.statusCode}: ${raw}`));
                    resolve(parsed);
                } catch {
                    reject(new Error(`PayPal non-JSON response: ${raw}`));
                }
            });
        });

        req.on('error', reject);
        if (bodyData) req.write(bodyData);
        req.end();
    });
}

module.exports = { createOrder, captureOrder, getOrder, sendPayoutByEmail };
