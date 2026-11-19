using System;
using System.Collections.Generic;

namespace Rustlight.Core.Items
{
    /// <summary>One inventory slot: an item id and how many are stacked in it.</summary>
    [Serializable]
    public sealed class ItemStack
    {
        public string itemId;
        public int count;
    }

    /// <summary>
    /// Slot-limited bag. Stackable items fill existing stacks first; <see cref="TryAdd"/> is
    /// all-or-nothing so a pickup never half-succeeds.
    /// </summary>
    public sealed class Inventory
    {
        private readonly ItemDatabase _db;
        private readonly List<ItemStack> _stacks;

        public int Capacity { get; private set; }
        public IReadOnlyList<ItemStack> Stacks => _stacks;
        public event Action Changed;

        /// <summary>Wraps an existing stack list (usually the one inside the save data).</summary>
        public Inventory(ItemDatabase db, List<ItemStack> stacks, int capacity)
        {
            _db = db;
            _stacks = stacks;
            Capacity = capacity;
        }

        /// <summary>
        /// Changes the slot count (e.g. when a DLC is installed or removed). Shrinking never deletes items:
        /// the bag is simply over capacity and accepts no new stacks until space frees up.
        /// </summary>
        public void SetCapacity(int capacity)
        {
            if (capacity == Capacity) return;
            Capacity = capacity < 0 ? 0 : capacity;
            Changed?.Invoke();
        }

        public int CountOf(string itemId)
        {
            int total = 0;
            foreach (var s in _stacks) if (s.itemId == itemId) total += s.count;
            return total;
        }

        /// <summary>Adds <paramref name="count"/> items if they all fit; otherwise changes nothing.</summary>
        public bool TryAdd(string itemId, int count = 1)
        {
            var def = _db.Get(itemId);
            if (def == null || count <= 0) return false;
            if (!Fits(def, count)) return false;

            int remaining = count;
            foreach (var s in _stacks)
            {
                if (remaining == 0) break;
                if (s.itemId != itemId || s.count >= def.MaxStack) continue;
                int add = Math.Min(def.MaxStack - s.count, remaining);
                s.count += add;
                remaining -= add;
            }
            while (remaining > 0)
            {
                int add = Math.Min(def.MaxStack, remaining);
                _stacks.Add(new ItemStack { itemId = itemId, count = add });
                remaining -= add;
            }
            Changed?.Invoke();
            return true;
        }

        /// <summary>Removes <paramref name="count"/> items if that many are held; otherwise changes nothing.</summary>
        public bool TryRemove(string itemId, int count = 1)
        {
            if (count <= 0 || CountOf(itemId) < count) return false;
            int remaining = count;
            for (int i = _stacks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var s = _stacks[i];
                if (s.itemId != itemId) continue;
                int take = Math.Min(s.count, remaining);
                s.count -= take;
                remaining -= take;
                if (s.count == 0) _stacks.RemoveAt(i);
            }
            Changed?.Invoke();
            return true;
        }

        private bool Fits(ItemDefinition def, int count)
        {
            int room = 0;
            foreach (var s in _stacks)
                if (s.itemId == def.Id) room += def.MaxStack - s.count;
            if (room >= count) return true;
            int freeSlots = Capacity - _stacks.Count;
            long slotRoom = (long)freeSlots * def.MaxStack;
            return freeSlots > 0 && room + slotRoom >= count;
        }
    }
}
