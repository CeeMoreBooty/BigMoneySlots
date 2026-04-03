# Checklist — Assets/Scripts/UI

## SlotUI.cs
- [ ] Spin button wired to `SlotMachine.Spin()` via `onClick` listener
- [ ] Spin button disabled while reels are spinning (re-enabled in `OnSpinComplete`)
- [ ] Coin balance display subscribed to `PlayerEconomy.OnCoinsChanged`
- [ ] Bet amount controls (min, max, +, −) update `SlotMachine.currentBet`
- [ ] Insufficient-funds check disables Spin button when `coins < minBet`
- [ ] Win display shown after `OnSpinComplete` fires — cleared before next spin
- [ ] Big-win animation triggered when payout ≥ `SlotGameConfig.bigWinThreshold`
- [ ] Jackpot value label subscribed to `ProgressiveJackpot.OnJackpotChanged`
- [ ] Free-spin counter visible and accurate during free-spin mode
- [ ] All Inspector references (buttons, text fields, panels) assigned — no `Find` calls

## ChatUI.cs
- [ ] Message input field sends on Enter key as well as send button
- [ ] Message list scrolls to bottom when new message arrives
- [ ] Own messages visually distinct (right-aligned, different colour)
- [ ] Long messages truncated in display with "show more" if needed
- [ ] Empty / whitespace input blocked before send
- [ ] `MessageBubblePrefab` pooled — not Instantiated per message at runtime
- [ ] Channel tab (Global / Room) switches correctly

## DirectMessageUI.cs
- [ ] Opens correct thread for the selected friend ID
- [ ] `GET /api/chat/dm/thread/:friendId` fetched on open
- [ ] Infinite scroll / pagination for old messages (load earlier)
- [ ] Send reply calls `POST /api/chat/send` with `targetId` set
- [ ] Thread list (`GET /api/chat/dm/threads`) refreshed when DM panel opens

## FriendsUI.cs
- [ ] Friends list and pending-requests list both populated from `FriendsManager`
- [ ] Accept / Decline buttons visible only on pending rows
- [ ] Remove button requires confirmation dialog before firing
- [ ] Search input debounced (300 ms) — not one call per keystroke
- [ ] `FriendRowPrefab` pooled — not Instantiated per friend entry
- [ ] Empty-state message shown when list is empty

## InviteUI.cs
- [ ] Invite code fetched from backend on open — displays loading state during fetch
- [ ] "Copy to clipboard" button works on Android (`GUIUtility.systemCopyBuffer`)
- [ ] Redemption input validated — empty/whitespace not submitted
- [ ] Success and error states shown clearly after redeem attempt
- [ ] Gem reward amount displayed in success message

## JackpotUI.cs
- [ ] Subscribed to `ProgressiveJackpot.OnJackpotChanged` event
- [ ] Counter animates smoothly to new value (lerp or tween — not instant jump)
- [ ] Jackpot win celebration animation plays on `OnJackpotWon` event
- [ ] Formatted with comma separators (e.g. "1,234,567")

## AccountLinkingUI.cs
- [ ] All four provider buttons (Google, Facebook, Discord, Phone) wired in Inspector
- [ ] Linked state shown with a ✓ / green indicator per provider
- [ ] Unlink button shown only for already-linked providers
- [ ] Bonus-already-claimed state communicated to user (no misleading "get bonus" prompt)

## General
- [ ] All scripts compile with zero errors/warnings in Unity 2022.3.20f1
- [ ] All UI panels open/close via `SetActive` — no scene loading for panels
- [ ] Back-button (Android hardware back) closes the topmost open panel
- [ ] UI looks correct at 1080×1920, 1080×2340, and 720×1280 resolutions
- [ ] Text does not overflow or clip at any of the above resolutions
