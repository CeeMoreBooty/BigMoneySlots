const request    = require('supertest');
const Tournament = require('../models/Tournament');
const { connect, disconnect, clearCollections, buildApp } = require('./__helpers__/setup');

let app;

async function register(deviceId) {
    const res = await request(app).post('/api/auth/register').send({ deviceId });
    return res.body.token;
}

async function createActiveTournament() {
    return Tournament.create({
        name:        'Test Cup',
        status:      'active',
        startTime:   new Date(),
        endTime:     new Date(Date.now() + 3_600_000),
        prizePool:   1_000_000,
        totalPlayers: 0,
    });
}

beforeAll(async () => { await connect(); app = buildApp(); });
afterEach(async () => clearCollections());
afterAll(async () => disconnect());

describe('GET /api/tournament/current', () => {
    it('returns null tournament when none is active', async () => {
        const token = await register('tourn-dev-1');
        const res   = await request(app)
            .get('/api/tournament/current')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.tournament).toBeNull();
    });

    it('returns active tournament', async () => {
        const token = await register('tourn-dev-2');
        await createActiveTournament();
        const res   = await request(app)
            .get('/api/tournament/current')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.tournament.name).toBe('Test Cup');
    });
});

describe('POST /api/tournament/join', () => {
    it('returns 404 when no active tournament', async () => {
        const token = await register('tourn-dev-3');
        const res   = await request(app)
            .post('/api/tournament/join')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(404);
    });

    it('creates entry and increments totalPlayers', async () => {
        const token = await register('tourn-dev-4');
        await createActiveTournament();
        const res = await request(app)
            .post('/api/tournament/join')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(201);
        expect(res.body.entry).toBeDefined();

        const updated = await Tournament.findOne({ status: 'active' });
        expect(updated.totalPlayers).toBe(1);
    });

    it('returns alreadyJoined flag on second join attempt', async () => {
        const token = await register('tourn-dev-5');
        await createActiveTournament();
        await request(app).post('/api/tournament/join').set('Authorization', `Bearer ${token}`);
        const res = await request(app)
            .post('/api/tournament/join')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.alreadyJoined).toBe(true);
    });
});

describe('POST /api/tournament/score', () => {
    it('updates score and spinsPlayed', async () => {
        const token = await register('tourn-dev-6');
        await createActiveTournament();
        await request(app).post('/api/tournament/join').set('Authorization', `Bearer ${token}`);

        const res = await request(app)
            .post('/api/tournament/score')
            .set('Authorization', `Bearer ${token}`)
            .send({ coinsWon: 5000, biggestWin: 5000 });

        expect(res.status).toBe(200);
        expect(res.body.entry.score).toBe(5000);
        expect(res.body.entry.spinsPlayed).toBe(1);
    });
});

describe('GET /api/tournament/leaderboard', () => {
    it('returns empty leaderboard when no tournament', async () => {
        const token = await register('tourn-dev-7');
        const res   = await request(app)
            .get('/api/tournament/leaderboard')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.leaderboard).toEqual([]);
    });
});

describe('GET /api/tournament/history', () => {
    it('returns empty history when no ended tournaments', async () => {
        const token = await register('tourn-dev-8');
        const res   = await request(app)
            .get('/api/tournament/history')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.tournaments).toEqual([]);
    });
});
