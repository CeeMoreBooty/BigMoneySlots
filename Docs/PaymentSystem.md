# Payment System — Company Documentation

## Supported Payment Methods

| Method              | Direction      | Product Types               | File                            |
|---------------------|---------------|-----------------------------|---------------------------------|
| Google Play Billing | Player → Us   | IAP products (coins, gems)  | `routes/payments.js`            |
| PayPal Checkout     | Player → Us   | IAP products (coins, gems)  | `routes/payments.js`            |
| Stripe Checkout     | Player → Us   | IAP products (coins, gems)  | `routes/payments.js` + `public/stripe-checkout.html` |
| PayPal Payouts      | Us → Player   | Withdrawals / prizes        | `services/paypalService.js`     |
| Pay by Email        | Us → Player   | Tournament prizes, support  | `services/paypalService.js`     |
| E-Transfer          | Player → Us   | Manual top-up (CA only)     | Send to: tech.crew151@gmail.com |

## Owner / Business Email

**tech.crew151@gmail.com** — receives:
- PayPal payments and PayPal.me transfers
- E-Transfer payments (Canadian players)
- Stripe receipt notifications (optional — configure in Stripe Dashboard)

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
  → Player approves on PayPal (funds go to tech.crew151@gmail.com)
  → Unity deep-link callback triggers POST /api/payments/paypal/capture { orderId, productId }
  → Backend captures payment
  → Coins / gems credited to Player
  → Transaction record written
```

## Stripe Purchase Flow (PGCN Checkout Page)

```
Unity opens /stripe-checkout.html?productId=gems_500&token=<JWT>
  → Page fetches GET /api/payments/stripe/config → publishable key
  → Page fetches POST /api/payments/stripe/create-payment-intent { productId }
  → Backend creates PaymentIntent (funds route to your Stripe account)
  → Returns { clientSecret }
  → Stripe.js card form mounts; player enters card details
  → confirmCardPayment() — card is charged, money sent to your Stripe account
  → Stripe calls POST /api/payments/stripe/webhook (payment_intent.succeeded)
  → Backend verifies Stripe signature, grants product to player
  → Coins / gems credited to Player
  → Transaction record written
  → Page deep-links back: bigmoneyslots://stripe/success?productId=...
```

### Stripe Keys Explained

| Key | Where it goes | What it does |
|-----|--------------|--------------|
| `sk_test_...` (Secret key) | `.env` on server ONLY | Authorises the server to create charges and receive money. **Never expose this.** |
| `pk_test_...` (Publishable key) | Checkout page / client | Lets Stripe.js tokenize card details safely. Safe to expose. |
| `whsec_...` (Webhook secret) | `.env` on server ONLY | Verifies webhook events are genuinely from Stripe. Get from Stripe Dashboard → Webhooks. |

> **Note:** Keys beginning with `sk_test_` / `pk_test_` are test-mode keys. Switch to `sk_live_` / `pk_live_` for real charges.

### Setting Up the Stripe Webhook
1. Go to **Stripe Dashboard → Developers → Webhooks → Add endpoint**
2. URL: `https://your-server.com/api/payments/stripe/webhook`
3. Events to listen for: `payment_intent.succeeded`
4. Copy the **Signing secret** (`whsec_...`) into your `.env` as `STRIPE_WEBHOOK_SECRET`

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
| purchaseToken | Google Play / PayPal order ID / Stripe PaymentIntent ID (unique index) |
| verified      | true when backend-verified                       |
| balanceAfter  | Coin balance after transaction                   |

## Security
- Purchase tokens are deduplicated at database level (unique index) — no double-grants
- Server is authoritative on all balances — client cannot self-issue coins
- All payment endpoints require valid JWT (auth middleware)
- Stripe webhook requests are signature-verified — spoofed POSTs are rejected
- IP logging on all API calls
- Rate limiting: 120 requests / 15 min per IP
- Full hacking/abuse tracker — see `Docs/SecuritySystem.md`

## Environment Variables Required
```
PAYPAL_CLIENT_ID=
PAYPAL_CLIENT_SECRET=
PAYPAL_MODE=sandbox            # change to "live" for production
PAYPAL_RETURN_URL=bigmoneyslots://paypal/success
PAYPAL_CANCEL_URL=bigmoneyslots://paypal/cancel
GOOGLE_APPLICATION_CREDENTIALS=./config/google-service-account.json
GOOGLE_PLAY_PACKAGE_NAME=com.yourstudio.bigmoneyslots
STRIPE_SECRET_KEY=sk_test_...
STRIPE_PUBLISHABLE_KEY=pk_test_...
STRIPE_WEBHOOK_SECRET=whsec_...
OWNER_EMAIL=tech.crew151@gmail.com
```

## Files
- `Backend/routes/payments.js`               — All payment endpoints (Google Play, PayPal, Stripe)
- `Backend/services/paypalService.js`        — PayPal REST API wrapper
- `Backend/services/stripeService.js`        — Stripe SDK wrapper
- `Backend/services/googlePlayVerifier.js`   — Google Play receipt verification
- `Backend/models/Transaction.js`            — Transaction model
- `Backend/public/stripe-checkout.html`      — PGCN-branded Stripe checkout page
- `Assets/Scripts/WelcomeOffer/IAPHandler.cs` — Unity IAP client
