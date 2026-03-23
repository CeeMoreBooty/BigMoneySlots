# BigMoneySlots — Network Storage Server

A lightweight Node.js REST API server that persists BigMoneySlots player data (coins, progress, daily challenges, etc.) to a local JSON file.

## Requirements

- Node.js >= 18

## Setup

```bash
npm install
```

## Running the server

```bash
npm start          # production
npm run dev        # development (auto-restarts on file changes, Node 18+)
```

The server listens on port **3000** by default. Set the `PORT` environment variable to change it.

```bash
PORT=8080 npm start
```

## API Reference

### Health check

```
GET /health
```

Response:
```json
{ "status": "ok", "time": "2026-03-23T00:00:00.000Z" }
```

---

### List all players

```
GET /players
```

---

### Get a player

```
GET /players/:id
```

Returns `404` if the player does not exist.

---

### Create / replace a player

```
POST /players/:id
Content-Type: application/json

{
  "coins": 1000,
  "level": 1,
  "dailyChallenges": {}
}
```

Returns `201` with the saved record.

---

### Partially update a player

```
PATCH /players/:id
Content-Type: application/json

{ "coins": 5000 }
```

Merges the provided fields into the existing record. Returns `404` if the player does not exist.

---

### Delete a player

```
DELETE /players/:id
```

Returns `204 No Content` on success, `404` if the player does not exist.

---

## Data storage

Player records are stored in `data/players.json`. The file is created automatically on first run. The `data/` directory is excluded from version control.

## Running tests

```bash
npm test
```
