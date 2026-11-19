using System;
using System.Collections.Generic;
using Rustlight.Core.Save;

namespace Rustlight.Core.Economy
{
    public enum CosmeticKind { Suit, Drone }

    /// <summary>A purely visual unlock. Cosmetics never change stats, so DLC is never pay-to-win.</summary>
    public sealed class CosmeticDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public CosmeticKind Kind { get; }
        /// <summary>Price in Cores; 0 means it can only be obtained from a DLC or a quest.</summary>
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
        private readonly Func<string, bool> _grantedExternally;

        public event Action Changed;
        public IEnumerable<CosmeticDefinition> Catalog => _catalog.Values;

        /// <param name="grantedExternally">Ownership that is not stored in the save, e.g. cosmetics from an owned DLC.</param>
        public CosmeticShop(IEnumerable<CosmeticDefinition> catalog, SaveData save, Wallet wallet,
            Func<string, bool> grantedExternally = null)
        {
            foreach (var c in catalog) _catalog[c.Id] = c;
            _save = save;
            _wallet = wallet;
            _grantedExternally = grantedExternally ?? (_ => false);
        }

        public bool Owns(string cosmeticId) =>
            _save.ownedCosmetics.Contains(cosmeticId) || _grantedExternally(cosmeticId);

        /// <summary>Falls back to the default looks if a worn cosmetic is no longer owned (e.g. DLC refunded).</summary>
        public void EnsureWornAreOwned(string defaultSuit, string defaultDrone)
        {
            bool changed = false;
            if (!Owns(_save.activeSuit)) { _save.activeSuit = defaultSuit; changed = true; }
            if (!Owns(_save.activeDrone)) { _save.activeDrone = defaultDrone; changed = true; }
            if (changed) Changed?.Invoke();
        }

        /// <summary>Re-raises <see cref="Changed"/> after outside ownership changed (a DLC was installed).</summary>
        public void NotifyOwnershipChanged() => Changed?.Invoke();

        /// <summary>Unlocks without charging (quest rewards). Returns false if already owned or unknown.</summary>
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
