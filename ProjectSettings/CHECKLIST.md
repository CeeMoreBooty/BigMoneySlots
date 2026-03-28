# Checklist — ProjectSettings

## ProjectVersion.txt
- [ ] Unity version is `2022.3.20f1` — all team members use this exact version
- [ ] Unity Hub configured to open the project with `2022.3.20f1` (not a newer/older version)

## ProjectSettings.asset
- [ ] Company Name: `BigMoneySlotsStudio` (or your actual studio name)
- [ ] Product Name: `BigMoneySlots`
- [ ] Bundle Identifier: `com.yourstudio.bigmoneyslots` — matches Play Console exactly
- [ ] Version: `1.0`
- [ ] Bundle Version Code: `1` (increment for every Play Store upload)
- [ ] Default Icon set (512×512 PNG assigned)
- [ ] Scripting Backend: **IL2CPP** (required by Google Play)
- [ ] Api Compatibility Level: **.NET Standard 2.1**
- [ ] Target Architectures: ✅ ARMv7 + ✅ ARM64
- [ ] Minimum API Level: 23 (Android 6.0)
- [ ] Target API Level: 34 (Android 14)
- [ ] Internet Access: **Require**
- [ ] Write Permission: External (SD Card) — only if saving files outside app sandbox
- [ ] Active Input Handling: **Both** or **New Input System** (confirm no legacy-only Input calls)

## EditorBuildSettings.asset
- [ ] `Assets/Scenes/MainScene.unity` is the only scene, at index 0
- [ ] Scene is enabled (checkbox ticked)

## AudioManager.asset
- [ ] Global volume set to 1 (not accidentally muted)
- [ ] Audio Mixer asset assigned if using Unity Audio Mixer groups

## GraphicsSettings.asset
- [ ] Rendering path appropriate for 2D/UI (Forward rendering)
- [ ] No desktop-only render features enabled (deferred, HDRP, etc.)
- [ ] Lightmap settings: Realtime off, Baked off (2D slot game — no lightmapping needed)

## QualitySettings.asset
- [ ] Quality levels trimmed — only the levels needed for mobile remain
- [ ] VSync disabled on mobile (target frame rate set in code: `Application.targetFrameRate = 60`)
- [ ] Texture Quality: Full Res

## Physics2DSettings.asset & DynamicsManager.asset
- [ ] Gravity set correctly (if any physics used — likely not for a slots UI game)
- [ ] Consider disabling physics simulation entirely if no physics objects used (performance)

## InputManager.asset
- [ ] Using New Input System OR legacy Input Manager — confirm consistent with scripts
- [ ] Android Back button mapped if using legacy Input Manager

## TagManager.asset
- [ ] Custom tags/layers added only if used by scripts (keep minimal)

## General
- [ ] All `.asset` files committed to version control
- [ ] No local ProjectSettings overrides that differ from the committed versions
- [ ] Project opens cleanly on a fresh clone with no import errors
