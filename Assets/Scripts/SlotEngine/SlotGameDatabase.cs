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
        public string   BgColor;    // hex
        public string   AccentColor;
    }

    public static readonly List<GameDefinition> All = new List<GameDefinition>
    {
        // ── 1 ─ Inspired by Slotomania classics ─────────────────────────────
        new GameDefinition {
            GameId = "diamond_fever", GameName = "Diamond Fever",
            Theme = "Classic Jewels", Description = "Brilliant diamonds line up for sparkling riches!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Diamond","Ruby","Emerald","Sapphire","Gold Bar","Seven","Bell","Wild","Scatter"},
            BgColor = "#0D0D2B", AccentColor = "#00CFFF"
        },

        // ── 2 ──────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "lucky_lion", GameName = "Lucky Lion",
            Theme = "Asian Luck", Description = "Roar into fortune with the mighty golden lion!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Golden Lion","Dragon","Koi Fish","Lucky Coin","Lantern","Cherry Blossom","Yin Yang","Wild","Scatter"},
            BgColor = "#1A0000", AccentColor = "#FFD700"
        },

        // ── 3 ──────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "golden_pharaoh", GameName = "Golden Pharaoh",
            Theme = "Ancient Egypt", Description = "Unlock the secrets of the pyramid for endless gold!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 10_000_000, BaseRTP = 0.95f, MaxWinMultiplier = 250f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Pharaoh","Sphinx","Ankh","Eye of Ra","Scarab","Pyramid","Cleopatra","Wild","Scatter"},
            BgColor = "#1C1200", AccentColor = "#FFB800"
        },

        // ── 4 ──────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "jungle_jackpot", GameName = "Jungle Jackpot",
            Theme = "Safari & Jungle", Description = "Swing through the jungle canopy to massive payouts!",
            ReelCount = 5, RowCount = 4, PaylineCount = 40,
            MinBet = 200, MaxBet = 6_000_000, BaseRTP = 0.92f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Tiger","Gorilla","Parrot","Snake","Elephant","Monkey","Toucan","Wild","Scatter"},
            BgColor = "#001A00", AccentColor = "#39FF14"
        },

        // ── 5 ──────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "mermaids_treasure", GameName = "Mermaid's Treasure",
            Theme = "Underwater Fantasy", Description = "Dive deep for sunken treasures and oceanic wins!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 180f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Mermaid","Seahorse","Starfish","Pearl","Treasure Chest","Dolphin","Coral","Wild","Scatter"},
            BgColor = "#001433", AccentColor = "#00E5FF"
        },

        // ── 6 ──────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "dragon_fortune", GameName = "Dragon Fortune",
            Theme = "Dragon & Fire", Description = "The dragon's breath ignites your reels with fire wins!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 500, MaxBet = 12_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 300f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 8,
            SymbolNames = new[]{"Red Dragon","Fire Orb","Dragon Egg","Gold Coin Stack","Jade","Sword","Phoenix","Wild","Scatter"},
            BgColor = "#1A0500", AccentColor = "#FF4500"
        },

        // ── 7 ──────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "neon_nights", GameName = "Neon Nights",
            Theme = "Vegas Neon", Description = "Light up the Las Vegas strip in this neon extravaganza!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = false, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Neon Seven","Neon Bar","Neon Bell","Neon Cherry","Neon Star","Playing Card","Dice","Wild","Scatter"},
            BgColor = "#000033", AccentColor = "#FF00FF"
        },

        // ── 8 ──────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "wild_west_riches", GameName = "Wild West Riches",
            Theme = "Western", Description = "Draw faster than the sheriff for golden outlaw riches!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 7_000_000, BaseRTP = 0.92f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Sheriff Star","Cowboy Hat","Revolver","Horseshoe","Gold Nugget","Cactus","Wanted Poster","Wild","Scatter"},
            BgColor = "#1A1000", AccentColor = "#C8860A"
        },

        // ── 9 ──────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "mystic_moon", GameName = "Mystic Moon",
            Theme = "Mystical & Lunar", Description = "Under the full moon, mystical powers multiply your wins!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Full Moon","Crystal Ball","Owl","Wolf","Pentagram","Potion","Rune Stone","Wild","Scatter"},
            BgColor = "#05001A", AccentColor = "#C0A0FF"
        },

        // ── 10 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "sugar_spree", GameName = "Sugar Spree",
            Theme = "Candy & Sweet", Description = "A sugar-coated world of sweet, sticky jackpots!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 100, MaxBet = 3_000_000, BaseRTP = 0.91f, MaxWinMultiplier = 80f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Lollipop","Cupcake","Gummy Bear","Ice Cream","Candy Cane","Jelly Bean","Cotton Candy","Wild","Scatter"},
            BgColor = "#1A0020", AccentColor = "#FF69B4"
        },

        // ── 11 ─ Inspired by Pop Slots ─────────────────────────────────────
        new GameDefinition {
            GameId = "vegas_strip", GameName = "Vegas Strip",
            Theme = "Classic Vegas", Description = "The glitz and glamour of the famous Vegas Strip!",
            ReelCount = 3, RowCount = 3, PaylineCount = 5,
            MinBet = 100, MaxBet = 1_000_000, BaseRTP = 0.95f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = false, HasBonus = false, HasFreeSpins = false, HasProgressive = true, FreeSpinsCount = 0,
            SymbolNames = new[]{"Triple Seven","Bar","Double Bar","Triple Bar","Bell","Cherry","Lemon","Wild","Scatter"},
            BgColor = "#0D0000", AccentColor = "#FFD700"
        },

        // ── 12 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "hollywood_dreams", GameName = "Hollywood Dreams",
            Theme = "Movies & Glamour", Description = "Your moment in the spotlight — lights, camera, jackpot!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Oscar Trophy","Film Reel","Clapperboard","Star","Director's Chair","Popcorn","Sunglasses","Wild","Scatter"},
            BgColor = "#0A000A", AccentColor = "#FFD700"
        },

        // ── 13 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "rock_star_riches", GameName = "Rock Star Riches",
            Theme = "Music & Rock", Description = "Turn up the volume and shred your way to riches!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 6_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 180f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Electric Guitar","Microphone","Drum Kit","Bass","Vinyl Record","Amp","Rock Fist","Wild","Scatter"},
            BgColor = "#0D0005", AccentColor = "#FF0055"
        },

        // ── 14 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "party_palace", GameName = "Party Palace",
            Theme = "Celebration & Party", Description = "Non-stop party with big wins at every corner!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.92f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Champagne","Confetti","Party Hat","Balloon","DJ Booth","Dance Floor","Trophy","Wild","Scatter"},
            BgColor = "#0A0020", AccentColor = "#FF00CC"
        },

        // ── 15 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "tropical_paradise", GameName = "Tropical Paradise",
            Theme = "Beach & Tropical", Description = "Sip a coconut drink while collecting tropical treasures!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Palm Tree","Toucan","Hibiscus","Coconut","Surfboard","Pineapple","Sunset","Wild","Scatter"},
            BgColor = "#001A1A", AccentColor = "#00FFD0"
        },

        // ── 16 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "lucky_clover", GameName = "Lucky Clover",
            Theme = "Irish Luck", Description = "The luck of the Irish brings four-leaf fortunes!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 3_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Four Leaf Clover","Leprechaun","Rainbow","Gold Pot","Horseshoe","Harp","Shamrock","Wild","Scatter"},
            BgColor = "#001200", AccentColor = "#00CC44"
        },

        // ── 17 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "speed_demon", GameName = "Speed Demon",
            Theme = "Racing & Speed", Description = "Hit the throttle and race to the jackpot finish line!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 7_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 8,
            SymbolNames = new[]{"Race Car","Checkered Flag","Helmet","Trophy","Pit Stop","Speedometer","Tire","Wild","Scatter"},
            BgColor = "#0D0000", AccentColor = "#FF3300"
        },

        // ── 18 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "space_quest", GameName = "Space Quest",
            Theme = "Space & Sci-Fi", Description = "Blast off into the cosmos for out-of-this-world jackpots!",
            ReelCount = 5, RowCount = 4, PaylineCount = 40,
            MinBet = 200, MaxBet = 10_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 300f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Rocket","Alien","Planet","Asteroid","Space Station","Astronaut","Black Hole","Wild","Scatter"},
            BgColor = "#00001A", AccentColor = "#4400FF"
        },

        // ── 19 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "pirates_plunder", GameName = "Pirate's Plunder",
            Theme = "Pirates", Description = "Set sail for treasure islands and plunder the high seas!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 6_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Pirate Captain","Treasure Map","Skull & Crossbones","Cannon","Parrot","Treasure Chest","Compass","Wild","Scatter"},
            BgColor = "#000D1A", AccentColor = "#C8A000"
        },

        // ── 20 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "enchanted_forest", GameName = "Enchanted Forest",
            Theme = "Fairy Tale", Description = "Wander through a magical forest of wonder and wealth!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.92f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Fairy","Unicorn","Magic Mushroom","Elf","Dragonfly","Ancient Tree","Crystal","Wild","Scatter"},
            BgColor = "#001400", AccentColor = "#88FF44"
        },

        // ── 21 ─ Inspired by Grand Slots ───────────────────────────────────
        new GameDefinition {
            GameId = "casino_royale", GameName = "Casino Royale",
            Theme = "Classic Casino", Description = "The ultimate casino experience — refined, lavish, lucrative!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 500, MaxBet = 20_000_000, BaseRTP = 0.96f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Ace","King","Queen","Jack","Ten","Tuxedo","Martini","Wild","Scatter"},
            BgColor = "#0A000A", AccentColor = "#C0A000"
        },

        // ── 22 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "golden_eagle", GameName = "Golden Eagle",
            Theme = "American Pride", Description = "The majestic eagle soars high for patriotic payouts!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 175f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Bald Eagle","American Flag","Liberty Bell","Shield","Stars","Arrow","Mountain","Wild","Scatter"},
            BgColor = "#00001A", AccentColor = "#FF4400"
        },

        // ── 23 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "ancient_rome", GameName = "Ancient Rome",
            Theme = "Roman Empire", Description = "Conquer the empire and claim a Caesar's fortune!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 10_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 250f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Caesar","Colosseum","Roman Sword","Laurel Wreath","Eagle Standard","Chariot","Gladiator","Wild","Scatter"},
            BgColor = "#1A0D00", AccentColor = "#FFB800"
        },

        // ── 24 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "aztec_empire", GameName = "Aztec Empire",
            Theme = "Aztec Civilization", Description = "Uncover the lost temples and claim Aztec gold!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 10_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 300f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 8,
            SymbolNames = new[]{"Sun Calendar","Jaguar","Quetzal","Gold Mask","Pyramid","Sacrificial Knife","Serpent","Wild","Scatter"},
            BgColor = "#1A0800", AccentColor = "#FF8C00"
        },

        // ── 25 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "samurai_spirit", GameName = "Samurai Spirit",
            Theme = "Feudal Japan", Description = "The way of the warrior leads to a path of pure gold!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Samurai Warrior","Katana","Mount Fuji","Torii Gate","Geisha","Koi","Dragon Scroll","Wild","Scatter"},
            BgColor = "#0D0000", AccentColor = "#FF0022"
        },

        // ── 26 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "winter_jackpot", GameName = "Winter Jackpot",
            Theme = "Winter & Christmas", Description = "Santa's sleigh delivers jackpots straight to your screen!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 150f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Santa","Snowflake","Christmas Tree","Gift Box","Reindeer","Stocking","Bell","Wild","Scatter"},
            BgColor = "#00001A", AccentColor = "#00CCFF"
        },

        // ── 27 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "fire_and_ice", GameName = "Fire & Ice",
            Theme = "Elemental", Description = "Two forces collide — fire and ice battle for ultimate prizes!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 200, MaxBet = 10_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 350f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 8,
            SymbolNames = new[]{"Fire Dragon","Ice Dragon","Flame","Snowflake","Lava","Glacier","Phoenix","Wild","Scatter"},
            BgColor = "#0D0005", AccentColor = "#FF6600"
        },

        // ── 28 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "oceans_fortune", GameName = "Ocean's Fortune",
            Theme = "Deep Sea", Description = "The ocean's depths hide treasures beyond imagination!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 6_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 160f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Shark","Whale","Octopus","Sunken Ship","Pearl Oyster","Anglerfish","Sea Turtle","Wild","Scatter"},
            BgColor = "#000D1A", AccentColor = "#0088FF"
        },

        // ── 29 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "circus_big_top", GameName = "Circus Big Top",
            Theme = "Circus", Description = "Roll up, roll up — the greatest slot show on Earth!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.92f, MaxWinMultiplier = 100f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Ringmaster","Acrobat","Lion","Clown","Trapeze","Magic Hat","Big Top","Wild","Scatter"},
            BgColor = "#0A0000", AccentColor = "#FF2200"
        },

        // ── 30 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "monster_bash", GameName = "Monster Bash",
            Theme = "Monsters & Halloween", Description = "Monsters are loose and jackpots are lurking in the dark!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 180f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Frankenstein","Dracula","Werewolf","Witch","Mummy","Pumpkin","Ghost","Wild","Scatter"},
            BgColor = "#0A0005", AccentColor = "#AA00FF"
        },

        // ── 31 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "big_money_express", GameName = "Big Money Express",
            Theme = "Trains & Travel", Description = "All aboard the Big Money Express — next stop: jackpot!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 200f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Steam Engine","Gold Briefcase","Train Ticket","Stack of Cash","Station Clock","Conductor","Coal","Wild","Scatter"},
            BgColor = "#0D0800", AccentColor = "#FFD700"
        },

        // ── 32 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "rainbow_riches_slots", GameName = "Rainbow Riches",
            Theme = "Rainbow & Luck", Description = "Follow the rainbow to find your pot of golden riches!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 3_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Rainbow","Pot of Gold","Leprechaun","Wishing Well","Road to Riches","Pots of Plenty","Toadstool","Wild","Scatter"},
            BgColor = "#000D00", AccentColor = "#FFDD00"
        },

        // ── 33 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "crystal_kingdom", GameName = "Crystal Kingdom",
            Theme = "Fantasy Crystals", Description = "Crystals of power align to unleash legendary jackpots!",
            ReelCount = 5, RowCount = 4, PaylineCount = 40,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 250f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Amethyst","Topaz","Obsidian","Sapphire Crown","Crystal Sword","Runed Crystal","Crystal Dragon","Wild","Scatter"},
            BgColor = "#05000F", AccentColor = "#AA55FF"
        },

        // ── 34 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "thunder_gods", GameName = "Thunder Gods",
            Theme = "Norse Mythology", Description = "Thor and Odin bless your reels with thunderous wins!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 300, MaxBet = 12_000_000, BaseRTP = 0.94f, MaxWinMultiplier = 350f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 8,
            SymbolNames = new[]{"Thor","Odin","Mjolnir","Valhalla","Raven","Runic Symbol","Viking Ship","Wild","Scatter"},
            BgColor = "#00051A", AccentColor = "#4488FF"
        },

        // ── 35 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "lucky_panda", GameName = "Lucky Panda",
            Theme = "Panda & Bamboo", Description = "This adorable panda brings bamboo-loads of good fortune!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 100, MaxBet = 4_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 120f,
            Volatility = SlotGameConfig.Volatility.Low,
            HasWild = true, HasScatter = true, HasBonus = false, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 15,
            SymbolNames = new[]{"Giant Panda","Red Panda","Bamboo","Fortune Cookie","Jade","Lotus","Fireworks","Wild","Scatter"},
            BgColor = "#001A00", AccentColor = "#00CC88"
        },

        // ── 36 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "viking_voyage", GameName = "Viking Voyage",
            Theme = "Viking Adventure", Description = "Set sail with fearless Vikings to raid treasure shores!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 9_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 220f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Viking Warrior","Longship","Battle Axe","Horned Helmet","Serpent","Rune Chest","Nordic Knot","Wild","Scatter"},
            BgColor = "#00030D", AccentColor = "#5599FF"
        },

        // ── 37 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "vampires_vault", GameName = "Vampire's Vault",
            Theme = "Gothic Vampire", Description = "Unlock the vampire's vault to claim immortal riches!",
            ReelCount = 5, RowCount = 3, PaylineCount = 25,
            MinBet = 200, MaxBet = 8_000_000, BaseRTP = 0.93f, MaxWinMultiplier = 250f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 10,
            SymbolNames = new[]{"Vampire Lord","Bat","Blood Vial","Gothic Cross","Castle","Full Moon","Coffin","Wild","Scatter"},
            BgColor = "#050005", AccentColor = "#CC0000"
        },

        // ── 38 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "witchs_cauldron", GameName = "Witch's Cauldron",
            Theme = "Witches & Magic", Description = "Brew up a potion of staggering slot magic!",
            ReelCount = 5, RowCount = 3, PaylineCount = 20,
            MinBet = 100, MaxBet = 5_000_000, BaseRTP = 0.92f, MaxWinMultiplier = 160f,
            Volatility = SlotGameConfig.Volatility.Medium,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = false, FreeSpinsCount = 12,
            SymbolNames = new[]{"Witch","Cauldron","Broomstick","Spell Book","Black Cat","Toad","Potion Bottle","Wild","Scatter"},
            BgColor = "#050014", AccentColor = "#8800FF"
        },

        // ── 39 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "golden_gate", GameName = "Golden Gate",
            Theme = "Luxury & Prestige", Description = "Cross the Golden Gate to the land of opulent jackpots!",
            ReelCount = 5, RowCount = 3, PaylineCount = 30,
            MinBet = 500, MaxBet = 15_000_000, BaseRTP = 0.95f, MaxWinMultiplier = 300f,
            Volatility = SlotGameConfig.Volatility.High,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 10,
            SymbolNames = new[]{"Golden Bridge","Luxury Car","Diamond Ring","Penthouse","Private Jet","Gold Bars","Crown","Wild","Scatter"},
            BgColor = "#0D0800", AccentColor = "#FFD700"
        },

        // ── 40 ─────────────────────────────────────────────────────────────
        new GameDefinition {
            GameId = "mega_jackpot_mania", GameName = "Mega Jackpot Mania",
            Theme = "Ultimate Jackpot", Description = "The mother of all jackpots — spin for life-changing riches!",
            ReelCount = 5, RowCount = 4, PaylineCount = 50,
            MinBet = 1000, MaxBet = 50_000_000, BaseRTP = 0.96f, MaxWinMultiplier = 500f,
            Volatility = SlotGameConfig.Volatility.VeryHigh,
            HasWild = true, HasScatter = true, HasBonus = true, HasFreeSpins = true, HasProgressive = true, FreeSpinsCount = 5,
            SymbolNames = new[]{"Mega 7","Jackpot Bell","Diamond Star","Money Bag","Golden Trophy","Jackpot Wheel","Lucky Number","Wild","Scatter"},
            BgColor = "#0A0000", AccentColor = "#FFD700"
        },
    };
}
