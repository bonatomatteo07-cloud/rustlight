using System;
using System.Collections.Generic;
using Rustlight.Core.Save;

namespace Rustlight.Core.Economy
{
    public enum CosmeticKind { Suit, Drone }

    /// <summary>A purely visual unlock. Cosmetics never change stats, so nothing sold is pay-to-win.</summary>
    public sealed class CosmeticDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public CosmeticKind Kind { get; }
        /// <summary>Price in Cores; 0 means it can only be obtained from a pack or quest.</summary>
        public int PriceCores { get; }

        public CosmeticDefinition(string id, string displayName, CosmeticKind kind, int priceCores)
        {
            Id = id; DisplayName = displayName; Kind = kind; PriceCores = priceCores;
        }
    }

    /// <summary>Buys cosmetics with Cores, tracks ownership and which suit and drone skin are worn.</summary>
    public sealed class CosmeticShop
    {
        private readonly Dictionary<string, CosmeticDefinition> _catalog = new Dictionary<string, CosmeticDefinition>();
        private readonly SaveData _save;
        private readonly Wallet _wallet;

        public event Action Changed;
        public IEnumerable<CosmeticDefinition> Catalog => _catalog.Values;

        public CosmeticShop(IEnumerable<CosmeticDefinition> catalog, SaveData save, Wallet wallet)
        {
            foreach (var c in catalog) _catalog[c.Id] = c;
            _save = save;
            _wallet = wallet;
        }

        public bool Owns(string cosmeticId) => _save.ownedCosmetics.Contains(cosmeticId);

        /// <summary>Unlocks without charging (pack rewards, quests). Returns false if already owned or unknown.</summary>
        public bool Grant(string cosmeticId)
        {
            if (!_catalog.ContainsKey(cosmeticId) || Owns(cosmeticId)) return false;
            _save.ownedCosmetics.Add(cosmeticId);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Charges the Core price and unlocks; fails if unknown, owned, not for sale or unaffordable.</summary>
        public bool TryBuy(string cosmeticId)
        {
            if (!_catalog.TryGetValue(cosmeticId, out var def) || Owns(cosmeticId) || def.PriceCores <= 0) return false;
            if (!_wallet.TrySpendCores(def.PriceCores)) return false;
            return Grant(cosmeticId);
        }

        /// <summary>Wears an owned cosmetic in its slot.</summary>
        public bool TryWear(string cosmeticId)
        {
            if (!_catalog.TryGetValue(cosmeticId, out var def) || !Owns(cosmeticId)) return false;
            if (def.Kind == CosmeticKind.Suit) _save.activeSuit = cosmeticId;
            else _save.activeDrone = cosmeticId;
            Changed?.Invoke();
            return true;
        }
    }
}
