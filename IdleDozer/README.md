# 🚜 Idle Dozer

A simple idle/incremental Android game where you build a bulldozer empire to earn passive virtual income. Monetised with Google AdMob (banner, interstitial, and rewarded ads).

---

## Game Loop

| Action | Effect |
|--------|--------|
| **Tap the TAP button** | Earn coins immediately (scales with total EPS) |
| **Buy dozers** | Each dozer earns coins automatically every second |
| **Watch a rewarded ad** | Activate a **2× earnings boost** for 60 seconds |
| **Offline earnings** | Earn 50 % of your EPS while the app is closed (capped at 8 hours) |

### Dozer Catalogue

| # | Name | Base Cost | EPS each |
|---|------|-----------|----------|
| 1 | Mini Dozer 🚜 | $10 | $0.10/s |
| 2 | Worker Dozer 🏗️ | $100 | $1.20/s |
| 3 | Standard Dozer 🚧 | $1 K | $13/s |
| 4 | Heavy Dozer ⚙️ | $12 K | $160/s |
| 5 | Mega Dozer 🏔️ | $130 K | $2,100/s |
| 6 | Ultra Dozer 💥 | $1.4 M | $28,000/s |
| 7 | Titan Dozer 🌋 | $20 M | $380,000/s |
| 8 | Legendary Dozer 👑 | $300 M | $5,500,000/s |

Each purchase increases the next unit's cost by **15 %** (standard idle-game scaling).

---

## Project Structure

```
IdleDozer/
├── app/
│   └── src/main/
│       ├── java/com/ceemoorebooty/idledozer/
│       │   ├── MainActivity.kt      ← UI + game loop
│       │   ├── DozerAdapter.kt      ← RecyclerView adapter for the shop
│       │   ├── DozerItem.kt         ← Data model
│       │   ├── GameEngine.kt        ← Core idle-game logic
│       │   ├── AdManager.kt         ← AdMob integration
│       │   └── SaveManager.kt       ← SharedPreferences save/load
│       ├── res/
│       │   ├── layout/activity_main.xml
│       │   ├── layout/item_dozer.xml
│       │   └── values/{colors,strings,themes}.xml
│       └── AndroidManifest.xml
└── README.md  ← you are here
```

---

## Setup: Before Building

### 1. Create an AdMob Account
1. Sign up at [admob.google.com](https://admob.google.com).
2. Create an **Android app** in AdMob and note your **App ID**.
3. Create three ad units: **Banner**, **Interstitial**, **Rewarded Video**.

### 2. Replace Test Ad IDs

| File | Constant / attribute | Replace with |
|------|---------------------|--------------|
| `AndroidManifest.xml` | `com.google.android.gms.ads.APPLICATION_ID` value | Your AdMob App ID |
| `AdManager.kt` | `BANNER_AD_UNIT_ID` | Your Banner ad unit ID |
| `AdManager.kt` | `INTERSTITIAL_AD_UNIT_ID` | Your Interstitial ad unit ID |
| `AdManager.kt` | `REWARDED_AD_UNIT_ID` | Your Rewarded ad unit ID |
| `activity_main.xml` | `ads:adUnitId` on the `AdView` | Your Banner ad unit ID |

### 3. Open in Android Studio
1. **File → Open** → select the `IdleDozer/` folder.
2. Let Gradle sync complete.
3. **Run** on a device or emulator (API 21+).

---

## Building a Release APK / AAB for the Play Store

```bash
# From the IdleDozer/ directory:
./gradlew bundleRelease        # produces app-release.aab
# or
./gradlew assembleRelease      # produces app-release.apk
```

Sign the output with your upload keystore before uploading to Google Play Console.

### Play Store Checklist
- [ ] Replace all test AdMob IDs (see above)
- [ ] Set a unique `applicationId` (already set to `com.ceemoorebooty.idledozer`)
- [ ] Add a Privacy Policy URL (required by AdMob & Play Store)
- [ ] Generate a signed release build (upload keystore)
- [ ] Prepare screenshots, icon, short & long description
- [ ] Set content rating to **Everyone** in Play Console
- [ ] Publish to Internal Testing → Closed Testing → Production

---

## Monetisation at a Glance

| Format | Trigger | Revenue type |
|--------|---------|-------------|
| **Banner** | Always visible at bottom | CPM |
| **Interstitial** | Every 5 purchases | CPM / CPC |
| **Rewarded Video** | Player taps "Watch Ad" | CPM (highest eCPM) |

> **Note:** All virtual currency in this game is fictional. There is no real-money gambling.
> Content rating: **Simulated Gambling** is **not** required — this is a construction idle game.

---

## Expanding the Game (Ideas)

- **Prestige system** — reset for a permanent multiplier
- **Daily quests** — extra engagement, more rewarded ad opportunities
- **Google Play Games** — leaderboards & achievements
- **IAP coin packs** — via Google Play Billing library (highest revenue ceiling)
- **Push notifications** — "Your dozers are waiting!" to drive re-engagement
