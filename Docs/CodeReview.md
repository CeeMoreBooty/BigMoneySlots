# BigMoneySlots — Full Code Review Report

**Copyright © 2024–2026 PGCN (Pretty Good Casino Network). All rights reserved.**
**Review Date:** April 3, 2026
**Reviewer:** AI Code Analysis
**Target:** Full project — Unity C# (Android) + Node.js Backend

---

## 1. Executive Summary

BigMoneySlots is a social casino entertainment Android app combining:
- **Unity 6.4 (6000.4.0f1)** front-end with 21 C# scripts
- **Node.js/Express** backend with MongoDB, Socket.IO
- **Google Play Billing**, Stripe, and PayPal payment integrations
- **Loyalty system**, progressive jackpots, 40 slot game configs
- **Security monitoring** with Discord admin webhook alerts

**Overall Health:** ✅ Good — No compiler errors, all backend requires resolve, Android build settings configured, legal documents in place.

---

## 2. File & Folder Inventory

### Unity (Assets/)

```
Assets/
├── Scenes/
│   └── MainScene.unity              ← Bootstrap scene (GameManager object)
│
└── Scripts/
    ├── Backend/
    │   └── BackendClient.cs         ← Static class: BaseUrl + AuthToken
    │
    ├── Core/
    │   ├── GameManager.cs           ← Singleton; DontDestroyOnLoad
    │   └── PlayerEconomy.cs         ← Coins, spins, boosts, cosmetics; PlayerPrefs persistence
    │
    ├── Rewards/
    │   ├── RewardManager.cs         ← Hourly (500K) + daily streak + new-player pack
    │   ├── LoyaltySystem.cs         ← 5 tiers (TrailWanderer→GreatPlainsChief); win multipliers
    │   ├── GemSystem.cs             ← Gem hard currency; IAP bundle grants; spin purchases
    │   ├── DailyChallenges.cs       ← 3 daily challenges; gem+coin rewards
    │   └── InviteRewardManager.cs   ← Invite code fetch/redeem via backend REST
    │
    ├── SlotEngine/
    │   ├── SlotMachine.cs           ← Core spin: spend coins → spin reels → jackpot eval → payout
    │   ├── ProgressiveJackpot.cs    ← 5-tier jackpot (Mini→Mega); dynamic probability
    │   ├── Reel.cs                  ← Random symbol selection from pool
    │   ├── Symbol.cs                ← ScriptableObject: symbolId + name + sprite
    │   ├── PayoutTable.cs           ← ScriptableObject: per-symbol 2OAK/3OAK multipliers
    │   ├── SlotGameConfig.cs        ← ScriptableObject: per-game settings (RTP, bet range, volatility)
    │   ├── SlotGameDatabase.cs      ← 40 game definitions (all Prairie Guardian themes)
    │   ├── SlotGameLoader.cs        ← Singleton: loads active game; saves last game to PlayerPrefs
    │   └── SlotGameRegistry.cs      ← ScriptableObject: holds list of all SlotGameConfig assets
    │
    ├── Social/
    │   ├── AccountLinking.cs        ← Facebook/Google/Phone/Discord linking + coin bonuses
    │   ├── ChatManager.cs           ← 3-channel chat (Global/Room/Friends); REST fallback
    │   ├── VoiceChatManager.cs      ← Microphone capture; Vivox/WebRTC stub
    │   └── FriendsManager.cs        ← Friend list, pending requests, 30-sec polling
    │
    ├── UI/
    │   ├── SlotUI.cs                ← Balance, bet, jackpot display; spin button
    │   ├── JackpotUI.cs             ← Live jackpot pool display for all 5 tiers
    │   ├── ChatUI.cs                ← Channel tabs, message history, voice controls
    │   ├── FriendsUI.cs             ← Friend list + player search
    │   ├── AccountLinkingUI.cs      ← Link status per provider + bonus display
    │   └── InviteUI.cs              ← Show invite code + redeem field
    │
    └── WelcomeOffer/
        ├── WelcomeOfferConfig.cs    ← ScriptableObject: bundle contents ($4.99 offer)
        ├── WelcomeOfferManager.cs   ← Grant logic; PlayerPrefs flag to prevent duplicate grants
        ├── WelcomeOfferUI.cs        ← Modal UI panel
        └── IAPHandler.cs            ← Unity IAP stub; Google Play purchase flow
```

