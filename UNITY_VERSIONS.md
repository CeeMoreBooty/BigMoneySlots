# Unity Version Compatibility — BigMoneySlots

This document lists all Unity versions compatible with BigMoneySlots, their support level, and any version-specific notes for developers.

---

## Primary Target Version

| Unity Version | Release | Support Level |
|---|---|---|
| **Unity 6.4 (6000.4.0f1)** | 2025 | ✅ **PRIMARY — Fully Supported** |

---

## Compatibility Matrix

| Unity Version | Status | Notes |
|---|---|---|
| **6.4 (6000.4.0f1)** | ✅ Primary Target | Full support — all features tested here |
| **6.3 (6000.3.x)** | ✅ Compatible | Functionally identical to 6.4 for this project |
| **6.2 (6000.2.x)** | ✅ Compatible | All features supported |
| **6.1 (6000.1.x)** | ✅ Compatible | All features supported |
| **6.0 (6000.0.x)** | ✅ Minimum Unity 6 | Unity 6 baseline — all APIs available |
| **2022.3 LTS** | ⚠️ With Adjustments | C# 9 features may need downgrade; see notes |
| **2021.3 LTS** | ❌ Not Supported | Missing APIs used in this project |
| **< 2021** | ❌ Not Supported | Do not use |

---

## C# Language Version Requirements

BigMoneySlots scripts use **C# 9** features. The following table shows the C# version available per Unity version:

| Unity Version | C# Version | Compatibility |
|---|---|---|
| Unity 6.x (6000.x) | C# 9 | ✅ Full support |
| Unity 2022.3 LTS | C# 9 (via .NET Standard 2.1) | ✅ Supported |
| Unity 2021.3 LTS | C# 8 | ❌ Breaking changes |

### C# Features Used in This Project

- `using var` declarations (C# 8+)
- Records / init-only properties (C# 9)
- Target-typed `new` expressions (C# 9)
- Nullable reference types (C# 8+)
- Pattern matching enhancements (C# 9)

---

## Required Unity Packages

The following packages must be installed via **Window → Package Manager**:

| Package | Minimum Version | Required For |
|---|---|---|
| **TextMeshPro** | 3.0.6+ | All UI text rendering (SlotUI, ChatUI, JackpotUI, etc.) |
| **Unity UI (uGUI)** | 1.0.0+ | Canvas-based UI components |
| **Unity Purchasing (IAP)** | 4.9.3+ | In-app purchases (IAPHandler.cs) |
| **Android Build Support** | Matches Unity | AAB / APK builds |

### Installing TextMeshPro
1. **Window → Package Manager**
2. Search for **TextMeshPro** → Install
3. **Window → TextMeshPro → Import TMP Essential Resources**

### Installing Unity IAP
1. **Window → Package Manager**
2. Search for **In App Purchasing** → Install
3. Enable in **Edit → Project Settings → Services → In-App Purchasing**

---

## Version-Specific Considerations

### Unity 6.x (Fully Supported)

- Uses `.NET Standard 2.1` or `.NET Framework 4.x` — either works
- IL2CPP scripting backend fully supported for Android
- ARM64 target architecture fully supported
- `UnityWebRequest` API unchanged from 2022.x
- Socket.IO via `NativeWebSocket` or WebGL-compatible WebSocket

### Unity 2022.3 LTS (With Adjustments)

If using Unity 2022.3 instead of Unity 6, the following adjustments may be needed:

1. **C# `using var` in IEnumerators**: Unity 2022.3 with Roslyn C# 9 supports this, but if you encounter errors, convert to explicit `using() { }` blocks:
   ```csharp
   // Before (C# 8+ syntax)
   using var req = UnityWebRequest.Get(url);
   
   // After (compatible with all versions)
   using (var req = UnityWebRequest.Get(url))
   {
       yield return req.SendWebRequest();
   }
   ```

2. **TextMeshPro**: Use version 3.0.6 — do NOT use TMP 4.x which ships with Unity 6.

3. **Android SDK**: Use API Level 31 as target (33 is fine in 2022.3 too).

4. **Project Settings → Player → Api Compatibility Level**: Set to `.NET Standard 2.1`.

### Unity 6.0 Minimum (6000.0.x)

- Unity 6.0 is the absolute minimum for Unity 6-series
- All scripts compile without modification
- Some UI layout improvements in 6.1+ may differ slightly

---

## ProjectSettings/ProjectVersion.txt

The project is configured for the primary target version:

```
m_EditorVersion: 6000.4.0f1
m_EditorVersionWithRevision: 6000.4.0f1 (a1b2c3d4e5f6)
```

> **Note:** You can open the project in any compatible Unity version listed above. Unity will automatically upgrade the project version when you save. Do NOT downgrade below Unity 6000.0.x.

---

## How to Switch Unity Versions

1. Open **Unity Hub**
2. Click the ▾ dropdown next to the project
3. Select **"Open with different editor version"**
4. Choose a version from the compatible list above
5. Accept any upgrade prompt — Unity will migrate the project files

---

## Recommended Development Setup

For the best experience, use the exact primary target version:

1. Open **Unity Hub → Installs → Install Editor**
2. Search for **6000.4.0f1** (or the closest available 6.x release)
3. Enable the following modules:
   - ✅ Android Build Support
   - ✅ Android SDK & NDK Tools
   - ✅ OpenJDK

---

*Last updated: March 28, 2026*
