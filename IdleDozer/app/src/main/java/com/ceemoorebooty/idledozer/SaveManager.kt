package com.ceemoorebooty.idledozer

import android.content.Context
import android.content.SharedPreferences

/**
 * Persists and restores the game state using SharedPreferences.
 * Also tracks the timestamp of the last save so that offline earnings can be calculated.
 */
class SaveManager(context: Context) {

    private val prefs: SharedPreferences =
        context.getSharedPreferences("idledozer_save", Context.MODE_PRIVATE)

    fun saveGame(engine: GameEngine) {
        prefs.edit().apply {
            putString("balance", engine.balance.toString())
            engine.dozers.forEach { putInt("dozer_${it.id}", it.owned) }
            putLong("save_time", System.currentTimeMillis())
            apply()
        }
    }

    fun loadGame(): GameEngine {
        val engine = GameEngine()
        engine.balance = prefs.getString("balance", "0.0")?.toDoubleOrNull() ?: 0.0
        engine.dozers.forEach { it.owned = prefs.getInt("dozer_${it.id}", 0) }
        return engine
    }

    fun getLastSaveTime(): Long = prefs.getLong("save_time", 0L)
}
