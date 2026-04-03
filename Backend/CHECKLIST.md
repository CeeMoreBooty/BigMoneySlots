# Checklist — Backend

## Environment & Configuration
- [ ] `.env` created from `.env.example` — all placeholder values replaced
- [ ] `JWT_SECRET` is a long random string (min 32 chars) — not `changeme`
- [ ] `MONGO_URI` points to a live, accessible MongoDB instance
- [ ] `STRIPE_SECRET_KEY` is a **live** key (`sk_live_...`) for production
- [ ] `STRIPE_WEBHOOK_SECRET` matches the signing secret in Stripe Dashboard
- [ ] `STRIPE_PUBLISHABLE_KEY` is a **live** key (`pk_live_...`) for production
- [ ] `PAYPAL_CLIENT_ID` and `PAYPAL_CLIENT_SECRET` are **live** credentials
- [ ] `PAYPAL_MODE=live` set for production (not `sandbox`)
- [ ] `GOOGLE_APPLICATION_CREDENTIALS` path points to a valid service-account JSON file
- [ ] `GOOGLE_PLAY_PACKAGE_NAME` matches the package name in Play Console exactly
- [ ] `ADMIN_SECRET` is a long random string — not shared publicly
- [ ] `SECURITY_WEBHOOK_SECRET` set if `SECURITY_WEBHOOK_ENABLED=true`
- [ ] `ALLOWED_ORIGIN` set to the deployed game's public domain

## Security
- [ ] HTTPS / TLS termination in place (nginx, Caddy, or load balancer in front of Node)
- [ ] Rate limiting active — 120 req / 15 min per IP (already configured in `app.js`)
- [ ] `securityGuard` middleware enabled — blocks known attack patterns
- [ ] IP logging enabled (`IP_LOGGING_ENABLED=true`) for fraud detection
- [ ] `ADMIN_SECRET` not included in any committed file or CI log
- [ ] All secrets loaded from environment — no hard-coded values in source code
- [ ] `npm audit` run — no critical or high vulnerabilities in dependencies

## Deployment
- [ ] `docker-compose up -d` starts without errors
- [ ] `docker ps` shows both `backend` and `mongo` containers healthy
- [ ] `curl https://yourserver.com/health` returns `{"status":"ok"}`
- [ ] Server logs show `MongoDB connected` on startup
- [ ] Stripe webhook endpoint registered at `https://yourserver.com/api/payments/stripe/webhook`
- [ ] Backend accessible from the Unity client (test `BackendClient.RegisterOrLogin()` on device)

## Code Quality
- [ ] `npm test` passes all 57 tests
- [ ] No `console.log` statements left with sensitive data
- [ ] All routes return consistent error shapes: `{ error: "message" }`
- [ ] All async route handlers wrapped in try/catch (no unhandled promise rejections)
- [ ] `server.js` graceful shutdown on `SIGTERM` — closes DB connection cleanly

## Monitoring
- [ ] Server error logs monitored (Docker logs or log aggregator)
- [ ] MongoDB disk usage monitored — `IpLog` and `SecurityEvent` collections capped or TTL-indexed
- [ ] Payment provider dashboards checked for failed transactions post-launch
- [ ] `tournamentScheduler` confirmed running — new tournaments created automatically
