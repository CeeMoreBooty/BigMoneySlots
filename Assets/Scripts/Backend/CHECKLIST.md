# Checklist — Assets/Scripts/Backend

## BackendClient.cs
- [ ] `BACKEND_URL` constant updated to deployed server URL (not `localhost`)
- [ ] HTTPS used — HTTP connections rejected in production builds
- [ ] JWT token stored in `PlayerPrefs` after login — loaded on app start
- [ ] `Authorization: Bearer <token>` header attached to every authenticated request
- [ ] `RegisterOrLogin(string deviceId)` called on first launch to obtain token
- [ ] `VerifyPurchase(string receipt, string productId)` sends Google Play receipt to backend
- [ ] `SyncEconomy(long coins, int gems, int spins)` sends balance to backend and applies server response
- [ ] Socket.IO connection established with JWT in auth handshake
- [ ] Socket reconnects automatically on disconnect (built-in Socket.IO-client retry)
- [ ] All requests time out after 10 seconds — `UnityWebRequest.timeout = 10`
- [ ] Non-2xx responses logged and surfaced as `onError` callback — never silently ignored
- [ ] 401 response triggers re-login flow (token expired or revoked)
- [ ] Offline detection: `Application.internetReachability` checked before sending requests
- [ ] No secrets (JWT, keys) logged to Unity console

## General
- [ ] Script compiles with zero errors/warnings in Unity 2022.3.20f1
- [ ] Singleton pattern prevents duplicate instances
- [ ] `DontDestroyOnLoad` called so connection survives scene transitions
- [ ] All public methods are coroutines or accept callbacks — no blocking main thread
