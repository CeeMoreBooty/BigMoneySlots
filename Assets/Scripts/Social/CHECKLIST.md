# Checklist — Assets/Scripts/Social

## AccountLinking.cs
- [ ] Supported providers: `google`, `facebook`, `discord`, `phone`
- [ ] `LinkAccount(string provider, string token)` calls `POST /api/account/link`
- [ ] `UnlinkAccount(string provider)` calls `DELETE /api/account/link/:provider`
- [ ] `LoadLinkedAccounts()` calls `GET /api/account/links` on profile screen open
- [ ] One-time coin bonus credited from server response (`newBalance` applied to `PlayerEconomy`)
- [ ] Error shown if provider already linked to another account (409 response)
- [ ] `AccountLinkingUI` notified via event on link/unlink success

## ChatManager.cs
- [ ] Socket.IO connection established using `BackendClient` JWT token
- [ ] Reconnection logic handles drops gracefully (exponential backoff)
- [ ] `SendMessage(string text, string channel)` calls `POST /api/chat/send`
- [ ] Incoming real-time messages dispatched to `ChatUI` via event
- [ ] Text sanitised client-side before send (strip leading/trailing whitespace, max 200 chars enforced)
- [ ] Channel switching (global → room) clears message list and fetches new history
- [ ] `GET /api/chat/history` called on channel open to preload recent messages

## FriendsManager.cs
- [ ] `LoadFriends()` calls `GET /api/friends` on friends panel open
- [ ] `SearchPlayers(string query)` calls `GET /api/friends/search?q=` with debounce (300 ms)
- [ ] `SendRequest(string targetId)` calls `POST /api/friends/request`
- [ ] `AcceptRequest(string requesterId)` calls `POST /api/friends/accept`
- [ ] `DeclineRequest(string requesterId)` calls `POST /api/friends/decline`
- [ ] `RemoveFriend(string friendId)` calls `POST /api/friends/remove`
- [ ] `FriendsUI` updated immediately on success (optimistic UI or re-fetch)
- [ ] Error message shown for duplicate requests and self-requests

## VoiceChatManager.cs
- [ ] `JoinVoiceRoom(string roomId)` calls `POST /api/chat/voice/join`
- [ ] `LeaveVoiceRoom(string roomId)` calls `POST /api/chat/voice/leave`
- [ ] Leave called automatically in `OnApplicationPause(true)` and `OnDestroy`
- [ ] Microphone permission requested before joining (Android runtime permission)
- [ ] Mute/unmute toggle implemented
- [ ] Active voice room indicator shown in `ChatUI`

## General
- [ ] All scripts compile with zero errors/warnings in Unity 2022.3.20f1
- [ ] `BackendClient` reference obtained via singleton — not via `FindObjectOfType`
- [ ] All network calls handle offline gracefully (show "No connection" message)
- [ ] No PII logged to Unity console in production builds
