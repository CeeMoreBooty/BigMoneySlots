# BigMoneySlots 🎰

A feature-rich Unity slot machine game with Node.js backend, featuring progressive jackpots, daily challenges, social features, and monetization systems.

## 🚀 Automatic Deployment

This repository is configured with GitHub Actions for automatic deployment:

### Backend Deployment
- **Workflow**: `.github/workflows/deploy-backend.yml`
- **Triggers**: Pushes to `Main` branch affecting `Backend/**` files
- **Supported Platforms**:
  - **Heroku** (recommended for quick setup)
  - **Railway** (modern, easy to use)
  - **Render** (free tier available)
  - Any Node.js hosting service

#### Quick Backend Deploy Options:

**Option 1: Heroku**
1. Create a Heroku account at https://heroku.com
2. Create a new app: `heroku create bigmoneyslots-api`
3. Add secrets to GitHub repository:
   - `HEROKU_API_KEY` - Your Heroku API key
   - `HEROKU_EMAIL` - Your Heroku email
4. Uncomment Heroku section in `.github/workflows/deploy-backend.yml`
5. Push to Main branch - automatic deployment!

**Option 2: Railway**
1. Sign up at https://railway.app
2. Connect your GitHub repo
3. Railway will auto-detect the Node.js app in `Backend/`
4. Set environment variables in Railway dashboard
5. Deploy with one click!

**Option 3: Render**
1. Sign up at https://render.com
2. Create a new Web Service
3. Connect this repository
4. Set root directory to `Backend`
5. Use `npm install && node server.js` as start command

### Unity Build Deployment
- **Workflow**: `.github/workflows/unity-build.yml`
- **Triggers**: Pushes to `Main` affecting Unity files
- **Builds**: Android APK/AAB
- **Optional**: Auto-upload to Google Play Internal Track

#### Unity Build Setup:
1. Add Unity license secrets to GitHub:
   - `UNITY_LICENSE` - Your Unity license file content
   - `UNITY_EMAIL` - Unity account email
   - `UNITY_PASSWORD` - Unity account password
2. Push changes - builds will be created automatically
3. Download artifacts from GitHub Actions

## 📦 Project Structure

```
BigMoneySlots/
├── Assets/Scripts/          # Unity C# scripts
│   ├── Backend/            # Backend API client
│   ├── Core/               # Game core systems
│   ├── Rewards/            # Daily challenges, loyalty, gems
│   ├── SlotEngine/         # Slot machine mechanics
│   ├── Social/             # Friends, chat, voice
│   ├── UI/                 # User interface
│   └── WelcomeOffer/       # IAP and welcome offers
├── Backend/                # Node.js Express server
│   ├── config/            # Database configuration
│   ├── middleware/        # Auth, security, logging
│   ├── models/            # MongoDB models
│   ├── routes/            # API endpoints
│   ├── services/          # Payment, security services
│   └── server.js          # Entry point
├── Docs/                  # Documentation
└── GooglePlay/            # Android deployment checklists
```

## 🛠️ Local Development

### Backend Setup
```bash
cd Backend
npm install
cp .env.example .env
# Edit .env with your configuration
npm start
```

### Unity Setup
1. Open project in Unity 2021.3 or later
2. Update `BackendClient.cs` with your backend URL
3. Import required packages:
   - TextMeshPro
   - Unity IAP (optional)
   - Socket.IO client (for real-time chat)

## 🔧 Configuration

### Backend Environment Variables
Create `Backend/.env`:
```env
PORT=3000
MONGODB_URI=your_mongodb_connection_string
JWT_SECRET=your_jwt_secret
STRIPE_SECRET_KEY=your_stripe_key
PAYPAL_CLIENT_ID=your_paypal_id
```

### Unity Configuration
Update in `Assets/Scripts/Backend/BackendClient.cs`:
```csharp
public const string BaseUrl = "https://your-deployed-backend.com";
```

## ✨ Features

### Game Features
- 🎰 **Slot Machine Engine** - Customizable reels, symbols, and payouts
- 💎 **Progressive Jackpots** - 5-tier jackpot system
- 🎯 **Daily Challenges** - 3 rotating challenges per day
- ⭐ **Loyalty System** - Player tiers with bonuses
- 💰 **Economy System** - Coins, gems, multipliers
- 🎁 **Welcome Offers** - First-time purchase incentives

### Social Features
- 👥 **Friends System** - Add, manage, send gifts
- 💬 **Text Chat** - Global, room, and private messaging
- 🎤 **Voice Chat** - Vivox integration ready
- 🔗 **Account Linking** - Cross-platform play

### Monetization
- 💳 **IAP System** - Unity IAP integration
- 💵 **Payment Gateways** - Stripe, PayPal, Google Play
- 🎟️ **Virtual Currencies** - Coins and gems
- 🏆 **Tournaments** - Competitive play with entry fees

### Backend Services
- 🔐 **Authentication** - JWT-based auth
- 🛡️ **Security** - Rate limiting, IP logging, ban system
- 📊 **Analytics** - Transaction and security event tracking
- 🎮 **Tournament Scheduler** - Automated tournament management

## 🚢 Deployment Checklist

- [x] All compiler issues fixed
- [x] GitHub Actions workflows configured
- [ ] Choose and configure hosting service (Heroku/Railway/Render)
- [ ] Set up MongoDB database (MongoDB Atlas recommended)
- [ ] Configure environment variables
- [ ] Set up payment gateways (Stripe/PayPal)
- [ ] Configure Unity build settings for Android
- [ ] Test backend API endpoints
- [ ] Build and test Unity app
- [ ] Submit to Google Play (optional)

## 📚 Documentation

- [System Overview](Docs/SystemOverview.md)
- [Payment System](Docs/PaymentSystem.md)
- [Security System](Docs/SecuritySystem.md)
- [Account Linking](Docs/AccountLinking.md)
- [Android Build Guide](GooglePlay/BuildSettings/AndroidBuildChecklist.md)
- [Google Play Console Guide](GooglePlay/GooglePlayConsoleChecklist.md)

## 🔒 Security Notes

- Never commit `.env` files or secrets to the repository
- Use GitHub Secrets for all sensitive credentials
- Enable 2FA on all service accounts
- Regular security audits recommended
- Rate limiting is configured by default

## 📄 License

See [LICENSE](LICENSE) file for details.

## 🤝 Contributing

This is a private project. For issues or questions, contact the repository owner.

---

**Ready to deploy?** Choose a hosting option above and push to the `Main` branch to trigger automatic deployment!
