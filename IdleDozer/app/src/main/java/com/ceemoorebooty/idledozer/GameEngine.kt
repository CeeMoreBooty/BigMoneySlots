package com.ceemoorebooty.idledozer

import kotlin.math.min

/**
 * Core idle-game logic.
 *
 * The engine is tick-driven: [tick] is called 10 times per second by the UI layer.
 * All monetary values are in virtual dollars (Double).
 */
class GameEngine {

    var balance: Double = 0.0

    /** Remaining boost ticks at 10 ticks/sec (e.g. 600 = 60 seconds of 2× earnings). */
    private var boostTicksLeft: Int = 0

    val boostSecondsLeft: Int
        get() = boostTicksLeft / 10

    // ── Dozer catalogue ──────────────────────────────────────────────────────────

    val dozers: List<DozerItem> = listOf(
        DozerItem(0, "Mini Dozer",       "🚜",  10.0,              0.1),
        DozerItem(1, "Worker Dozer",     "🏗️",  100.0,             1.2),
        DozerItem(2, "Standard Dozer",   "🚧",  1_000.0,          13.0),
        DozerItem(3, "Heavy Dozer",      "⚙️",  12_000.0,        160.0),
        DozerItem(4, "Mega Dozer",       "🏔️",  130_000.0,      2_100.0),
        DozerItem(5, "Ultra Dozer",      "💥",  1_400_000.0,    28_000.0),
        DozerItem(6, "Titan Dozer",      "🌋",  20_000_000.0,  380_000.0),
        DozerItem(7, "Legendary Dozer",  "👑",  300_000_000.0, 5_500_000.0)
    )

    // ── Derived stats ─────────────────────────────────────────────────────────────

    /** Base earnings per second (without boost). */
    private val baseEarningsPerSecond: Double
        get() = dozers.sumOf { it.totalEarningsPerSecond }

    /** Effective earnings per second (includes 2× boost if active). */
    val earningsPerSecond: Double
        get() = if (boostTicksLeft > 0) baseEarningsPerSecond * 2.0 else baseEarningsPerSecond

    /** Coins earned by a single manual tap. */
    val tapEarnings: Double
        get() {
            val base = maxOf(0.10, baseEarningsPerSecond * 0.01)
            return if (boostTicksLeft > 0) base * 2.0 else base
        }

    // ── Game actions ──────────────────────────────────────────────────────────────

    /** Advance the game by one tick (called 10×/sec). */
    fun tick() {
        balance += earningsPerSecond / 10.0
        if (boostTicksLeft > 0) boostTicksLeft--
    }

    /** Manual tap to earn coins immediately. */
    fun tap() {
        balance += tapEarnings
    }

    /**
     * Attempt to purchase one unit of [dozer].
     * @return true if the player could afford it, false otherwise.
     */
    fun buyDozer(dozer: DozerItem): Boolean {
        if (balance < dozer.currentCost) return false
        balance -= dozer.currentCost
        dozer.owned++
        return true
    }

    /** Activate the 2× earnings boost for [seconds] seconds. */
    fun activateBoost(seconds: Int) {
        boostTicksLeft = seconds * 10
    }

    /**
     * Credit offline earnings based on the time elapsed since [lastSaveTime].
     * Capped at 8 hours and reduced to 50 % efficiency (standard idle-game convention).
     */
    fun processOfflineEarnings(lastSaveTime: Long) {
        if (lastSaveTime <= 0L) return
        val elapsedSeconds = (System.currentTimeMillis() - lastSaveTime) / 1_000.0
        val cappedSeconds = min(elapsedSeconds, 8.0 * 3_600.0)
        if (cappedSeconds > 0) {
            balance += baseEarningsPerSecond * cappedSeconds * 0.5
        }
    }
}
