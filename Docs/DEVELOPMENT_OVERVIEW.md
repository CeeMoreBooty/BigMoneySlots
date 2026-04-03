# Big Money Slots — Development Overview

A full account of how the application was designed and built: the methods and strategies used at every layer, what every major system does, and why each decision was made.

---

## Table of Contents

1. [Project Vision & Architecture Strategy](#1-project-vision--architecture-strategy)
2. [Technology Stack](#2-technology-stack)
3. [Unity Client — How It Was Built](#3-unity-client--how-it-was-built)
   - 3.1 [Core Architecture Pattern — Singletons + Events](#31-core-architecture-pattern--singletons--events)
   - 3.2 [PlayerEconomy System](#32-playereconomy-system)
   - 3.3 [Slot Engine System](#33-slot-engine-system)
   - 3.4 [Progressive Jackpot System](#34-progressive-jackpot-system)
   - 3.5 [Loyalty & Retention Systems](#35-loyalty--retention-systems)
   - 3.6 [Daily Challenges System](#36-daily-challenges-system)
   - 3.7 [Social Systems — Chat, Friends, Account Linking](#37-social-systems--chat-friends-account-linking)
   - 3.8 [Welcome Offer & IAP System](#38-welcome-offer--iap-system)
   - 3.9 [UI Architecture](#39-ui-architecture)
4. [Backend — How It Was Built](#4-backend--how-it-was-built)
   - 4.1 [Framework & App Factory Strategy](#41-framework--app-factory-strategy)
   - 4.2 [Authentication System](#42-authentication-system)
   - 4.3 [Coin Economy System](#43-coin-economy-system)
   - 4.4 [Payment System](#44-payment-system)
   - 4.5 [Tournament System](#45-tournament-system)
   - 4.6 [Chat & Messaging System](#46-chat--messaging-system)
   - 4.7 [Friends System](#47-friends-system)
   - 4.8 [Account Linking System](#48-account-linking-system)
   - 4.9 [Invite & Referral System](#49-invite--referral-system)
   - 4.10 [Security System](#410-security-system)
   - 4.11 [Real-Time Layer — Socket.IO](#411-real-time-layer--socketio)
5. [Database Strategy — MongoDB](#5-database-strategy--mongodb)
6. [DevOps & Deployment Strategy](#6-devops--deployment-strategy)
7. [Testing Strategy](#7-testing-strategy)
8. [Google Play Release Strategy](#8-google-play-release-strategy)
9. [Key Design Decisions & Why](#9-key-design-decisions--why)

---

## 1. Project Vision & Architecture Strategy

Big Money Slots is a **free-to-play mobile casino-style slots game** for Android. The core concept is an entirely virtual-coin economy — players never wager real money on outcomes, but they can purchase coin packs, gems, and spin bundles via real in-app purchases.

The architecture was split into two completely separate layers from day one:

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Unity Client** | C# / Unity 2022.3 LTS | Game loop, slot engine, UI, local persistence |
| **Node.js Backend** | Express + MongoDB + Socket.IO | Authentication, economy sync, payments, social features, security |

**Why two separate layers?**  
A purely client-side game can be trivially cheated — any memory editor can set the coin balance. By keeping the server as the authoritative source of truth and verifying all purchases server-side, player balances and purchases cannot be fabricated. The client handles the real-time game feel; the server handles trust.

---

## 2. Technology Stack

### Client
- **Unity 2022.3.20f1 LTS** — Stable long-term-support version, best Android build toolchain support
- **C# (.NET Standard 2.1)** — Type-safe, performant scripting language
- **IL2CPP** — AOT compilation for Android, faster runtime and harder to reverse-engineer than Mono
- **UnityEngine.PlayerPrefs** — Local persistence for economy state, settings, and session data
- **System.Security.Cryptography.RandomNumberGenerator** — Cryptographically secure RNG for all gambling outcomes
- **Unity IAP** — In-app purchase abstraction layer for Google Play Billing

### Backend
- **Node.js + Express** — Lightweight, non-blocking I/O well-suited to a high-concurrency game backend
- **MongoDB + Mongoose** — Flexible document store; suits game data (varied player states, event logs) better than rigid relational tables
- **Socket.IO** — Real-time bidirectional communication for live chat and admin alerts
- **JSON Web Tokens (JWT)** — Stateless authentication; no session store needed, scales horizontally
- **bcryptjs** — Password hashing for any future credential-based login flows
- **Stripe SDK** — Hosted checkout page payments for web-based purchase flow
- **googleapis** — Google Play Developer API for server-side receipt verification
- **Docker + Docker Compose** — Containerised deployment; same environment from dev to production

### Infrastructure
- **GitHub Actions** — CI/CD: Unity Android builds and automated backend deployment
- **GHCR (GitHub Container Registry)** — Docker image hosting for the backend
- **MongoDB (self-hosted or Atlas)** — Primary data store

---

## 3. Unity Client — How It Was Built

### 3.1 Core Architecture Pattern — Singletons + Events

Every major game system is a **MonoBehaviour singleton** that persists across scene loads via `DontDestroyOnLoad`. This means:

- There is exactly one `GameManager`, one `PlayerEconomy`, one `SlotMachine`, one `ProgressiveJackpot`, one `LoyaltySystem`, etc. in memory at all times.
- Systems communicate via **C# events** (`static event Action<...>`) rather than direct method calls where possible. For example, `ProgressiveJackpot.OnJackpotWon` fires whenever a jackpot is won; every interested system (UI, DailyChallenges, LoyaltySystem) subscribes independently without the jackpot system needing to know about them.

This pattern was chosen because:
- It avoids tight coupling between systems
- It makes adding new features easy — subscribe to an existing event rather than editing the source system
- It prevents race conditions from multiple instances of the same manager being created during scene transitions

### 3.2 PlayerEconomy System

**File:** `Assets/Scripts/Core/PlayerEconomy.cs`

The economy system manages all in-game currencies and cosmetics:

| Currency | Type | Notes |
|----------|------|-------|
| Coins | `long` | Must be `long` (not `int`) — balances can exceed 2.1 billion |
| Free Spins | `int` | Standard free spin type |
| Super Spins | `int` | Premium spin with higher rewards |
| Ultra Spins | `int` | Rarest spin type |
| Gems | `int` | Secondary currency for cosmetics and boosts |
| Win Multiplier | `float` | Temporary timed boost applied to all wins |

**Strategy — Coins as `long`:**  
The Loyalty System awards up to 15,000,000,000 coins at milestone 50,000 spins. The Welcome Offer pack grants 150,000,000,000 coins. An `int` (max ~2.1B) would overflow silently and corrupt balances. All coin arithmetic uses `long` throughout the codebase.

**Strategy — PlayerPrefs for persistence:**  
Rather than requiring a network call on every balance change, the economy is persisted locally to `PlayerPrefs` immediately after every mutation. Coin values are stored as strings (`PlayerPrefs.SetString`) because `PlayerPrefs.SetInt` is limited to a 32-bit integer. When the backend is available, `SyncWithBackend()` overwrites the local state with the server's authoritative value.

**Strategy — Defensive spend checking:**  
`SpendCoins(long amount)` returns `bool` — it checks balance before deducting and refuses if insufficient. The `SlotMachine` refunds the bet if a reel returns `null` (corrupted state), preventing balance loss from bugs.

### 3.3 Slot Engine System

**Files:** `Assets/Scripts/SlotEngine/`

The slot engine is built from composable components:

```
SlotMachine
  ├── Reel × 3         — each reel holds a symbol strip and spins independently
  ├── PayoutTable       — maps symbol combos to payout multipliers
  ├── SlotGameConfig    — data container: RTP, bet limits, jackpot config, reel skin
  ├── SlotGameLoader    — loads and swaps the active config at runtime
  ├── SlotGameRegistry  — ScriptableObject holding all available game configs
  └── SecureRandom      — cryptographic RNG
```

**Strategy — SecureRandom instead of Unity's Random:**  
`UnityEngine.Random` uses a pseudo-random algorithm with a predictable seed. A sufficiently motivated player can reverse-engineer the seed and predict spin outcomes. `SecureRandom` uses `System.Security.Cryptography.RandomNumberGenerator` — a hardware-seeded CSPRNG. Rejection sampling eliminates modulo bias. This is the standard requirement for any gambling-adjacent application.

**Strategy — RTP enforcement in `CalculatePayout`:**  
The Return-to-Player (RTP) percentage (default 70%) is enforced at the payout calculation step. After calculating the raw symbol match payout, a second `SecureRandom.Value()` roll is compared to the game's `baseRTP` value. If the roll exceeds RTP, the win is zeroed out even though the symbols matched. This ensures the house edge is preserved mathematically without rigging the visible reel outcomes — the symbols spin fairly, but not every match pays. The RTP value is configurable per `SlotGameConfig`.

**Strategy — ScriptableObject data containers:**  
All game configuration (payout tables, symbol definitions, game configs) lives in Unity ScriptableObjects rather than hard-coded values. This means game designers can adjust RTP, bet limits, payout multipliers, and jackpot seeds in the Unity Inspector without touching code. Three complete game variants ship: Classic Slots, Jungle Jackpot, and Penny Paradise.

### 3.4 Progressive Jackpot System

**File:** `Assets/Scripts/SlotEngine/ProgressiveJackpot.cs`

Five jackpot tiers exist simultaneously, each with its own pool:

| Tier | Base Pool | Contribution/Spin | Fire Chance Range |
|------|-----------|-------------------|-------------------|
| Mini | 50,000 | 0.4% of bet | 8% – 18% |
| Minor | 500,000 | 0.6% of bet | 2.5% – 7% |
| Major | 5,000,000 | 0.8% of bet | 0.6% – 2.2% |
| Grand | 50,000,000 | 1.0% of bet | 0.15% – 0.6% |
| Mega | 500,000,000 | 1.2% of bet | 0.035% – 0.18% |

**Strategy — Dynamic win probability:**  
As each jackpot pool grows, its trigger chance increases linearly from `baseChance` to `softCapChance`. A larger pool fires more frequently, creating a natural feedback loop: players see the jackpot growing, get more excited, and when it gets very large, it also becomes more likely to pay out. This keeps players engaged without the jackpot growing indefinitely.

Each spin contributes a percentage of the bet to all five tier pools simultaneously. On each spin, tiers are evaluated highest-to-lowest (Mega first) — at most one jackpot fires per spin. When a tier fires, its pool resets to `basePool`.

Jackpot pools are persisted to `PlayerPrefs` and survive app restarts.

### 3.5 Loyalty & Retention Systems

**File:** `Assets/Scripts/Rewards/LoyaltySystem.cs`

The loyalty system tracks **lifetime spins** and unlocks rewards at two levels:

**Tier progression (5 tiers based on lifetime spins):**

| Tier | Spins Required | Win Multiplier | Daily Coin Bonus |
|------|---------------|----------------|-----------------|
| Trail Wanderer | 0 | 1× | — |
| Prairie Keeper | 1,000 | 1.1× | 10M coins |
| Guardian Scout | 5,000 | 1.25× | 50M coins |
| Spirit Warrior | 15,000 | 1.5× | 200M coins |
| Great Plains Chief | 50,000 | 2× | 1B coins |

**Spin milestones (one-time dumps at specific spin counts):**  
At 100, 500, 1,000, 2,500, 5,000, 10,000, 25,000, and 50,000 lifetime spins, the player receives a large burst of coins, free spins, and super spins. The 50,000-spin milestone awards 15B coins.

**Timed session rewards:**  
Every 30 minutes of continuous play, the player receives 50M coins + 10 free spins automatically.

**Comeback bonus:**  
If a player returns after 24+ hours away, they receive 200M coins + 20 free spins + 2 super spins. Granted once per calendar day.

**Strategy — Retention mechanics:**  
These systems are designed to give players a reason to play every day (daily tier bonus), reward long sessions (session reward), and pull back lapsed players (comeback bonus). The tier multiplier on wins means loyal players objectively get better outcomes, which reinforces continued play.

### 3.6 Daily Challenges System

**File:** `Assets/Scripts/Rewards/DailyChallenges.cs`

Three challenges are generated each day using the UTC date as a deterministic seed. The same seed produces the same three challenges for all players on the same day — ensuring fairness.

Challenge types: Spin N times, land a big win, use free spins, enter a tournament, win N coins total, play N different slot games.

**Strategy — Date-seeded determinism:**  
Using `new System.Random(dateKey.GetHashCode())` means challenges can be regenerated identically from just the date string. No separate server call needed; no possibility of a player getting different challenges than intended.

**Strategy — `long` progress tracking:**  
The "Win 5B coins today" challenge requires tracking a cumulative coin amount that exceeds `int.MaxValue` (~2.1B). Progress is stored as `long` throughout.

### 3.7 Social Systems — Chat, Friends, Account Linking

**Files:** `Assets/Scripts/Social/`

**ChatManager:**  
Supports three channels — Global (all players), Room (current game), and Friends (direct messages). Primary transport is Socket.IO; a REST POST endpoint is used as a fallback when the socket connection is unavailable. Incoming messages are stored in a fixed-size history buffer (100 messages per channel), oldest messages trimmed when the buffer fills. All outgoing text is sanitised (HTML tags stripped, 200-character limit enforced client-side).

**FriendsManager:**  
Polls the `/api/friends` endpoint every 30 seconds to keep the friends list current. Supports send/accept/decline friend requests and removal. An event system (`OnFriendsUpdated`, `OnFriendRequestReceived`, `OnFriendAccepted`) drives UI updates without polling from UI code.

**AccountLinking:**  
Allows one player account to be associated with Google, Facebook, Discord, or phone number. Each provider link is verified server-side. A one-time bonus coin reward is granted per unique provider linked. This encourages players to link accounts (improving retention through cross-device continuity) while providing a direct reward incentive.

### 3.8 Welcome Offer & IAP System

**Files:** `Assets/Scripts/WelcomeOffer/`

The Welcome Offer is the primary monetisation entry point — a one-time purchase shown to new players. It uses Unity IAP (`IAPHandler.cs`) to interface with Google Play Billing.

**Flow:**
1. `WelcomeOfferManager` detects first launch and shows `WelcomeOfferUI`
2. Player taps "Buy" — `IAPHandler.PurchaseWelcomeOffer()` triggers Google Play checkout
3. On success, Unity IAP calls `ProcessPurchase` with the receipt
4. Receipt is sent to `BackendClient.VerifyPurchase()` → backend calls Google Play Developer API
5. Only after backend confirmation does the reward get granted to `PlayerEconomy`
6. The `welcomeOfferPurchased` flag is stored — the offer never appears again

**Strategy — Server-side receipt verification:**  
Never grant rewards based solely on the client receiving a purchase success callback. A malicious client can fake that callback. The Google Play receipt must be verified against the real Google Play Developer API on the server before any reward is granted.

### 3.9 UI Architecture

**Files:** `Assets/Scripts/UI/`

All UI panels exist in the single `MainScene` and are shown/hidden via `SetActive(true/false)` — no scene loading for any panel. This avoids load times and keeps the game state alive while panels open and close.

The Canvas is set to **Scale With Screen Size** (reference 1080×1920) to handle the wide variety of Android screen dimensions and aspect ratios.

Key UI components:
- **SlotUI** — spin button, coin display, bet controls, win display, free-spin counter
- **JackpotUI** — animated counter showing live Mega jackpot pool value
- **ChatUI** — tabbed channel view with message input
- **FriendsUI** — friend list with accept/decline/remove actions
- **DirectMessageUI** — per-friend conversation thread
- **InviteUI** — invite code display and redemption form
- **AccountLinkingUI** — per-provider link/unlink buttons with bonus status
- **WelcomeOfferUI** — purchase popup for new players

---

## 4. Backend — How It Was Built

### 4.1 Framework & App Factory Strategy

**Files:** `Backend/app.js`, `Backend/server.js`

The Express app is created by an `createApp()` factory function exported from `app.js`. The actual server setup (Socket.IO, DB connection, port binding) lives in `server.js`.

**Why an app factory?**  
Tests import `createApp({ disableRateLimit: true })` without triggering `connectDB()` or `server.listen()`. This means the entire REST API can be integration-tested in-process using `supertest` against a MongoDB Memory Server — no real network ports, no real database, no test pollution.

Middleware layers are applied in a deliberate order in every request:

1. `express.json({ limit: '10kb' })` — parse body, reject oversized payloads immediately
2. Rate limiter (120 req / 15 min per IP) — before any DB access to prevent flooding
3. `ipLogger` — log every request IP for security audit
4. `securityGuard` — full threat detection pass (see Section 4.10)
5. Route handlers

### 4.2 Authentication System

**File:** `Backend/routes/auth.js`

Players authenticate using a **device ID** on first launch — no username/password required. The device ID (generated by Unity and stored in `PlayerPrefs`) uniquely identifies the installation. On first call, a new `Player` document is created and a JWT is returned. On subsequent calls, the existing player is found and a fresh JWT issued.

JWT tokens are signed with `JWT_SECRET` (from environment), expire after 30 days, and contain the player's MongoDB `_id`. Every protected route uses the `auth` middleware to verify the token and attach `req.player` before the route handler runs.

**Strategy — Device ID auth with no password:**  
A casino-style social game with virtual currency does not need email/password auth — that friction would reduce installs. Device ID auth is frictionless while still being unique per installation. Account linking (Google, Facebook, etc.) provides the cross-device continuity players need if they change phones.

### 4.3 Coin Economy System

**File:** `Backend/routes/coins.js`

The backend stores the authoritative coin balance for each player. The Unity client syncs after every significant event. The server's balance always wins in any conflict with the client's local `PlayerPrefs` value.

Coins are stored as a MongoDB `Number` (64-bit float in JS, sufficient for values up to 2^53 which is ~9 quadrillion — well above any achievable game balance). The `BigInt` type is used only for the Welcome Offer product definition constant (`150_000_000_000n`) as a readability aid.

### 4.4 Payment System

**File:** `Backend/routes/payments.js`  
**Services:** `Backend/services/googlePlayVerifier.js`, `Backend/services/paypalService.js`, `Backend/services/stripeService.js`

Three payment providers are supported:

| Provider | Use Case | Verification Method |
|----------|---------|---------------------|
| Google Play | Primary Android IAP | `googleapis` — calls Google Play Developer API with service account credentials |
| PayPal | Alternative web payment | REST order create → player approves in browser → capture on return |
| Stripe | Web-based checkout | Hosted Stripe checkout page served from `Backend/public/stripe-checkout.html`; webhook confirms payment |

**Strategy — Duplicate purchase prevention:**  
Every Google Play purchase token is stored in a `Transaction` document before granting rewards. If the same token arrives again (replay attack, network retry), `Transaction.findOne({ purchaseToken })` returns the existing record and a 409 Conflict is returned. A unique index on `purchaseToken` provides a second layer of protection against race conditions.

**In-app product catalog:**  
The PRODUCTS map in `payments.js` is the single authoritative source of all purchasable items and their rewards. Neither the client nor any other backend file should define product contents independently.

### 4.5 Tournament System

**File:** `Backend/routes/tournament.js`  
**Service:** `Backend/services/tournamentScheduler.js`

Tournaments run on a **30-minute cycle** managed entirely by the server — no client involvement in scheduling. On startup, `tournamentScheduler.start()` immediately runs `tick()` and then sets a 30-minute interval.

Each `tick()`:
1. Finds all active tournaments whose `endTime` has passed
2. Scores and ranks all entries; credits coin prizes to top-20 finishers via `Player.findByIdAndUpdate($inc)`
3. Creates a new tournament if none is currently active

Each tournament selects one of 40 themed slot games at random. Prize tiers are embedded in the `Tournament` document at creation time.

**Strategy — Server-driven scheduling:**  
If tournament timing were client-driven, clients could submit scores outside the valid window or manipulate timing. Server-side `setInterval` ensures the tournament lifecycle is fully authoritative.

### 4.6 Chat & Messaging System

**File:** `Backend/routes/chat.js`

Chat supports:
- **Global channel** — all connected players
- **Room channel** — players in the same tournament/game room
- **Direct Messages** — private per-pair threads stored in the `ChatMessage` collection

Real-time delivery uses Socket.IO rooms. `POST /api/chat/send` saves the message and emits it to the appropriate Socket.IO room (`global`, `room:<id>`, or `player:<id>` for DMs). `GET /api/chat/history` allows clients to load recent messages on channel open.

### 4.7 Friends System

**File:** `Backend/routes/friends.js`

Friendship is modelled as a `Friendship` document with a `status` field: `pending`, `accepted`, or `declined`. This avoids two-document duplication and makes querying simple.

Operations supported: search players by name, send request, accept, decline, remove. The requester's perspective (`isOutgoing`) vs. recipient's perspective (`isPending`) is derived from the document's `requesterId` field at query time.

### 4.8 Account Linking System

**File:** `Backend/routes/accountLinking.js`

Links a player account to an external provider identity. The `LinkedAccount` document stores `playerId`, `provider`, and `providerUserId`. A unique compound index on `(provider, providerUserId)` prevents two player accounts from claiming the same provider identity.

A **one-time coin bonus** is granted when a new provider is linked (amount varies by provider). The bonus is not granted if the same player links the same provider again (idempotent).

### 4.9 Invite & Referral System

**File:** `Backend/routes/invite.js`

Each player has a unique invite code stored in their `Player` document. When a new player redeems a code, both the referrer and the new player receive a gem reward. The `InviteReward` document records which players have redeemed which codes, preventing double-redemption.

### 4.10 Security System

**Files:** `Backend/middleware/securityGuard.js`, `Backend/services/securityService.js`, `Backend/routes/security.js`  
**Models:** `Backend/models/SecurityEvent.js`, `Backend/models/BannedEntity.js`

The security system is the most complex part of the backend. It runs on **every single API request** via the `securityGuard` middleware, and operates at two levels:

**Real-time in-memory detection (fast path):**  
Sliding-window counters stored in `Map` objects track brute-force auth attempts (per IP) and request rates (per IP per endpoint). These checks are O(1) and add negligible latency.

**Detection capabilities:**

| Threat | Method | Severity | Auto-Ban Threshold |
|--------|--------|----------|-------------------|
| Brute-force login | Sliding window per IP | high → critical | 20 attempts / 10 min |
| Request rate abuse | Sliding window per IP+endpoint | high → critical | 120 req / 1 min |
| JWT multi-IP reuse | Set of IPs per token hash | high | > 3 distinct IPs |
| Injection attacks | Regex on request body | critical | Immediate |
| Known exploit tools | User-agent regex | high | Immediate |
| Coin manipulation | Delta sanity check | critical | Immediate |
| Duplicate purchase replay | Token lookup in Transaction | 409 response | — |
| Payload size abuse | Content-Length header | medium | Soft flag |

**Auto-ban:**  
A critical event triggers an immediate ban. Five or more high-severity events from the same IP within 30 minutes also trigger an auto-ban. Bans are stored in `BannedEntity` and can be IP bans, player bans, or device bans. Temporary bans include an `expiresAt` timestamp; the guard middleware expires lapsed bans on every request.

**Admin alerts:**  
High and critical events emit a `security_alert` Socket.IO event to the `admin_room` channel. A dashboard listening on this channel sees threats in real time.

**Webhook forwarding:**  
Every security event and ban is forwarded to a configurable company server URL (`SECURITY_WEBHOOK_URL`) using HMAC-SHA256 signed payloads. The receiving server can verify authenticity using the shared `SECURITY_WEBHOOK_SECRET`.

**Strategy — Defence in depth:**  
Security is layered: rate limiting at the Express middleware level catches volumetric abuse before it reaches the database; `securityGuard` catches sophisticated attacks; route-level auth middleware blocks unauthorised access; the `adminOnly` middleware restricts sensitive admin routes to requests carrying the `ADMIN_SECRET` header.

### 4.11 Real-Time Layer — Socket.IO

**File:** `Backend/server.js`

A Socket.IO server runs on the same HTTP server as Express. Clients join named rooms on connection:
- `join_room <roomId>` — enters a tournament room to receive game-specific chat
- `join_player_room <playerId>` — enters `player:<id>` for personal notifications (DMs, friend requests, jackpot alerts)
- `chat_message` — clients can emit directly to broadcast to a room (global or game room)

**Strategy — Shared HTTP server:**  
Socket.IO runs on the same `http.Server` instance as Express. This means one port, one TLS certificate, and no CORS complexity between the REST and WebSocket APIs.

---

## 5. Database Strategy — MongoDB

MongoDB was chosen over a relational database for several reasons:

1. **Flexible player documents** — Player state (coins, spins, badges, settings) evolves as features are added. A JSON document does not require migration scripts when new fields are added.
2. **Event log collections** — `SecurityEvent`, `IpLog`, and `Transaction` are write-mostly audit logs with no relational joins needed. Document storage is a natural fit.
3. **Speed of development** — Mongoose ODM with JavaScript provides a fast feedback loop compared to SQL migrations + ORM setup.

**Collections:**

| Collection | Purpose |
|-----------|---------|
| `players` | One document per player — coins, spins, gems, device ID, invite code |
| `transactions` | Every coin credit/debit with type, amount, and reference |
| `tournaments` | One document per tournament — game, times, prize tiers, status |
| `tournamententries` | One document per player per tournament — score, rank, prize |
| `chatmessages` | All chat messages — indexed by channel + timestamp |
| `friendships` | Friend graph — indexed by requester and recipient |
| `linkedaccounts` | Provider identity links per player |
| `inviterewards` | Records of redeemed invite codes |
| `securityevents` | Threat event log — indexed by IP, player, event type |
| `bannedentities` | Active bans — indexed by type + value |
| `iplogs` | Raw request IP log for fraud investigation |

---

## 6. DevOps & Deployment Strategy

**CI/CD — GitHub Actions:**  
Two workflows automate the release pipeline:

- **`build-android.yml`** — On push to `main`, uses `game-ci/unity-builder@v4` to build the Android APK/AAB in a headless Unity container. The built artifact is uploaded for download. Requires `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` secrets, and optionally Android keystore secrets.
- **`deploy-backend.yml`** — On push to `main` when any `Backend/**` file changes, builds a Docker image and pushes it to GHCR. Optionally SSH-deploys the new image to a production server using `DEPLOY_HOST`, `DEPLOY_USER`, `DEPLOY_KEY` secrets.

**Containerisation:**  
`Backend/Dockerfile` builds a minimal Node.js production image. `Backend/docker-compose.yml` defines both the `backend` service and a `mongo` service for local development, with a named volume for MongoDB data persistence. The same `docker-compose.yml` can be used on a VPS for production deployment.

---

## 7. Testing Strategy

**Framework:** Jest + Supertest + mongodb-memory-server

All backend tests run in-process against an in-memory MongoDB instance — no real database, no real ports, no external dependencies. The `tests/__helpers__/setup.js` file bootstraps and tears down the memory server around the test suite.

Tests are structured per feature route:

| Test File | What It Tests |
|-----------|-------------|
| `auth.test.js` | Device registration, duplicate handling, JWT issuance |
| `coins.test.js` | Coin balance sync, add/spend operations |
| `friends.test.js` | Request, accept, decline, remove, search |
| `chat.test.js` | Send message, fetch history, DM threads |
| `tournament.test.js` | Create, enter, score, end, prize distribution |
| `invite.test.js` | Code generation, redemption, duplicate prevention |
| `accountLinking.test.js` | Link, unlink, bonus grant, conflict detection |

**Strategy — App factory for testability:**  
The `createApp({ disableRateLimit: true })` pattern allows tests to call the API at full speed without hitting rate limits. This was the primary reason for extracting the app factory from `server.js`.

---

## 8. Google Play Release Strategy

The `GooglePlay/` folder documents the full submission process:

1. **Build** — Unity produces an `.aab` (Android App Bundle) via `build-android.yml`. AAB is required by Google Play (smaller, architecture-optimised delivery).
2. **Internal Testing** — First upload goes to the Internal Testing track (available to up to 100 specified testers) for verification before wider release.
3. **Content Rating** — The IARC questionnaire must be completed selecting the "Gambling simulation" category. This results in ESRB and PEGI age ratings being assigned.
4. **Target Audience** — Declared as 18+ due to casino/gambling content.
5. **In-App Products** — All IAP product IDs defined in `routes/payments.js` must be created in the Play Console with matching IDs exactly. The Welcome Offer Pack (`welcomeofferpack`) is the primary monetisation SKU.
6. **Data Safety** — Google requires disclosure of all data collected. The app collects device identifiers and purchase history; both are declared.
7. **Privacy Policy** — Required for any app with gambling content or in-app purchases. Must be hosted at a public URL.

---

## 9. Key Design Decisions & Why

| Decision | Why |
|----------|-----|
| **`long` for all coin values in Unity** | Balances regularly exceed 2.1B (the `int` maximum). Silent overflow would corrupt player data invisibly. |
| **Server-side purchase verification** | Prevents clients from faking purchase receipts to grant themselves coins/gems for free. |
| **`SecureRandom` (CSPRNG) for all gambling outcomes** | A predictable PRNG seed can be reverse-engineered, allowing players to time spins for guaranteed wins. |
| **RTP enforced server-side in `SlotGameConfig`** | Keeps the house edge configurable and separate from the symbol/reel logic. |
| **App factory pattern for Express** | Allows the full API to be integration-tested without spawning a real server or connecting to a real database. |
| **JWT stateless auth (no session store)** | The backend can scale horizontally — any server instance can verify any token without shared session state. |
| **Single `MainScene`** | Avoids load screens between game modes. All panels are shown/hidden in-place. |
| **MongoDB over SQL** | Player state is document-shaped, not relational. Event logs benefit from write-heavy document storage. |
| **Socket.IO on same HTTP server** | One port, one TLS certificate, no separate WebSocket origin configuration. |
| **Security in middleware, not routes** | Threat detection runs on every request including unauthenticated probes, not just logged-in player endpoints. |
| **In-memory sliding windows for rate/brute-force detection** | DB queries for every request would be too slow. In-memory Maps are O(1); the DB is the persistent record, not the hot path. |
| **Deterministic daily challenges** | Using the date as a seed means challenges can be regenerated identically without a server call, while being consistent across all players. |
| **Dynamic jackpot probability (fill-based)** | A jackpot that grows in hit-chance as it grows in value creates a positive feedback loop — big pots are more exciting AND more likely to fire, keeping players engaged without indefinite growth. |
