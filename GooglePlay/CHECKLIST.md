# Checklist — GooglePlay

## GooglePlayConsoleChecklist.md
- [ ] Reviewed and all steps worked through in Play Console
- [ ] App created with correct package name (`com.yourstudio.bigmoneyslots`)
- [ ] Internal testing release uploaded and tested on real device
- [ ] Production release submitted and approved

## BuildSettings/AndroidBuildChecklist.md
- [ ] All Unity Player Settings configured per checklist
- [ ] Keystore created, backed up, and passwords stored securely
- [ ] AAB built with Development Build **OFF**
- [ ] Bundle Version Code incremented before each new release upload

## Store Listing Assets (to be created)
- [ ] `StoreListing/short_description.txt` — max 80 characters
- [ ] `StoreListing/description.txt` — max 4000 characters, highlights features
- [ ] `StoreListing/privacy_policy.txt` (or hosted URL) — required for 18+ / casino apps
- [ ] `StoreListing/Graphics/app_icon_512x512.png` — 512×512 PNG, no alpha
- [ ] `StoreListing/Graphics/feature_graphic_1024x500.png` — 1024×500 PNG or JPG
- [ ] `StoreListing/Screenshots/phone_1.png` through `phone_8.png` — min 2, max 8
- [ ] `StoreListing/Screenshots/tablet_7inch_1.png` — optional but recommended
- [ ] `StoreListing/Screenshots/tablet_10inch_1.png` — optional

## Content & Policy
- [ ] IARC content rating questionnaire completed (Gambling / Casino category)
- [ ] Target audience set to 18+ in App Content section
- [ ] Data safety form completed:
  - [ ] Device or other identifiers collected: ✅ (device ID)
  - [ ] Financial info collected: ✅ (purchase history)
  - [ ] Data encrypted in transit: ✅
  - [ ] Users can request deletion: declare policy
- [ ] Privacy policy published at a reachable public URL
- [ ] Privacy policy URL entered in Play Console

## In-App Products
- [ ] `welcomeofferpack` — Active, correct price
- [ ] `gems_100` — Active
- [ ] `gems_500` — Active
- [ ] `gems_1200` — Active
- [ ] `gems_2500` — Active
- [ ] `gems_6500` — Active
- [ ] All product IDs match exactly what is in `IAPHandler.cs`

## Post-Launch
- [ ] Android Vitals monitored weekly (crash rate, ANR rate)
- [ ] User reviews responded to within 48 hours
- [ ] Target API Level updated each year per Google's annual requirement
- [ ] Bundle Version Code incremented for every update
