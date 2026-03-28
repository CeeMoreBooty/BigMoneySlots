const Tournament      = require('../models/Tournament');
const TournamentEntry = require('../models/TournamentEntry');
const Player          = require('../models/Player');
const Transaction     = require('../models/Transaction');

// 40 available game IDs (from SlotGameDatabase)
const GAME_IDS = [
    'spirit_buffalo_run','thunderhawk_sentinel','golden_coyote_watch','sacred_wolf_circle',
    'great_bear_vigil','prairie_fire_spirit','wind_eagle_keeper','moonlit_fox_guardian',
    'storm_bison_watcher','sunstone_hawk','iron_deer_dance','whispering_owl_shrine',
    'red_tail_protector','star_pony_trails','blue_heron_blessing','sage_wolf_crossing',
    'thunder_elk_guardian','painted_mustang_spirit','crow_nation_shield','wildflower_warden',
    'prairie_dog_kingdom','sacred_crane_watch','horizon_bison_chief','lightning_hawk_tribe',
    'snow_owl_vigil','spirit_bear_den','rolling_thunder_stag','antelope_run_keeper',
    'medicine_hawk_circle','dusty_coyote_trail','great_plains_watcher','crimson_fox_spirit',
    'wandering_bison_guard','dawn_eagle_rising','prairie_wolf_moon','stone_bear_totem',
    'silver_hawk_blessing','sacred_serpent_plains','golden_prairie_chief','eternal_guardian_spirit',
];

const GAME_NAMES = {
    spirit_buffalo_run: 'Spirit Buffalo Run', thunderhawk_sentinel: 'Thunderhawk Sentinel',
    golden_coyote_watch: 'Golden Coyote Watch', sacred_wolf_circle: 'Sacred Wolf Circle',
    great_bear_vigil: 'Great Bear Vigil', prairie_fire_spirit: 'Prairie Fire Spirit',
    wind_eagle_keeper: 'Wind Eagle Keeper', moonlit_fox_guardian: 'Moonlit Fox Guardian',
    storm_bison_watcher: 'Storm Bison Watcher', sunstone_hawk: 'Sunstone Hawk',
    iron_deer_dance: 'Iron Deer Dance', whispering_owl_shrine: 'Whispering Owl Shrine',
    red_tail_protector: 'Red Tail Protector', star_pony_trails: 'Star Pony Trails',
    blue_heron_blessing: 'Blue Heron Blessing', sage_wolf_crossing: 'Sage Wolf Crossing',
    thunder_elk_guardian: 'Thunder Elk Guardian', painted_mustang_spirit: 'Painted Mustang Spirit',
    crow_nation_shield: 'Crow Nation Shield', wildflower_warden: 'Wildflower Warden',
    prairie_dog_kingdom: 'Prairie Dog Kingdom', sacred_crane_watch: 'Sacred Crane Watch',
    horizon_bison_chief: 'Horizon Bison Chief', lightning_hawk_tribe: 'Lightning Hawk Tribe',
    snow_owl_vigil: 'Snow Owl Vigil', spirit_bear_den: 'Spirit Bear Den',
    rolling_thunder_stag: 'Rolling Thunder Stag', antelope_run_keeper: 'Antelope Run Keeper',
    medicine_hawk_circle: 'Medicine Hawk Circle', dusty_coyote_trail: 'Dusty Coyote Trail',
    great_plains_watcher: 'Great Plains Watcher', crimson_fox_spirit: 'Crimson Fox Spirit',
    wandering_bison_guard: 'Wandering Bison Guard', dawn_eagle_rising: 'Dawn Eagle Rising',
    prairie_wolf_moon: 'Prairie Wolf Moon', stone_bear_totem: 'Stone Bear Totem',
    silver_hawk_blessing: 'Silver Hawk Blessing', sacred_serpent_plains: 'Sacred Serpent Plains',
    golden_prairie_chief: 'Golden Prairie Chief', eternal_guardian_spirit: 'Eternal Guardian Spirit',
};

function pickRandomGame() {
    const id = GAME_IDS[Math.floor(Math.random() * GAME_IDS.length)];
    return { gameId: id, gameName: GAME_NAMES[id] || id };
}

// ── Create a new tournament ───────────────────────────────────────────────────
async function createTournament() {
    const now       = new Date();
    const endTime   = new Date(now.getTime() + 30 * 60 * 1000); // +30 min
    const game      = pickRandomGame();

    const tournament = await Tournament.create({
        startTime:    now,
        endTime,
        status:       'active',
        selectedGame: game,
    });
    console.log(`[TournamentScheduler] New tournament: ${tournament._id} — ${game.gameName}`);
    return tournament;
}

// ── End tournament + award prizes ─────────────────────────────────────────────
async function endTournament(tournament) {
    if (tournament.winnersAwarded) return;

    tournament.status = 'ended';
    await tournament.save();

    const entries = await TournamentEntry.find({ tournamentId: tournament._id })
        .sort({ score: -1 })
        .limit(20);

    for (let i = 0; i < entries.length; i++) {
        const entry = entries[i];
        entry.rank  = i + 1;

        const tier = tournament.prizeTiers.find(t => t.rank === i + 1);
        if (tier && tier.coinsReward > 0) {
            entry.prizeAwarded = tier.coinsReward;
            await Player.findByIdAndUpdate(entry.playerId, { $inc: { coins: tier.coinsReward } });
            await Transaction.create({
                playerId:     entry.playerId,
                type:         'tournament_prize',
                amount:       tier.coinsReward,
                currency:     'coins',
                description:  `Tournament #${tournament._id} — Rank ${i + 1}`,
                referenceId:  String(tournament._id),
                balanceAfter: 0, // approximate; real value fetched on next sync
            });
        }
        await entry.save();
    }

    tournament.winnersAwarded = true;
    await tournament.save();
    console.log(`[TournamentScheduler] Tournament ${tournament._id} ended. ${entries.length} players ranked.`);
}

// ── Scheduler loop (runs every 30 min) ───────────────────────────────────────
async function tick() {
    try {
        // End any active tournaments that have expired
        const expired = await Tournament.find({ status: 'active', endTime: { $lte: new Date() } });
        for (const t of expired) await endTournament(t);

        // Create new tournament if none is active
        const active = await Tournament.findOne({ status: 'active' });
        if (!active) await createTournament();
    } catch (err) {
        console.error('[TournamentScheduler] tick error:', err.message);
    }
}

function start() {
    console.log('[TournamentScheduler] Starting — 30-min tournament cycle.');
    tick(); // run immediately on boot
    setInterval(tick, 30 * 60 * 1000); // then every 30 min
}

module.exports = { start, createTournament, endTournament };
