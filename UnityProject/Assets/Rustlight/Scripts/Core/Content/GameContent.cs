using System.Collections.Generic;
using Rustlight.Core.Economy;
using Rustlight.Core.Items;
using Rustlight.Core.Quests;
using Rustlight.Core.Stats;

namespace Rustlight.Core.Content
{
    /// <summary>
    /// First-zone content as code so it can be tested outside Unity. Move it to ScriptableObjects
    /// once designers need to edit it in the Inspector.
    /// </summary>
    public static class GameContent
    {
        public static IEnumerable<ItemDefinition> Items() => new[]
        {
            new ItemDefinition("scrap_metal", "Rottame metallico", maxStack: 99, sellValue: 2),
            new ItemDefinition("medkit", "Medikit", maxStack: 10, sellValue: 15),
            new ItemDefinition("pipe_wrench", "Chiave a tubo", EquipSlot.Weapon, Rarity.Common, sellValue: 10,
                modifiers: new[] { new StatModifier(StatType.Attack, ModifierKind.Flat, 4) }),
            new ItemDefinition("rail_carbine", "Carabina a rotaia", EquipSlot.Weapon, Rarity.Rare, sellValue: 120,
                modifiers: new[]
                {
                    new StatModifier(StatType.Attack, ModifierKind.Flat, 12),
                    new StatModifier(StatType.CritChance, ModifierKind.Flat, 0.05f),
                }),
            new ItemDefinition("patched_vest", "Giubbotto rattoppato", EquipSlot.Armor, Rarity.Common, sellValue: 12,
                modifiers: new[] { new StatModifier(StatType.Armor, ModifierKind.Flat, 8) }),
            new ItemDefinition("rad_plating", "Piastre anti-radiazioni", EquipSlot.Armor, Rarity.Uncommon, sellValue: 45,
                modifiers: new[]
                {
                    new StatModifier(StatType.Armor, ModifierKind.Flat, 15),
                    new StatModifier(StatType.MaxHealth, ModifierKind.Percent, 0.10f),
                }),
            new ItemDefinition("servo_boots", "Stivali servoassistiti", EquipSlot.Gadget, Rarity.Uncommon, sellValue: 40,
                modifiers: new[] { new StatModifier(StatType.MoveSpeed, ModifierKind.Percent, 0.15f) }),
        };

        public static IEnumerable<CosmeticDefinition> Cosmetics() => new[]
        {
            new CosmeticDefinition("suit_default", "Tuta da recuperatore", CosmeticKind.Suit, 0),
            new CosmeticDefinition("suit_scavenger", "Predone delle dune", CosmeticKind.Suit, 0),
            new CosmeticDefinition("suit_vanguard", "Avanguardia", CosmeticKind.Suit, 0),
            new CosmeticDefinition("suit_diver", "Palombaro della Città Sommersa", CosmeticKind.Suit, 0),
            new CosmeticDefinition("suit_neon", "Neon del vecchio mondo", CosmeticKind.Suit, 40),
            new CosmeticDefinition("drone_default", "Drone standard", CosmeticKind.Drone, 0),
            new CosmeticDefinition("drone_halo", "Drone Aureola", CosmeticKind.Drone, 0),
            new CosmeticDefinition("drone_rust", "Drone Ruggine", CosmeticKind.Drone, 25),
        };

        public static IEnumerable<QuestDefinition> Quests() => new[]
        {
            new QuestDefinition("q_first_steps", "Primi passi fuori dal Bunker", ObjectiveType.Reach, "zone_scrapyard", 1, 50, 20),
            new QuestDefinition("q_raiders", "Ripulire il deposito", ObjectiveType.Kill, "raider", 5, 120, 60, rewardCores: 10),
            new QuestDefinition("q_scrap", "Materiale per il generatore", ObjectiveType.Collect, "scrap_metal", 20, 80, 40, rewardCores: 5),
        };
    }
}
