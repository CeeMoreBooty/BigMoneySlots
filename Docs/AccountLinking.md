# Account Linking — Company Documentation

## Overview
Players can link up to 4 external accounts to their Big Money Slots profile.
Each link awards a **one-time coin bonus** as an incentive.

| Provider  | Bonus           | Notes                                   |
|-----------|----------------|----------------------------------------|
| Facebook  | 2.5T coins     | OAuth 2.0 via Facebook Login SDK       |
| Google    | 2.5T coins     | OAuth 2.0 via Google Sign-In SDK       |
| Phone     | 2.5T coins     | SMS OTP verification (Twilio / Firebase)|
| Discord   | 5T coins       | OAuth 2.0 via Discord API              |

**Total maximum bonus per player: 12.5 Trillion coins**

## Business Purpose
- Increases account retention (harder to abandon a linked account)
- Provides verified identity signals for fraud prevention
- Enables cross-platform login recovery
- Drives new-user acquisition via Discord community sharing

## Technical Flow

```
Player taps "Link" in AccountLinkingUI
      ↓
Native OAuth SDK opens (Facebook / Google / Discord) or SMS OTP dialog (Phone)
      ↓
Unity receives access token / OTP result
      ↓
AccountLinking.BeginLink(provider, token)
      ↓
POST /api/account/link  { provider, token }
      ↓
Backend validates token with provider API (TODO: implement per provider)
      ↓
LinkedAccount document created in MongoDB
      ↓
Bonus coins credited to Player.coins
      ↓
Transaction record written (type: "admin_grant", description: "Account link bonus: {provider}")
      ↓
Response: { bonusGranted: true, displayName, newBalance }
      ↓
Unity grants coins locally via PlayerEconomy.AddCoins()
      ↓
AccountLinkingUI.Refresh() updates badge + status
```

## Data Stored
Collection: `linkedaccounts`

| Field        | Type     | Description                           |
|--------------|----------|---------------------------------------|
| playerId     | ObjectId | Reference to Player                   |
| provider     | String   | facebook / google / phone / discord   |
| providerUid  | String   | UID from OAuth provider               |
| displayName  | String   | Email or username from provider       |
| bonusGranted | Boolean  | Whether one-time bonus was paid out   |
| linkedAt     | Date     | Timestamp of initial link             |

## Security Controls
- Each `(provider, providerUid)` pair is unique across the database — one provider account cannot be linked to two players
- Bonus is granted exactly once (`bonusGranted` flag, atomic upsert)
- All link events are recorded in the Transaction ledger for audit
- IP is logged on every API call (see `middleware/ipLogger.js`)

## Privacy & Compliance
- Access tokens are **never stored** — only the provider's UID and display name
- Players may unlink at any time via `DELETE /api/account/link/:provider`
- Unlinking does **not** reverse the coin bonus (one-time reward is non-refundable)
- Disclose account linking data collection in your Privacy Policy (GDPR Article 13 / CCPA §1798.100)

## Files
- `Assets/Scripts/Social/AccountLinking.cs` — Unity client
- `Assets/Scripts/UI/AccountLinkingUI.cs`   — Unity UI
- `Backend/models/LinkedAccount.js`         — Mongoose model
- `Backend/routes/accountLinking.js`        — Express routes