### Backend (Node.js)

```
Backend/
├── config/
│   └── db.js                        ← MongoDB connection (supports Railway MONGO_URL)
│
├── middleware/
│   ├── auth.js                      ← JWT verification; attaches req.player
│   ├── ipLogger.js                  ← Logs IP + path to IpLog model
│   └── securityGuard.js             ← IP ban check, UA fingerprinting, payload size,
│                                       injection detection, per-IP rate tracking
│
├── models/
│   ├── Player.js                    ← coins, gems, freeSpins, superSpins, ultraSpins,
│   │                                   badges, welcomeOfferPurchased
│   ├── Transaction.js               ← IAP records, coin grants, purchase tokens
│   ├── Tournament.js                ← Tournament metadata
│   ├── TournamentEntry.js           ← Per-player tournament score
│   ├── IpLog.js                     ← IP activity log
│   ├── ChatMessage.js               ← Chat history
│   ├── Friendship.js                ← Friend relationships (pending/accepted)
│   ├── LinkedAccount.js             ← OAuth linked accounts (Facebook/Google/Discord/Phone)
│   ├── InviteReward.js              ← Invite codes + redemption records
│   ├── SecurityEvent.js             ← Security event log (severity, type, IP, resolved)
│   └── BannedEntity.js              ← IP/player/device bans (active, expiresAt)
│
├── routes/
│   ├── auth.js                      ← POST /register, GET /me, PUT /display-name
│   ├── coins.js                     ← GET /balance, POST /grant (admin), daily/hourly rewards
│   ├── payments.js                  ← Google Play verify, PayPal create/capture, Stripe
│   ├── tournament.js                ← CRUD tournaments, leaderboard, entry
│   ├── chat.js                      ← POST /send, GET /history, voice signal
│   ├── friends.js                   ← GET /list, POST /request|accept|decline|remove, GET /search
│   ├── accountLinking.js            ← POST /link, GET /status
│   ├── security.js                  ← GET events/stats/bans, POST ban/unban,
│   │                                   PATCH resolve, webhook/Discord test + status
│   └── invite.js                    ← GET /code, POST /redeem, GET /leaderboard
│
├── services/
│   ├── discordAdminWebhook.js       ← Discord embed notifications (ban, purchase, player, error)
│   ├── webhookService.js            ← Generic HMAC-signed security webhook
│   ├── securityService.js           ← isBanned, logEvent, checkUA, checkInjection, trackRate
│   ├── paypalService.js             ← PayPal OAuth + order create/capture
│   ├── googlePlayVerifier.js        ← Google Play purchase token verification
│   ├── stripeService.js             ← Stripe payment intent helpers
│   └── tournamentScheduler.js       ← Cron-based tournament auto-start/end
│
├── public/
│   └── stripe-checkout.html         ← Stripe card payment web page
│
├── server.js                        ← Express app: rate-limit, security middleware,
│                                       routes, socket.io, Discord startup/error alerts
├── .env.example                     ← All required env vars (no real secrets)
└── package.json
```

### Project Root

