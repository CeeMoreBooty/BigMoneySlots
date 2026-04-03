using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static database of all 40 slot game definitions.
/// Used at runtime and by the Editor tool to generate SlotGameConfig assets.
/// </summary>
public static class SlotGameDatabase
{
    public struct GameDefinition
    {
        public string   GameId;
        public string   GameName;
        public string   Theme;
        public string   Description;
        public int      ReelCount;
        public int      RowCount;
        public int      PaylineCount;
        public long     MinBet;
        public long     MaxBet;
        public float    BaseRTP;
        public float    MaxWinMultiplier;
        public SlotGameConfig.Volatility Volatility;
        public bool     HasWild;
        public bool     HasScatter;
        public bool     HasBonus;
        public bool     HasFreeSpins;
        public bool     HasProgressive;
        public int      FreeSpinsCount;
        public string[] SymbolNames;
        public string   BgColor;    // hex e.g. "#0D0D2B"
        public string   AccentColor;

        /// <summary>Returns BgColor as a Unity Color (falls back to black).</summary>
        public Color ToBackgroundColor()
        {
            Color c = Color.black;
            if (!ColorUtility.TryParseHtmlString(BgColor, out c))
                Debug.LogWarning($"[SlotGameDatabase] Invalid BgColor '{BgColor}' for game '{GameId}'; using black.");
            return c;
        }

        /// <summary>Returns AccentColor as a Unity Color (falls back to yellow).</summary>
        public Color ToAccentColor()
        {
            Color c = Color.yellow;
            if (!ColorUtility.TryParseHtmlString(AccentColor, out c))
                Debug.LogWarning($"[SlotGameDatabase] Invalid AccentColor '{AccentColor}' for game '{GameId}'; using yellow.");
            return c;
        }
    }

    public static readonly List<GameDefinition> All = new List<GameDefinition>
    {
        // ── 1 ─ Prairie Guardian: Spirit Buffalo Run ─────────────────────────
        new GameDefinition {
            GameId = "spirit_buffalo_run", GameName = "Spirit Buffalo Run",
            Theme = "Prairie Guardian — Sacred Plains Herd",
            Description = "The great spirit buffalo thunders across the plains, stamping golden riches into every reel!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Diamond","Ruby","Emerald","Sapphire","Gold Bar","Seven","Bell","Wild","Scatter"},
            BgColor = "#0D0D2B", AccentColor = "#00CFFF"
        },

