package com.ceemoorebooty.idledozer

import android.app.Activity
import android.util.Log
import com.google.android.gms.ads.AdError
import com.google.android.gms.ads.AdRequest
import com.google.android.gms.ads.FullScreenContentCallback
import com.google.android.gms.ads.LoadAdError
import com.google.android.gms.ads.interstitial.InterstitialAd
import com.google.android.gms.ads.interstitial.InterstitialAdLoadCallback
import com.google.android.gms.ads.rewarded.RewardedAd
import com.google.android.gms.ads.rewarded.RewardedAdLoadCallback

/**
 * Manages all AdMob ad formats: banner (loaded directly in the layout), interstitial,
 * and rewarded.
 *
 * ⚠️  The ad-unit IDs below are Google's official TEST IDs.
 *      Replace them with your real AdMob ad-unit IDs before publishing.
 */
class AdManager(private val activity: Activity) {

    companion object {
        private const val TAG = "AdManager"

        // ── Replace these with your real AdMob ad-unit IDs ──────────────────────
        const val BANNER_AD_UNIT_ID       = "ca-app-pub-3940256099942544/6300978111"
        const val INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-3940256099942544/1033173712"
        const val REWARDED_AD_UNIT_ID     = "ca-app-pub-3940256099942544/5224354917"
        // ────────────────────────────────────────────────────────────────────────

        /** Show an interstitial after every N purchases. */
        private const val INTERSTITIAL_PURCHASE_INTERVAL = 5
    }

    private var rewardedAd: RewardedAd? = null
    private var interstitialAd: InterstitialAd? = null
    private var purchaseCount = 0

    // ── Interstitial ──────────────────────────────────────────────────────────────

    fun loadInterstitialAd() {
        InterstitialAd.load(
            activity,
            INTERSTITIAL_AD_UNIT_ID,
            AdRequest.Builder().build(),
            object : InterstitialAdLoadCallback() {
                override fun onAdLoaded(ad: InterstitialAd) {
                    interstitialAd = ad
                }
                override fun onAdFailedToLoad(error: LoadAdError) {
                    interstitialAd = null
                    Log.w(TAG, "Interstitial failed to load: ${error.message}")
                }
            }
        )
    }

    private fun showInterstitialAd() {
        val ad = interstitialAd ?: run { loadInterstitialAd(); return }
        ad.fullScreenContentCallback = object : FullScreenContentCallback() {
            override fun onAdDismissedFullScreenContent() {
                interstitialAd = null
                loadInterstitialAd()
            }
            override fun onAdFailedToShowFullScreenContent(adError: AdError) {
                interstitialAd = null
                loadInterstitialAd()
            }
        }
        ad.show(activity)
    }

    // ── Rewarded ──────────────────────────────────────────────────────────────────

    fun loadRewardedAd() {
        RewardedAd.load(
            activity,
            REWARDED_AD_UNIT_ID,
            AdRequest.Builder().build(),
            object : RewardedAdLoadCallback() {
                override fun onAdLoaded(ad: RewardedAd) {
                    rewardedAd = ad
                }
                override fun onAdFailedToLoad(error: LoadAdError) {
                    rewardedAd = null
                    Log.w(TAG, "Rewarded ad failed to load: ${error.message}")
                }
            }
        )
    }

    fun isRewardedAdReady(): Boolean = rewardedAd != null

    /**
     * Show the rewarded ad. [onRewarded] is called with `true` when the player
     * earns the reward, or `false` if no ad was available.
     */
    fun showRewardedAd(onRewarded: (Boolean) -> Unit) {
        val ad = rewardedAd ?: run { loadRewardedAd(); onRewarded(false); return }
        ad.fullScreenContentCallback = object : FullScreenContentCallback() {
            override fun onAdDismissedFullScreenContent() {
                rewardedAd = null
                loadRewardedAd()
            }
            override fun onAdFailedToShowFullScreenContent(adError: AdError) {
                rewardedAd = null
                loadRewardedAd()
            }
        }
        ad.show(activity) { onRewarded(true) }
    }

    // ── Purchase trigger ──────────────────────────────────────────────────────────

    /** Call after every successful purchase to trigger interstitials on schedule. */
    fun onPurchase() {
        purchaseCount++
        if (purchaseCount % INTERSTITIAL_PURCHASE_INTERVAL == 0) {
            showInterstitialAd()
        }
    }
}
