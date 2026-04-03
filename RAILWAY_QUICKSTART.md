# 🚂 Railway Quick Start

## One-Click Deploy to Railway

[![Deploy on Railway](https://railway.app/button.svg)](https://railway.app/template/bigmoneyslots)

## Manual Setup (5 Minutes)

### 1. Create Railway Project
```bash
# Visit https://railway.app
# Click "New Project"
# Select "Deploy from GitHub repo"
# Choose: BigMoneySlots → railway-deployment branch
```

### 2. Add MongoDB
```bash
# In your Railway project dashboard:
# Click "+ New" → "Database" → "MongoDB"
# Railway automatically sets MONGO_URL
```

### 3. Set Environment Variables
```bash
# Go to your service → "Variables" tab
# Click "RAW Editor" and paste:

JWT_SECRET=generate_a_long_random_string_here_min_32_chars
ADMIN_SECRET=another_long_random_string_for_admin_access
OWNER_EMAIL=your-email@example.com
IP_LOGGING_ENABLED=true
```

### 4. Deploy!
```bash
# Railway auto-deploys when variables are saved
# Get your URL: https://bigmoneyslots-production.up.railway.app
# Test: https://your-url.railway.app/health
```

## Update Unity App

Edit `Assets/Scripts/Backend/BackendClient.cs`:
```csharp
#if UNITY_EDITOR
    public const string BaseUrl = "http://localhost:3000";
#else
    public const string BaseUrl = "https://your-app.up.railway.app";  // Your Railway URL
#endif
```

## Need Help?

See full guide: [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md)

## Cost

- **Free tier**: $5 credit/month (perfect for development)
- **Production**: ~$5-20/month depending on traffic
- **MongoDB**: Included with Railway plugin

---

**That's it! Your backend is live! 🎉**
