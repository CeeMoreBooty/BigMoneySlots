# Code Index — Big Money Slots

Complete index of every source file in the repository, grouped by layer.

---

## Unity Client — `Assets/Scripts/`

### Core
| File | Purpose |
|------|---------|
| `Core/GameManager.cs` | Singleton bootstrapper — initialises all systems on startup, wires references, handles app lifecycle (pause/resume for daily bonus check) |
| `Core/PlayerEconomy.cs` | Local coin/gem/spin ledger persisted via `PlayerPrefs`. Seeds 10,000 coins on first launch. Syncs with backend after each spin. |

### Slot Engine
| File | Purpose |
|------|---------|
| `SlotEngine/SlotMachine.cs` | Core spin logic — deducts bet, calls `Reel.Spin()` on each reel, evaluates result against `PayoutTable`, fires win events |
| `SlotEngine/Reel.cs` | Manages one physical reel — holds an array of `SymbolData`, picks a random stop position using `SecureRandom`, drives UI animation |
| `SlotEngine/Symbol.cs` | `SymbolData` ScriptableObject — stores symbol name, sprite, payout multipliers, and flags (Wild, Scatter) |
| `SlotEngine/PayoutTable.cs` | `PayoutTableData` ScriptableObject — maps 3-symbol combinations to coin multipliers |
| `SlotEngine/SecureRandom.cs` | Wraps `System.Security.Cryptography.RNGCryptoServiceProvider` for provably fair reel stops |
| `SlotEngine/ProgressiveJackpot.cs` | Accumulates a jackpot seed from each spin (configurable %). Triggers payout when player hits jackpot combination. Persists via `PlayerPrefs`. |
| `SlotEngine/SlotGameConfig.cs` | `SlotGameConfig` ScriptableObject — defines a slot variant (name, reel skin, symbol set, payout table, min bet) |
| `SlotEngine/SlotGameDatabase.cs` | Static lookup table built from all `SlotGameConfig` assets at runtime |
| `SlotEngine/SlotGameLoader.cs` | MonoBehaviour that loads a `SlotGameConfig` by ID and applies it to the active `SlotMachine` and reels |
| `SlotEngine/SlotGameRegistry.cs` | `SlotGameRegistry` ScriptableObject — master list of all `SlotGameConfig` assets |

### Rewards
| File | Purpose |
|------|---------|
| `Rewards/DailyChallenges.cs` | Generates 3 daily challenge tasks (e.g. "Spin 50 times", "Win 5B coins"). Tracks progress via `PlayerPrefs` strings (supports `long` values). Resets at midnight. |
| `Rewards/GemSystem.cs` | Manages the gem currency — gem balance, spend, earn. Gems used for cosmetic unlocks and invite rewards. |
| `Rewards/InviteRewardManager.cs` | Fetches invite code from backend, displays it, handles redemption flow and resulting gem grant. |
| `Rewards/LoyaltySystem.cs` | Daily login streak — grants escalating coin bonuses. Streak stored in `PlayerPrefs`. |
| `Rewards/RewardManager.cs` | Central dispatcher — shows reward popups (coins, gems, spins) from any system that needs to grant rewards to the player. |

### Social
| File | Purpose |
|------|---------|
| `Social/AccountLinking.cs` | Calls `/api/account/link` for Facebook, Google, Discord, Phone providers. Handles bonus coin grant on first link. |
| `Social/ChatManager.cs` | WebSocket (Socket.IO) listener for incoming chat messages. Calls `/api/chat/send` for outgoing. Manages global and room channels. |
| `Social/FriendsManager.cs` | Fetches friends list, pending requests. Sends/accepts/declines/removes friend requests via `/api/friends/*`. |
| `Social/VoiceChatManager.cs` | Signals backend when player joins/leaves a voice room via `/api/chat/voice/join` and `/api/chat/voice/leave`. |

