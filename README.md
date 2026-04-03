# 🎰 BigMoneySlots

[![Unity](https://img.shields.io/badge/Unity-6.4%20(6000.4.0f1)-black?logo=unity)](https://unity.com/)
[![Node.js](https://img.shields.io/badge/Node.js-18%2B-green?logo=node.js)](https://nodejs.org/)
[![MongoDB](https://img.shields.io/badge/MongoDB-Atlas-brightgreen?logo=mongodb)](https://www.mongodb.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Android-green?logo=android)](https://play.google.com/)

**BigMoneySlots** is a free-to-play social casino slot machine game built with **Unity 6.4** for Android. It features 40 slot games, a 5-tier progressive jackpot, real-time tournaments, social features (chat, friends, voice), and an in-app purchase economy backed by a **Node.js / MongoDB** server.

> ⚠️ **For Entertainment Only** — BigMoneySlots uses virtual currency only. No real money gambling. No cash-out of winnings.

---

## 📑 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Requirements](#-requirements)
- [Backend Setup](#-backend-setup)
- [Unity Setup](#-unity-setup)
- [Building for Android](#-building-for-android)
- [In-App Purchases](#-in-app-purchases)
- [Documentation](#-documentation)
- [Terms of Service](#-terms-of-service)
- [Privacy Policy](#-privacy-policy)
- [Refund Policy](#-refund-policy)
- [Contact](#-contact)

---

## ✨ Features

| Category | Details |
|---|---|
| 🎰 **Slot Games** | 40 unique slot machines, 70% base RTP, 5-reel engine |
| 💰 **Progressive Jackpot** | 5 tiers: Mini / Minor / Major / Grand / Mega |
| 🏆 **Tournaments** | Automated scheduled tournaments with leaderboards |
| 👥 **Social** | Real-time chat, friends system, voice chat |
| 🔗 **Account Linking** | Facebook, Google, Discord, Phone number |
| 🎁 **Rewards** | Daily challenges, loyalty tiers, session bonuses (100B coins/10 min) |
| 💎 **Gems & Coins** | Dual-currency economy; gems purchasable via IAP |
| 🔒 **Security** | IP logging, JWT auth, rate limiting, injection detection, ban system |
| 📱 **Platform** | Android (Google Play), AAB builds, IL2CPP |

---

## 🏗️ Tech Stack

| Layer | Technology |
|---|---|
| Game Engine | Unity 6.4 (6000.4.0f1), C# 9 |
| Backend | Node.js 18+, Express 4, Socket.IO 4 |
| Database | MongoDB (Mongoose 8) — MongoDB Atlas recommended |
| Auth | JWT (jsonwebtoken) |
| Payments | Google Play Billing v3, PayPal REST v2, Stripe |
| Real-time | Socket.IO (chat, notifications, tournament scores) |
| Voice Chat | Unity Microphone API + Vivox SDK |
| Build | IL2CPP, ARM64, Android AAB |

---

## 📁 Project Structure

```
BigMoneySlots/
├── Assets/
│   └── Scripts/
│       ├── Backend/          BackendClient.cs — shared API base URL & auth token
│       ├── Core/             GameManager.cs, PlayerEconomy.cs
│       ├── Rewards/          RewardManager.cs, LoyaltySystem.cs, GemSystem.cs,
│       │                     DailyChallenges.cs, InviteRewardManager.cs
│       ├── SlotEngine/       SlotMachine.cs, ProgressiveJackpot.cs, Reel.cs,
│       │                     Symbol.cs, PayoutTable.cs, SlotGameConfig.cs,
│       │                     SlotGameDatabase.cs, SlotGameLoader.cs, SlotGameRegistry.cs
│       ├── Social/           AccountLinking.cs, ChatManager.cs, FriendsManager.cs,
│       │                     VoiceChatManager.cs
│       ├── UI/               SlotUI.cs, JackpotUI.cs, ChatUI.cs, FriendsUI.cs,
│       │                     AccountLinkingUI.cs, InviteUI.cs, WelcomeOfferUI.cs
│       └── WelcomeOffer/     WelcomeOfferConfig.cs, WelcomeOfferManager.cs, IAPHandler.cs
│
├── Backend/                  Node.js / Express / MongoDB back-end
│   ├── config/               db.js — MongoDB connection
│   ├── middleware/           auth.js, ipLogger.js, securityGuard.js
│   ├── models/               Player, Transaction, Tournament, ChatMessage, Friendship, …
│   ├── routes/               auth, coins, payments, tournament, chat, friends,
│   │                         accountLinking, security, invite
│   ├── services/             paypalService, stripeService, googlePlayVerifier,
│   │                         tournamentScheduler, webhookService, securityService
│   ├── public/               stripe-checkout.html
│   ├── .env.example          All required environment variables
│   ├── package.json
│   └── server.js             Express app entry point
│
├── Docs/
│   ├── AccountLinking.md
│   ├── PaymentSystem.md
│   ├── SecuritySystem.md
│   └── SystemOverview.md
│
├── GooglePlay/
│   ├── BuildSettings/        AndroidBuildChecklist.md
│   └── GooglePlayConsoleChecklist.md
│
├── UNITY_VERSIONS.md         Unity version compatibility table
├── CLEANUP.md                Branches eligible for deletion
└── README.md                 ← this file
```

---

## 📋 Requirements

### Unity (Game Client)

| Requirement | Version |
|---|---|
| Unity Editor | **6.4 (6000.4.0f1)** — primary target |
| Unity Hub | 3.x |
| Android Build Support | Installed via Unity Hub |
| Android NDK | r23c (bundled with Unity) |
| Android SDK API | 33+ (Target), 22+ (Min) |
| JDK | 11 (OpenJDK, bundled with Unity) |

### Backend (Node.js Server)

| Requirement | Version |
|---|---|
| Node.js | 18 LTS or later |
| npm | 9+ |
| MongoDB | Atlas M0 (free tier) or local 6.0+ |

### Google Play

- Google Play Developer Account ($25 one-time fee)
- Keystore file (generate once, keep forever)
- Service Account JSON for Google Play Billing verification

---

## 🔧 Backend Setup

```bash
# 1. Navigate to the Backend directory
cd Backend

# 2. Install dependencies
npm install

# 3. Copy the example environment file
cp .env.example .env

# 4. Edit .env with your real values
nano .env   # or use VS Code / any editor
```

**Required `.env` variables:**

```env
PORT=3000
MONGO_URI=mongodb+srv://<user>:<pass>@cluster0.mongodb.net/bigmoneyslots
JWT_SECRET=your_super_secret_jwt_key_here

# Stripe
STRIPE_SECRET_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...

# PayPal
PAYPAL_CLIENT_ID=your_paypal_client_id
PAYPAL_CLIENT_SECRET=your_paypal_client_secret
PAYPAL_MODE=live   # or "sandbox" for testing

# Google Play Billing
GOOGLE_SERVICE_ACCOUNT_JSON=./config/google-service-account.json
GOOGLE_PLAY_PACKAGE=com.yourstudio.bigmoneyslots
```

```bash
# 5. Start the server
npm start          # production
npm run dev        # development (auto-restart with nodemon)
```

The server listens on `http://localhost:3000` by default.

---

## 🎮 Unity Setup

1. **Install Unity Hub** from [unity.com/download](https://unity.com/download)
2. **Install Unity 6.4 (6000.4.0f1)** via Unity Hub → Installs → Add
   - Include **Android Build Support**, **Android SDK & NDK Tools**, **OpenJDK**
3. **Open the project**: Unity Hub → Open → select the `BigMoneySlots/` root folder
4. **Wait for import** — Unity will compile all C# scripts (~2–5 minutes first run)
5. **Set backend URL**: Open `Assets/Scripts/Backend/BackendClient.cs`
   - Change the `#else` branch URL to your deployed server address
6. **Open the main scene**: `Assets/Scenes/Main.unity`

---

## 📱 Building for Android

1. **File → Build Settings** → select **Android** → **Switch Platform**
2. **Player Settings**:
   - Package Name: `com.yourstudio.bigmoneyslots`
   - Minimum API Level: 22 (Android 5.1)
   - Target API Level: 33 (Android 13)
   - Scripting Backend: **IL2CPP**
   - Target Architectures: ✅ ARM64
   - Build App Bundle (AAB): ✅ checked (required for Play Store)
3. **Keystore**: Player Settings → Publishing Settings → create/use existing keystore
4. Click **Build** — select output folder — wait for Gradle build (~5–15 minutes)
5. Upload the `.aab` file to Google Play Console

See [`GooglePlay/BuildSettings/AndroidBuildChecklist.md`](GooglePlay/BuildSettings/AndroidBuildChecklist.md) for the full checklist.

---

## 💰 In-App Purchases

All IAP product IDs are defined in `Assets/Scripts/WelcomeOffer/IAPHandler.cs`:

| Product ID | Contents | Price (USD) |
|---|---|---|
| `welcomeofferpack` | Starter gem bundle | $4.99 |
| `gems_100` | 100 gems | $0.99 |
| `gems_500` | 500 gems | $4.99 |
| `gems_1200` | 1,200 gems | $9.99 |
| `gems_2500` | 2,500 gems | $19.99 |
| `gems_6500` | 6,500 gems | $49.99 |

> Gems are virtual currency with no cash value. All purchases are final.

---

## 📚 Documentation

| File | Description |
|---|---|
| [`Docs/SystemOverview.md`](Docs/SystemOverview.md) | Architecture, tech stack, loyalty tiers |
| [`Docs/AccountLinking.md`](Docs/AccountLinking.md) | Social account linking (Facebook, Google, Discord) |
| [`Docs/PaymentSystem.md`](Docs/PaymentSystem.md) | Payments, IAP, Stripe, PayPal, Google Play Billing |
| [`Docs/SecuritySystem.md`](Docs/SecuritySystem.md) | Security architecture, IP logging, ban system |
| [`GooglePlay/GooglePlayConsoleChecklist.md`](GooglePlay/GooglePlayConsoleChecklist.md) | Step-by-step Play Store submission |
| [`GooglePlay/BuildSettings/AndroidBuildChecklist.md`](GooglePlay/BuildSettings/AndroidBuildChecklist.md) | Android build configuration |
| [`UNITY_VERSIONS.md`](UNITY_VERSIONS.md) | Unity version compatibility matrix |

---

---

## 📜 Terms of Service

**Last Updated: March 28, 2026**
**Effective Date: March 28, 2026**
**Operator: PGCN (Pretty Good Casino Network)**
**Contact: tech.crew151@gmail.com**

### 1. Acceptance of Terms

By downloading, installing, or playing **BigMoneySlots** (the "Game"), you agree to be bound by these Terms of Service ("Terms"). If you do not agree, do not use the Game. We may update these Terms at any time; continued use constitutes acceptance of the updated Terms.

### 2. Eligibility

You must be at least **13 years of age** to use this Game. By using the Game, you represent that you meet this age requirement. If you are under 18, you must have parental or guardian consent.

### 3. Entertainment Only — No Real Money Gambling

BigMoneySlots is a **free-to-play social casino** application. All gameplay uses virtual currency (coins and gems) that has **no real-world monetary value** and **cannot be redeemed, exchanged, or transferred for real money, goods, or services**. The Game does not constitute gambling under any jurisdiction. Virtual currency wins do not represent real winnings.

### 4. Virtual Currency

- **Coins** are earned through gameplay, daily challenges, loyalty bonuses, and session rewards.
- **Gems** are a premium virtual currency that may be purchased via in-app purchase.
- All virtual currency is licensed, not sold. It has no monetary value and is non-transferable.
- We reserve the right to modify, manage, regulate, control, or eliminate virtual currency at any time.

### 5. In-App Purchases

- The Game offers optional in-app purchases of virtual gems for real money.
- **All purchases are final. No refunds will be issued under any circumstances.** See our [Refund Policy](#-refund-policy) below.
- Purchases are processed by Google Play Billing, Stripe, or PayPal.
- You are responsible for all charges on your payment account.
- We are not responsible for any unauthorized purchases; contact your payment provider for disputes.

### 6. User Accounts

- You are responsible for maintaining the confidentiality of your account credentials.
- You may not share, sell, or transfer your account.
- We reserve the right to suspend or terminate accounts that violate these Terms.
- Account data may be linked to Facebook, Google, Discord, or phone number for convenience.

### 7. Prohibited Conduct

You agree NOT to:
- Use cheats, exploits, automation software, bots, hacks, or any unauthorized third-party software
- Attempt to reverse-engineer, decompile, or modify the Game
- Use the Game for any commercial purpose
- Engage in harassing, abusive, or offensive behavior toward other players
- Impersonate any person or entity
- Upload or transmit viruses or malicious code
- Attempt to gain unauthorized access to servers or other accounts
- Exploit bugs or glitches — report them at tech.crew151@gmail.com

### 8. Intellectual Property

All content in BigMoneySlots — including but not limited to graphics, audio, code, game mechanics, and documentation — is owned by or licensed to PGCN. You are granted a limited, non-exclusive, non-transferable license to use the Game for personal, non-commercial entertainment.

### 9. Chat and Social Features

- The chat system is public; do not share personal information.
- We reserve the right to moderate, remove, or block content or users at our discretion.
- Voice chat is provided as-is; we are not responsible for content exchanged via voice.

### 10. Disclaimer of Warranties

THE GAME IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND. TO THE MAXIMUM EXTENT PERMITTED BY LAW, PGCN DISCLAIMS ALL WARRANTIES, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND NON-INFRINGEMENT.

### 11. Limitation of Liability

TO THE MAXIMUM EXTENT PERMITTED BY LAW, PGCN SHALL NOT BE LIABLE FOR ANY INDIRECT, INCIDENTAL, SPECIAL, CONSEQUENTIAL, OR PUNITIVE DAMAGES ARISING OUT OF YOUR USE OF THE GAME, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGES. OUR TOTAL LIABILITY SHALL NOT EXCEED THE AMOUNT YOU PAID FOR IN-APP PURCHASES IN THE PAST 90 DAYS, OR $10.00, WHICHEVER IS GREATER.

### 12. Termination

We may suspend or terminate your access to the Game at any time for any reason, including violation of these Terms, without prior notice and without liability.

### 13. Governing Law

These Terms shall be governed by and construed in accordance with applicable law. Any disputes shall be resolved through binding arbitration.

### 14. Contact

For questions about these Terms, contact us at: **tech.crew151@gmail.com**

---

---

## 🔐 Privacy Policy

**Last Updated: March 28, 2026**
**Effective Date: March 28, 2026**
**Operator: PGCN (Pretty Good Casino Network)**
**Contact: tech.crew151@gmail.com**

### 1. Introduction

This Privacy Policy describes how PGCN ("we," "us," or "our") collects, uses, and shares information when you use **BigMoneySlots** (the "Game"). By using the Game, you consent to the practices described in this Privacy Policy.

### 2. Information We Collect

#### Information You Provide
- **Account registration data**: username, email address (if provided)
- **Payment information**: processed by Google Play, Stripe, or PayPal — we do not store full card numbers
- **Social account identifiers**: if you link Facebook, Google, Discord, or phone number

#### Information Collected Automatically
- **Device information**: device model, OS version, unique device identifiers
- **IP address**: collected for security, fraud prevention, and abuse detection
- **Gameplay data**: spin history, purchase history, tournament scores, coin/gem balances
- **Session data**: play time, login timestamps, feature usage
- **Chat messages**: stored in our database for moderation purposes
- **Technical logs**: crash reports, error logs

### 3. How We Use Your Information

We use collected information to:
- Provide and maintain the Game
- Process in-app purchases and deliver virtual currency
- Authenticate your account and maintain session security
- Detect, prevent, and investigate fraud, cheating, and abuse
- Send service-related notifications (optional marketing with consent)
- Analyze usage to improve the Game
- Comply with legal obligations
- Enforce these Terms of Service

### 4. IP Address & Security Logging

We log IP addresses for fraud prevention and security. This includes:
- Login attempts and authentication events
- Purchase transactions
- Suspicious activity detection
- Ban enforcement

IP logs are retained for **90 days** and may be shared with law enforcement upon legal request.

### 5. Information Sharing

We do **not** sell your personal information. We may share information with:
- **Service providers**: MongoDB Atlas (database), Stripe (payments), PayPal (payments), Google (Play Billing, analytics)
- **Legal authorities**: when required by law, court order, or to protect safety
- **Business transfers**: in the event of a merger, acquisition, or asset sale

### 6. Data Retention

- Account data: retained until account deletion is requested
- Chat messages: retained for 90 days, then purged
- IP logs: retained for 90 days
- Payment records: retained for 7 years for tax/legal compliance

### 7. Children's Privacy

BigMoneySlots is not directed to children under 13. We do not knowingly collect personal information from children under 13. If you believe a child under 13 has provided us information, contact us at tech.crew151@gmail.com and we will delete it.

### 8. Security

We implement industry-standard security measures including JWT authentication, rate limiting, encrypted transmissions (HTTPS/TLS), and IP-based threat detection. No method of transmission or storage is 100% secure; we cannot guarantee absolute security.

### 9. Your Rights

Depending on your jurisdiction, you may have rights to:
- Access the personal data we hold about you
- Request correction of inaccurate data
- Request deletion of your data
- Withdraw consent for optional data uses

To exercise these rights, contact: **tech.crew151@gmail.com**

### 10. Third-Party Services

The Game integrates with third-party services that have their own privacy policies:
- [Google Play Privacy Policy](https://policies.google.com/privacy)
- [Stripe Privacy Policy](https://stripe.com/privacy)
- [PayPal Privacy Policy](https://www.paypal.com/webapps/mpp/ua/privacy-full)
- [MongoDB Atlas Privacy Policy](https://www.mongodb.com/legal/privacy-policy)

### 11. Changes to This Policy

We may update this Privacy Policy at any time. We will notify you of significant changes by updating the "Last Updated" date. Continued use of the Game constitutes acceptance of the updated policy.

### 12. Contact

Privacy questions or requests: **tech.crew151@gmail.com**

---

---

## 🚫 Refund Policy

**Last Updated: March 28, 2026**
**Operator: PGCN (Pretty Good Casino Network)**
**Contact: tech.crew151@gmail.com**

### ALL SALES ARE FINAL — NO REFUNDS

PGCN operates a **strict no-refund policy** for all in-app purchases made in BigMoneySlots. By completing a purchase, you explicitly acknowledge and agree that:

1. **No refunds will be issued** for any in-app purchase of virtual gems, coin packages, or any other virtual items, regardless of the reason.

2. **Virtual currency has no monetary value.** Gems and coins are virtual items that cannot be exchanged for real money. Purchasing virtual currency is a one-way transaction.

3. **Accidental purchases** — We are not responsible for purchases made accidentally, by family members, or by unauthorized users on your device. Enable purchase confirmation and parental controls on your device to prevent accidental purchases.

4. **Technical issues** — If you did not receive purchased items due to a genuine technical error on our end (verified by our servers), contact us at tech.crew151@gmail.com within **7 days** of the purchase and we will investigate and credit your account with the equivalent virtual currency.

5. **Chargebacks are prohibited.** Initiating an unauthorized chargeback or payment dispute will result in:
   - **Immediate permanent ban** of your account
   - Forfeiture of all virtual currency and items
   - Possible legal action to recover costs

6. **Platform purchases**: If you purchased through Google Play, you may submit a refund request directly to Google within their refund window (typically 48 hours). Google's decision is final and outside our control. For purchases outside of Google Play's refund window, no refund will be issued.

### Contacting Support

For genuine technical issues (item not received, duplicate charge due to server error), contact:

📧 **tech.crew151@gmail.com**

Include:
- Your username/player ID
- Transaction ID or receipt
- Date and time of purchase
- Description of the issue

We respond within **3–5 business days**.

---

## 📞 Contact

| Purpose | Contact |
|---|---|
| General Support | tech.crew151@gmail.com |
| Privacy Requests | tech.crew151@gmail.com |
| Legal / ToS | tech.crew151@gmail.com |
| Bug Reports | tech.crew151@gmail.com |

---

*© 2026 PGCN (Pretty Good Casino Network). All rights reserved.*
*BigMoneySlots is intended for entertainment purposes only. No real money gambling.*
