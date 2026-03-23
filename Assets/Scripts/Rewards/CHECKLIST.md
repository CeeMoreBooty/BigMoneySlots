# Checklist — Assets/Scripts/Rewards

## DailyChallenges.cs
- [ ] Three challenges generated at midnight — new seed each day
- [ ] Challenge types implemented: spin N times, win N coins, hit jackpot, play N minutes
- [ ] Progress values stored as `long` strings in `PlayerPrefs` (supports 5 B coin goals)
- [ ] Reset timestamp stored and checked on each app launch / resume
- [ ] `UpdateProgress(ChallengeType, long amount)` increments correctly and clamps at target
- [ ] Completion popup fires exactly once per challenge per day
- [ ] Gem reward granted via `GemSystem.AddGems()` on completion
- [ ] UI updates live during a session (event fired on progress change)

## GemSystem.cs
- [ ] Gem balance persisted in `PlayerPrefs`
- [ ] `AddGems(int amount)` and `SpendGems(int amount)` are the only mutation points
- [ ] `CanAffordGems(int cost)` used before every spend call
- [ ] `OnGemsChanged` event fires — `SlotUI` or HUD subscribed
- [ ] Gem balance synced with backend on launch (server is source of truth)

## InviteRewardManager.cs
- [ ] `FetchInviteCode()` calls `GET /api/invite/code` on launch and caches result
- [ ] `RedeemCode(string code)` calls `POST /api/invite/redeem` and shows result popup
- [ ] Error messages shown for own-code, already-redeemed, and invalid-code cases
- [ ] Gem reward from backend response applied via `GemSystem.AddGems()`
- [ ] `InviteUI` reference assigned in Inspector

## LoyaltySystem.cs
- [ ] Login streak stored in `PlayerPrefs` as day-count integer
- [ ] Streak increments only once per calendar day (UTC date comparison)
- [ ] Streak resets to 1 if more than 24 hours missed
- [ ] Reward tiers defined: day 1–7, day 8–14, day 15–30, day 30+
- [ ] Coin rewards escalate with streak length
- [ ] `RewardManager.ShowReward()` called to display grant popup
- [ ] Streak milestone popup (e.g. "7-day streak!") shown at key thresholds

## RewardManager.cs
- [ ] `ShowCoinReward(long amount)`, `ShowGemReward(int amount)`, `ShowSpinReward(int amount)` all implemented
- [ ] Popup animates in and auto-dismisses after configurable duration
- [ ] Reward actually credited to `PlayerEconomy` / `GemSystem` before popup shown
- [ ] Multiple rewards can be queued — shown sequentially, not overlapping
- [ ] Sound effect played for each reward type

## General
- [ ] All scripts compile with zero errors/warnings in Unity 2022.3.20f1
- [ ] `PlayerPrefs` keys documented — no key collisions between reward systems
- [ ] EditMode tests for DailyChallenges: reset logic, progress clamping, streak calculation