### UI
| File | Purpose |
|------|---------|
| `UI/SlotUI.cs` | Main HUD — wires Spin button, displays coin balance, bet amount, jackpot value, last result text. Calls `SlotMachine.Spin()`. |
| `UI/ChatUI.cs` | Renders global / room chat messages. Sends messages. Spawns `MessageBubblePrefab` per message. |
| `UI/DirectMessageUI.cs` | Private DM conversation view — loads thread from `/api/chat/dm/thread/:friendId`, sends replies. |
| `UI/FriendsUI.cs` | Friends panel — lists friends and pending requests. Buttons for accept/decline/remove. Uses `FriendRowPrefab`. |
| `UI/InviteUI.cs` | Shows player's invite code, copy-to-clipboard, redemption input field. Displays gem rewards. |
| `UI/JackpotUI.cs` | Animated jackpot ticker — subscribes to `ProgressiveJackpot.OnJackpotChanged` and updates display. |
| `UI/AccountLinkingUI.cs` | Shows linked account status for each provider (Facebook/Google/Discord/Phone). Link/unlink buttons. |

### Welcome Offer / IAP
| File | Purpose |
|------|---------|
| `WelcomeOffer/IAPHandler.cs` | Unity IAP initialisation and purchase callbacks. Sends receipt to backend for verification via `BackendClient`. |
| `WelcomeOffer/WelcomeOfferConfig.cs` | `WelcomeOfferConfig` ScriptableObject — product ID, price display string, reward description. |
| `WelcomeOffer/WelcomeOfferManager.cs` | Controls whether to show the Welcome Offer popup. Only shown once (`welcomeOfferPurchased` flag from backend). |
| `WelcomeOffer/WelcomeOfferUI.cs` | Renders the Welcome Offer popup — description, price, Buy/Close buttons. |

### Backend Client
| File | Purpose |
|------|---------|
| `Backend/BackendClient.cs` | Singleton HTTP client using `UnityWebRequest`. Handles JWT token storage, auth headers, and all REST calls to the backend API. Also manages Socket.IO connection for real-time events. |

---

## Unity Assets

### Scenes
| File | Purpose |
|------|---------|
| `Assets/Scenes/MainScene.unity` | The single game scene — contains `SlotCanvas` (UI), `SlotMachine`, `Reel1/2/3`, `GameManager` |

### ScriptableObjects
| File | Purpose |
|------|---------|
| `ScriptableObjects/Symbols/Symbol_Bell.asset` | Bell symbol — common, medium payout |
| `ScriptableObjects/Symbols/Symbol_Cherry.asset` | Cherry symbol — most common, low payout |
| `ScriptableObjects/Symbols/Symbol_Scatter.asset` | Scatter — triggers free spins |
| `ScriptableObjects/Symbols/Symbol_Seven.asset` | Lucky 7 — rarest, highest payout |
| `ScriptableObjects/Symbols/Symbol_Wild.asset` | Wild — substitutes for any symbol |
| `ScriptableObjects/PayoutTables/PayoutTable_Classic.asset` | Standard 3-reel payout table |
| `ScriptableObjects/PayoutTables/PayoutTable_HighRoller.asset` | High-risk payout table — bigger wins, lower frequency |
| `ScriptableObjects/PayoutTables/PayoutTable_Penny.asset` | Low-stakes payout table |
| `ScriptableObjects/SlotGames/SlotGameConfig_classic_slots.asset` | Classic Slots game variant config |
| `ScriptableObjects/SlotGames/SlotGameConfig_jungle_jackpot.asset` | Jungle Jackpot game variant config |
| `ScriptableObjects/SlotGames/SlotGameConfig_penny_paradise.asset` | Penny Paradise game variant config |
| `ScriptableObjects/SlotGameRegistry.asset` | Master registry of all slot game configs |

### Art
| File | Purpose |
|------|---------|
| `Art/Sprites/Symbols/Bell.png` | Bell symbol sprite |
| `Art/Sprites/Symbols/Cherry.png` | Cherry symbol sprite |
| `Art/Sprites/Symbols/Scatter.png` | Scatter symbol sprite |
| `Art/Sprites/Symbols/Seven.png` | Lucky 7 symbol sprite |
| `Art/Sprites/Symbols/Wild.png` | Wild symbol sprite |
| `Art/Sprites/UI/Background.png` | Main game background |
| `Art/Sprites/UI/Button.png` | Default button sprite |
| `Art/Sprites/UI/ButtonHighlight.png` | Button pressed/hover state |
| `Art/Sprites/UI/Panel.png` | UI panel/modal background |
| `Art/Sprites/UI/SpinButton.png` | Large spin button sprite |
| `Resources/Sprites/ClassicSlots.png` | Reel skin — Classic Slots |
| `Resources/Sprites/JungleJackpot.png` | Reel skin — Jungle Jackpot |
| `Resources/Sprites/PennyParadise.png` | Reel skin — Penny Paradise |

