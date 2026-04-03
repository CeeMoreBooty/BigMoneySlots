'use strict';

const express = require('express');
const playersRouter = require('./routes/players');

const app = express();
const PORT = process.env.PORT || 3000;

app.use(express.json());

// Health check
app.get('/health', (req, res) => {
  res.json({ status: 'ok', time: new Date().toISOString() });
});

// Player storage routes
app.use('/players', playersRouter);

// 404 fallback
app.use((req, res) => {
  res.status(404).json({ error: 'Not found' });
});

// Start server (only when run directly, not during tests)
if (require.main === module) {
  app.listen(PORT, () => {
    console.log(`BigMoneySlots storage server running on port ${PORT}`);
  });
}

module.exports = app;
