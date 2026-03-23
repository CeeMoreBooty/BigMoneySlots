'use strict';

const express = require('express');
const router = express.Router();
const storage = require('../storage');

// GET /players — list all players
router.get('/', async (req, res) => {
  const all = await storage.getAllPlayers();
  res.json(all);
});

// GET /players/:id — get a single player's data
router.get('/:id', async (req, res) => {
  const player = await storage.getPlayer(req.params.id);
  if (!player) {
    return res.status(404).json({ error: 'Player not found' });
  }
  res.json(player);
});

// POST /players/:id — create or replace a player record
router.post('/:id', async (req, res) => {
  const { id } = req.params;
  if (!req.body || typeof req.body !== 'object') {
    return res.status(400).json({ error: 'Request body must be a JSON object' });
  }
  const player = await storage.setPlayer(id, req.body);
  res.status(201).json(player);
});

// PATCH /players/:id — partially update a player record
router.patch('/:id', async (req, res) => {
  const { id } = req.params;
  if (!req.body || typeof req.body !== 'object') {
    return res.status(400).json({ error: 'Request body must be a JSON object' });
  }
  const player = await storage.updatePlayer(id, req.body);
  if (!player) {
    return res.status(404).json({ error: 'Player not found' });
  }
  res.json(player);
});

// DELETE /players/:id — delete a player record
router.delete('/:id', async (req, res) => {
  const deleted = await storage.deletePlayer(req.params.id);
  if (!deleted) {
    return res.status(404).json({ error: 'Player not found' });
  }
  res.status(204).send();
});

module.exports = router;
