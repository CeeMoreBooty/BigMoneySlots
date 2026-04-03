# 🎰 BigMoneySlots

**Copyright © 2024–2026 PGCN (Pretty Good Casino Network). All rights reserved.**

BigMoneySlots is a social casino entertainment app built in **Unity 6.4** for **Android** (Google Play). It features a 40-game slot machine engine, progressive jackpots, a loyalty system, daily challenges, social features (friends, chat, voice), invite rewards, and a full monetization pipeline.

> ⚠️ **Entertainment only.** Virtual currency has no real-world value. All sales are final — see [Terms of Service](Docs/TermsOfService.md) §5 No Refund Policy.

---

## 📱 Platform

| Target | Details |
|--------|---------|
| **Primary** | Android (Google Play) |
| **Min SDK** | Android API 24 (Android 7.0) |
| **Target SDK** | Android API 35 (Android 15) |
| **Unity Version** | 6000.4.0f1 (Unity 6.4) |
| **Architecture** | ARM64 |
| **Scripting Backend** | IL2CPP |
| **Bundle ID** | `com.PGCN.BigMoneySlots` |

---

## 🏗️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Game Engine | Unity 6.4 |
| Language | C# / Node.js |
| Backend | Express.js + Socket.IO |
| Database | MongoDB (Mongoose) |
| Payments | Google Play Billing, Stripe, PayPal |
| Auth | JWT (device-ID) |
| Admin Alerts | Discord Webhook |

---

## 📁 Project Structure

```
BigMoneySlots/
├── Assets/Scripts/
│   ├── Backend/         BackendClient.cs
│   ├── Core/            GameManager.cs, PlayerEconomy.cs
│   ├── Rewards/         RewardManager, LoyaltySystem, GemSystem, DailyChallenges, InviteRewardManager
│   ├── SlotEngine/      SlotMachine, ProgressiveJackpot, Reel, Symbol, PayoutTable,
│   │                    SlotGameConfig, SlotGameDatabase, SlotGameLoader, SlotGameRegistry
│   ├── Social/          AccountLinking, ChatManager, VoiceChatManager, FriendsManager
│   ├── UI/              SlotUI, JackpotUI, ChatUI, FriendsUI, AccountLinkingUI, InviteUI
│   └── WelcomeOffer/    WelcomeOfferConfig, WelcomeOfferManager, WelcomeOfferUI, IAPHandler
│
├── Backend/
│   ├── config/db.js
│   ├── middleware/      auth, ipLogger, securityGuard
│   ├── models/          Player, Transaction, Tournament, TournamentEntry, IpLog,
│   │                    ChatMessage, Friendship, LinkedAccount, InviteReward,
│   │                    SecurityEvent, BannedEntity
│   ├── routes/          auth, coins, payments, tournament, chat, friends,
│   │                    accountLinking, security, invite
│   ├── services/        discordAdminWebhook, webhookService, paypalService,
│   │                    googlePlayVerifier, stripeService, tournamentScheduler, securityService
│   └── server.js
│
├── Docs/
│   ├── TermsOfService.md   (incl. No Refund Policy)
│   ├── PrivacyPolicy.md
│   ├── PaymentSystem.md
│   ├── SecuritySystem.md
│   ├── AccountLinking.md
│   └── SystemOverview.md
│
├── GooglePlay/          AndroidBuildChecklist, GooglePlayConsoleChecklist
├── Packages/manifest.json
├── ProjectSettings/     (Unity 6.4 / Android build config)
├── LICENSE              Proprietary — © 2024-2026 PGCN
├── NOTICE               Third-party attributions
└── README.md
```

---

## 🚀 Backend Setup

```bash
cd Backend
cp .env.example .env   # fill in your credentials
npm install
npm start
```

Key env vars: `MONGO_URI`, `JWT_SECRET`, `GOOGLE_PLAY_PACKAGE_NAME`,
`STRIPE_SECRET_KEY`, `PAYPAL_CLIENT_ID/SECRET`, `ADMIN_SECRET`,
`ADMIN_DISCORD_WEBHOOK_URL`.

---

## 🎮 Unity Setup (Android)

1. Install Unity Hub + editor **6000.4.0f1** with **Android Build Support**
2. Open this folder as a Unity project
3. File → Build Settings → Android → Switch Platform
4. Player Settings → set Keystore for release signing
5. Build → Android App Bundle (`.aab`) for Play Store

---

## 💰 In-App Products

| Product ID | Price | Contents |
|-----------|-------|---------|
| `welcomeofferpack` | $4.99 | 150B Coins + 200 Free Spins + VIP badge |
| `gems_100` | $0.99 | 100 Gems |
| `gems_500` | $4.99 | 500 Gems |
| `gems_1200` | $9.99 | 1,200 Gems |
| `gems_2500` | $19.99 | 2,500 Gems |
| `gems_6500` | $49.99 | 6,500 Gems |

> **ALL SALES FINAL — NO REFUNDS.** See [Terms of Service §5](Docs/TermsOfService.md).

---

## 🔒 Discord Admin Webhook

Set `ADMIN_DISCORD_WEBHOOK_ENABLED=true` and `ADMIN_DISCORD_WEBHOOK_URL=<url>` in `.env`.
Test: `POST /api/security/discord/test` (with `x-admin-secret` header).

Notifications cover: server start, new players, purchases, bans, security events, errors.

---

## 📄 Legal

| | |
|-|-|
| [Terms of Service](Docs/TermsOfService.md) | Full T&C including **No Refund Policy §5** |
| [Privacy Policy](Docs/PrivacyPolicy.md) | Data collection and use |
| [LICENSE](LICENSE) | Proprietary — all rights reserved |
| [NOTICE](NOTICE) | Third-party open-source attributions |

---

**Copyright © 2024–2026 PGCN (Pretty Good Casino Network). All rights reserved.**
