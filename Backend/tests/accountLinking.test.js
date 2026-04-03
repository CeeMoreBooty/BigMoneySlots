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

describe('GET /api/account/links', () => {
    it('returns empty links for a new player', async () => {
        const { token } = await register('acct-dev-1');
        const res = await request(app)
            .get('/api/account/links')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body.links).toEqual([]);
    });

    it('returns 401 without a token', async () => {
        const res = await request(app).get('/api/account/links');
        expect(res.status).toBe(401);
    });
});

describe('POST /api/account/link', () => {
    it('links a valid provider and grants a coin bonus', async () => {
        const { token } = await register('acct-dev-2', 'LinkPlayer');
        const res = await request(app)
            .post('/api/account/link')
            .set('Authorization', `Bearer ${token}`)
            .send({ provider: 'google', token: 'google-token-abc' });

        expect(res.status).toBe(200);
        expect(res.body.success).toBe(true);
        expect(res.body.bonusGranted).toBe(true);
        expect(res.body.newBalance).toBeGreaterThan(0);
    });

    it('does not grant bonus twice for the same provider', async () => {
        const { token } = await register('acct-dev-3');
        await request(app)
            .post('/api/account/link')
            .set('Authorization', `Bearer ${token}`)
            .send({ provider: 'facebook', token: 'fb-token-1' });

        const res = await request(app)
            .post('/api/account/link')
            .set('Authorization', `Bearer ${token}`)
            .send({ provider: 'facebook', token: 'fb-token-1' });

        expect(res.status).toBe(200);
        expect(res.body.bonusGranted).toBe(false);
    });

    it('returns 400 for an invalid provider', async () => {
        const { token } = await register('acct-dev-4');
        const res = await request(app)
            .post('/api/account/link')
            .set('Authorization', `Bearer ${token}`)
            .send({ provider: 'twitter', token: 'tw-token' });

        expect(res.status).toBe(400);
        expect(res.body.error).toMatch(/invalid provider/i);
    });

    it('returns 400 when provider or token is missing', async () => {
        const { token } = await register('acct-dev-5');
        const res = await request(app)
            .post('/api/account/link')
            .set('Authorization', `Bearer ${token}`)
            .send({ provider: 'discord' });     // missing token field

        expect(res.status).toBe(400);
    });

    it('returns 409 when a provider token is already linked to another player', async () => {
        const { token: t1 } = await register('acct-dev-6');
        const { token: t2 } = await register('acct-dev-7');
        const sharedToken   = 'shared-provider-uid';

        await request(app)
            .post('/api/account/link')
            .set('Authorization', `Bearer ${t1}`)
            .send({ provider: 'phone', token: sharedToken });

        const res = await request(app)
            .post('/api/account/link')
            .set('Authorization', `Bearer ${t2}`)
            .send({ provider: 'phone', token: sharedToken });

        expect(res.status).toBe(409);
        expect(res.body.error).toMatch(/already linked/i);
    });

    it('grants a larger bonus for discord than other providers', async () => {
        const { token: tg } = await register('acct-dev-8');
        const { token: td } = await register('acct-dev-9');

        const googleRes  = await request(app).post('/api/account/link').set('Authorization', `Bearer ${tg}`).send({ provider: 'google',  token: 'g-uid' });
        const discordRes = await request(app).post('/api/account/link').set('Authorization', `Bearer ${td}`).send({ provider: 'discord', token: 'd-uid' });

        expect(discordRes.body.newBalance).toBeGreaterThan(googleRes.body.newBalance);
    });
});

describe('DELETE /api/account/link/:provider', () => {
    it('unlinks a linked provider', async () => {
        const { token } = await register('acct-dev-10');
        await request(app)
            .post('/api/account/link')
            .set('Authorization', `Bearer ${token}`)
            .send({ provider: 'google', token: 'g-token-unlink' });

        const res = await request(app)
            .delete('/api/account/link/google')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body.ok).toBe(true);

        // Confirm it's gone
        const linksRes = await request(app).get('/api/account/links').set('Authorization', `Bearer ${token}`);
        expect(linksRes.body.links).toEqual([]);
    });

    it('returns 200 even when unlinking a provider that was never linked', async () => {
        const { token } = await register('acct-dev-11');
        const res = await request(app)
            .delete('/api/account/link/facebook')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
    });
});
