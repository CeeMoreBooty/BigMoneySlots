# 🎰 Big Money Slots

**Copyright © 2024 CeeMoreBooty / Big Money Slots. All Rights Reserved.**
*This is proprietary software. Unauthorized use, reproduction, or distribution is strictly prohibited.*

---

## Overview

Big Money Slots is a feature-rich, free-to-play social casino mobile game built with **Unity 2021.3.45f2**
targeting **Android** (package: `com.ceemorebooty.bigmoneyslots`), backed by a **Node.js / MongoDB** server.

---

## 📁 Project Structure

```
BigMoneySlots/
├── Assets/
│   ├── Resources/           # ScriptableObject assets (symbols, payout table, registry, WelcomeOffer)
│   ├── Scenes/              # Bootstrap → MainMenu → Game
│   └── Scripts/
│       ├── Analytics/       # AnalyticsManager, DeviceTracker
│       ├── Backend/         # BackendClient (shared base URL + auth token)
│       ├── Core/            # GameManager, PlayerEconomy, BootstrapLoader, SceneLoader
│       ├── Rewards/         # DailyChallenges, GemSystem, InviteRewardManager, LoyaltySystem, RewardManager
│       ├── Security/        # FraudPrevention
│       ├── SlotEngine/      # SlotMachine, Reel, PayoutTable, ProgressiveJackpot, SlotGameLoader, Symbol
│       ├── Social/          # ChatManager, FriendsManager, VoiceChatManager, AccountLinking
│       ├── UI/              # UIBuilder, SlotUI, JackpotUI, HUDManager, LobbyUI, SettingsUI,
│       │                    #   ChatUI, FriendsUI, InviteUI, AccountLinkingUI, WelcomeOfferUI,
│       │                    #   TermsAndConditionsUI
│       └── WelcomeOffer/    # IAPHandler, WelcomeOfferConfig, WelcomeOfferManager, WelcomeOfferUI
├── Backend/
│   ├── config/              # db.js (MongoDB connection)
│   ├── middleware/          # auth.js, ipLogger.js, securityGuard.js
│   ├── models/              # Mongoose models
│   ├── public/              # stripe-checkout.html
│   ├── routes/              # auth, coins, payments, tournament, chat, friends, accountLinking,
│   │                        #   security, invite, analytics
│   ├── services/            # securityService, webhookService, discordAdminWebhook,
│   │                        #   stripeService, paypalService, googlePlayVerifier, tournamentScheduler
│   ├── .env.example         # Environment variable template
│   ├── package.json
│   └── server.js
├── Docs/                    # SystemOverview, PaymentSystem, SecuritySystem, AccountLinking, CodeReview
├── GooglePlay/              # Build checklists
├── Packages/                # Unity packages (manifest.json)
├── ProjectSettings/         # Unity project settings (Android target)
├── LICENSE
├── TERMS_AND_CONDITIONS.md
├── DEPLOYMENT_CHECKLIST.md
└── README.md
```

---

## 🎮 Unity Scenes

| Build Index | Scene       | Purpose                             |
|-------------|-------------|-------------------------------------|
| 0           | Bootstrap   | Initialises singletons, loads Main  |
| 1           | MainMenu    | Lobby, balance display, navigation  |
| 2           | Game        | Full slot machine with all systems  |

---

## �� Systems & Wiring

| System              | Script                  | Event Connections                                        |
|---------------------|-------------------------|----------------------------------------------------------|
| Spin Engine         | `SlotMachine`           | Calls FraudPrevention → DailyChallenges → Analytics      |
| Fraud Prevention    | `FraudPrevention`       | Rate-limits spins; ban-checks device on Start()          |
| Analytics           | `AnalyticsManager`      | Auto-subscribes OnJackpotWon, OnTierUp, OnMilestone      |
| Device Tracking     | `DeviceTracker`         | MD5 hashed device ID; daily IP/geo lookup via ipapi.co   |
| Daily Challenges    | `DailyChallenges`       | Hooks: OnJackpotWon, OnGameChanged; RecordSpin/Win calls |
| Loyalty             | `LoyaltySystem`         | ApplyTierBonus on every win; session reward timer        |
| Jackpot             | `ProgressiveJackpot`    | 5-tier pool; EvaluateSpin on every spin                  |
| UI                  | `UIBuilder`             | Builds full Canvas at runtime; no Inspector wiring needed|
| Social – Chat       | `ChatManager`           | REST + Socket.IO; 3 channels (Global/Room/Friends DM)    |
| Social – Friends    | `FriendsManager`        | Polls every 30 s; add/accept/decline/remove              |
| Social – Voice      | `VoiceChatManager`      | Join/leave/mute via backend API                          |
| IAP                 | `IAPHandler`            | Google Play + PayPal verification                        |

---

## 🖥️ Backend API Routes

| Method | Path                              | Description                          |
|--------|-----------------------------------|--------------------------------------|
| POST   | `/api/auth/register`              | Register new player                  |
| POST   | `/api/auth/login`                 | Login / get JWT                      |
| GET    | `/api/coins/balance`              | Get coin balance                     |
| POST   | `/api/payments/google-play/verify`| Verify & grant Google Play purchase  |
| POST   | `/api/payments/paypal/create`     | Create PayPal order                  |
| POST   | `/api/payments/paypal/capture`    | Capture PayPal payment               |
| POST   | `/api/analytics/batch`            | Receive Unity analytics event batch  |
| GET    | `/api/analytics/recent`           | Admin: view recent events            |
| GET    | `/api/friends`                    | List friends & pending requests      |
| POST   | `/api/friends/request`            | Send friend request                  |
| POST   | `/api/chat/send`                  | Send chat message                    |
| GET    | `/api/security/events`            | Admin: list security events          |
| POST   | `/api/security/ban`               | Admin: manual ban                    |
| GET    | `/health`                         | Server health check                  |

---

## 🛡️ Security & Admin Notifications

All admin events are forwarded to your Discord server via `ADMIN_DISCORD_WEBHOOK_URL`:
- **🟢 Server Start** — notified every time the backend boots
- **🔨 Bans** — auto-bans (by fraud detection) and manual admin bans
- **⚠️ Fraud Events** — high/critical security events (brute force, injection, impossible wins)
- **💰 Purchases** — every completed in-app purchase

Configure in `.env`:
```
ADMIN_DISCORD_WEBHOOK_ENABLED=true
ADMIN_DISCORD_WEBHOOK_URL=https://discord.com/api/webhooks/YOUR_ID/YOUR_TOKEN
```

---

## 📱 Android Build Settings

- **Package Name:** `com.ceemorebooty.bigmoneyslots`
- **Scripting Backend:** IL2CPP
- **Target Architectures:** ARM64
- **Minimum SDK:** 22 (Android 5.1)
- **Target SDK:** 33 (Android 13)
- **Build System:** Gradle

---

## 🚀 Deployment

See [`DEPLOYMENT_CHECKLIST.md`](DEPLOYMENT_CHECKLIST.md) for the complete deployment guide.

**Quick backend start:**
```bash
cd Backend
cp .env.example .env   # Fill in your secrets
npm install
npm start
```

---

## ⚖️ Legal

- **Terms & Conditions:** [TERMS_AND_CONDITIONS.md](TERMS_AND_CONDITIONS.md)
  — includes the **No Refund Policy** and full usage terms.
- **License:** [LICENSE](LICENSE) — Proprietary. All Rights Reserved.
- **Contact:** tech.crew151@gmail.com

---

*Copyright © 2024 CeeMoreBooty / Big Money Slots. All Rights Reserved.*