### Audio
| File | Purpose |
|------|---------|
| `Audio/spin.wav` | Reel spinning sound |
| `Audio/win.wav` | Small win sound |
| `Audio/bigwin.wav` | Big win / jackpot sound |
| `Audio/click.wav` | Button click UI sound |
| `Audio/coins.wav` | Coin award sound |

### Prefabs
| File | Purpose |
|------|---------|
| `Prefabs/FriendRowPrefab.prefab` | One row in the Friends list UI |
| `Prefabs/InboxRowPrefab.prefab` | One row in the DM inbox / thread list |
| `Prefabs/MessageBubblePrefab.prefab` | Individual chat bubble |
| `Prefabs/MessageRowPrefab.prefab` | One row in DM thread view |

---

## Backend — `Backend/`

### Entry Points
| File | Purpose |
|------|---------|
| `server.js` | Main entry point — connects MongoDB, creates HTTP server, sets up Socket.IO, mounts all routes, starts listening |
| `app.js` | Express app factory — same route wiring without DB connect or `listen`. Used by the test suite. |

### Config
| File | Purpose |
|------|---------|
| `config/db.js` | Mongoose connection helper — reads `MONGO_URI` from env |

### Middleware
| File | Purpose |
|------|---------|
| `middleware/auth.js` | JWT bearer token verification — attaches `req.player` on success |
| `middleware/adminOnly.js` | Checks `x-admin-secret` header — used on admin-only endpoints |
| `middleware/ipLogger.js` | Logs every request IP to the `IpLog` collection |
| `middleware/securityGuard.js` | Detects suspicious patterns (rapid requests, banned IPs, known attack signatures) — logs `SecurityEvent` and blocks threats |

### Models (MongoDB / Mongoose)
| File | Purpose |
|------|---------|
| `models/Player.js` | Core player document — coins, gems, spins, cosmetics, flags. Password hashing via bcrypt pre-save hook. |
| `models/Transaction.js` | Immutable ledger entry — every coin/gem change recorded with type, amount, balanceAfter, optional purchaseToken |
| `models/Tournament.js` | Tournament document — name, status, startTime, endTime, prizePool, totalPlayers |
| `models/TournamentEntry.js` | Player's entry in a tournament — score, spinsPlayed, biggestWin, rank, prizeAwarded |
| `models/ChatMessage.js` | Single chat message — senderId, senderName, text, channel, roomId, targetId, deleted flag |
| `models/Friendship.js` | Friend relationship — requester, recipient, status (pending/accepted) |
| `models/InviteReward.js` | Invite code record — inviterId, inviteCode (unique), array of redemptions |
| `models/LinkedAccount.js` | Social account link — playerId, provider, providerUid, displayName, bonusGranted flag |
| `models/IpLog.js` | IP address log entry — ip, path, method, timestamp |
| `models/SecurityEvent.js` | Security alert — eventType, severity, ip, playerId, details, resolved flag |
| `models/BannedEntity.js` | Banned IP or player ID — type, value, reason, expiresAt |

### Routes
| File | Endpoints |
|------|-----------|
| `routes/auth.js` | `POST /register`, `GET /me`, `PATCH /display-name` |
| `routes/coins.js` | `GET /balance`, `GET /transactions`, `POST /sync` |
| `routes/payments.js` | `POST /google-play/verify`, `POST /paypal/create-order`, `POST /paypal/capture`, `POST /paypal/payout` |
| `routes/tournament.js` | `GET /current`, `POST /join`, `POST /score`, `GET /leaderboard`, `GET /history` |
| `routes/chat.js` | `GET /history`, `POST /send`, `GET /dm/threads`, `GET /dm/thread/:friendId`, `POST /voice/join`, `POST /voice/leave` |
| `routes/friends.js` | `GET /`, `GET /search`, `POST /request`, `POST /accept`, `POST /decline`, `POST /remove` |
| `routes/accountLinking.js` | `GET /links`, `POST /link`, `DELETE /link/:provider` |
| `routes/security.js` | `GET /events`, `POST /ban`, `POST /unban`, `POST /resolve` *(admin-only)* |
| `routes/invite.js` | `GET /code`, `POST /redeem`, `GET /stats` |

