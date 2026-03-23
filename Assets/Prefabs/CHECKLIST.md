# Checklist — Assets/Prefabs

## FriendRowPrefab.prefab
- [ ] Displays: friend avatar (placeholder icon), display name, online/offline indicator
- [ ] "Remove" button calls `FriendsManager.RemoveFriend()` with confirmation dialog
- [ ] Row height consistent (recommended 80–100 px at 1080p)
- [ ] `FriendRowPrefab` script component has no missing references
- [ ] Pooled by `FriendsUI` — not Instantiated/Destroyed per frame

## InboxRowPrefab.prefab
- [ ] Displays: friend name, last message preview (truncated to 40 chars), unread badge
- [ ] Tap opens `DirectMessageUI` for the correct `friendId`
- [ ] Unread count badge hides when count = 0
- [ ] `InboxRowPrefab` script component has no missing references
- [ ] Pooled by `DirectMessageUI` thread list

## MessageBubblePrefab.prefab
- [ ] Displays: sender name, message text, timestamp (HH:mm)
- [ ] Own messages right-aligned, others left-aligned (via layout group anchor)
- [ ] Text wraps correctly for long messages — no overflow outside bubble
- [ ] Bubble background 9-sliced for correct resize at any text length
- [ ] `MessageBubblePrefab` script component has no missing references
- [ ] Pooled by `ChatUI` — not Instantiated per incoming message

## MessageRowPrefab.prefab
- [ ] Same display as `MessageBubblePrefab` but used specifically in DM thread view
- [ ] Consistent visual style with `MessageBubblePrefab`
- [ ] Pooled by `DirectMessageUI`

## General
- [ ] All prefabs open without missing script warnings in Unity
- [ ] All prefab variants have `.prefab.meta` files committed
- [ ] Prefab hierarchy is minimal — no unnecessary child GameObjects
- [ ] Canvas Scaler in any nested Canvas set to Scale With Screen Size
- [ ] All text components use TextMeshPro (not legacy `Text`)
