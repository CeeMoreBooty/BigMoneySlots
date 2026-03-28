# Security System — Company Documentation

## Overview

The Big Money Slots backend includes a multi-layer preventive security system that detects, logs, and automatically blocks hacking attempts, cheating, and abuse.

## Architecture

```
Incoming Request
       │
       ▼
securityGuard (middleware)
  ├── Banned IP/device check        → 403 if banned
  ├── Exploit tool user-agent check → 403 if known attack tool
  ├── Payload size check            → flag if >1 MB
  ├── Injection pattern scan        → 400 if NoSQL/SQL/XSS/RCE detected
  ├── Per-IP per-endpoint rate      → 429 + auto-ban if abusive
  └── JWT multi-IP tracker          → flag if token used from 3+ IPs
       │
       ▼
Route Handler
  └── (optional) securityService helpers:
        checkCoinDelta()    — validates spin win is plausible
        trackFailedAuth()   — brute-force detection on login
```

## Threat Detection Capabilities

| Threat | Detection Method | Auto-Response |
|--------|-----------------|---------------|
| Brute-force login | Sliding window counter per IP (10 min) | Flag at 10 attempts, ban at 20 |
| Abnormal request rate | Per IP+endpoint counter (1 min) | Flag at 60 hits, ban at 120 |
| NoSQL / SQL injection | Regex scan on request body | Block + critical event |
| XSS / script injection | Regex scan on request body | Block + critical event |
| Path traversal / RCE | Regex scan on request body | Block + critical event |
| Known exploit tools | User-agent fingerprinting (sqlmap, nikto, Burp Suite, etc.) | Block + high event |
| JWT token sharing | Track distinct IPs per token | Flag at 3 IPs |
| Coin manipulation | Impossible win delta check | Block + critical event |
| Duplicate purchase replay | purchaseToken unique index in DB | 409 conflict |
| Banned entity re-attempt | Check BannedEntity on every request | 403 |

## Event Types

All threats are stored in the `SecurityEvent` collection:

| Event Type | Severity | Description |
|-----------|----------|-------------|
| `brute_force_auth` | high / critical | Repeated failed login attempts |
| `rate_limit_exceeded` | medium | Express rate limiter hit |
| `banned_ip_attempt` | medium | Banned IP attempted access |
| `banned_player_attempt` | medium | Banned player used API |
| `coin_manipulation` | critical | Client claimed impossible coin win |
| `spin_result_tampering` | critical | Client-side spin outcome altered |
| `jwt_reuse_multi_ip` | high | Same token used from multiple IPs |
| `invalid_jwt` | low | Malformed or expired token |
| `injection_attempt` | critical | NoSQL/SQL/XSS/RCE in request body |
| `bot_fingerprint` | high | Known exploit tool user-agent |
| `abnormal_request_rate` | high / critical | Extreme request rate to same endpoint |
| `payload_too_large` | medium | Request body >1 MB |
| `forbidden_endpoint` | medium | Attempt to access admin-only route |
| `duplicate_purchase` | medium | Same purchase token submitted twice |

## Ban System

Bans are stored in the `BannedEntity` collection. Three entity types:

| Type | Value | Example |
|------|-------|---------|
| `ip` | IP address string | `"203.0.113.42"` |
| `player` | MongoDB ObjectId string | `"665abc..."` |
| `device` | Device fingerprint string | `"abc123"` |

- **Temporary bans** have an `expiresAt` date and auto-expire
- **Permanent bans** have `expiresAt: null`
- Expired temporary bans are cleaned up on each request (background)

## Admin API

All endpoints require header: `x-admin-secret: <ADMIN_SECRET>`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/security/events` | GET | List security events (filter by severity, type, ip) |
| `/api/security/stats` | GET | 24h dashboard: counts by severity and event type |
| `/api/security/bans` | GET | List all active bans |
| `/api/security/ban` | POST | Manually ban an IP/player/device |
| `/api/security/unban` | POST | Remove a ban |
| `/api/security/events/:id/resolve` | PATCH | Mark event as resolved |

### Example: Manual IP ban
```json
POST /api/security/ban
x-admin-secret: your_admin_secret

{
  "type": "ip",
  "value": "203.0.113.42",
  "reason": "Manual review — cheating",
  "expiresAt": "2026-04-01T00:00:00Z"
}
```

### Example: View today's critical events
```
GET /api/security/events?severity=critical&limit=100
x-admin-secret: your_admin_secret
```

## Real-Time Admin Alerts

When a high or critical event occurs, the backend emits a Socket.IO event to the `admin_room`:

```js
// Event: 'security_alert'
{
  eventType: 'injection_attempt',
  severity:  'critical',
  ip:        '1.2.3.4',
  playerId:  '665abc...',
  path:      '/api/coins/earn',
  autoBanned: true,
  time:      '2026-03-22T17:00:00.000Z'
}

// Event: 'player_banned'
{
  ip:       '1.2.3.4',
  playerId: '665abc...',
  reason:   'injection_attempt',
  time:     '2026-03-22T17:00:00.000Z'
}
```

## Environment Variables

```
IP_LOGGING_ENABLED=true
ADMIN_SECRET=change_this_to_a_long_random_string
```

## Files

| File | Purpose |
|------|---------|
| `Backend/models/SecurityEvent.js` | Threat log document model |
| `Backend/models/BannedEntity.js` | IP/player/device ban model |
| `Backend/services/securityService.js` | Detection engine and ban helpers |
| `Backend/middleware/securityGuard.js` | Per-request guard middleware |
| `Backend/routes/security.js` | Admin view/ban/unban API |
