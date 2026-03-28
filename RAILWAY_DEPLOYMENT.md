# 🚂 Railway Deployment Guide for BigMoneySlots

This guide walks you through deploying the BigMoneySlots backend to Railway.app.

## 🎯 What is Railway?

Railway is a modern cloud platform that makes deploying applications incredibly simple:
- **Zero Config Deployments** - Railway auto-detects your Node.js app
- **Instant Database** - Add MongoDB with one click
- **Automatic HTTPS** - Get a secure URL automatically
- **Environment Variables** - Easy configuration in the dashboard
- **Free Tier** - $5/month free credit to get started

## 📋 Prerequisites

1. **Railway Account** - Sign up at https://railway.app
2. **GitHub Account** - Railway deploys directly from your repository
3. **MongoDB** - Use Railway's MongoDB plugin (easiest) or external MongoDB Atlas

## 🚀 Quick Start Deployment

### Step 1: Create New Project

1. Go to https://railway.app
2. Click **"New Project"**
3. Select **"Deploy from GitHub repo"**
4. Choose the **BigMoneySlots** repository
5. Select the **`railway-deployment`** branch

### Step 2: Add MongoDB Database

Railway will auto-detect your Node.js app. Now add MongoDB:

1. In your project, click **"+ New"**
2. Select **"Database"** → **"Add MongoDB"**
3. Railway automatically creates a `MONGO_URL` environment variable
4. Your app will connect to it automatically!

### Step 3: Configure Environment Variables

Go to your service → **Variables** tab and add the following:

#### Required Variables

```bash
# JWT Authentication
JWT_SECRET=your_long_random_jwt_secret_min_32_chars
JWT_EXPIRES_IN=30d

# Admin Access
ADMIN_SECRET=your_long_random_admin_secret

# Payment Services (if using)
STRIPE_SECRET_KEY=sk_test_your_key
STRIPE_PUBLISHABLE_KEY=pk_test_your_key
STRIPE_WEBHOOK_SECRET=whsec_your_secret

PAYPAL_CLIENT_ID=your_client_id
PAYPAL_CLIENT_SECRET=your_secret
PAYPAL_MODE=sandbox

# Google Play (if using)
GOOGLE_PLAY_PACKAGE_NAME=com.bigmoneyslots.app

# Business Email
OWNER_EMAIL=your-email@example.com

# Security
IP_LOGGING_ENABLED=true
```

#### Optional Variables

```bash
# Security Webhooks
SECURITY_WEBHOOK_ENABLED=false
SECURITY_WEBHOOK_URL=https://your-webhook.com
SECURITY_WEBHOOK_SECRET=your_secret
SECURITY_WEBHOOK_MIN_SEVERITY=medium
```

### Step 4: Deploy!

1. Railway automatically deploys when you add/update variables
2. Watch the deploy logs in the **Deployments** tab
3. Once deployed, Railway provides a public URL like: `https://bigmoneyslots-production.up.railway.app`

## 🔧 Advanced Configuration

### Custom Domain

1. Go to **Settings** → **Networking**
2. Click **"Generate Domain"** for a Railway subdomain
3. Or add your own custom domain:
   - Add your domain in Railway
   - Update your DNS with Railway's CNAME record
   - Railway handles SSL certificates automatically

### Environment-Specific Deployments

Deploy different branches to different environments:

**Production:**
- Branch: `main` or `railway-deployment`
- Environment: Production

**Staging:**
- Create a new service from the same repo
- Branch: `develop` or `staging`
- Use different environment variables

### Auto-Deploy on Push

Railway automatically redeploys when you push to the connected branch:

1. Push changes to `railway-deployment` branch
2. Railway detects the push
3. Automatically builds and deploys
4. Zero downtime deployments!

## 📦 Project Structure

Railway expects this structure (which is already configured):

```
BigMoneySlots/
├── Backend/              # Node.js application
│   ├── server.js        # Entry point
│   ├── package.json     # Dependencies
│   └── ...
├── railway.json         # Railway configuration
├── nixpacks.toml        # Build configuration
├── Procfile             # Process configuration
└── Backend/.env.railway # Environment variable template
```

## 🔍 Monitoring & Logs

### View Logs

1. Go to your service in Railway
2. Click **"Deployments"** tab
3. Click on any deployment to see logs
4. Real-time logs show startup, requests, and errors

