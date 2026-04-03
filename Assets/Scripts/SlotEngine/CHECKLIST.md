# Checklist — Assets/Scripts/SlotEngine

## SlotMachine.cs
- [ ] `Spin()` is guarded — cannot be called while reels are already spinning
- [ ] Bet deducted from `PlayerEconomy` before reels start, refunded on error
- [ ] All three `Reel` references assigned in Inspector
- [ ] Win result evaluated only after all three reels have stopped
- [ ] `OnSpinComplete` event fires with full result (symbols, payout, jackpot flag)
- [ ] `SlotUI` subscribed to `OnSpinComplete` — not polled in `Update`
- [ ] Free-spin mode supported — bet not deducted when `freeSpinsRemaining > 0`

## Reel.cs
- [ ] Uses `SecureRandom` (not `UnityEngine.Random`) for stop position
- [ ] `SymbolData[]` array populated from assigned `SlotGameConfig` — not hard-coded
- [ ] Spin animation coroutine driven by `SlotGameConfig.reelSpinDuration`
- [ ] `OnReelStopped` callback/event fires when animation completes
- [ ] Stop position index exposed as `int StopIndex { get; }` for payout evaluation

## Symbol.cs / PayoutTable.cs
- [ ] `SymbolData` ScriptableObject has: name, sprite, isWild, isScatter, and per-count multipliers
- [ ] `PayoutTableData` covers all 3-symbol combinations including Wild substitutions
- [ ] Wild symbol correctly substitutes in payout calculation
- [ ] Scatter symbol payout (free spins) handled separately from line payout
- [ ] All three payout tables (Classic, HighRoller, Penny) have entries for every symbol combo

## SecureRandom.cs
- [ ] Uses `System.Security.Cryptography.RNGCryptoServiceProvider`
- [ ] `NextInt(int min, int max)` produces uniformly distributed results (no modulo bias)
- [ ] Disposed properly — `IDisposable` implemented or static instance managed

## ProgressiveJackpot.cs
- [ ] Jackpot seed value set in `SlotGameConfig` (not hard-coded)
- [ ] Contribution percentage per spin set in config (e.g. 1%)
- [ ] Jackpot balance persisted in `PlayerPrefs` between sessions
- [ ] `OnJackpotChanged` event fires after every contribution — `JackpotUI` subscribed
- [ ] Jackpot resets to seed value immediately after payout
- [ ] Jackpot win triggers `bigwin.wav` audio and a dedicated celebration UI

## SlotGameConfig / Database / Loader / Registry
- [ ] All three `SlotGameConfig` assets (Classic, JungleJackpot, PennyParadise) referenced in `SlotGameRegistry`
- [ ] `SlotGameLoader` applies config at runtime without scene reload
- [ ] `SlotGameDatabase` built from Registry on `Awake` — no duplicates
- [ ] Switching game variant swaps: reel skin sprite, symbol set, payout table, min bet

## General
- [ ] All scripts compile with zero errors/warnings in Unity 2022.3.20f1
- [ ] EditMode unit tests cover: payout calculation, Wild substitution, jackpot accumulation
- [ ] PlayMode smoke test: 10 spins complete without exception