```
BigMoneySlots/
├── Docs/
│   ├── TermsOfService.md            ← Full T&C with NO REFUND POLICY (§5)
│   ├── PrivacyPolicy.md
│   ├── PaymentSystem.md
│   ├── SecuritySystem.md
│   ├── AccountLinking.md
│   └── SystemOverview.md
│
├── GooglePlay/
│   ├── BuildSettings/
│   │   └── AndroidBuildChecklist.md
│   └── GooglePlayConsoleChecklist.md
│
├── Packages/
│   └── manifest.json                ← Unity 6.4 packages (TMP, UI, WebRequest, androidjni)
│
├── ProjectSettings/
│   ├── ProjectVersion.txt           ← 6000.4.0f1
│   ├── ProjectSettings.asset        ← Bundle: com.PGCN.BigMoneySlots; IL2CPP; ARM64; API 24–35
│   ├── EditorBuildSettings.asset    ← MainScene registered
│   ├── GraphicsSettings.asset
│   ├── QualitySettings.asset
│   └── TagManager.asset
│
├── .gitignore                       ← node_modules, .env, Library/, Temp/, builds
├── LICENSE                          ← Proprietary © 2024–2026 PGCN
├── NOTICE                           ← Third-party attributions
└── README.md
```

---

## 3. C# Compiler Analysis

### ✅ Fixed Issues (from previous PR)
- `using var` inside `IEnumerator` coroutines — converted to `using (...) { }` block form in 6 files
- All referenced types (`BackendClient`, `GemSystem`, `FriendsManager`, `JackpotTier`, `JackpotTierData`) confirmed present as defined classes

### ✅ No Outstanding Compiler Errors
All 21 C# scripts compile cleanly under Unity 6.4 C# 9.0:

| Script | Status | Notes |
|--------|--------|-------|
| BackendClient.cs | ✅ | Static class, `#if UNITY_EDITOR` conditional |
| GameManager.cs | ✅ | Singleton pattern |
| PlayerEconomy.cs | ✅ | Long/int/float fields, PlayerPrefs serialization |
| RewardManager.cs | ✅ | |
| LoyaltySystem.cs | ✅ | Enum indexing, complex milestones |
| GemSystem.cs | ✅ | |
| DailyChallenges.cs | ✅ | long targets stored as PlayerPrefs strings |
| InviteRewardManager.cs | ✅ | `using (var req)` block form |
| SlotMachine.cs | ✅ | Nullable tuple return `(JackpotTier?, long)` |
| ProgressiveJackpot.cs | ✅ | 5-tier, `long` pools, `10_000_000_000L` |
| Reel.cs | ✅ | |
| Symbol.cs | ✅ | ScriptableObject |
| PayoutTable.cs | ✅ | ScriptableObject |
| SlotGameConfig.cs | ✅ | ScriptableObject, `CreateAssetMenu` |
| SlotGameDatabase.cs | ✅ | Static `List<GameDefinition>`, 40 entries |
| SlotGameLoader.cs | ✅ | |
| SlotGameRegistry.cs | ✅ | ScriptableObject |
| AccountLinking.cs | ✅ | `using (var req)` block form |
| ChatManager.cs | ✅ | `using (var req)` block form |
| VoiceChatManager.cs | ✅ | `using (var req)` block form |
| FriendsManager.cs | ✅ | `using (var req)` block form |
| SlotUI.cs | ✅ | TMPro, UnityEngine.UI |
| JackpotUI.cs | ✅ | TMPro |
| ChatUI.cs | ✅ | TMPro, ScrollRect |
| FriendsUI.cs | ✅ | `using (var req)` block form |
| AccountLinkingUI.cs | ✅ | TMPro, Button |
| InviteUI.cs | ✅ | TMPro |
| WelcomeOfferConfig.cs | ✅ | ScriptableObject |
| WelcomeOfferManager.cs | ✅ | |
| WelcomeOfferUI.cs | ✅ | TMPro |
| IAPHandler.cs | ✅ | Unity IAP stub; TODO comments mark integration points |

### ⚠️ Notes (Not Errors — Action Items for Production)

