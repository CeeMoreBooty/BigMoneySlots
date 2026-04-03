# Checklist — Assets/ScriptableObjects

## Symbols/
- [ ] Symbol_Bell.asset — name, sprite (Bell.png), multipliers, isWild=false, isScatter=false
- [ ] Symbol_Cherry.asset — name, sprite (Cherry.png), multipliers, isWild=false, isScatter=false
- [ ] Symbol_Scatter.asset — name, sprite (Scatter.png), free-spin count, isScatter=true
- [ ] Symbol_Seven.asset — name, sprite (Seven.png), highest multipliers, isWild=false
- [ ] Symbol_Wild.asset — name, sprite (Wild.png), isWild=true
- [ ] All five symbols assigned to at least one `SlotGameConfig` reel
- [ ] Multipliers balance-tested (RTP target: 85–95% for casino compliance)

## PayoutTables/
- [ ] PayoutTable_Classic.asset — entries for every 3-symbol combination including Wilds
- [ ] PayoutTable_HighRoller.asset — higher payouts, lower frequency hits
- [ ] PayoutTable_Penny.asset — frequent small wins, low jackpot
- [ ] All tables verified: Wild substitution entries present for every non-Scatter symbol
- [ ] Scatter entries in each table specify free-spin count (not coin multiplier)
- [ ] Each table assigned to its corresponding `SlotGameConfig`

## SlotGames/
- [ ] SlotGameConfig_classic_slots.asset — payout table, symbol set, min bet, reel skin, game name
- [ ] SlotGameConfig_jungle_jackpot.asset — payout table, symbol set, min bet, reel skin, game name
- [ ] SlotGameConfig_penny_paradise.asset — payout table, symbol set, min bet, reel skin, game name
- [ ] Each config has `reelSpinDuration` set (e.g. 1.5 s)
- [ ] Each config has `bigWinThreshold` multiplier set (e.g. 50×)
- [ ] Each config has `jackpotSeed` value set
- [ ] Each config has `jackpotContributionPercent` set (e.g. 1%)
- [ ] All three configs listed in `SlotGameRegistry.asset`

## SlotGameRegistry.asset
- [ ] All three `SlotGameConfig` assets referenced in the registry list
- [ ] Registry asset assigned to `SlotGameDatabase` / `SlotGameLoader` in scene

## General
- [ ] All `.asset` files have corresponding `.asset.meta` files committed
- [ ] No asset references point to missing objects (no yellow warning icons in Project window)
- [ ] Balance sheet reviewed: expected RTP per table calculated and acceptable for target markets
