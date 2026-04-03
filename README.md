# BigMoneySlots 🎰

A full-featured mobile casino slots game built with **Unity 2021.3 LTS**, featuring real IAP via Unity Purchasing, leaderboards, achievements, daily bonuses, and a Node.js backend.

---

## Table of Contents
- [Features](#features)
- [Project Structure](#project-structure)
- [Quick Start](#quick-start)
- [Scene Overview](#scene-overview)
- [Scripts Reference](#scripts-reference)
- [Backend Setup](#backend-setup)
- [Building for Mobile](#building-for-mobile)
- [CI/CD](#cicd)
- [Configuration](#configuration)

---

## Features

| Feature | Description |
|---------|-------------|
| 🎰 Slot Machine | 5×3 reel grid, 10 paylines, scatter/wild symbols, free spins |
| 💰 Jackpot | Progressive jackpot seeded from spin bets |
| 🏆 Achievements | 20+ milestone achievements with coin rewards |
| 📅 Daily Bonus | 7-day streak calendar with escalating rewards |
| 🎯 Daily Challenges | 3 fresh challenges every day (long targets up to 5B coins) |
| 🛒 Shop | 6 IAP coin packages + free coins via rewarded ads |
| 🏅 Leaderboard | Global leaderboard synced to Railway/Node backend |
| 🔊 Sound Manager | Background music + categorised SFX with persistent volume |
| 💬 Localization-ready | All UI text via TextMeshPro, easy to extend |

---

## Project Structure

```
BigMoneySlots/
├── Assets/
│   ├── Animations/          # Animator controllers (Fade, CoinPulse, JackpotPulse, WinPopup)
│   ├── Audio/               # BGM + SFX clips (add your .wav/.ogg files here)
│   ├── Prefabs/UI/          # Reusable UI prefabs
│   │   ├── CoinPackageItem.prefab
│   │   ├── DaySlotUI.prefab
│   │   └── PaylineRow.prefab
│   ├── Resources/
│   │   └── DefaultSlotConfig.asset  # SlotConfig ScriptableObject
│   ├── Scenes/
│   │   ├── Bootstrap.unity   # Entry point — initialises singletons
│   │   ├── MainMenu.unity    # Main menu with leaderboard/challenges/daily bonus
│   │   ├── Game.unity        # Slot machine gameplay
│   │   ├── Shop.unity        # IAP coin shop
│   │   ├── Settings.unity    # Music/SFX sliders, reset
│   │   └── Paytable.unity    # Symbol payouts & payline diagram
│   ├── Scripts/
│   │   ├── Audio/            # SoundManager.cs
│   │   ├── Data/             # GameData.cs, BackendClient.cs
│   │   ├── Network/          # BackendClient.cs (REST API)
│   │   ├── Rewards/          # DailyBonusController, DailyChallenges, AchievementManager
│   │   ├── Settings/         # SettingsController
│   │   ├── Shop/             # ShopController (Unity IAP)
│   │   ├── Slots/            # SlotMachine, ReelController, SlotConfig, SlotSymbol, PaylineDefinition
│   │   └── UI/               # All UI controllers + UIManager singleton
│   └── Sprites/              # Symbol + UI sprites (add your PNGs here)
├── Backend/                  # Node.js + Express + MongoDB REST API
├── ProjectSettings/
└── Packages/
    └── manifest.json         # TextMeshPro, Unity IAP, Unity Purchasing
```

---

## Quick Start

### Prerequisites
- **Unity 2021.3.16f1** or newer LTS
- Unity IAP package (installed via `Packages/manifest.json`)
- TextMeshPro (installed via `Packages/manifest.json`)
- Node.js ≥ 18 (for backend, optional)

### Open in Unity
1. Clone the repo
2. Open **Unity Hub** → Add Project → select the repo root
3. Wait for import (first import ~2 min)
4. Set the **Bootstrap** scene as scene 0 in Build Settings

### Add Assets
1. Drop your symbol sprites into `Assets/Sprites/` (10 symbols: Seven, Diamond, Wild, Bell, Horseshoe, Scatter, Bonus, Cherry, Lemon, Bar)
2. Drop your audio clips into `Assets/Audio/` (bgm_main, bgm_game, sfx_spin, sfx_win, sfx_bigwin, sfx_jackpot, sfx_click, sfx_coin, sfx_error)
3. Assign them via the **Inspector** on each scene's controller

### Wire References in Unity
Each scene's controller has `[SerializeField]` fields. After import:
1. Select the controller GameObject in the scene
2. Drag the matching UI elements into the Inspector slots
3. Assign the `DefaultSlotConfig` asset to `SlotMachine.slotConfig`

---

## Scene Overview

| Scene | Build Index | Entry Controller |
|-------|-------------|-----------------|
| `Bootstrap` | 0 | `BootstrapController` |
| `MainMenu` | 1 | `MainMenuController` |
| `Game` | 2 | `GameUIController` |
| `Shop` | 3 | `ShopController` |
| `Settings` | 4 | `SettingsController` |
| `Paytable` | 5 | `PaytableController` |

---

## Scripts Reference

### Singletons (DontDestroyOnLoad)
| Script | Purpose |
|--------|---------|
| `UIManager` | Scene navigation with fade transitions |
| `SoundManager` | BGM + SFX, volume persistence |
| `AchievementManager` | Unlock tracking + toast notifications |
| `LoadingScreen` | Reusable loading overlay |

### Slot Machine
| Script | Purpose |
|--------|---------|
| `SlotMachine` | Spin orchestration, payline eval, jackpot |
| `ReelController` | Per-reel animation + symbol display |
| `BetController` | Bet +/−/max validation |
| `SlotConfig` | ScriptableObject: symbols + paylines |
| `SlotSymbol` | Symbol data (id, name, payout, sprite) |
| `PaylineDefinition` | Row-pattern payline data |

### UI Controllers
| Script | Purpose |
|--------|---------|
| `GameUIController` | Master game UI wiring |
| `MainMenuController` | Menu buttons + leaderboard/challenges |
| `WinPopup` | Win tier popup (Normal/BigWin/Mega/Jackpot) |
| `CoinDisplay` | Animated TMP coin counter |
| `InsufficientFundsPopup` | Low-balance nudge popup |
| `LeaderboardController` | Global leaderboard panel |
| `PlayerNameController` | First-run name input |

### Rewards
| Script | Purpose |
|--------|---------|
| `DailyBonusController` | 7-day streak calendar |
| `DailyChallenges` | 3 daily tasks with long coin targets |
| `AchievementManager` | Milestone achievements |

---

## Backend Setup

```bash
cd Backend
npm install
# Copy .env.example to .env and set MONGO_URI, PORT
npm start
```

See `RAILWAY_DEPLOYMENT.md` for production deployment.

**Endpoints used by the game:**
- `GET /api/leaderboard?limit=10` — fetch top players
- `POST /api/leaderboard` — submit/update player score

---

## Building for Mobile

### Android
1. Open **Build Settings** (File → Build Settings)
2. Switch platform to **Android**
3. Player Settings → Company/Product name, Bundle ID (`com.yourco.bigmoneyslots`)
4. Enable **Minify** (Release) for smaller APK
5. Set your keystore in Player Settings → Publishing Settings

### iOS
1. Switch platform to **iOS**
2. Set Bundle ID and signing team in Player Settings
3. Build → open in Xcode → Archive → distribute

---

## CI/CD

GitHub Actions runs on every push to `main` or `copilot/**`:
- **Validate** — Unity Edit Mode tests
- **Build Android** — produces APK artifact (main branch only)
- **Build iOS** — produces Xcode project artifact (main branch only)

### Required Secrets
| Secret | Description |
|--------|-------------|
| `UNITY_LICENSE` | Unity license (from `unity-activate` workflow) |
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password |
| `ANDROID_KEYSTORE_BASE64` | Base64-encoded `.keystore` file |
| `ANDROID_KEYSTORE_PASS` | Keystore password |
| `ANDROID_KEYALIAS_NAME` | Key alias |
| `ANDROID_KEYALIAS_PASS` | Key alias password |

---

## Configuration

### SlotConfig (ScriptableObject)
Located at `Assets/Resources/DefaultSlotConfig.asset`.

| Field | Default | Description |
|-------|---------|-------------|
| `jackpotSeed` | 500,000 | Starting jackpot value |
| `jackpotContribution` | 1% | % of each bet added to jackpot |
| `freeSpinsScatterCount` | 3 | Scatters needed to trigger free spins |
| `freeSpinCount` | 10 | Free spins awarded |
| `wildMultiplier` | 2× | Wild symbol multiplier |

### IAP Product IDs
Edit `ShopController.cs` → `packages` array → set `productId` to match your App Store / Google Play product IDs.

---

## License
See [LICENSE](LICENSE).
