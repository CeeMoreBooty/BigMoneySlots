# Checklist — Assets/Scripts/WelcomeOffer

## WelcomeOfferConfig.cs (ScriptableObject)
- [ ] `productId` matches the product ID in Google Play Console exactly (`welcomeofferpack`)
- [ ] `priceDisplayString` set (e.g. "$4.99") for display before IAP initialisation completes
- [ ] `rewardDescription` text populated (shown in popup body)
- [ ] Config asset assigned in `WelcomeOfferManager` Inspector field

## WelcomeOfferManager.cs
- [ ] `HasBeenOffered` flag read from `PlayerPrefs` — popup only shown to new players
- [ ] Popup triggered on first launch after `IAPHandler` signals initialisation complete
- [ ] `welcomeOfferPurchased` flag set on successful purchase — suppresses future offers
- [ ] `WelcomeOfferUI.Show()` / `WelcomeOfferUI.Hide()` called appropriately
- [ ] `IAPHandler` reference obtained without `FindObjectOfType` in `Update`

## WelcomeOfferUI.cs
- [ ] Buy button wired to `IAPHandler.PurchaseWelcomeOffer()`
- [ ] Close / "No thanks" button wired to hide the panel (sets `HasBeenOffered = true`)
- [ ] Price label populated from `WelcomeOfferConfig.priceDisplayString`
- [ ] Reward description label populated from `WelcomeOfferConfig.rewardDescription`
- [ ] Loading spinner shown while IAP purchase is in progress
- [ ] Error label shown if purchase fails (user cancelled, billing unavailable, etc.)

## IAPHandler.cs
- [ ] `UnityPurchasing.Initialize()` called in `Start` with correct product list
- [ ] `OnInitializeFailed` handled — logged and non-fatal (game works without IAP)
- [ ] `ProcessPurchase` sends receipt to backend (`BackendClient.VerifyPurchase()`) before granting reward
- [ ] Reward only granted after successful backend verification response
- [ ] `OnPurchaseFailed` shows user-friendly error via `WelcomeOfferUI`
- [ ] `IStoreController` and `IExtensionProvider` cached after `OnInitialized`
- [ ] Unity IAP package present in `Packages/manifest.json` (`com.unity.purchasing`)
- [ ] Google Play billing permission in `AndroidManifest.xml`: `com.android.vending.BILLING`

## General
- [ ] All scripts compile with zero errors/warnings in Unity 2022.3.20f1
- [ ] Welcome offer tested end-to-end on a real Android device with a test Google account
- [ ] Purchase verified in Google Play Console → Order management
- [ ] Reward (coins, spins, gems, badge, skin) granted correctly after purchase
- [ ] Offer does not re-appear after purchase or after reinstall with the same account