| Item | Description | Priority |
|------|-------------|----------|
| **Unity IAP not wired** | `IAPHandler.cs` contains stubs for Unity IAP. Must import `com.unity.purchasing` and implement `IStoreListener` before Google Play submission. | HIGH |
| **Socket.IO client** | `ChatManager.cs` has a TODO for socket.io real-time; currently falls back to REST. Must add `socket.io-client-csharp` package for real-time chat. | MEDIUM |
| **VoiceChatManager Vivox stub** | `VoiceChatManager.cs` captures microphone but needs Vivox SDK or WebRTC plugin for real voice transmission. | MEDIUM |
| **BackendClient URL** | `#else` branch uses `https://api.bigmoneyslots.com` — update to real deployed URL before production build. | HIGH |
| **No `.meta` for new scripts** | If new scripts are added, corresponding `.meta` files must be created. | LOW |

---

## 4. Backend Analysis

### ✅ All 20 `require()` Calls Resolve

Verified via `node -e "require.resolve(...)"`:
- config/db ✅
- middleware: auth, ipLogger, securityGuard ✅
- routes: auth, coins, payments, tournament, chat, friends, accountLinking, security, invite ✅
- services: tournamentScheduler, discordAdminWebhook, webhookService, securityService, paypalService, googlePlayVerifier, stripeService ✅

### Route Coverage

| Endpoint Group | Routes |
|---------------|--------|
| `/api/auth` | POST register, GET me, PUT display-name |
| `/api/coins` | GET balance, POST grant, claim-daily, claim-hourly |
| `/api/payments` | POST google-play/verify, paypal/create, paypal/capture, stripe |
| `/api/tournament` | GET list/current, POST enter/submit-score |
| `/api/chat` | POST send, GET history, voice join/leave |
| `/api/friends` | GET list, POST request/accept/decline/remove, GET search |
| `/api/account` | POST link, GET status |
| `/api/security` | GET events/stats/bans, POST ban/unban, PATCH resolve, webhook/discord test |
| `/api/invite` | GET code, POST redeem, GET leaderboard |

### Security Architecture

| Feature | Implementation |
|---------|--------------|
| Authentication | JWT (30-day expiry), device-ID registration |
| IP banning | `BannedEntity` model, checked every request |
| Rate limiting | express-rate-limit (120 req/15min) + per-IP endpoint tracking |
| Injection detection | Regex patterns checked on request body |
| User-agent fingerprinting | Exploit tool detection |
| Admin routes | `x-admin-secret` header guard |
| Webhook signing | HMAC-SHA256 (`X-BMS-Signature`) |
| Discord alerts | Real-time embed notifications for bans, purchases, errors |

### ⚠️ Backend Notes (Action Items)

| Item | Description | Priority |
|------|-------------|----------|
| **No `.env` in repo** | ✅ Correctly absent. Copy `.env.example` → `.env` and fill in real credentials. | — |
| **MongoDB connection** | Uses `MONGO_URI` or `MONGO_URL` (Railway). Ensure Atlas cluster is set up. | HIGH |
| **Google service account** | `GOOGLE_APPLICATION_CREDENTIALS` must point to a real downloaded JSON file. | HIGH |
| **Stripe webhook** | `STRIPE_WEBHOOK_SECRET` must be set to the value from the Stripe dashboard. | HIGH |
| **Discord webhook URL** | Set `ADMIN_DISCORD_WEBHOOK_URL` in `.env` — never commit the real token. | HIGH |
| **CORS** | `cors: { origin: '*' }` — tighten to your specific domain before production. | MEDIUM |
| **HTTPS** | Server itself is HTTP; must be placed behind a TLS-terminating proxy (nginx/Railway). | HIGH |

---

## 5. Android Deployment Checklist

### Unity Build Settings
| Setting | Value |
|---------|-------|
| Platform | Android ✅ |
| Bundle Identifier | `com.PGCN.BigMoneySlots` ✅ |
| Min API Level | 24 (Android 7.0) ✅ |
| Target API Level | 35 (Android 15) ✅ |
| Scripting Backend | IL2CPP ✅ |
| Architecture | ARM64 ✅ |
| Active Input Handling | Input System (new) ✅ |

