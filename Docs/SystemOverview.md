# Big Money Slots — System Overview

## Architecture

```
BigMoneySlots/
├── Assets/Scripts/
│   ├── Backend/          BackendClient.cs — shared API base URL
│   ├── Core/             GameManager.cs, PlayerEconomy.cs
│   ├── Rewards/          RewardManager.cs, LoyaltySystem.cs, GemSystem.cs, DailyChallenges.cs
│   ├── SlotEngine/       SlotMachine.cs, ProgressiveJackpot.cs, Reel.cs, Symbol.cs,
│   │                     PayoutTable.cs, SlotGameConfig.cs, SlotGameDatabase.cs,
│   │                     SlotGameLoader.cs, SlotGameRegistry.cs
│   ├── Social/           AccountLinking.cs, ChatManager.cs, VoiceChatManager.cs, FriendsManager.cs
│   ├── UI/               SlotUI.cs, JackpotUI.cs, ChatUI.cs, FriendsUI.cs,
│   │                     AccountLinkingUI.cs, WelcomeOfferUI.cs
│   └── WelcomeOffer/     WelcomeOfferConfig.cs, WelcomeOfferManager.cs, IAPHandler.cs
│
├── Backend/              Node.js / Express / MongoDB / Socket.IO
│   ├── config/           db.js
│   ├── middleware/        auth.js, ipLogger.js
│   ├── models/           Player.js, Transaction.js, Tournament.js, TournamentEntry.js,
│   │                     IpLog.js, ChatMessage.js, Friendship.js, LinkedAccount.js
│   ├── routes/           auth.js, coins.js, payments.js, tournament.js,
│   │                     chat.js, friends.js, accountLinking.js
│   ├── services/         paypalService.js, googlePlayVerifier.js, tournamentScheduler.js
│   ├── package.json
│   ├── server.js
│   └── .env.example
│
├── GooglePlay/
│   ├── BuildSettings/    AndroidBuildChecklist.md
│   └── GooglePlayConsoleChecklist.md
│
└── Docs/
    ├── SystemOverview.md        ← this file
    ├── AccountLinking.md
    └── PaymentSystem.md
```

## Tech Stack
| Layer       | Technology                              |
|-------------|----------------------------------------|
| Game Engine | Unity (C#)                             |
| Backend     | Node.js, Express 4, Socket.IO 4        |
| Database    | MongoDB (Mongoose 8)                   |
| Auth        | JWT (jsonwebtoken)                     |
| Payments    | Google Play Billing v3, PayPal REST v2 |
| Real-time   | Socket.IO (chat, friend notifications) |
| Voice Chat  | Unity Microphone API + Vivox SDK stubs |

## House Edge
All 40 slot games use `baseRTP = 0.70` (70% return-to-player).
This means the house retains **30 cents per $1 wagered** across all spins.
The 5-tier progressive jackpot contributes an additional 4.0% of every bet
across Mini/Minor/Major/Grand/Mega pools, with dynamic trigger probability
that increases as each pool fills.

## Loyalty Tiers
| Tier               | Spins Required | Win Multiplier | Daily Bonus       |
|--------------------|---------------|----------------|-------------------|
| Trail Wanderer     | 0             | 1.00×          | —                 |
| Prairie Keeper     | 1,000         | 1.10×          | 10M coins         |
| Guardian Scout     | 5,000         | 1.25×          | 50M coins         |
| Spirit Warrior     | 15,000        | 1.50×          | 200M coins        |
| Great Plains Chief | 50,000        | 2.00×          | 1B coins          |

## 10-Minute Session Reward
Players receive **100,000,000,000 (100B) coins** every 10 minutes of continuous play.
This is tracked in `LoyaltySystem.cs` via `_lastSessionRewardTime`.
