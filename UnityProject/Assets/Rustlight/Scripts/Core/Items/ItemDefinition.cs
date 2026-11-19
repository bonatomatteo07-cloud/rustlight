using System.Collections.Generic;
using Rustlight.Core.Stats;

namespace Rustlight.Core.Items
{
    public enum EquipSlot { None, Weapon, Armor, Gadget }

    public enum Rarity { Common, Uncommon, Rare, Epic }

    /// <summary>Static data for an item type. Instances in the inventory refer to it by <see cref="Id"/>.</summary>
    public sealed class ItemDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public EquipSlot Slot { get; }
        public Rarity Rarity { get; }
        public int MaxStack { get; }
        public int SellValue { get; }
        public IReadOnlyList<StatModifier> Modifiers { get; }

        public ItemDefinition(string id, string displayName, EquipSlot slot = EquipSlot.None,
            Rarity rarity = Rarity.Common, int maxStack = 1, int sellValue = 0,
            IReadOnlyList<StatModifier> modifiers = null)
        {
            Id = id;
            DisplayName = displayName;
            Slot = slot;
            Rarity = rarity;
            MaxStack = maxStack < 1 ? 1 : maxStack;
            SellValue = sellValue;
            Modifiers = modifiers ?? new StatModifier[0];
        }

        public bool IsEquippable => Slot != EquipSlot.None;
    }

    /// <summary>Lookup of item definitions by id.</summary>
    public sealed class ItemDatabase
    {
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

        public ItemDatabase(IEnumerable<ItemDefinition> items)
        {
            foreach (var item in items) _items[item.Id] = item;
        }

        public ItemDefinition Get(string id) => id != null && _items.TryGetValue(id, out var d) ? d : null;
    }
}
