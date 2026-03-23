package com.ceemoorebooty.idledozer

import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.view.View
import android.widget.Button
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.google.android.gms.ads.AdRequest
import com.google.android.gms.ads.AdView
import com.google.android.gms.ads.MobileAds

class MainActivity : AppCompatActivity() {

    private lateinit var gameEngine: GameEngine
    private lateinit var adManager: AdManager
    private lateinit var saveManager: SaveManager

    private lateinit var tvBalance: TextView
    private lateinit var tvEps: TextView
    private lateinit var tvBoostStatus: TextView
    private lateinit var btnTap: Button
    private lateinit var btnWatchAd: Button
    private lateinit var rvDozers: RecyclerView
    private lateinit var bannerAdView: AdView
    private lateinit var dozerAdapter: DozerAdapter

    private val handler = Handler(Looper.getMainLooper())

    /** Runs 10× per second — advances game state and updates the balance display. */
    private val tickRunnable = object : Runnable {
        override fun run() {
            gameEngine.tick()
            updateHeader()
            handler.postDelayed(this, 100L)
        }
    }

    /** Runs every second — refreshes the dozer shop (affordability highlights). */
    private val shopUpdateRunnable = object : Runnable {
        override fun run() {
            dozerAdapter.currentBalance = gameEngine.balance
            dozerAdapter.notifyDataSetChanged()
            handler.postDelayed(this, 1_000L)
        }
    }

    /** Auto-saves every 5 seconds. */
    private val saveRunnable = object : Runnable {
        override fun run() {
            saveManager.saveGame(gameEngine)
            handler.postDelayed(this, 5_000L)
        }
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────────

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        MobileAds.initialize(this)

        saveManager = SaveManager(this)
        gameEngine  = saveManager.loadGame()
        adManager   = AdManager(this)

        bindViews()
        setupRecyclerView()
        loadAds()
    }

    override fun onResume() {
        super.onResume()
        gameEngine.processOfflineEarnings(saveManager.getLastSaveTime())
        handler.post(tickRunnable)
        handler.post(shopUpdateRunnable)
        handler.post(saveRunnable)
        bannerAdView.resume()
    }

    override fun onPause() {
        handler.removeCallbacks(tickRunnable)
        handler.removeCallbacks(shopUpdateRunnable)
        handler.removeCallbacks(saveRunnable)
        saveManager.saveGame(gameEngine)
        bannerAdView.pause()
        super.onPause()
    }

    override fun onDestroy() {
        handler.removeCallbacks(tickRunnable)
        handler.removeCallbacks(shopUpdateRunnable)
        handler.removeCallbacks(saveRunnable)
        bannerAdView.destroy()
        super.onDestroy()
    }

    // ── Setup helpers ─────────────────────────────────────────────────────────────

    private fun bindViews() {
        tvBalance     = findViewById(R.id.tv_balance)
        tvEps         = findViewById(R.id.tv_eps)
        tvBoostStatus = findViewById(R.id.tv_boost_status)
        btnTap        = findViewById(R.id.btn_tap)
        btnWatchAd    = findViewById(R.id.btn_watch_ad)
        bannerAdView  = findViewById(R.id.banner_ad_view)

        btnTap.setOnClickListener { gameEngine.tap() }

        btnWatchAd.setOnClickListener {
            adManager.showRewardedAd { rewarded ->
                if (rewarded) {
                    gameEngine.activateBoost(60)
                    updateHeader()
                }
            }
        }
    }

    private fun setupRecyclerView() {
        rvDozers     = findViewById(R.id.rv_dozers)
        dozerAdapter = DozerAdapter(gameEngine.dozers) { dozer ->
            if (gameEngine.buyDozer(dozer)) {
                adManager.onPurchase()
                dozerAdapter.currentBalance = gameEngine.balance
                dozerAdapter.notifyDataSetChanged()
                updateHeader()
            }
        }
        rvDozers.layoutManager = LinearLayoutManager(this)
        rvDozers.adapter       = dozerAdapter
    }

    private fun loadAds() {
        bannerAdView.loadAd(AdRequest.Builder().build())
        adManager.loadRewardedAd()
        adManager.loadInterstitialAd()
    }

    // ── UI updates ────────────────────────────────────────────────────────────────

    private fun updateHeader() {
        tvBalance.text = "\$${formatMoney(gameEngine.balance)}"
        tvEps.text     = "\$${formatMoney(gameEngine.earningsPerSecond)}/sec"

        val boostLeft = gameEngine.boostSecondsLeft
        if (boostLeft > 0) {
            tvBoostStatus.visibility = View.VISIBLE
            tvBoostStatus.text = "⚡ 2× BOOST: ${boostLeft}s left"
            btnWatchAd.isEnabled = false
        } else {
            tvBoostStatus.visibility = View.GONE
            btnWatchAd.isEnabled = adManager.isRewardedAdReady()
        }
    }

    // ── Shared utility ────────────────────────────────────────────────────────────

    companion object {
        fun formatMoney(amount: Double): String = when {
            amount >= 1_000_000_000_000.0 -> "%.2fT".format(amount / 1_000_000_000_000.0)
            amount >= 1_000_000_000.0     -> "%.2fB".format(amount / 1_000_000_000.0)
            amount >= 1_000_000.0         -> "%.2fM".format(amount / 1_000_000.0)
            amount >= 1_000.0             -> "%.2fK".format(amount / 1_000.0)
            else                          -> "%.2f".format(amount)
        }
    }
}
