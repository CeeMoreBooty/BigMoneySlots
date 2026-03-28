# Big Money Slots 🎰

A mobile casino slots game built with **Unity 2022.3.20f1** (Android) and a **Node.js / Express** backend.

---

## Table of Contents
1. [Project Structure](#project-structure)
2. [How to Build the Game (Unity → Android)](#how-to-build-the-game-unity--android)
3. [How to Run the Backend](#how-to-run-the-backend)
4. [Running the Backend Tests](#running-the-backend-tests)
5. [Google Play Store Submission Checklist](#google-play-store-submission-checklist)
6. [Functionality Checklist — Is It Ready?](#functionality-checklist--is-it-ready)

---

## Project Structure

```
BigMoneySlots/
├── Assets/                          Unity game assets and C# scripts
│   ├── Art/Sprites/                 Symbol and UI sprites
│   ├── Audio/                       Sound effects (spin, win, bigwin, click, coins)
│   ├── Prefabs/                     Reusable UI prefabs (friends, chat, DM rows)
│   ├── Resources/Sprites/           Reel skin sprites (loaded at runtime)
│   ├── Scenes/MainScene.unity       The single main scene
│   ├── ScriptableObjects/           Symbol, SlotGameConfig, PayoutTable, Registry assets
│   └── Scripts/
│       ├── Backend/                 BackendClient.cs — HTTP + WebSocket client
│       ├── Core/                    GameManager.cs, PlayerEconomy.cs
│       ├── Rewards/                 DailyChallenges, GemSystem, InviteReward, Loyalty, RewardManager
│       ├── SlotEngine/              SlotMachine, Reel, Symbol, PayoutTable, ProgressiveJackpot,
│       │                            SecureRandom, SlotGameConfig, SlotGameDatabase/Loader/Registry
│       ├── Social/                  AccountLinking, ChatManager, FriendsManager, VoiceChatManager
│       ├── UI/                      SlotUI, ChatUI, FriendsUI, DirectMessageUI, InviteUI,
│       │                            JackpotUI, AccountLinkingUI
│       └── WelcomeOffer/            IAPHandler, WelcomeOfferConfig/Manager/UI
├── Backend/                         Node.js / Express API server
│   ├── config/db.js                 Mongoose connection
│   ├── middleware/                  auth, adminOnly, ipLogger, securityGuard
│   ├── models/                      Player, Transaction, Tournament, TournamentEntry,
│   │                                ChatMessage, Friendship, InviteReward, LinkedAccount,
│   │                                IpLog, SecurityEvent, BannedEntity
│   ├── routes/                      auth, coins, payments, tournament, chat, friends,
│   │                                accountLinking, security, invite
│   ├── services/                    googlePlayVerifier, paypalService, stripeService,
│   │                                securityService, tournamentScheduler, webhookService
│   ├── tests/                       Jest test suite (57 tests)
│   ├── app.js                       Express app factory (no DB/listen — used by tests)
│   ├── server.js                    Entry point — connects DB and starts server
│   ├── .env.example                 Copy to .env and fill in secrets
│   ├── Dockerfile
│   └── docker-compose.yml
├── Docs/                            Design documents (AccountLinking, Payment, Security, Overview)
├── GooglePlay/                      Play Store assets and checklists
│   ├── BuildSettings/AndroidBuildChecklist.md
│   └── GooglePlayConsoleChecklist.md
├── Packages/manifest.json           Unity package dependencies
└── ProjectSettings/                 Unity project settings (version 2022.3.20f1)
```

---

## How to Build the Game (Unity → Android)

### Prerequisites

| Tool | Version / Notes |
|------|----------------|
| Unity Hub | Latest — [unity.com/download](https://unity.com/download) |
| Unity Editor | **2022.3.20f1 LTS** — install via Unity Hub |
| Android Build Support | Install the Android module inside Unity Hub alongside the editor |
| Android SDK / NDK / JDK | Bundled with Unity — Unity Hub installs these automatically |

### Step 1 — Open the Project
1. Open **Unity Hub** → click **Open** → select the `BigMoneySlots/` root folder.
2. Unity imports all assets on first open (a few minutes).

### Step 2 — Configure Player Settings
Go to **Edit → Project Settings → Player → Android tab**:

| Setting | Required Value |
|---------|---------------|
| Package Name | `com.yourstudio.bigmoneyslots` *(change to your own)* |
| Version | `1.0` |
| Bundle Version Code | `1` |
| Scripting Backend | **IL2CPP** *(required by Google Play)* |
| Target Architectures | ✅ ARMv7 + ✅ ARM64 |
| Minimum API Level | API 23 (Android 6.0) |
| Target API Level | API 34 (Android 14) |
| Internet Access | **Require** |

### Step 3 — Create a Keystore *(first time only)*
1. **Player Settings → Publishing Settings → Keystore Manager → Create New**
2. Save outside the project (e.g. `~/keystores/bigmoneyslots.keystore`)
3. Set a strong password, alias (e.g. `bigmoneyslots`), and alias password
4. Enable **Custom Keystore** and point to the file

> ⚠️ **Back up your keystore and passwords. You cannot update the app on Google Play without the original keystore.**

### Step 4 — Build the AAB
1. **File → Build Settings → Android → Switch Platform** *(one-time)*
2. Tick ✅ **Build App Bundle (.aab)** — Google Play requires `.aab`, not `.apk`
3. Untick **Development Build** before a release build
4. Click **Build** → produces `BigMoneySlots.aab`

---

## How to Run the Backend

### Option A — Docker Compose *(recommended)*

```bash
cd Backend/

# 1. Copy env file and fill in your secrets
cp .env.example .env
# Edit .env — set MONGO_URI, JWT_SECRET, payment keys, etc.

# 2. Start server + MongoDB
docker-compose up -d

# Health check
curl http://localhost:3000/health
```

To stop: `docker-compose down`

### Option B — Local Node.js *(development)*

```bash
cd Backend/
npm install
cp .env.example .env   # fill in secrets

# Ensure MongoDB is running locally on port 27017

npm run dev    # auto-restart on changes (nodemon)
# or
npm start
```

### Required Environment Variables

Copy `Backend/.env.example` → `Backend/.env` and fill in every value:

| Variable | Description |
|----------|-------------|
| `JWT_SECRET` | Long random string — signs all player tokens |
| `MONGO_URI` | MongoDB connection string |
| `STRIPE_SECRET_KEY` | From [dashboard.stripe.com → API Keys](https://dashboard.stripe.com/apikeys) |
| `STRIPE_WEBHOOK_SECRET` | Stripe webhook endpoint signing secret |
| `STRIPE_PUBLISHABLE_KEY` | Stripe publishable key (used in checkout HTML) |
| `PAYPAL_CLIENT_ID` | From [developer.paypal.com](https://developer.paypal.com) |
| `PAYPAL_CLIENT_SECRET` | PayPal app secret |
| `GOOGLE_APPLICATION_CREDENTIALS` | Path to Google service-account JSON key |
| `GOOGLE_PLAY_PACKAGE_NAME` | e.g. `com.yourstudio.bigmoneyslots` |
| `ADMIN_SECRET` | Long random string — protects `/api/security/*` endpoints |
| `ALLOWED_ORIGIN` | Your client origin for CORS (e.g. `https://yourdomain.com`) |

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/register` | Register or log in by device ID |
| GET | `/api/auth/me` | Get current player profile |
| PATCH | `/api/auth/display-name` | Update display name |
| GET | `/api/coins/balance` | Get coin/gem/spin balance |
| GET | `/api/coins/transactions` | Transaction history |
| POST | `/api/coins/sync` | Sync economy (server is authoritative) |
| POST | `/api/payments/google-play/verify` | Verify & fulfil a Google Play IAP |
| POST | `/api/payments/paypal/create-order` | Create a PayPal order |
| POST | `/api/payments/paypal/capture` | Capture a PayPal order |
| GET | `/api/tournament/current` | Active tournament |
| POST | `/api/tournament/join` | Join the active tournament |
| POST | `/api/tournament/score` | Submit a spin score |
| GET | `/api/tournament/leaderboard` | Tournament rankings |
| GET | `/api/tournament/history` | Past tournaments |
| GET | `/api/friends` | Friends list + pending requests |
| GET | `/api/friends/search?q=` | Search players by name |
| POST | `/api/friends/request` | Send a friend request |
| POST | `/api/friends/accept` | Accept a friend request |
| POST | `/api/friends/decline` | Decline a friend request |
| POST | `/api/friends/remove` | Remove a friend |
| GET | `/api/chat/history` | Load chat messages |
| POST | `/api/chat/send` | Send a chat message |
| GET | `/api/chat/dm/threads` | DM thread list |
| GET | `/api/chat/dm/thread/:friendId` | Full conversation with a friend |
| GET | `/api/invite/code` | Get personal invite code |
| POST | `/api/invite/redeem` | Redeem an invite code for gems |
| GET | `/api/invite/stats` | Invite statistics |
| GET | `/api/account/links` | Linked social accounts |
| POST | `/api/account/link` | Link a social account |
| DELETE | `/api/account/link/:provider` | Unlink a social account |
| GET | `/health` | Server health check |

---

## Running the Backend Tests

```bash
cd Backend/
npm test
```

Tests use **Jest** + **Supertest** + **mongodb-memory-server** (in-memory MongoDB — no real DB needed).

> **Note:** `mongodb-memory-server` downloads a MongoDB binary on first run. This requires internet access to `fastdl.mongodb.org`. In a network-restricted CI environment, pre-download the binary or use `MONGOMS_PREFER_GLOBAL_PATH=1` with a pre-installed MongoDB.

**Test coverage:**

| File | Tests | Areas covered |
|------|-------|--------------|
| `auth.test.js` | 8 | Register, duplicate register, /me, display-name update |
| `coins.test.js` | 5 | Balance, transactions, sync (server-authoritative) |
| `tournament.test.js` | 8 | Current, join, duplicate join, score, leaderboard, history |
| `friends.test.js` | 11 | Search, request, self-request, duplicate, accept, decline, remove, list |
| `invite.test.js` | 7 | Code generation, idempotency, redeem, own-code guard, double-redeem, invalid code |
| `chat.test.js` | 9 | Send, empty text, truncation, history, DM thread, DM threads, voice join/leave |
| `accountLinking.test.js` | 9 | List, link, bonus grant, no double bonus, invalid provider, conflict, unlink |
| **Total** | **57** | |

---

## Google Play Store Submission Checklist

Work through each phase in order inside the [Google Play Console](https://play.google.com/console).

---

### Phase 1 — Build & Sign

- [ ] Unity Player Settings configured (package name, IL2CPP, ARMv7+ARM64, API 34)
- [ ] Keystore created and **backed up securely**
- [ ] `.aab` built with Development Build **OFF**
- [ ] Game tested on a real Android device — launches and spins without errors
- [ ] `Bundle Version Code` = `1`, `Version` = `1.0`

---

### Phase 2 — Google Play Developer Account

- [ ] Account created at [play.google.com/console](https://play.google.com/console) (one-time $25 USD fee)
- [ ] Identity verification completed
- [ ] Developer Distribution Agreement accepted

---

### Phase 3 — Create the App

- [ ] Click **Create app**
- [ ] App name: `Big Money Slots`
- [ ] Language: `English (United States)`
- [ ] Type: **Game** | Free | In-app purchases
- [ ] Accept all declarations

---

### Phase 4 — Store Listing

- [ ] Short description (max 80 chars)
- [ ] Full description (max 4000 chars)
- [ ] App icon — 512×512 PNG
- [ ] Feature graphic — 1024×500 PNG/JPG
- [ ] Phone screenshots — minimum 2, maximum 8
- [ ] Tablet screenshots — optional

---

### Phase 5 — Content Rating

- [ ] Complete IARC questionnaire → category: **Gambling simulation / Casino**
- [ ] Submit — ratings issued automatically (expect **17+ / PEGI 18**)

---

### Phase 6 — App Content & Policy Declarations

- [ ] Privacy policy published at a public URL
- [ ] Ads: **No**
- [ ] In-app purchases: **Yes**
- [ ] Target audience: **18+**
- [ ] Data safety form completed (device ID, purchase history, encrypted in transit)

---

### Phase 7 — In-App Products

- [ ] Go to **Monetise → In-app products** and create:

| Product ID | Name | Price |
|-----------|------|-------|
| `welcomeofferpack` | Welcome Offer Pack | $4.99 |
| `gems_100` | 100 Gems | $0.99 |
| `gems_500` | 500 Gems | $4.99 |
| `gems_1200` | 1200 Gems | $9.99 |
| `gems_2500` | 2500 Gems | $19.99 |
| `gems_6500` | 6500 Gems | $49.99 |

- [ ] All products set to **Active**

---

### Phase 8 — Backend Deployment

- [ ] Google service-account JSON key created with Android Publisher API access
- [ ] `GOOGLE_APPLICATION_CREDENTIALS` set in `.env`
- [ ] `GOOGLE_PLAY_PACKAGE_NAME` set in `.env`
- [ ] All other `.env` secrets filled in
- [ ] Backend deployed to a public **HTTPS** server (`docker-compose up -d`)
- [ ] `curl https://yourserver.com/health` returns `{"status":"ok"}`
- [ ] `ALLOWED_ORIGIN` set to your game's public domain

---

### Phase 9 — Upload & Internal Test

- [ ] **Testing → Internal testing → Create new release**
- [ ] Upload `BigMoneySlots.aab`
- [ ] Add release notes
- [ ] Add yourself as tester and install via opt-in link
- [ ] Confirm game launches, spins, and purchases work on a real device

---

### Phase 10 — Production Release

- [ ] Increment `Bundle Version Code` for each new build
- [ ] All Dashboard sections show green ticks
- [ ] Promote to Production → **Send for review**
- [ ] Monitor for policy violations (response typically 1–3 business days)

---

### Ongoing After Launch

- [ ] Monitor Android vitals (crashes, ANRs) in Play Console
- [ ] Respond to user reviews
- [ ] Keep Target API Level current (Google mandates updates annually)
- [ ] Renew payment provider credentials before expiry

---

## Functionality Checklist — Is It Ready?

Use this to verify each system is wired up before submitting.

### Unity Client
- [ ] **MainScene loads** — `GameManager`, `PlayerEconomy`, `SlotMachine` all present in scene
- [ ] **Spin works** — tap Spin button deducts bet, reels animate, win calculated and displayed
- [ ] **PlayerPrefs persist** — coins survive app restart
- [ ] **BackendClient** — `BACKEND_URL` constant updated to your deployed server URL
- [ ] **IAP** — `IAPHandler` initialised; `welcomeofferpack` product ID matches Play Console
- [ ] **Welcome Offer** — appears on first launch only (`welcomeOfferPurchased` flag)
- [ ] **Daily Challenges** — reset at midnight, progress tracked correctly
- [ ] **Progressive Jackpot** — seed value set, trigger condition tested
- [ ] **Social** — Friends, Chat, DM, Voice UI panels all have their GameObject references assigned in Inspector
- [ ] **Account Linking** — Facebook/Google/Discord/Phone buttons assigned in `AccountLinkingUI`
- [ ] **Invite** — `InviteUI` shows generated code and handles redemption result

### Backend
- [ ] All `.env` variables set (no placeholder values)
- [ ] MongoDB accessible and `mongoose.connect()` succeeds on startup
- [ ] `JWT_SECRET` is a strong random value (not `changeme`)
- [ ] Stripe webhook endpoint registered and `STRIPE_WEBHOOK_SECRET` matches
- [ ] Google Play Billing service account has **Android Publisher** API enabled
- [ ] PayPal app is in **Live** mode (not Sandbox) for production
- [ ] `ADMIN_SECRET` is set to a strong value and not shared publicly
- [ ] HTTPS / TLS termination in place (via reverse proxy or load balancer)
- [ ] Rate limiting active (already configured — 120 req/15min per IP)
- [ ] `npm test` passes all 57 tests in your CI environment
