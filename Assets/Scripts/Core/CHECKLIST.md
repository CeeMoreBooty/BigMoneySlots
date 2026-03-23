# Checklist — Assets/Scripts/Core

## GameManager.cs
- [ ] `Awake` singleton pattern prevents duplicate instances across scene reloads
- [ ] All system references (SlotMachine, PlayerEconomy, BackendClient, RewardManager) assigned in Inspector
- [ ] `OnApplicationPause(true)` saves PlayerPrefs and flushes pending backend syncs
- [ ] `OnApplicationFocus(false)` triggers daily-bonus / login-streak check
- [ ] Daily bonus popup fires only once per calendar day (checked on resume/focus)
- [ ] No hard-coded magic numbers — use constants or config references

## PlayerEconomy.cs
- [ ] Starting coins (10,000) seeded only on first launch (`firstLaunch` flag checked)
- [ ] `AddCoins` / `SpendCoins` / `AddGems` / `SpendGems` / `AddSpins` / `SpendSpins` all save to `PlayerPrefs` immediately
- [ ] `SyncWithBackend()` called after every spin result — server balance is authoritative
- [ ] Server response overwrites local balance (no client-side inflation possible)
- [ ] Coin balance uses `long` (not `int`) to support values above 2.1 B
- [ ] Gem and spin balances use `int` (sufficient range)
- [ ] `PlayerPrefs` keys are unique string constants (no typos)
- [ ] Insufficient-funds check fires before deducting bet — `CanAffordBet(long bet)` used everywhere

## General
- [ ] Both scripts compile with zero errors and zero warnings in Unity 2022.3.20f1
- [ ] No `FindObjectOfType` calls in `Update` — cached in `Awake`/`Start` only
- [ ] Unit tests added to `Assets/Tests/EditMode/` for pure-logic methods (economy math, seeding)
