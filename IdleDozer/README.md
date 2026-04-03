# Idle Dozer

A simple idle/incremental Android game where you build a bulldozer empire to earn passive virtual income. Monetised with Google AdMob (banner, interstitial, and rewarded ads).

---

## Game Loop

| Action | Effect |
|--------|--------|
| **Tap the TAP button** | Earn coins immediately (scales with total EPS) |
| **Buy dozers** | Each dozer earns coins automatically every second |
| **Watch a rewarded ad** | Activate a **2x earnings boost** for 60 seconds |
| **Offline earnings** | Earn 50% of your EPS while the app is closed (capped at 8 hours) |

### Dozer Catalogue

| # | Name | Base Cost | EPS each |
|---|------|-----------|----------|
| 1 | Mini Dozer | $10 | $0.10/s |
| 2 | Worker Dozer | $100 | $1.20/s |
| 3 | Standard Dozer | $1 K | $13/s |
| 4 | Heavy Dozer | $12 K | $160/s |
| 5 | Mega Dozer | $130 K | $2,100/s |
| 6 | Ultra Dozer | $1.4 M | $28,000/s |
| 7 | Titan Dozer | $20 M | $380,000/s |
| 8 | Legendary Dozer | $300 M | $5,500,000/s |

Each purchase increases the next unit's cost by **15%** (standard idle-game scaling).

---

## Project Structure

```
IdleDozer/
.github/workflows/idledozer.yml  <- CI/CD (debug on push, signed AAB on tag)
app/src/main/
  java/com/ceemoorebooty/idledozer/
    MainActivity.kt      <- UI + game loop
    DozerAdapter.kt      <- RecyclerView shop adapter
    DozerItem.kt         <- Data model + cost-scaling
    GameEngine.kt        <- Core idle-game logic
    AdManager.kt         <- AdMob integration
    SaveManager.kt       <- SharedPreferences save/load
  res/layout/            <- activity_main.xml, item_dozer.xml
  AndroidManifest.xml
assets/
  privacy_policy.html    <- Host this; paste the URL into Play Console
fastlane/
  Appfile                <- Package name + Play Store key path
  Fastfile               <- Lanes: test, build_debug, build_release, deploy_*
  play-store-key.json.example
scripts/
  generate-keystore.sh   <- One-command keystore generation
store-listing/
  en-US/
    title.txt
    short_description.txt
    full_description.txt
    changelogs/1.txt
  content_rating_notes.txt  <- How to answer the Play Console questionnaire
  screenshot_requirements.txt
Gemfile                  <- Ruby deps for Fastlane
gradlew / gradlew.bat    <- Gradle wrapper scripts
keystore.properties.template  <- Copy -> keystore.properties, fill in passwords
README.md
```

---

## Quick Start (Open in Android Studio)

1. **File -> Open** -> select the `IdleDozer/` folder.
2. Let Gradle sync complete (downloads dependencies automatically).
3. **Run** on a device or emulator (API 21+, Android 5.0+).

The app runs immediately with Google's **test ad IDs** -- no AdMob account needed for development.

---

## Before Publishing -- Checklist

### Already done for you
- [x] `applicationId` set to `com.ceemoorebooty.idledozer`
- [x] Gradle signing config wired up (reads `keystore.properties`)
- [x] ProGuard / R8 minification enabled for release
- [x] Privacy policy HTML at `assets/privacy_policy.html`
- [x] Play Store store listing text at `store-listing/en-US/`
- [x] Content rating notes at `store-listing/content_rating_notes.txt`
- [x] Fastlane lanes for build, test, and deploy
- [x] GitHub Actions CI/CD workflow (`.github/workflows/idledozer.yml`)
- [x] `.gitignore` -- keystores and secrets will never be committed

### You need to do (one-time setup)
- [ ] **Create an AdMob account** -> admob.google.com
  - Create an Android app -> get your **App ID**
  - Create 3 ad units: Banner, Interstitial, Rewarded Video
  - Replace the 4 test IDs (see Replace Ad IDs section below)
- [ ] **Generate your upload keystore** (see Keystore section below)
- [ ] **Host the privacy policy** (see Privacy Policy section below)
- [ ] **Take screenshots** (see `store-listing/screenshot_requirements.txt`)
- [ ] **Create a Google Play developer account** -> play.google.com/console ($25 one-time fee)

