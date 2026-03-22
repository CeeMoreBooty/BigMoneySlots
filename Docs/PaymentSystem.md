# Payment System — Company Documentation

## Supported Payment Methods

| Method              | Direction      | Product Types               | File                            |
|---------------------|---------------|-----------------------------|---------------------------------|
| Google Play Billing | Player → Us   | IAP products (coins, gems)  | `routes/payments.js`            |
| PayPal Checkout     | Player → Us   | IAP products (coins, gems)  | `routes/payments.js`            |
| PayPal Payouts      | Us → Player   | Withdrawals / prizes        | `services/paypalService.js`     |
| Pay by Email        | Us → Player   | Tournament prizes, support  | `services/paypalService.js`     |

## IAP Product Catalog

| Product ID         | Contents                                                     | Price  |
|--------------------|--------------------------------------------------------------|--------|
| `welcomeofferpack` | 150B coins, 200 free spins, 25 super spins, 1 ultra spin, 10 jackpot tickets, 3 mystery chests, 24h 2× boost, VIP badge, golden reel skin | $4.99 |
| `gems_100`         | 100 gems                                                     | $0.99  |
| `gems_500`         | 500 gems                                                     | $4.99  |
| `gems_1200`        | 1,200 gems                                                   | $9.99  |
| `gems_2500`        | 2,500 gems                                                   | $19.99 |
| `gems_6500`        | 6,500 gems                                                   | $49.99 |

## Google Play Purchase Flow

```
Unity IAPHandler.BuyWelcomeOffer()
  → Google Play billing dialog
  → OnPurchaseCompleted(productId, purchaseToken)
  → POST /api/payments/google-play/verify { productId, purchaseToken }
  → Backend verifies with Android Publisher API
  → Coins / gems credited to Player
  → Transaction record written
```

## PayPal Purchase Flow

```
Unity requests POST /api/payments/paypal/create-order { productId }
  → Backend creates PayPal Order via REST API
  → Returns { orderId, approveUrl }
  → Unity opens approveUrl via Application.OpenURL() (deep-link return)
  → Player approves on PayPal
  → Unity deep-link callback triggers POST /api/payments/paypal/capture { orderId, productId }
  → Backend captures payment
  → Coins / gems credited to Player
  → Transaction record written
```

## Payout / Pay-by-Email Flow

```
Admin / internal service calls:
  POST /api/payments/paypal/payout { recipientEmail, amount, note }
  → Backend calls PayPal Payouts API
  → Player receives email from PayPal with funds
```

## Transaction Ledger
All credits and debits are stored in the `transactions` collection.

| Field         | Description                                      |
|---------------|--------------------------------------------------|
| playerId      | Player receiving / spending                      |
| type          | iap_purchase / spin_win / tournament_prize / etc.|
| amount        | Positive = credit, Negative = debit              |
| currency      | coins / gems / freeSpins / superSpins            |
| purchaseToken | Google Play or PayPal order ID (unique index)    |
| verified      | true when backend-verified                       |
| balanceAfter  | Coin balance after transaction                   |

## Security
- Purchase tokens are deduplicated at database level (unique index) — no double-grants
- Server is authoritative on all balances — client cannot self-issue coins
- All payment endpoints require valid JWT (auth middleware)
- IP logging on all API calls
- Rate limiting: 120 requests / 15 min per IP

## Environment Variables Required
```
PAYPAL_CLIENT_ID=
PAYPAL_CLIENT_SECRET=
PAYPAL_MODE=sandbox            # change to "live" for production
PAYPAL_RETURN_URL=bigmoneyslots://paypal/success
PAYPAL_CANCEL_URL=bigmoneyslots://paypal/cancel
GOOGLE_APPLICATION_CREDENTIALS=./config/google-service-account.json
GOOGLE_PLAY_PACKAGE_NAME=com.yourstudio.bigmoneyslots
```

## Files
- `Backend/routes/payments.js`           — All payment endpoints
- `Backend/services/paypalService.js`    — PayPal REST API wrapper
- `Backend/services/googlePlayVerifier.js` — Google Play receipt verification
- `Backend/models/Transaction.js`        — Transaction model
- `Assets/Scripts/WelcomeOffer/IAPHandler.cs` — Unity IAP client
