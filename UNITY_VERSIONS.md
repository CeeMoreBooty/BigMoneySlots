# Unity Version Compatibility — BigMoneySlots

This document lists all Unity versions compatible with BigMoneySlots, their support level, and any version-specific notes for developers.

---

## Supported Versions

| Unity Version | Release | Support Level |
|---|---|---|
| **Unity 6 (6000.x)** | 2025 | ✅ **Fully Supported** |
| **Unity 2021.3.45f2** | 2024 LTS | ✅ **Fully Supported** |

---

## Compatibility Matrix

| Unity Version | Status | Notes |
|---|---|---|
| **6.4 (6000.4.0f1)** | ✅ Supported | Full support — all features tested |
| **6.3 (6000.3.x)** | ✅ Compatible | Functionally identical to 6.4 for this project |
| **6.2 (6000.2.x)** | ✅ Compatible | All features supported |
| **6.1 (6000.1.x)** | ✅ Compatible | All features supported |
| **6.0 (6000.0.x)** | ✅ Minimum Unity 6 | Unity 6 baseline — all APIs available |
| **2021.3.45f2** | ✅ Supported | LTS — all scripts compile clean |
| **2022.3 LTS** | ✅ Compatible | C# 9 features supported; see notes |
| **< 2021.2** | ❌ Not Supported | Missing C# 9 support |

---

## C# Language Version Requirements

BigMoneySlots scripts use **C# 9** features. The following table shows the C# version available per Unity version:

| Unity Version | C# Version | Compatibility |
|---|---|---|
| Unity 6.x (6000.x) | C# 9 | ✅ Full support |
| Unity 2022.3 LTS | C# 9 | ✅ Supported |
| Unity 2021.3 LTS (2021.2+) | C# 9 | ✅ Supported |
| Unity 2021.1 and earlier | C# 8 | ❌ Breaking changes |

### C# Features Used in This Project

- `using var` declarations (C# 8+)
- Switch expressions (C# 8+)
- Value tuples (C# 7+)
- String interpolation (C# 6+)
- Null-conditional operators (C# 6+)

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

### Unity 6.x

- Uses `.NET Standard 2.1` or `.NET Framework 4.x` — either works
- IL2CPP scripting backend fully supported for Android
- ARM64 target architecture fully supported
- `UnityWebRequest` API unchanged from 2022.x

### Unity 2021.3.45f2

- C# 9 supported (Unity 2021.2+ includes Roslyn with C# 9)
- TextMeshPro: Use version 3.0.6 — do NOT use TMP 4.x
- Android SDK: Use API Level 31+ as target
- Project Settings → Player → Api Compatibility Level: `.NET Standard 2.1`
- IL2CPP scripting backend supported for Android ARM64

### Unity 2022.3 LTS

- C# 9 fully supported
- TextMeshPro: Use version 3.0.6 — do NOT use TMP 4.x
- Android SDK: Use API Level 31–33 as target

---

## ProjectSettings/ProjectVersion.txt

The project ships with:

```
m_EditorVersion: 2021.3.45f2
m_EditorVersionWithRevision: 2021.3.45f2 (0da89fac8e79)
```

> **Note:** You can open the project in any compatible Unity version listed above. Unity will automatically upgrade the project version when you save.

---

## How to Switch Unity Versions

1. Open **Unity Hub**
2. Click the ▾ dropdown next to the project
3. Select **"Open with different editor version"**
4. Choose a version from the compatible list above
5. Accept any upgrade prompt — Unity will migrate the project files

---

## Recommended Development Setup

For the best experience, use one of the two primary target versions:

### Option A: Unity 6
1. Open **Unity Hub → Installs → Install Editor**
2. Search for **6000.4.0f1** (or the closest available 6.x release)
3. Enable the following modules:
   - ✅ Android Build Support
   - ✅ Android SDK & NDK Tools
   - ✅ OpenJDK

### Option B: Unity 2021.3.45f2
1. Open **Unity Hub → Installs → Install Editor**
2. Search for **2021.3.45f2**
3. Enable the following modules:
   - ✅ Android Build Support
   - ✅ Android SDK & NDK Tools
   - ✅ OpenJDK

---

*Last updated: April 5, 2026*