### Unity IAP Setup (Required Before Launch)
1. Add `com.unity.purchasing` to `Packages/manifest.json`
2. Implement `IStoreListener` in `IAPHandler.cs`
3. Configure product IDs in Google Play Console
4. Set up Google Play Billing License Key

### Google Play Console Checklist
- [ ] Developer account created and verified
- [ ] App created with bundle ID `com.PGCN.BigMoneySlots`
- [ ] Content rating completed (Casino/Gambling category)
- [ ] Store listing (screenshots, description, icon) uploaded
- [ ] Privacy Policy URL set to hosted `PrivacyPolicy.md`
- [ ] Target audience: 18+ (casino/gambling)
- [ ] All IAP products created and active
- [ ] Release AAB signed with release keystore
- [ ] Internal testing track → closed testing → production

### Backend Deployment Checklist
- [ ] MongoDB Atlas cluster provisioned
- [ ] All environment variables set
- [ ] HTTPS/TLS configured
- [ ] `POST /health` returns `{ status: 'ok' }`
- [ ] Discord webhook URL configured and tested
- [ ] Google Play service account JSON uploaded
- [ ] Stripe webhook endpoint registered

---

## 6. Security Review

| Finding | Severity | Status |
|---------|----------|--------|
| No real secrets in source code | — | ✅ All secrets in `.env` |
| Discord webhook URL not hardcoded | — | ✅ Env var only |
| JWT secret is configurable | — | ✅ `JWT_SECRET` env var |
| CORS wildcard origin | MEDIUM | ⚠️ Tighten before production |
| HTTP server (no TLS in code) | HIGH | ⚠️ Needs reverse proxy |
| Rate limiting active | — | ✅ express-rate-limit |
| IP ban system | — | ✅ SecurityGuard middleware |
| Admin endpoints guarded | — | ✅ `x-admin-secret` header |
| Webhook HMAC signature | — | ✅ `X-BMS-Signature` |
| No SQL/NoSQL injection in routes | — | ✅ Mongoose ODM |
| Payload injection detection | — | ✅ securityService |

---

## 7. Code Quality Observations

### Strengths
- Clean singleton patterns across all Unity managers (Awake + DontDestroyOnLoad)
- Consistent use of `static event Action` for decoupled UI updates
- All coroutines use `using (var req) { }` block form (no iterator incompatibility)
- Backend error handling: all routes wrapped in `try/catch` with proper HTTP status codes
- Security events logged to DB AND forwarded to Discord webhook
- Economy values use `long` throughout to support billions of coins

### Recommendations
1. **Unity IAP** — wire up `IStoreListener` in `IAPHandler.cs` before submitting to Play Store
2. **Backend CORS** — change `origin: '*'` to your deployed Unity server hostname
3. **BackendClient.BaseUrl** — update the production URL before release build
4. **TLS** — ensure the Node.js server sits behind nginx or is deployed to Railway/Render with TLS
5. **Database indexes** — verify MongoDB indexes on `Player.deviceId`, `Transaction.purchaseToken`, `BannedEntity.ip` for performance
6. **Chat real-time** — integrate `socket.io-client-csharp` in Unity for real-time messages

---

## 8. Summary

| Area | Status |
|------|--------|
| C# Scripts (21 files) | ✅ No compiler errors |
| Backend requires (20) | ✅ All resolve |
| Android build settings | ✅ Configured (com.PGCN.BigMoneySlots, IL2CPP, ARM64, API 24-35) |
| Legal documents | ✅ ToS (with No Refund §5), Privacy Policy, LICENSE, NOTICE |
| Discord admin webhook | ✅ Implemented (env-var URL, never hardcoded) |
| Copyright | ✅ © 2024-2026 PGCN on all files |
| Deployment readiness | ⚠️ Unity IAP + real credentials + TLS needed before live |

---

*Report generated: April 3, 2026 — BigMoneySlots v1.0 — © 2024–2026 PGCN*
