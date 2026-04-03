# Android Build Settings Checklist

## Unity Player Settings (Edit > Project Settings > Player > Android)

### Identity
- [ ] Package Name: `com.yourstudio.bigmoneyslots`
- [ ] Version: `1.0`
- [ ] Bundle Version Code: `1`

### Icon & Splash
- [ ] Assign Adaptive Icon (Foreground + Background layers)
- [ ] Assign legacy icon (512×512 PNG)
- [ ] Set Splash Screen image and background color

### Resolution & Presentation
- [ ] Default Orientation: Portrait (or Landscape — choose one)
- [ ] Disable Status Bar: ✅

### Other Settings
- [ ] Scripting Backend: **IL2CPP** (required by Google Play)
- [ ] Target Architectures: ✅ **ARMv7** + ✅ **ARM64** (both required)
- [ ] Minimum API Level: **API 23 (Android 6.0 Marshmallow)**
- [ ] Target API Level: **API 34 (Android 14)** or latest stable
- [ ] Internet Access: **Require**
- [ ] Write Permission: External (SDCard) — if saving files externally

### Publishing Settings
- [ ] Create a Keystore (`Edit > Project Settings > Player > Publishing Settings`)
- [ ] Set Keystore path, password, alias, alias password
- [ ] Enable Custom Keystore: ✅
- [ ] Build App Bundle (.aab): ✅ (`File > Build Settings > Build App Bundle`)

### Stripping
- [ ] Managed Stripping Level: **Minimal** (avoids stripping needed Unity IAP code)

---

## Build
- [ ] Switch Platform to Android (`File > Build Settings > Android > Switch Platform`)
- [ ] Enable Development Build for testing: ✅ (disable before final release)
- [ ] Click **Build** → produces `.aab` file for upload to Google Play
