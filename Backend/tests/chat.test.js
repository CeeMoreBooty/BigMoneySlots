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

describe('POST /api/chat/send', () => {
    it('sends a message to the global channel', async () => {
        const { token } = await register('chat-dev-1', 'ChatUser');
        const res = await request(app)
            .post('/api/chat/send')
            .set('Authorization', `Bearer ${token}`)
            .send({ text: 'Hello world', channel: 'global' });

        expect(res.status).toBe(201);
        expect(res.body.message.text).toBe('Hello world');
        expect(res.body.message.senderName).toBe('ChatUser');
        expect(res.body.message.channel).toBe('global');
    });

    it('returns 400 when text is empty', async () => {
        const { token } = await register('chat-dev-2');
        const res = await request(app)
            .post('/api/chat/send')
            .set('Authorization', `Bearer ${token}`)
            .send({ text: '   ' });
        expect(res.status).toBe(400);
    });

    it('truncates text to 200 characters', async () => {
        const { token } = await register('chat-dev-3');
        const res = await request(app)
            .post('/api/chat/send')
            .set('Authorization', `Bearer ${token}`)
            .send({ text: 'A'.repeat(300) });
        expect(res.status).toBe(201);
        expect(res.body.message.text.length).toBe(200);
    });

    it('returns 401 without token', async () => {
        const res = await request(app).post('/api/chat/send').send({ text: 'Hi' });
        expect(res.status).toBe(401);
    });
});

describe('GET /api/chat/history', () => {
    it('returns messages in the global channel', async () => {
        const { token } = await register('chat-dev-4', 'HistUser');

        await request(app)
            .post('/api/chat/send')
            .set('Authorization', `Bearer ${token}`)
            .send({ text: 'First message', channel: 'global' });

        const res = await request(app)
            .get('/api/chat/history?channel=global')
            .set('Authorization', `Bearer ${token}`);

        expect(res.status).toBe(200);
        expect(res.body.messages.length).toBe(1);
        expect(res.body.messages[0].text).toBe('First message');
    });

    it('returns an empty array when no messages exist', async () => {
        const { token } = await register('chat-dev-5');
        const res = await request(app)
            .get('/api/chat/history?channel=global')
            .set('Authorization', `Bearer ${token}`);
        expect(res.status).toBe(200);
        expect(res.body.messages).toEqual([]);
    });
});

describe('GET /api/chat/dm/thread/:friendId', () => {
    it('returns conversation messages between two players', async () => {
        const { token: t1, playerId: p1 } = await register('chat-dev-6', 'Alpha');
        const { token: t2, playerId: p2 } = await register('chat-dev-7', 'Beta');

        // Alpha sends a DM to Beta
        await request(app)
            .post('/api/chat/send')
            .set('Authorization', `Bearer ${t1}`)
            .send({ text: 'Hey Beta!', channel: 'friends', targetId: p2 });

        const res = await request(app)
            .get(`/api/chat/dm/thread/${p2}`)
            .set('Authorization', `Bearer ${t1}`);

        expect(res.status).toBe(200);
        expect(res.body.messages.length).toBe(1);
        expect(res.body.messages[0].text).toBe('Hey Beta!');
    });
});

describe('GET /api/chat/dm/threads', () => {
    it('returns thread list for a player with DM conversations', async () => {
        const { token: t1, playerId: p1 } = await register('chat-dev-8', 'Gamma');
        const { token: t2, playerId: p2 } = await register('chat-dev-9', 'Delta');

        await request(app)
            .post('/api/chat/send')
            .set('Authorization', `Bearer ${t1}`)
            .send({ text: 'Hello Delta', channel: 'friends', targetId: p2 });

        const res = await request(app)
            .get('/api/chat/dm/threads')
            .set('Authorization', `Bearer ${t1}`);

        expect(res.status).toBe(200);
        expect(res.body.threads.length).toBe(1);
    });
});

describe('POST /api/chat/voice/join and /api/chat/voice/leave', () => {
    it('returns ok for voice join', async () => {
        const { token } = await register('chat-dev-10');
        const res = await request(app)
            .post('/api/chat/voice/join')
            .set('Authorization', `Bearer ${token}`)
            .send({ roomId: 'room-abc' });
        expect(res.status).toBe(200);
        expect(res.body.ok).toBe(true);
    });

    it('returns ok for voice leave', async () => {
        const { token } = await register('chat-dev-11');
        const res = await request(app)
            .post('/api/chat/voice/leave')
            .set('Authorization', `Bearer ${token}`)
            .send({ roomId: 'room-abc' });
        expect(res.status).toBe(200);
        expect(res.body.ok).toBe(true);
    });
});
