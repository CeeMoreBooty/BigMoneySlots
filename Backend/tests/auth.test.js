const request = require('supertest');
const { connect, disconnect, clearCollections, buildApp } = require('./__helpers__/setup');

let app;

beforeAll(async () => { await connect(); app = buildApp(); });
afterEach(async () => clearCollections());
afterAll(async () => disconnect());

describe('POST /api/auth/register', () => {
    it('creates a new player and returns a JWT', async () => {
        const res = await request(app)
            .post('/api/auth/register')
            .send({ deviceId: 'device-001', displayName: 'TestPlayer' });

        expect(res.status).toBe(201);
        expect(res.body.token).toBeDefined();
        expect(res.body.isNew).toBe(true);
        expect(res.body.playerId).toBeDefined();
    });

    it('returns a JWT for an already-registered deviceId without re-creating player', async () => {
        await request(app).post('/api/auth/register').send({ deviceId: 'device-002' });
        const res = await request(app).post('/api/auth/register').send({ deviceId: 'device-002' });

        expect(res.status).toBe(200);
        expect(res.body.isNew).toBe(false);
        expect(res.body.token).toBeDefined();
    });

    it('returns 400 when deviceId is missing', async () => {
        const res = await request(app).post('/api/auth/register').send({});
        expect(res.status).toBe(400);
        expect(res.body.error).toMatch(/deviceId/i);
    });
});

describe('GET /api/auth/me', () => {
    it('returns player profile with valid token', async () => {
        const reg = await request(app).post('/api/auth/register').send({ deviceId: 'device-me-1', displayName: 'Tester' });
        const { token } = reg.body;

        const res = await request(app)
            .get('/api/auth/me')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body.displayName).toBe('Tester');
        expect(res.body.coins).toBeDefined();
    });

    it('returns 401 without a token', async () => {
        const res = await request(app).get('/api/auth/me');
        expect(res.status).toBe(401);
    });

    it('returns 401 with an invalid token', async () => {
        const res = await request(app)
            .get('/api/auth/me')
            .set('Authorization', 'Bearer invalid.token.here');
        expect(res.status).toBe(401);
    });
});

describe('PATCH /api/auth/display-name', () => {
    it('updates display name', async () => {
        const reg = await request(app).post('/api/auth/register').send({ deviceId: 'device-dn-1' });
        const { token } = reg.body;

        const res = await request(app)
            .patch('/api/auth/display-name')
            .set('Authorization', `Bearer ${token}`)
            .send({ displayName: 'NewName' });

        expect(res.status).toBe(200);
        expect(res.body.displayName).toBe('NewName');
    });

    it('rejects a display name shorter than 2 characters', async () => {
        const reg = await request(app).post('/api/auth/register').send({ deviceId: 'device-dn-2' });
        const { token } = reg.body;

        const res = await request(app)
            .patch('/api/auth/display-name')
            .set('Authorization', `Bearer ${token}`)
            .send({ displayName: 'X' });

        expect(res.status).toBe(400);
    });

    it('truncates display name to 24 characters', async () => {
        const reg = await request(app).post('/api/auth/register').send({ deviceId: 'device-dn-3' });
        const { token } = reg.body;

        const res = await request(app)
            .patch('/api/auth/display-name')
            .set('Authorization', `Bearer ${token}`)
            .send({ displayName: 'A'.repeat(30) });

        expect(res.status).toBe(200);
        expect(res.body.displayName.length).toBe(24);
    });
});
