'use strict';

const fs = require('fs/promises');
const path = require('path');

const DATA_DIR = path.join(__dirname, 'data');
const DATA_FILE = path.join(DATA_DIR, 'players.json');

// Serialize all writes through a queue to prevent race conditions
let writeQueue = Promise.resolve();

async function ensureDataFile() {
  await fs.mkdir(DATA_DIR, { recursive: true });
  try {
    await fs.access(DATA_FILE);
  } catch {
    await fs.writeFile(DATA_FILE, JSON.stringify({}), 'utf8');
  }
}

async function readAll() {
  await ensureDataFile();
  const raw = await fs.readFile(DATA_FILE, 'utf8');
  return JSON.parse(raw);
}

function writeAll(data) {
  writeQueue = writeQueue.then(async () => {
    await ensureDataFile();
    await fs.writeFile(DATA_FILE, JSON.stringify(data, null, 2), 'utf8');
  });
  return writeQueue;
}

async function getPlayer(id) {
  const all = await readAll();
  return all[id] || null;
}

async function setPlayer(id, data) {
  const all = await readAll();
  all[id] = { ...data, updatedAt: new Date().toISOString() };
  await writeAll(all);
  return all[id];
}

async function updatePlayer(id, patch) {
  const all = await readAll();
  if (!all[id]) return null;
  all[id] = { ...all[id], ...patch, updatedAt: new Date().toISOString() };
  await writeAll(all);
  return all[id];
}

async function deletePlayer(id) {
  const all = await readAll();
  if (!all[id]) return false;
  delete all[id];
  await writeAll(all);
  return true;
}

async function getAllPlayers() {
  return readAll();
}

// Upsert-merge: create the player if they don't exist, or merge the patch into
// their existing record if they do.  This is the "push" operation — safe to
// call whether or not the player has been seen before.
async function pushPlayer(id, patch) {
  const all = await readAll();
  all[id] = { ...(all[id] || {}), ...patch, updatedAt: new Date().toISOString() };
  await writeAll(all);
  return all[id];
}

module.exports = { getPlayer, setPlayer, updatePlayer, deletePlayer, getAllPlayers, pushPlayer };
