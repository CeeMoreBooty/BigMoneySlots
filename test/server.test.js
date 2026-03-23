'use strict';

const http = require('http');
const fs = require('fs');
const path = require('path');
const app = require('../server');

// Clean up test data before running
const dataFile = path.join(__dirname, '../data/players.json');
if (fs.existsSync(dataFile)) {
  fs.writeFileSync(dataFile, JSON.stringify({}), 'utf8');
}

const server = http.createServer(app);
let port;

function request(method, urlPath, body) {
  return new Promise((resolve, reject) => {
    const payload = body ? JSON.stringify(body) : null;
    const opts = {
      hostname: '127.0.0.1',
      port,
      path: urlPath,
      method,
      headers: {
        'Content-Type': 'application/json',
        ...(payload ? { 'Content-Length': Buffer.byteLength(payload) } : {}),
      },
    };
    const req = http.request(opts, (res) => {
      let data = '';
      res.on('data', (chunk) => (data += chunk));
      res.on('end', () => {
        let parsed;
        try { parsed = JSON.parse(data); } catch { parsed = data; }
        resolve({ status: res.statusCode, body: parsed });
      });
    });
    req.on('error', reject);
    if (payload) req.write(payload);
    req.end();
  });
}

function assert(condition, message) {
  if (!condition) throw new Error(`FAIL: ${message}`);
  console.log(`  PASS: ${message}`);
}

async function runTests() {
  await new Promise((resolve) => server.listen(0, '127.0.0.1', () => {
    port = server.address().port;
    resolve();
  }));

  console.log('\nRunning tests...\n');

  // Health check
  const health = await request('GET', '/health');
  assert(health.status === 200, 'GET /health returns 200');
  assert(health.body.status === 'ok', 'Health body has status ok');

  // Create player
  const created = await request('POST', '/players/player1', { coins: 1000, level: 1 });
  assert(created.status === 201, 'POST /players/player1 returns 201');
  assert(created.body.coins === 1000, 'Created player has correct coins');
  assert(typeof created.body.updatedAt === 'string', 'Created player has updatedAt timestamp');

  // Get player
  const got = await request('GET', '/players/player1');
  assert(got.status === 200, 'GET /players/player1 returns 200');
  assert(got.body.coins === 1000, 'Retrieved player has correct coins');

  // Patch player
  const patched = await request('PATCH', '/players/player1', { coins: 5000 });
  assert(patched.status === 200, 'PATCH /players/player1 returns 200');
  assert(patched.body.coins === 5000, 'Patched player has updated coins');
  assert(patched.body.level === 1, 'Patched player retains existing fields');

  // List all players
  const all = await request('GET', '/players');
  assert(all.status === 200, 'GET /players returns 200');
  assert(all.body.player1 !== undefined, 'Player list contains player1');

  // 404 for missing player
  const missing = await request('GET', '/players/nobody');
  assert(missing.status === 404, 'GET /players/nobody returns 404');

  // Delete player
  const del = await request('DELETE', '/players/player1');
  assert(del.status === 204, 'DELETE /players/player1 returns 204');

  // Confirm deleted
  const gone = await request('GET', '/players/player1');
  assert(gone.status === 404, 'GET /players/player1 after delete returns 404');

  // 404 route
  const notFound = await request('GET', '/unknown-route');
  assert(notFound.status === 404, 'Unknown route returns 404');

  server.close();
  console.log('\nAll tests passed.\n');
}

runTests().catch((err) => {
  console.error('\nTest error:', err.message);
  process.exit(1);
});
