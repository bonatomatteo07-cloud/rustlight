using Rustlight.Core.Economy;
using Rustlight.Core.Items;
using Rustlight.Core.Save;

namespace Rustlight.Core.Monetization
{
    public enum GrantResult
    {
        /// <summary>Rewards applied: save, then confirm the purchase with the store.</summary>
        Granted,
        /// <summary>Already granted earlier (re-delivery or restore): confirm without granting again.</summary>
        AlreadyGranted,
        /// <summary>Not in <see cref="StoreCatalog"/>: do not confirm, investigate the product setup.</summary>
        UnknownProduct,
    }

    /// <summary>
    /// Applies the rewards of a store purchase exactly once. Consumables are de-duplicated by
    /// transaction id, non-consumables by product id (so a restore never pays out twice).
    /// The caller must persist the save before confirming the order with the store.
    /// </summary>
    public sealed class PurchaseGrantService
    {
        /// <summary>How many transaction ids to remember; re-deliveries only ever involve recent ones.</summary>
        public const int LedgerSize = 200;

        private readonly SaveData _save;
        private readonly Wallet _wallet;
        private readonly Inventory _inventory;
        private readonly CosmeticShop _cosmetics;

        public PurchaseGrantService(SaveData save, Wallet wallet, Inventory inventory, CosmeticShop cosmetics)
        {
            _save = save;
            _wallet = wallet;
            _inventory = inventory;
            _cosmetics = cosmetics;
        }

        public bool IsEntitled(string productId) => _save.entitlements.Contains(productId);

        public GrantResult Grant(string productId, string transactionId)
        {
            var product = StoreCatalog.Find(productId);
            if (product == null) return GrantResult.UnknownProduct;

            bool hasTx = !string.IsNullOrEmpty(transactionId);
            if (hasTx && _save.processedTransactions.Contains(transactionId)) return GrantResult.AlreadyGranted;

            if (product.Kind == StoreProductKind.NonConsumable && IsEntitled(productId))
            {
                Remember(transactionId);
                return GrantResult.AlreadyGranted;
            }

            if (product.Cores > 0) _wallet.AddCores(product.Cores);
            foreach (var cosmetic in product.Cosmetics) _cosmetics.Grant(cosmetic);
            if (product.InventorySlots > 0)
            {
                _inventory.ExpandCapacity(product.InventorySlots);
                _save.inventoryCapacity = _inventory.Capacity;
            }
            if (product.Kind == StoreProductKind.NonConsumable) _save.entitlements.Add(productId);
            Remember(transactionId);
            return GrantResult.Granted;
        }

        private void Remember(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId) || _save.processedTransactions.Contains(transactionId)) return;
            _save.processedTransactions.Add(transactionId);
            int overflow = _save.processedTransactions.Count - LedgerSize;
            if (overflow > 0) _save.processedTransactions.RemoveRange(0, overflow);
        }
    }
}
