# Google Play Console Setup Checklist

## Step 1 — Create the App
- [ ] Go to https://play.google.com/console
- [ ] Click **Create app**
- [ ] App name: `Big Money Slots`
- [ ] Default language: `English (United States)`
- [ ] App or Game: **Game**
- [ ] Free or Paid: **Free** (with in-app purchases)
- [ ] Accept declarations

---

## Step 2 — Upload the Build
- [ ] Go to **Testing > Internal testing** (or Closed/Open/Production)
- [ ] Create a new release
- [ ] Upload your `.aab` file from Unity build
- [ ] Add release notes (e.g., "Initial release")
- [ ] Save and review

---

## Step 3 — Store Listing
- [ ] App name: `Big Money Slots`
- [ ] Short description (max 80 chars): see `StoreListing/short_description.txt`
- [ ] Full description (max 4000 chars): see `StoreListing/description.txt`
- [ ] Upload Feature Graphic (1024×500 PNG/JPG) → `StoreListing/Graphics/`
- [ ] Upload App Icon (512×512 PNG) → `StoreListing/Graphics/`
- [ ] Upload Phone Screenshots (min 2, max 8) → `StoreListing/Screenshots/`
- [ ] Upload 7-inch Tablet Screenshots (optional)
- [ ] Upload 10-inch Tablet Screenshots (optional)

---

## Step 4 — Content Rating
- [ ] Go to **Policy > App content > Content rating**
- [ ] Fill out questionnaire (select: Gambling simulation)
- [ ] Submit for rating → receive ESRB / PEGI / etc. ratings

---

## Step 5 — App Content Declarations
- [ ] Ads: declare if app contains ads
- [ ] In-app purchases: **Yes** (welcomeofferpack)
- [ ] Target audience: **18+** (gambling/casino content)
- [ ] Privacy policy URL: add URL (see `StoreListing/privacy_policy.txt` for template)
- [ ] Data safety form: complete (coins/purchases stored locally via PlayerPrefs)

---

## Step 6 — In-App Products
- [ ] Go to **Monetize > In-app products**
- [ ] Click **Create product**
  - Product ID: `welcomeofferpack`
  - Name: `Welcome Offer Pack`
  - Description: `150B coins, 200 free spins, VIP badge, golden reel skin & more!`
  - Price: set per region (e.g., $4.99 USD)
  - Status: **Active**
- [ ] Save

---

## Step 7 — Final Review & Submit
- [ ] Complete all required sections (green checkmarks in Dashboard)
- [ ] Go to **Production > Releases**
- [ ] Promote build from Internal Testing to Production (or create Production release)
- [ ] Submit for Google review (typically 1–3 days)
