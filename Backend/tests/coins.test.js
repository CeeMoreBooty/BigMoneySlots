const request = require('supertest');
const { connect, disconnect, clearCollections, buildApp } = require('./__helpers__/setup');

let app, token;

async function registerPlayer(deviceId = 'coins-device-1') {
    const res = await request(app).post('/api/auth/register').send({ deviceId });
    return res.body.token;
}

beforeAll(async () => { await connect(); app = buildApp(); });
afterEach(async () => clearCollections());
afterAll(async () => disconnect());

describe('GET /api/coins/balance', () => {
    it('returns coin balance for authenticated player', async () => {
        token = await registerPlayer();
        const res = await request(app)
            .get('/api/coins/balance')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body).toMatchObject({
            coins:      expect.any(Number),
            freeSpins:  expect.any(Number),
            superSpins: expect.any(Number),
            ultraSpins: expect.any(Number),
            gems:       expect.any(Number),
        });
    });

    it('returns 401 without token', async () => {
        const res = await request(app).get('/api/coins/balance');
        expect(res.status).toBe(401);
    });
});

describe('GET /api/coins/transactions', () => {
    it('returns empty transaction list for new player', async () => {
        token = await registerPlayer('coins-device-2');
        const res = await request(app)
            .get('/api/coins/transactions')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body.transactions).toEqual([]);
        expect(res.body.page).toBe(1);
    });

    it('respects page and limit query params', async () => {
        token = await registerPlayer('coins-device-3');
        const res = await request(app)
            .get('/api/coins/transactions?page=2&limit=5')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body.page).toBe(2);
    });
});

describe('POST /api/coins/sync', () => {
    it('returns authoritative server balance (ignores client values)', async () => {
        token = await registerPlayer('coins-device-4');
        const res = await request(app)
            .post('/api/coins/sync')
            .set('Authorization', `Bearer ${token}`)
            .send({ coins: 999999999, freeSpins: 100 });   // inflated client values

        expect(res.status).toBe(200);
        // Server returns its own balance, not the client-supplied ones
        expect(res.body.coins).toBe(0);
        expect(res.body.freeSpins).toBe(0);
    });
});