---

## Step-by-Step Setup

### 1. Replace AdMob IDs

| File | What to change | Replace with |
|------|---------------|-------------|
| `AndroidManifest.xml` | `APPLICATION_ID` meta-data value | Your AdMob App ID |
| `AdManager.kt` | `BANNER_AD_UNIT_ID` | Your Banner ad unit ID |
| `AdManager.kt` | `INTERSTITIAL_AD_UNIT_ID` | Your Interstitial ad unit ID |
| `AdManager.kt` | `REWARDED_AD_UNIT_ID` | Your Rewarded Video ad unit ID |
| `activity_main.xml` | `ads:adUnitId` on AdView | Your Banner ad unit ID |

### 2. Generate Your Upload Keystore

```bash
cd IdleDozer/
./scripts/generate-keystore.sh
```

This creates `idledozer-release.jks` and `keystore.properties` (both ignored by git).
**Back up your keystore file securely -- losing it means you cannot update your app.**

### 3. Host the Privacy Policy

1. Push `assets/privacy_policy.html` to your own server OR use GitHub Pages:
   - Create a new public repo (e.g. `idledozer-privacy`) -> upload the file as `index.html`
   - Enable Pages in repo Settings -> your URL: `https://yourusername.github.io/idledozer-privacy/`
2. Edit the contact email inside `privacy_policy.html` (currently `support@example.com`).
3. Paste the public URL into Play Console -> App content -> Privacy policy.

### 4. Build the Release AAB

```bash
cd IdleDozer/

# With Android Studio: Build -> Generate Signed Bundle / APK -> Android App Bundle

# OR from the command line (after keystore.properties is in place):
./gradlew bundleRelease
# Output: app/build/outputs/bundle/release/app-release.aab
```

### 5. GitHub Actions (Automated Builds)

| Trigger | What happens |
|---------|-------------|
| Push to any branch (touching IdleDozer/) | Debug APK built, uploaded as artifact |
| Push a tag like `v1.0.1` | Signed release AAB built, uploaded as artifact |

**Secrets to add** in GitHub -> Settings -> Secrets -> Actions:

| Secret name | Value |
|-------------|-------|
| `KEYSTORE_BASE64` | `base64 -w0 idledozer-release.jks` (run on your machine) |
| `KEYSTORE_PASSWORD` | Your keystore password |
| `KEY_ALIAS` | `idledozer` |
| `KEY_PASSWORD` | Your key password |

### 6. Upload to the Play Store

1. Go to Google Play Console (play.google.com/console)
2. **Create app** -> fill in name, language, app type (Game), free
3. Complete **App content**: privacy policy URL, content rating questionnaire
   (use answers from `store-listing/content_rating_notes.txt`), target audience
4. Upload your `.aab` to the **Internal Testing** track
5. Add testers -> promote to **Closed Testing** -> then **Production** when ready

---

## Fastlane (Optional Automation)

```bash
cd IdleDozer/
gem install bundler
bundle install

bundle exec fastlane test            # Run unit tests
bundle exec fastlane build_debug     # Build debug APK
bundle exec fastlane build_release   # Build signed release AAB
bundle exec fastlane deploy_internal # Build + upload to Internal Testing
bundle exec fastlane upload_metadata # Push store listing text to Play Console
```

Requires `fastlane/play-store-key.json` for deploy/metadata lanes.
See `fastlane/play-store-key.json.example` for instructions.

---

## Monetisation at a Glance

| Format | Trigger | Notes |
|--------|---------|-------|
| **Banner** | Always visible at bottom | Lowest eCPM but always on |
| **Interstitial** | Every 5 purchases | Medium eCPM |
| **Rewarded Video** | Player-initiated 2x boost | Highest eCPM -- best revenue |

All virtual currency in this game is fictional. There is no real-money gambling.

---

## Expanding the Game (Ideas)

- **Prestige system** -- reset for a permanent earnings multiplier
- **Daily quests** -- extra engagement + more rewarded ad triggers
- **Google Play Games** -- leaderboards & achievements
- **IAP coin packs** -- via Google Play Billing (highest revenue ceiling)
- **Push notifications** -- "Your dozers are waiting!" to drive re-engagement
