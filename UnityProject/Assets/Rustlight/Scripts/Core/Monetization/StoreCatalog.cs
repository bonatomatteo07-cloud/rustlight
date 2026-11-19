using System.Collections.Generic;

namespace Rustlight.Core.Monetization
{
    /// <summary>Mirrors Unity IAP's product types without depending on the IAP package.</summary>
    public enum StoreProductKind { Consumable, NonConsumable }

    /// <summary>A real-money product and what it grants.</summary>
    public sealed class StoreProduct
    {
        public string ProductId { get; }
        public StoreProductKind Kind { get; }
        public int Cores { get; }
        public IReadOnlyList<string> Cosmetics { get; }
        public int InventorySlots { get; }

        public StoreProduct(string productId, StoreProductKind kind, int cores = 0,
            IReadOnlyList<string> cosmetics = null, int inventorySlots = 0)
        {
            ProductId = productId;
            Kind = kind;
            Cores = cores;
            Cosmetics = cosmetics ?? new string[0];
            InventorySlots = inventorySlots;
        }
    }

    /// <summary>
    /// The one authoritative list of real-money products. The ids must match the products
    /// created in App Store Connect exactly. Change <see cref="BundlePrefix"/> to your bundle id.
    /// </summary>
    public static class StoreCatalog
    {
        public const string BundlePrefix = "com.yourstudio.rustlight";

        public const string CoresSmall = BundlePrefix + ".cores_small";
        public const string CoresMedium = BundlePrefix + ".cores_medium";
        public const string CoresLarge = BundlePrefix + ".cores_large";
        public const string StarterPack = BundlePrefix + ".starter_pack";
        public const string SupporterPack = BundlePrefix + ".supporter_pack";

        public static readonly IReadOnlyList<StoreProduct> All = new[]
        {
            new StoreProduct(CoresSmall, StoreProductKind.Consumable, cores: 100),
            new StoreProduct(CoresMedium, StoreProductKind.Consumable, cores: 550),
            new StoreProduct(CoresLarge, StoreProductKind.Consumable, cores: 1200),
            new StoreProduct(StarterPack, StoreProductKind.NonConsumable, cores: 300,
                cosmetics: new[] { "suit_scavenger" }, inventorySlots: 10),
            new StoreProduct(SupporterPack, StoreProductKind.NonConsumable,
                cosmetics: new[] { "suit_vanguard", "drone_halo" }, inventorySlots: 20),
        };

        public static StoreProduct Find(string productId)
        {
            foreach (var p in All) if (p.ProductId == productId) return p;
            return null;
        }
    }
}
