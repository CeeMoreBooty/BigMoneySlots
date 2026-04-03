const request = require('supertest');
const { connect, disconnect, clearCollections, buildApp } = require('./__helpers__/setup');

let app;

async function register(deviceId, displayName = 'Player') {
    const res = await request(app)
        .post('/api/auth/register')
        .send({ deviceId, displayName });
    return { token: res.body.token, playerId: String(res.body.playerId) };
}

beforeAll(async () => { await connect(); app = buildApp(); });
afterEach(async () => clearCollections());
afterAll(async () => disconnect());

describe('GET /api/friends/search', () => {
    it('returns 400 for queries shorter than 2 characters', async () => {
        const { token } = await register('fr-dev-1');
        const res = await request(app)
            .get('/api/friends/search?q=A')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(400);
    });

    it('finds players by partial display name', async () => {
        await register('fr-dev-2', 'Searchable');
        const { token } = await register('fr-dev-3', 'Searcher');
        const res = await request(app)
            .get('/api/friends/search?q=Searcha')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.results.some(p => p.displayName === 'Searchable')).toBe(true);
    });

    it('does not return the requesting player in results', async () => {
        const { token } = await register('fr-dev-4', 'SelfSearch');
        const res = await request(app)
            .get('/api/friends/search?q=Self')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.results.find(p => p.displayName === 'SelfSearch')).toBeUndefined();
    });
});

describe('POST /api/friends/request', () => {
    it('creates a friend request', async () => {
        const { token: t1 } = await register('fr-dev-5', 'Alice');
        const { playerId: p2 } = await register('fr-dev-6', 'Bob');

        const res = await request(app)
            .post('/api/friends/request')
            .set('Authorization', `Bearer ${t1}`)
            .send({ targetPlayerId: p2 });

        expect(res.status).toBe(201);
        expect(res.body.friendship).toBeDefined();
    });

    it('returns 400 when sending a request to yourself', async () => {
        const { token, playerId } = await register('fr-dev-7', 'Solo');
        const res = await request(app)
            .post('/api/friends/request')
            .set('Authorization', `Bearer ${token}`)
            .send({ targetPlayerId: playerId });
        expect(res.status).toBe(400);
    });

    it('returns 409 on duplicate request', async () => {
        const { token: t1 } = await register('fr-dev-8', 'Carol');
        const { playerId: p2 } = await register('fr-dev-9', 'Dave');

        await request(app)
            .post('/api/friends/request')
            .set('Authorization', `Bearer ${t1}`)
            .send({ targetPlayerId: p2 });

        const res = await request(app)
            .post('/api/friends/request')
            .set('Authorization', `Bearer ${t1}`)
            .send({ targetPlayerId: p2 });

        expect(res.status).toBe(409);
    });

    it('returns 400 when targetPlayerId is missing', async () => {
        const { token } = await register('fr-dev-10');
        const res = await request(app)
            .post('/api/friends/request')
            .set('Authorization', `Bearer ${token}`)
            .send({});
        expect(res.status).toBe(400);
    });
});

describe('POST /api/friends/accept', () => {
    it('accepts a pending request and marks status as accepted', async () => {
        const { token: t1, playerId: p1 } = await register('fr-dev-11', 'Eve');
        const { token: t2, playerId: p2 } = await register('fr-dev-12', 'Frank');

        await request(app)
            .post('/api/friends/request')
            .set('Authorization', `Bearer ${t1}`)
            .send({ targetPlayerId: p2 });

        const res = await request(app)
            .post('/api/friends/accept')
            .set('Authorization', `Bearer ${t2}`)
            .send({ targetPlayerId: p1 });

        expect(res.status).toBe(200);
        expect(res.body.friendship.status).toBe('accepted');
    });

    it('returns 404 when no pending request exists', async () => {
        const { token: t1, playerId: p1 } = await register('fr-dev-13', 'Grace');
        const { token: t2 }               = await register('fr-dev-14', 'Hank');

        const res = await request(app)
            .post('/api/friends/accept')
            .set('Authorization', `Bearer ${t2}`)
            .send({ targetPlayerId: p1 });

        expect(res.status).toBe(404);
    });
});

describe('POST /api/friends/decline', () => {
    it('removes the pending request', async () => {
        const { token: t1, playerId: p1 } = await register('fr-dev-15', 'Iris');
        const { token: t2, playerId: p2 } = await register('fr-dev-16', 'Jack');

        await request(app)
            .post('/api/friends/request')
            .set('Authorization', `Bearer ${t1}`)
            .send({ targetPlayerId: p2 });

        const res = await request(app)
            .post('/api/friends/decline')
            .set('Authorization', `Bearer ${t2}`)
            .send({ targetPlayerId: p1 });

        expect(res.status).toBe(200);
        expect(res.body.ok).toBe(true);
    });
});

describe('POST /api/friends/remove', () => {
    it('removes an accepted friendship', async () => {
        const { token: t1, playerId: p1 } = await register('fr-dev-17', 'Kate');
        const { token: t2, playerId: p2 } = await register('fr-dev-18', 'Leo');

        // request → accept
        await request(app).post('/api/friends/request').set('Authorization', `Bearer ${t1}`).send({ targetPlayerId: p2 });
        await request(app).post('/api/friends/accept').set('Authorization', `Bearer ${t2}`).send({ targetPlayerId: p1 });

        const res = await request(app)
            .post('/api/friends/remove')
            .set('Authorization', `Bearer ${t1}`)
            .send({ targetPlayerId: p2 });

        expect(res.status).toBe(200);
        expect(res.body.ok).toBe(true);
    });
});

describe('GET /api/friends', () => {
    it('returns empty friends and pending lists for a new player', async () => {
        const { token } = await register('fr-dev-19', 'New');
        const res = await request(app)
            .get('/api/friends')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.friends).toEqual([]);
        expect(res.body.pending).toEqual([]);
    });
});
