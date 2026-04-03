# Big Money Slots — Deployment Checklist

**Copyright © 2024 CeeMoreBooty / Big Money Slots. All Rights Reserved.**

---

## ✅ Backend Deployment

### Prerequisites
- Node.js v18+
- MongoDB Atlas account (or self-hosted MongoDB)
- Google Cloud service account (for Google Play billing)
- Stripe account (for card payments)
- PayPal Developer account

### 1. Environment Variables (`.env`)
Copy `.env.example` to `.env` and fill in every value:

| Variable | Description | Required |
|---|---|---|
| `PORT` | Server port (default 3000) | Optional |
| `MONGO_URI` | MongoDB connection string | ✅ |
| `JWT_SECRET` | Long random string for JWT signing | ✅ |
| `JWT_EXPIRES_IN` | Token expiry e.g. `30d` | ✅ |
| `GOOGLE_APPLICATION_CREDENTIALS` | Path to Google service account JSON | ✅ |
| `GOOGLE_PLAY_PACKAGE_NAME` | `com.ceemorebooty.bigmoneyslots` | ✅ |
| `STRIPE_SECRET_KEY` | `sk_live_...` | ✅ |
| `STRIPE_PUBLISHABLE_KEY` | `pk_live_...` | ✅ |
| `STRIPE_WEBHOOK_SECRET` | `whsec_...` | ✅ |
| `PAYPAL_CLIENT_ID` | PayPal live client ID | ✅ |
| `PAYPAL_CLIENT_SECRET` | PayPal live client secret | ✅ |
| `PAYPAL_MODE` | `live` | ✅ |
| `ADMIN_SECRET` | Long random string for admin API access | ✅ |
| `SECURITY_WEBHOOK_ENABLED` | `true` | Optional |
| `SECURITY_WEBHOOK_URL` | Your HTTPS endpoint | Optional |
| `SECURITY_WEBHOOK_SECRET` | HMAC signing secret | Optional |
| `ADMIN_DISCORD_WEBHOOK_ENABLED` | `true` | Optional |
| `ADMIN_DISCORD_WEBHOOK_URL` | Discord Incoming Webhook URL | Optional |
| `IP_LOGGING_ENABLED` | `true` | Optional |

### 2. Install and Start
```bash
cd Backend
npm install --production
npm start
```

### 3. HTTPS / Reverse Proxy
- Set up Nginx or Caddy as a reverse proxy to the Node.js server
- Obtain SSL certificate (Let's Encrypt / Certbot recommended)
- Update `BackendClient.cs` in Unity with your production URL

### 4. MongoDB Indexes
The security and analytics models benefit from indexes — ensure MongoDB Atlas
has indexes on `SecurityEvent.createdAt`, `BannedEntity.value`, and `IpLog.ip`.

### 5. Verify Backend Health
```
GET https://your-domain.com/health
```
Should return: `{"status":"ok","time":"..."}`

---

## ✅ Unity / Android Build

### Prerequisites
- Unity 2021.3.45f2
- Android Build Support module installed in Unity Hub
- Android SDK API 33 + NDK
- Java JDK 11+
- Google Play Console access

### Build Settings Checklist

| Setting | Value |
|---|---|
| Platform | Android |
| Package Name | `com.ceemorebooty.bigmoneyslots` |
| Product Name | Big Money Slots |
| Version | Set appropriately (e.g. 1.0.0) |
| Bundle Version Code | Increment for each upload |
| Scripting Backend | IL2CPP |
| Target Architectures | ARM64 ✅ |
| Minimum API Level | 23 (Android 6.0) |
| Target API Level | 33 (Android 13) |
| Internet Permission | Required ✅ |
| Keystore | Configured ✅ |

### Steps
1. Open Unity → File → Build Settings → Android
2. Switch Platform to Android
3. Player Settings:
   - Set `Bundle Identifier`: `com.ceemorebooty.bigmoneyslots`
   - Set Version and Bundle Version Code
   - IL2CPP scripting backend, ARM64 target
   - Add Internet permission
4. Set keystore (Project Settings → Player → Publishing Settings)
5. Build → Build And Run (or build APK/AAB)
6. For Google Play: build an **AAB** (`.aab`)

### Pre-Build Checks
- [ ] BackendClient.cs `BaseUrl` points to production server
- [ ] All scene indices correct (Bootstrap=0, MainMenu=1, Game=2)
- [ ] No Console errors or warnings in Editor Play mode
- [ ] Test first spin in Editor — no null refs
- [ ] Keystore password stored securely (NOT in source control)

---

## ✅ Google Play Console

See `GooglePlay/GooglePlayConsoleChecklist.md` for the full upload guide.

Key steps:
1. Upload AAB to Internal Testing track
2. Fill in Store Listing (title, description, screenshots)
3. Set Content Rating (includes gambling category)
4. Add Privacy Policy URL
5. Add Terms of Service URL (link to TERMS_AND_CONDITIONS.md hosted page)
6. Configure In-App Products (product IDs must match `PRODUCTS` in payments.js)
7. Promote to Production once review passes

---

## ✅ Discord Admin Webhook Setup

1. Go to your Discord server
2. Server Settings → Integrations → Webhooks → New Webhook
3. Name it "Big Money Slots Admin"
4. Choose your admin channel
5. Copy the Webhook URL
6. Add to your server's `.env` file:
   ```
   ADMIN_DISCORD_WEBHOOK_ENABLED=true
   ADMIN_DISCORD_WEBHOOK_URL=<paste URL here>
   ```
7. Restart the server — you'll receive a 🟢 Server Started notification

---

## ✅ Security Hardening (Production)

- [ ] `ADMIN_SECRET` is a strong random string (32+ chars)
- [ ] `JWT_SECRET` is a strong random string (64+ chars)  
- [ ] `.env` is NOT committed to git (check `.gitignore`)
- [ ] HTTPS enforced on all endpoints
- [ ] Rate limiting is active (120 req / 15 min per IP)
- [ ] IP logging enabled
- [ ] Security Guard middleware active
- [ ] MongoDB access restricted to server IP only
- [ ] Discord webhook configured and tested

---

*Copyright © 2024 CeeMoreBooty / Big Money Slots. All Rights Reserved.*
