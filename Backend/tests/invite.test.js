const request = require('supertest');
const { connect, disconnect, clearCollections, buildApp } = require('./__helpers__/setup');

let app;

async function register(deviceId, displayName = 'Player') {
    const res = await request(app).post('/api/auth/register').send({ deviceId, displayName });
    return { token: res.body.token, playerId: String(res.body.playerId) };
}

beforeAll(async () => { await connect(); app = buildApp(); });
afterEach(async () => clearCollections());
afterAll(async () => disconnect());

describe('GET /api/invite/code', () => {
    it('generates an invite code for a new player', async () => {
        const { token } = await register('inv-dev-1');
        const res = await request(app)
            .get('/api/invite/code')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body.code).toMatch(/^BMS[0-9A-F]{8}$/);
        expect(res.body.totalInvites).toBe(0);
        expect(res.body.gemRewardPerInvite).toBe(100);
        expect(res.body.gemRewardForNew).toBe(50);
    });

    it('returns the same code on repeated calls', async () => {
        const { token } = await register('inv-dev-2');
        const res1 = await request(app).get('/api/invite/code').set('Authorization', `Bearer ${token}`);
        const res2 = await request(app).get('/api/invite/code').set('Authorization', `Bearer ${token}`);
        expect(res1.body.code).toBe(res2.body.code);
    });
});

describe('GET /api/invite/stats', () => {
    it('returns zero stats for a player with no redemptions', async () => {
        const { token } = await register('inv-dev-3');
        const res = await request(app)
            .get('/api/invite/stats')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body.totalInvites).toBe(0);
        expect(res.body.gemsEarned).toBe(0);
    });
});

describe('POST /api/invite/redeem', () => {
    it('grants gems to both inviter and redeemer', async () => {
        const { token: inviterToken } = await register('inv-dev-4', 'Inviter');
        const { token: redeemerToken } = await register('inv-dev-5', 'Redeemer');

        // Get inviter's code
        const codeRes = await request(app)
            .get('/api/invite/code')
            .set('Authorization', `Bearer ${inviterToken}`);
        const { code } = codeRes.body;

        // Redeemer uses the code
        const res = await request(app)
            .post('/api/invite/redeem')
            .set('Authorization', `Bearer ${redeemerToken}`)
            .send({ code });

        expect(res.status).toBe(200);
        expect(res.body.success).toBe(true);
        expect(res.body.gemsAwarded).toBe(50);
    });

    it('prevents a player from redeeming their own code', async () => {
        const { token } = await register('inv-dev-6');
        const codeRes  = await request(app).get('/api/invite/code').set('Authorization', `Bearer ${token}`);
        const { code } = codeRes.body;

        const res = await request(app)
            .post('/api/invite/redeem')
            .set('Authorization', `Bearer ${token}`)
            .send({ code });

        expect(res.status).toBe(400);
        expect(res.body.error).toMatch(/own/i);
    });

    it('prevents a player from redeeming a second invite code', async () => {
        const { token: t1 } = await register('inv-dev-7', 'Inv1');
        const { token: t2 } = await register('inv-dev-8', 'Inv2');
        const { token: redeemerToken } = await register('inv-dev-9', 'Redeemer2');

        const code1 = (await request(app).get('/api/invite/code').set('Authorization', `Bearer ${t1}`)).body.code;
        const code2 = (await request(app).get('/api/invite/code').set('Authorization', `Bearer ${t2}`)).body.code;

        await request(app).post('/api/invite/redeem').set('Authorization', `Bearer ${redeemerToken}`).send({ code: code1 });

        const res = await request(app)
            .post('/api/invite/redeem')
            .set('Authorization', `Bearer ${redeemerToken}`)
            .send({ code: code2 });

        expect(res.status).toBe(409);
        expect(res.body.error).toMatch(/already redeemed/i);
    });

    it('returns 404 for an invalid code', async () => {
        const { token } = await register('inv-dev-10');
        const res = await request(app)
            .post('/api/invite/redeem')
            .set('Authorization', `Bearer ${token}`)
            .send({ code: 'BMSDEADBEEF' });
        expect(res.status).toBe(404);
    });

    it('returns 400 when code field is missing', async () => {
        const { token } = await register('inv-dev-11');
        const res = await request(app)
            .post('/api/invite/redeem')
            .set('Authorization', `Bearer ${token}`)
            .send({});
        expect(res.status).toBe(400);
    });
});
