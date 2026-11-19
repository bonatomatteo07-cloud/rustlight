using System;
using System.Collections.Generic;
using Rustlight.Core.Stats;

namespace Rustlight.Core.Items
{
    /// <summary>Saved record of what is equipped in one slot.</summary>
    [Serializable]
    public sealed class EquippedEntry
    {
        public EquipSlot slot;
        public string itemId;
    }

    /// <summary>
    /// Moves items between the inventory and equip slots and keeps the stat block in sync.
    /// Each slot's modifiers use the source "equip:&lt;slot&gt;", so unequipping removes exactly them.
    /// </summary>
    public sealed class Equipment
    {
        private readonly ItemDatabase _db;
        private readonly Inventory _inventory;
        private readonly StatBlock _stats;
        private readonly List<EquippedEntry> _entries;

        public event Action Changed;

        public Equipment(ItemDatabase db, Inventory inventory, StatBlock stats, List<EquippedEntry> entries)
        {
            _db = db;
            _inventory = inventory;
            _stats = stats;
            _entries = entries;
            foreach (var e in _entries) ApplyModifiers(e.slot, _db.Get(e.itemId));
        }

        public string GetEquipped(EquipSlot slot)
        {
            foreach (var e in _entries) if (e.slot == slot) return e.itemId;
            return null;
        }

        /// <summary>Equips an item from the inventory, swapping out whatever was in its slot.</summary>
        public bool TryEquip(string itemId)
        {
            var def = _db.Get(itemId);
            if (def == null || !def.IsEquippable || _inventory.CountOf(itemId) == 0) return false;

            string previous = GetEquipped(def.Slot);
            if (!_inventory.TryRemove(itemId)) return false;
            if (previous != null && !_inventory.TryAdd(previous))
            {
                _inventory.TryAdd(itemId); // roll back: no room for the old item
                return false;
            }

            SetEntry(def.Slot, itemId);
            _stats.RemoveModifiersFrom(SourceFor(def.Slot));
            ApplyModifiers(def.Slot, def);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Returns the item in <paramref name="slot"/> to the inventory if there is room.</summary>
        public bool TryUnequip(EquipSlot slot)
        {
            string current = GetEquipped(slot);
            if (current == null || !_inventory.TryAdd(current)) return false;
            _entries.RemoveAll(e => e.slot == slot);
            _stats.RemoveModifiersFrom(SourceFor(slot));
            Changed?.Invoke();
            return true;
        }

        private void SetEntry(EquipSlot slot, string itemId)
        {
            foreach (var e in _entries)
                if (e.slot == slot) { e.itemId = itemId; return; }
            _entries.Add(new EquippedEntry { slot = slot, itemId = itemId });
        }

        private void ApplyModifiers(EquipSlot slot, ItemDefinition def)
        {
            if (def == null) return;
            foreach (var m in def.Modifiers) _stats.AddModifier(m.WithSource(SourceFor(slot)));
        }

        private static string SourceFor(EquipSlot slot) => "equip:" + slot;
    }
}
