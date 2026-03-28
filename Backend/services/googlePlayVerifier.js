const { google } = require('googleapis');

/**
 * Verifies a Google Play in-app purchase token with the Android Publisher API.
 * Requires a Google service account JSON key set in GOOGLE_APPLICATION_CREDENTIALS.
 */
async function verifyGooglePlayPurchase(packageName, productId, purchaseToken) {
    const auth = new google.auth.GoogleAuth({
        keyFile: process.env.GOOGLE_APPLICATION_CREDENTIALS,
        scopes:  ['https://www.googleapis.com/auth/androidpublisher'],
    });

    const androidPublisher = google.androidpublisher({ version: 'v3', auth });

    const response = await androidPublisher.purchases.products.get({
        packageName,
        productId,
        token: purchaseToken,
    });

    const purchase = response.data;

    // purchaseState: 0 = purchased, 1 = cancelled, 2 = pending
    if (purchase.purchaseState !== 0) {
        throw new Error(`Purchase not in completed state: ${purchase.purchaseState}`);
    }

    // consumptionState: 0 = not consumed, 1 = consumed
    // acknowledgementState: 0 = not acked, 1 = acked
    return {
        valid:            true,
        orderId:          purchase.orderId,
        purchaseTimeMs:   purchase.purchaseTimeMillis,
        acknowledged:     purchase.acknowledgementState === 1,
        consumed:         purchase.consumptionState === 1,
    };
}

module.exports = { verifyGooglePlayPurchase };
