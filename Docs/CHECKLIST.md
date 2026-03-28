# Checklist — Docs

## SystemOverview.md
- [ ] Architecture diagram is up to date (Unity client ↔ Backend ↔ MongoDB ↔ Payment providers)
- [ ] All backend services listed (auth, coins, payments, tournament, chat, friends, invite, security)
- [ ] WebSocket / Socket.IO real-time flow documented
- [ ] Data flow for a spin described end-to-end (client → backend sync → response)

## AccountLinking.md
- [ ] All four providers documented: Google, Facebook, Discord, Phone
- [ ] Bonus coin amounts per provider listed and match `routes/accountLinking.js`
- [ ] One-time bonus rule documented (second link of same provider grants no bonus)
- [ ] Provider token validation approach documented (placeholder vs. real OAuth noted)
- [ ] Conflict resolution documented (409 when provider already linked to another player)

## PaymentSystem.md
- [ ] Google Play IAP flow documented: purchase → receipt → backend verify → reward
- [ ] PayPal order create/capture flow documented
- [ ] Stripe checkout flow documented (hosted checkout page)
- [ ] Webhook handling documented for both Stripe and PayPal
- [ ] Idempotency strategy documented (purchase tokens checked before granting)
- [ ] Refund / chargeback policy noted

## SecuritySystem.md
- [ ] `securityGuard` middleware behaviour documented (what triggers a block)
- [ ] Ban types documented: IP ban, player ban, temporary vs. permanent
- [ ] Admin endpoints listed with required `x-admin-secret` header noted
- [ ] `SecurityEvent` severity levels defined: `low`, `medium`, `high`, `critical`
- [ ] Webhook notification flow documented (when events are forwarded)
- [ ] IP logging documented — data retention policy noted

## General
- [ ] All docs reviewed for accuracy against current code
- [ ] No outdated endpoint paths or field names
- [ ] Links between documents work (relative paths)
- [ ] Docs do not contain any secrets, keys, or credentials