### Services
| File | Purpose |
|------|---------|
| `services/googlePlayVerifier.js` | Calls Google Play Developer API to verify a purchase token before granting items |
| `services/paypalService.js` | Creates and captures PayPal orders; sends payouts via PayPal Payouts API |
| `services/stripeService.js` | Stripe payment intent creation and webhook event handling |
| `services/securityService.js` | Helpers used by `securityGuard` — ban checks, event logging, threat scoring |
| `services/tournamentScheduler.js` | Cron-style scheduler — auto-creates new tournaments, ends expired ones, awards prizes |
| `services/webhookService.js` | Sends admin alerts (e.g. security events, large wins) to a configured webhook URL |

### Tests
| File | Tests | What is covered |
|------|-------|----------------|
| `tests/auth.test.js` | 8 | Register new player, duplicate device ID, missing deviceId, /me with valid token, /me without token, /me with bad token, display-name update, display-name too short, truncation to 24 chars |
| `tests/coins.test.js` | 5 | Balance shape, 401 without token, empty transactions, pagination params, server-authoritative sync |
| `tests/tournament.test.js` | 8 | No active tournament, active tournament returned, join with no tournament, join creates entry, join duplicate, score updates entry, empty leaderboard, empty history |
| `tests/friends.test.js` | 11 | Search too short, search finds player, self excluded from search, create request, self-request blocked, duplicate request, missing target, accept request, no pending request, decline, remove |
| `tests/invite.test.js` | 7 | Code format, idempotent code, zero stats, redeem grants gems, own code blocked, double redeem blocked, invalid code, missing code field |
| `tests/chat.test.js` | 9 | Send global, empty text, text truncation, 401, history populated, empty history, DM thread, DM threads aggregate, voice join/leave |
| `tests/accountLinking.test.js` | 9 | Empty links, 401, link google (bonus granted), no double bonus, invalid provider, missing token, conflict across players, discord bonus larger, unlink, unlink nonexistent |
| `tests/__helpers__/setup.js` | — | Shared MongoMemoryServer setup/teardown, app factory, token helper |

### Config & Deployment
| File | Purpose |
|------|---------|
| `.env.example` | Template for all required environment variables |
| `Dockerfile` | Multi-stage Node.js image for production |
| `docker-compose.yml` | Orchestrates `backend` + `mongo:7` services with health check |
| `public/stripe-checkout.html` | Hosted Stripe checkout page served as static file |

---

## CI / GitHub Actions — `.github/workflows/`

| File | Purpose |
|------|---------|
| `build-android.yml` | Runs on push — uses `game-ci/unity-builder` to build the Android AAB in CI |
| `deploy-backend.yml` | Runs on push to `main` — SSHes to server and runs `docker-compose up --build -d` |

---

## Documentation — `Docs/`

| File | Contents |
|------|---------|
| `SystemOverview.md` | High-level architecture diagram and system interaction summary |
| `AccountLinking.md` | Account linking flow, bonus rules, provider token validation notes |
| `PaymentSystem.md` | IAP flow for Google Play and PayPal, webhook handling, idempotency |
| `SecuritySystem.md` | Security guard logic, ban system, IP logging, admin endpoints |

---

## Google Play Assets — `GooglePlay/`

| File | Contents |
|------|---------|
| `GooglePlayConsoleChecklist.md` | Step-by-step Play Console setup checklist |
| `BuildSettings/AndroidBuildChecklist.md` | Unity Android Player Settings checklist |

---

## Project Settings — `ProjectSettings/`

| File | Contents |
|------|---------|
| `ProjectVersion.txt` | Unity 2022.3.20f1 |
| `ProjectSettings.asset` | Company: BigMoneySlotsStudio, Product: BigMoneySlots, Bundle: 1.0 |
| `EditorBuildSettings.asset` | Scene list for build — includes `MainScene` |
| `InputManager.asset` | Unity legacy input axes |
| `AudioManager.asset` | Audio settings |
| `GraphicsSettings.asset` | Rendering pipeline settings |
| `QualitySettings.asset` | Quality levels |
| `Physics2DSettings.asset` | 2D physics configuration |
| `TagManager.asset` | Tags and layers |
| `TimeManager.asset` | Fixed/delta time settings |
| `DynamicsManager.asset` | 3D physics settings |
| `EditorSettings.asset` | Editor preferences |
