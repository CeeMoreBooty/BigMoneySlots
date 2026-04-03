# Checklist — Assets/Scenes

## MainScene.unity
- [ ] Scene opens without errors or missing-component warnings in Unity Console
- [ ] **GameManager** GameObject present — singleton, `DontDestroyOnLoad`
- [ ] **PlayerEconomy** reference wired in GameManager Inspector
- [ ] **BackendClient** GameObject present — `DontDestroyOnLoad`
- [ ] **SlotMachine** GameObject present with all 3 Reel child GameObjects assigned
- [ ] **SlotCanvas** (UI root) present with correct Canvas Scaler settings:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1080 × 1920
  - Match Width or Height: 0.5
- [ ] **SlotUI** component wired: Spin button, coin label, bet controls, win display
- [ ] **JackpotUI** component wired: jackpot label
- [ ] **ChatUI** panel present and initially hidden (`SetActive(false)`)
- [ ] **FriendsUI** panel present and initially hidden
- [ ] **DirectMessageUI** panel present and initially hidden
- [ ] **InviteUI** panel present and initially hidden
- [ ] **AccountLinkingUI** panel present and initially hidden
- [ ] **WelcomeOfferUI** panel present and initially hidden
- [ ] **RewardManager** GameObject present
- [ ] **AudioSource** components present and clips assigned (spin, win, bigwin, click, coins)
- [ ] **EventSystem** GameObject present (required for UI input)
- [ ] No duplicate cameras — only one Main Camera
- [ ] Camera background colour set (no default sky render on mobile)

## Build Settings
- [ ] `MainScene.unity` is the only scene in `EditorBuildSettings` (index 0)
- [ ] Scene committed to version control with `.meta` file

## Testing
- [ ] Scene runs in Unity Editor Play Mode without exceptions
- [ ] Spin flow tested end-to-end in Editor (bet → spin → result → balance update)
- [ ] All UI panels open and close without errors
- [ ] Game runs on a physical Android device without frame-rate drops below 30 FPS