        // ── 2 ─ Prairie Guardian: Thunderhawk Sentinel ───────────────────────
        new GameDefinition {
            GameId = "thunderhawk_sentinel", GameName = "Thunderhawk Sentinel",
            Theme = "Prairie Guardian — Sky Watcher",
            Description = "The thunderhawk circles high above the prairie, guarding golden treasures below!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Golden Lion","Dragon","Koi Fish","Lucky Coin","Lantern","Cherry Blossom","Yin Yang","Wild","Scatter"},
            BgColor = "#1A0000", AccentColor = "#FFD700"
        },

        // ── 3 ─ Prairie Guardian: Golden Coyote Watch ───────────────────────
        new GameDefinition {
            GameId = "golden_coyote_watch", GameName = "Golden Coyote Watch",
            Theme = "Prairie Guardian — Trickster Protector",
            Description = "The clever golden coyote guards the dunes with sly fortune and tricky bonus spins!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 10_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 250f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Pharaoh","Sphinx","Ankh","Eye of Ra","Scarab","Pyramid","Cleopatra","Wild","Scatter"},
            BgColor = "#1C1200", AccentColor = "#FFB800"
        },

        // ── 4 ─ Prairie Guardian: Sacred Wolf Circle ─────────────────────────
        new GameDefinition {
            GameId = "sacred_wolf_circle", GameName = "Sacred Wolf Circle",
            Theme = "Prairie Guardian — Pack Spirit",
            Description = "The sacred wolf pack runs in a mystical circle, howling wins into the night sky!",
            ReelCount = 5, RowCount = 4, PaylineCount = 40,
            MinBet = 200, MaxBet = 6_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Tiger","Gorilla","Parrot","Snake","Elephant","Monkey","Toucan","Wild","Scatter"},
            BgColor = "#001A00", AccentColor = "#39FF14"
        },

        // ── 5 ─ Prairie Guardian: Great Bear Vigil ───────────────────────────
        new GameDefinition {
            GameId = "great_bear_vigil", GameName = "Great Bear Vigil",
            Theme = "Prairie Guardian — Wilderness Keeper",
            Description = "The great bear stands vigil over sunken river treasures and oceanic prairie wins!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 180f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Mermaid","Seahorse","Starfish","Pearl","Treasure Chest","Dolphin","Coral","Wild","Scatter"},
            BgColor = "#001433", AccentColor = "#00E5FF"
        },

        // ── 6 ─ Prairie Guardian: Prairie Fire Spirit ────────────────────────
        new GameDefinition {
            GameId = "prairie_fire_spirit", GameName = "Prairie Fire Spirit",
            Theme = "Prairie Guardian — Flame Caller",
            Description = "The prairie fire spirit ignites the reels with blazing hot wins and fiery multipliers!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 500, MaxBet = 12_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 300f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 8,
            SymbolNames = new[]{"Red Dragon","Fire Orb","Dragon Egg","Gold Coin Stack","Jade","Sword","Phoenix","Wild","Scatter"},
            BgColor = "#1A0500", AccentColor = "#FF4500"
        },

        // ── 7 ─ Prairie Guardian: Wind Eagle Keeper ──────────────────────────
        new GameDefinition {
            GameId = "wind_eagle_keeper", GameName = "Wind Eagle Keeper",
            Theme = "Prairie Guardian — Wind Rider",
            Description = "The wind eagle soars over neon canyons, riding thermal currents straight to jackpot!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = false, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Neon Seven","Neon Bar","Neon Bell","Neon Cherry","Neon Star","Playing Card","Dice","Wild","Scatter"},
            BgColor = "#000033", AccentColor = "#FF00FF"
        },

        // ── 8 ─ Prairie Guardian: Moonlit Fox Guardian ───────────────────────
        new GameDefinition {
            GameId = "moonlit_fox_guardian", GameName = "Moonlit Fox Guardian",
            Theme = "Prairie Guardian — Night Watcher",
            Description = "Under the moonlit prairie sky the silver fox guards the golden outlaw trails!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 7_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Sheriff Star","Cowboy Hat","Revolver","Horseshoe","Gold Nugget","Cactus","Wanted Poster","Wild","Scatter"},
            BgColor = "#1A1000", AccentColor = "#C8860A"
        },

        // ── 9 ─ Prairie Guardian: Storm Bison Watcher ────────────────────────
        new GameDefinition {
            GameId = "storm_bison_watcher", GameName = "Storm Bison Watcher",
            Theme = "Prairie Guardian — Thunder Herd",
            Description = "Storm clouds gather as the bison watcher awakens mystical prairie powers!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Full Moon","Crystal Ball","Owl","Wolf","Pentagram","Potion","Rune Stone","Wild","Scatter"},
            BgColor = "#05001A", AccentColor = "#C0A0FF"
        },

        // ── 10 ─ Prairie Guardian: Sunstone Hawk ─────────────────────────────
        new GameDefinition {
            GameId = "sunstone_hawk", GameName = "Sunstone Hawk",
            Theme = "Prairie Guardian — Solar Sentinel",
            Description = "The sunstone hawk bathes the sweet grasslands in golden light and sticky jackpots!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 100, MaxBet = 3_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 80f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Lollipop","Cupcake","Gummy Bear","Ice Cream","Candy Cane","Jelly Bean","Cotton Candy","Wild","Scatter"},
            BgColor = "#1A0020", AccentColor = "#FF69B4"
        },

        // ── 11 ─ Prairie Guardian: Iron Deer Dance ───────────────────────────
        new GameDefinition {
            GameId = "iron_deer_dance", GameName = "Iron Deer Dance",
            Theme = "Prairie Guardian — Classic Dancer",
            Description = "The iron deer dances across the classic Vegas plains dropping golden antler wins!",
            ReelCount = 3, RowCount = 3, PaylineCount = 5,
            MinBet = 100, MaxBet = 1_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = false, HasBonus = false, HasFreeSpins = false, HasProgressive = true, FreeSpinsCount = 0,
            SymbolNames = new[]{"Triple Seven","Bar","Double Bar","Triple Bar","Bell","Cherry","Lemon","Wild","Scatter"},
            BgColor = "#0D0000", AccentColor = "#FFD700"
        },

        // ── 12 ─ Prairie Guardian: Whispering Owl Shrine ─────────────────────
        new GameDefinition {
            GameId = "whispering_owl_shrine", GameName = "Whispering Owl Shrine",
            Theme = "Prairie Guardian — Wisdom Keeper",
            Description = "The all-seeing owl whispers secrets of Hollywood gold from an ancient prairie shrine!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Oscar Trophy","Film Reel","Clapperboard","Star","Director's Chair","Popcorn","Sunglasses","Wild","Scatter"},
            BgColor = "#0A000A", AccentColor = "#FFD700"
        },

        // ── 13 ─ Prairie Guardian: Red Tail Protector ────────────────────────
        new GameDefinition {
            GameId = "red_tail_protector", GameName = "Red Tail Protector",
            Theme = "Prairie Guardian — Rock Raptor",
            Description = "The red tail hawk shreds through prairie storms shredding reel after reel of riches!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 6_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 180f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Electric Guitar","Microphone","Drum Kit","Bass","Vinyl Record","Amp","Rock Fist","Wild","Scatter"},
            BgColor = "#0D0005", AccentColor = "#FF0055"
        },

        // ── 14 ─ Prairie Guardian: Star Pony Trails ──────────────────────────
        new GameDefinition {
            GameId = "star_pony_trails", GameName = "Star Pony Trails",
            Theme = "Prairie Guardian — Celestial Rider",
            Description = "The star pony blazes glittering trails across the night prairie dropping party jackpots!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Champagne","Confetti","Party Hat","Balloon","DJ Booth","Dance Floor","Trophy","Wild","Scatter"},
            BgColor = "#0A0020", AccentColor = "#FF00CC"
        },

        // ── 15 ─ Prairie Guardian: Blue Heron Blessing ───────────────────────
        new GameDefinition {
            GameId = "blue_heron_blessing", GameName = "Blue Heron Blessing",
            Theme = "Prairie Guardian — Water Spirit",
            Description = "The blue heron blesses the wetland meadows bringing tropical coin showers!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Palm Tree","Toucan","Hibiscus","Coconut","Surfboard","Pineapple","Sunset","Wild","Scatter"},
            BgColor = "#001A1A", AccentColor = "#00FFD0"
        },

        // ── 16 ─ Prairie Guardian: Sage Wolf Crossing ────────────────────────
        new GameDefinition {
            GameId = "sage_wolf_crossing", GameName = "Sage Wolf Crossing",
            Theme = "Prairie Guardian — Wisdom Path",
            Description = "The sage wolf crosses fields of clover guiding lucky wanderers to fortune!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 3_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Four Leaf Clover","Leprechaun","Rainbow","Gold Pot","Horseshoe","Harp","Shamrock","Wild","Scatter"},
            BgColor = "#001200", AccentColor = "#00CC44"
        },

        // ── 17 ─ Prairie Guardian: Thunder Elk Guardian ──────────────────────
        new GameDefinition {
            GameId = "thunder_elk_guardian", GameName = "Thunder Elk Guardian",
            Theme = "Prairie Guardian — Speed Spirit",
            Description = "The thunder elk charges at full gallop — hit the throttle and race to jackpot glory!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 7_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 8,
            SymbolNames = new[]{"Race Car","Checkered Flag","Helmet","Trophy","Pit Stop","Speedometer","Tire","Wild","Scatter"},
            BgColor = "#0D0000", AccentColor = "#FF3300"
        },

        // ── 18 ─ Prairie Guardian: Painted Mustang Spirit ────────────────────
        new GameDefinition {
            GameId = "painted_mustang_spirit", GameName = "Painted Mustang Spirit",
            Theme = "Prairie Guardian — Cosmic Stallion",
            Description = "The painted mustang gallops through star-fields, blasting into cosmic jackpot territory!",
            ReelCount = 5, RowCount = 4, PaylineCount = 40,
            MinBet = 200, MaxBet = 10_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 300f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Rocket","Alien","Planet","Asteroid","Space Station","Astronaut","Black Hole","Wild","Scatter"},
            BgColor = "#00001A", AccentColor = "#4400FF"
        },

        // ── 19 ─ Prairie Guardian: Crow Nation Shield ────────────────────────
        new GameDefinition {
            GameId = "crow_nation_shield", GameName = "Crow Nation Shield",
            Theme = "Prairie Guardian — Warrior's Ward",
            Description = "The crow nation shield protects great plunder and seafaring prairie fortunes!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 6_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Pirate Captain","Treasure Map","Skull & Crossbones","Cannon","Parrot","Treasure Chest","Compass","Wild","Scatter"},
            BgColor = "#000D1A", AccentColor = "#C8A000"
        },

        // ── 20 ─ Prairie Guardian: Wildflower Warden ─────────────────────────
        new GameDefinition {
            GameId = "wildflower_warden", GameName = "Wildflower Warden",
            Theme = "Prairie Guardian — Meadow Keeper",
            Description = "The wildflower warden tends an enchanted meadow blooming with magical coin prizes!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Fairy","Unicorn","Magic Mushroom","Elf","Dragonfly","Ancient Tree","Crystal","Wild","Scatter"},
            BgColor = "#001400", AccentColor = "#88FF44"
        },

        // ── 21 ─ Prairie Guardian: Prairie Dog Kingdom ───────────────────────
        new GameDefinition {
            GameId = "prairie_dog_kingdom", GameName = "Prairie Dog Kingdom",
            Theme = "Prairie Guardian — Underground Royals",
            Description = "Deep beneath the royal prairie a kingdom of treasure and refined casino riches awaits!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 500, MaxBet = 20_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Ace","King","Queen","Jack","Ten","Tuxedo","Martini","Wild","Scatter"},
            BgColor = "#0A000A", AccentColor = "#C0A000"
        },

        // ── 22 ─ Prairie Guardian: Sacred Crane Watch ────────────────────────
        new GameDefinition {
            GameId = "sacred_crane_watch", GameName = "Sacred Crane Watch",
            Theme = "Prairie Guardian — Sky Sentinel",
            Description = "The sacred crane watches over the wide-open skies granting patriotic prairie payouts!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 175f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Bald Eagle","American Flag","Liberty Bell","Shield","Stars","Arrow","Mountain","Wild","Scatter"},
            BgColor = "#00001A", AccentColor = "#FF4400"
        },

        // ── 23 ─ Prairie Guardian: Horizon Bison Chief ───────────────────────
        new GameDefinition {
            GameId = "horizon_bison_chief", GameName = "Horizon Bison Chief",
            Theme = "Prairie Guardian — Plains Conqueror",
            Description = "The horizon bison chief leads the charge across ancient plains to claim Caesar's gold!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 10_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 250f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Caesar","Colosseum","Roman Sword","Laurel Wreath","Eagle Standard","Chariot","Gladiator","Wild","Scatter"},
            BgColor = "#1A0D00", AccentColor = "#FFB800"
        },

        // ── 24 ─ Prairie Guardian: Lightning Hawk Tribe ──────────────────────
        new GameDefinition {
            GameId = "lightning_hawk_tribe", GameName = "Lightning Hawk Tribe",
            Theme = "Prairie Guardian — Storm Tribe",
            Description = "The lightning hawk tribe calls thunder and rain to uncover the lost temple gold!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 10_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 300f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 8,
            SymbolNames = new[]{"Sun Calendar","Jaguar","Quetzal","Gold Mask","Pyramid","Sacrificial Knife","Serpent","Wild","Scatter"},
            BgColor = "#1A0800", AccentColor = "#FF8C00"
        },

        // ── 25 ─ Prairie Guardian: Snow Owl Vigil ────────────────────────────
        new GameDefinition {
            GameId = "snow_owl_vigil", GameName = "Snow Owl Vigil",
            Theme = "Prairie Guardian — Winter Watcher",
            Description = "The snow owl keeps vigil through frozen prairie nights guarding the warrior's gold!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Samurai Warrior","Katana","Mount Fuji","Torii Gate","Geisha","Koi","Dragon Scroll","Wild","Scatter"},
            BgColor = "#0D0000", AccentColor = "#FF0022"
        },

        // ── 26 ─ Prairie Guardian: Spirit Bear Den ───────────────────────────
        new GameDefinition {
            GameId = "spirit_bear_den", GameName = "Spirit Bear Den",
            Theme = "Prairie Guardian — Hibernation Hoard",
            Description = "Deep in the spirit bear's winter den lie mountains of hoarded jackpot treasure!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Santa","Snowflake","Christmas Tree","Gift Box","Reindeer","Stocking","Bell","Wild","Scatter"},
            BgColor = "#00001A", AccentColor = "#00CCFF"
        },

        // ── 27 ─ Prairie Guardian: Rolling Thunder Stag ──────────────────────
        new GameDefinition {
            GameId = "rolling_thunder_stag", GameName = "Rolling Thunder Stag",
            Theme = "Prairie Guardian — Storm Strider",
            Description = "The rolling thunder stag clashes fire and ice antlers for elemental jackpot forces!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 200, MaxBet = 10_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 350f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 8,
            SymbolNames = new[]{"Fire Dragon","Ice Dragon","Flame","Snowflake","Lava","Glacier","Phoenix","Wild","Scatter"},
            BgColor = "#0D0005", AccentColor = "#FF6600"
        },

        // ── 28 ─ Prairie Guardian: Antelope Run Keeper ───────────────────────
        new GameDefinition {
            GameId = "antelope_run_keeper", GameName = "Antelope Run Keeper",
            Theme = "Prairie Guardian — River Runner",
            Description = "The swift antelope keeper races along river bends guarding sunken prairie fortunes!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 6_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 160f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Shark","Whale","Octopus","Sunken Ship","Pearl Oyster","Anglerfish","Sea Turtle","Wild","Scatter"},
            BgColor = "#000D1A", AccentColor = "#0088FF"
        },

        // ── 29 ─ Prairie Guardian: Medicine Hawk Circle ──────────────────────
        new GameDefinition {
            GameId = "medicine_hawk_circle", GameName = "Medicine Hawk Circle",
            Theme = "Prairie Guardian — Healing Ring",
            Description = "The medicine hawk flies in sacred circles raining healing coins and circus prizes!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Ringmaster","Acrobat","Lion","Clown","Trapeze","Magic Hat","Big Top","Wild","Scatter"},
            BgColor = "#0A0000", AccentColor = "#FF2200"
        },

        // ── 30 ─ Prairie Guardian: Dusty Coyote Trail ────────────────────────
        new GameDefinition {
            GameId = "dusty_coyote_trail", GameName = "Dusty Coyote Trail",
            Theme = "Prairie Guardian — Dusk Wanderer",
            Description = "The dusty coyote prowls twilight trails where monster jackpots lurk in the shadows!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 180f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Frankenstein","Dracula","Werewolf","Witch","Mummy","Pumpkin","Ghost","Wild","Scatter"},
            BgColor = "#0A0005", AccentColor = "#AA00FF"
        },

        // ── 31 ─ Prairie Guardian: Great Plains Watcher ──────────────────────
        new GameDefinition {
            GameId = "great_plains_watcher", GameName = "Great Plains Watcher",
            Theme = "Prairie Guardian — Iron Rail Overseer",
            Description = "The great plains watcher stands at the iron crossroads — all aboard the jackpot express!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Steam Engine","Gold Briefcase","Train Ticket","Stack of Cash","Station Clock","Conductor","Coal","Wild","Scatter"},
            BgColor = "#0D0800", AccentColor = "#FFD700"
        },

        // ── 32 ─ Prairie Guardian: Crimson Fox Spirit ────────────────────────
        new GameDefinition {
            GameId = "crimson_fox_spirit", GameName = "Crimson Fox Spirit",
            Theme = "Prairie Guardian — Lucky Trickster",
            Description = "The crimson fox spirit follows the rainbow to pots of prairie riches beyond the horizon!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 3_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Rainbow","Pot of Gold","Leprechaun","Wishing Well","Road to Riches","Pots of Plenty","Toadstool","Wild","Scatter"},
            BgColor = "#000D00", AccentColor = "#FFDD00"
        },

        // ── 33 ─ Prairie Guardian: Wandering Bison Guard ─────────────────────
        new GameDefinition {
            GameId = "wandering_bison_guard", GameName = "Wandering Bison Guard",
            Theme = "Prairie Guardian — Crystal Herd",
            Description = "The wandering bison guard tramples crystal meadows unlocking legendary jackpot power!",
            ReelCount = 5, RowCount = 4, PaylineCount = 40,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 250f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Amethyst","Topaz","Obsidian","Sapphire Crown","Crystal Sword","Runed Crystal","Crystal Dragon","Wild","Scatter"},
            BgColor = "#05000F", AccentColor = "#AA55FF"
        },

        // ── 34 ─ Prairie Guardian: Dawn Eagle Rising ─────────────────────────
        new GameDefinition {
            GameId = "dawn_eagle_rising", GameName = "Dawn Eagle Rising",
            Theme = "Prairie Guardian — Thunder God of the Plains",
            Description = "At dawn the eagle rises with the thunder gods, blessing reels with Valhalla-worthy wins!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 300, MaxBet = 12_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 350f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 8,
            SymbolNames = new[]{"Thor","Odin","Mjolnir","Valhalla","Raven","Runic Symbol","Viking Ship","Wild","Scatter"},
            BgColor = "#00051A", AccentColor = "#4488FF"
        },

        // ── 35 ─ Prairie Guardian: Prairie Wolf Moon ─────────────────────────
        new GameDefinition {
            GameId = "prairie_wolf_moon", GameName = "Prairie Wolf Moon",
            Theme = "Prairie Guardian — Lunar Pack",
            Description = "Under the full prairie moon the wolf pack howls blessings of bamboo gold and fortune!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Giant Panda","Red Panda","Bamboo","Fortune Cookie","Jade","Lotus","Fireworks","Wild","Scatter"},
            BgColor = "#001A00", AccentColor = "#00CC88"
        },

        // ── 36 ─ Prairie Guardian: Stone Bear Totem ──────────────────────────
        new GameDefinition {
            GameId = "stone_bear_totem", GameName = "Stone Bear Totem",
            Theme = "Prairie Guardian — Ancient Totem",
            Description = "The ancient stone bear totem stands guard over Viking-raided prairie treasure shores!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 9_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 220f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Viking Warrior","Longship","Battle Axe","Horned Helmet","Serpent","Rune Chest","Nordic Knot","Wild","Scatter"},
            BgColor = "#00030D", AccentColor = "#5599FF"
        },

        // ── 37 ─ Prairie Guardian: Silver Hawk Blessing ──────────────────────
        new GameDefinition {
            GameId = "silver_hawk_blessing", GameName = "Silver Hawk Blessing",
            Theme = "Prairie Guardian — Moonlit Vault",
            Description = "The silver hawk blesses the moonlit vault with immortal prairie riches and gothic gold!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 250f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Vampire Lord","Bat","Blood Vial","Gothic Cross","Castle","Full Moon","Coffin","Wild","Scatter"},
            BgColor = "#050005", AccentColor = "#CC0000"
        },

        // ── 38 ─ Prairie Guardian: Sacred Serpent Plains ─────────────────────
        new GameDefinition {
            GameId = "sacred_serpent_plains", GameName = "Sacred Serpent Plains",
            Theme = "Prairie Guardian — Venom & Visions",
            Description = "The sacred serpent slithers through magic prairie plains brewing potions of staggering wins!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 160f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Witch","Cauldron","Broomstick","Spell Book","Black Cat","Toad","Potion Bottle","Wild","Scatter"},
            BgColor = "#050014", AccentColor = "#8800FF"
        },

        // ── 39 ─ Prairie Guardian: Golden Prairie Chief ───────────────────────
        new GameDefinition {
            GameId = "golden_prairie_chief", GameName = "Golden Prairie Chief",
            Theme = "Prairie Guardian — Prestige Elder",
            Description = "The golden prairie chief opens the gates to opulent plains of jackpot prestige!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 500, MaxBet = 15_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 300f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Golden Bridge","Luxury Car","Diamond Ring","Penthouse","Private Jet","Gold Bars","Crown","Wild","Scatter"},
            BgColor = "#0D0800", AccentColor = "#FFD700"
        },

        // ── 40 ─ Prairie Guardian: Eternal Guardian Spirit ───────────────────
        new GameDefinition {
            GameId = "eternal_guardian_spirit", GameName = "Eternal Guardian Spirit",
            Theme = "Prairie Guardian — Immortal Protector",
            Description = "The eternal guardian spirit of the great prairie watches over the mother of all jackpots!",
            ReelCount = 5, RowCount = 4, PaylineCount = 50,
            MinBet = 1000, MaxBet = 50_000_000, BaseRTP = 0.70f, MaxWinMultiplier = 500f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 5,
            SymbolNames = new[]{"Mega 7","Jackpot Bell","Diamond Star","Money Bag","Golden Trophy","Jackpot Wheel","Lucky Number","Wild","Scatter"},
            BgColor = "#0A0000", AccentColor = "#FFD700"
        },
    };
}