### Metrics

Railway provides:
- CPU usage
- Memory usage
- Network traffic
- Request volume

Access metrics in the **Metrics** tab.

## 🐛 Troubleshooting

### Error 19: Build Failed

**Symptoms:** Railway shows "Error: 19" during build/deployment

**Cause:** Railway couldn't detect the correct build configuration or start command

**Fix:**
1. Ensure you're deploying the `railway-deployment` branch (not `main`)
2. Check that `railway.toml`, `nixpacks.toml`, and `Procfile` exist in the root
3. Verify `Backend/package.json` exists with correct start script
4. In Railway dashboard, go to **Settings** → **Deploy** and set:
   - **Build Command:** `npm install --prefix Backend`
   - **Start Command:** `node Backend/server.js`
5. Redeploy the service

### Build Fails

**Check:**
1. `package.json` exists in `Backend/` folder
2. All dependencies are listed in `package.json`
3. Build logs for specific errors

**Fix:**
- Railway runs `cd Backend && npm install`
- Ensure all deps are in `dependencies` (not `devDependencies`)

### Connection Errors

**MongoDB Connection:**
- Verify `MONGO_URL` is set (automatically by Railway MongoDB plugin)
- Or set `MONGO_URI` if using external MongoDB
- Check MongoDB service is running

**Port Issues:**
- Railway automatically sets `PORT` environment variable
- Server.js already uses `process.env.PORT || 3000`
- Don't hardcode the port

### App Crashes on Startup

1. Check deployment logs for errors
2. Verify all required environment variables are set
3. Test MongoDB connection string
4. Ensure `npm start` command works locally

## 💰 Pricing

Railway's free tier includes:
- **$5 free credit per month**
- **500 hours of usage**
- **1GB outbound network**

Typical costs for BigMoneySlots backend:
- Small app: **$5-15/month**
- Medium traffic: **$20-40/month**
- High traffic: **$50+/month**

## 🔐 Security Best Practices

1. **Never commit secrets** - Use Railway environment variables
2. **Use strong JWT_SECRET** - Minimum 32 random characters
3. **Enable IP logging** - Set `IP_LOGGING_ENABLED=true`
4. **Use environment-specific keys** - Different keys for dev/prod
5. **Regular backups** - Railway doesn't auto-backup MongoDB

## 📱 Connecting Unity App

After deployment, update your Unity app:

1. Get your Railway URL: `https://your-app.up.railway.app`
2. Update `Assets/Scripts/Backend/BackendClient.cs`:

```csharp
#if UNITY_EDITOR
    public const string BaseUrl = "http://localhost:3000";
#else
    public const string BaseUrl = "https://your-app.up.railway.app";
#endif
```

3. Rebuild your Unity app
4. Test connection to Railway backend

## 🚀 Deployment Checklist

- [ ] Railway account created
- [ ] Project created from GitHub repo
- [ ] MongoDB plugin added
- [ ] All environment variables configured
- [ ] JWT_SECRET set (min 32 chars)
- [ ] ADMIN_SECRET set
- [ ] Payment credentials configured (if using)
- [ ] Deployment successful (check logs)
- [ ] Health check passes: `https://your-app.up.railway.app/health`
- [ ] Unity app updated with Railway URL
- [ ] Test API endpoints from Unity app

## 🆘 Support

- **Railway Docs**: https://docs.railway.app
- **Railway Discord**: https://discord.gg/railway
- **Railway Status**: https://status.railway.app

## 📚 Additional Resources

- [Railway Templates](https://railway.app/templates)
- [Node.js on Railway](https://docs.railway.app/guides/nodejs)
- [MongoDB on Railway](https://docs.railway.app/databases/mongodb)
- [Custom Domains](https://docs.railway.app/guides/public-networking#custom-domains)

---

## 🎉 You're Done!

Your BigMoneySlots backend is now running on Railway with:
- ✅ Automatic deployments on git push
- ✅ MongoDB database
- ✅ HTTPS encryption
- ✅ Environment variable management
- ✅ Real-time logs and metrics

**Next Steps:**
1. Test your API endpoints
2. Connect your Unity app
3. Monitor your deployments
4. Scale as needed!

Happy deploying! 🚂✨
