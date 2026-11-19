using System;
using System.Collections.Generic;
using System.Linq;
using Rustlight.Core.Monetization;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Rustlight.Runtime.Services
{
    /// <summary>
    /// Unity IAP 5 store wrapper. Flow: PurchaseProduct → OnPurchasePending → grant via
    /// <see cref="PurchaseGrantService"/> → save → ConfirmPurchase. If the save fails the order is
    /// left unconfirmed and the store re-delivers it on the next launch, so no purchase is lost.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        public bool IsReady { get; private set; }
        public event Action ProductsLoaded;
        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;   // productId, reason
        public event Action<string> PurchaseDeferred;         // Ask to Buy: waiting for a parent
        public event Action<bool, string> RestoreFinished;    // success, error

        private StoreController _store;

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _store = UnityIAPServices.StoreController();

            // Subscribe to everything BEFORE Connect: unfinished orders from a previous session fire immediately.
            _store.OnStoreConnected += OnStoreConnected;
            _store.OnStoreDisconnected += f => Debug.LogWarning($"[IAP] Store disconnected: {f.Message}");
            _store.OnProductsFetched += OnProductsFetched;
            _store.OnProductsFetchFailed += f => Debug.LogError($"[IAP] Product fetch failed: {f.FailureReason}");
            _store.OnPurchasesFetched += OnPurchasesFetched;
            _store.OnPurchasesFetchFailed += f => Debug.LogWarning($"[IAP] Purchase fetch failed: {f.Message}");
            _store.OnPurchasePending += OnPurchasePending;
            _store.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _store.OnPurchaseFailed += OnPurchaseFailed;
            _store.OnPurchaseDeferred += OnPurchaseDeferred;

            try
            {
                await _store.Connect();
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAP] Connect failed: {e}");
            }
        }

        /// <summary>Localized price ("4,99 €") for display, or null while products are not loaded.</summary>
        public string GetPrice(string productId) =>
            _store?.GetProductById(productId)?.metadata.localizedPriceString;

        public bool IsAvailable(string productId) =>
            IsReady && _store.GetProductById(productId)?.availableToPurchase == true;

        /// <summary>Starts the App Store purchase sheet for a product from <see cref="StoreCatalog"/>.</summary>
        public void Buy(string productId)
        {
            if (!IsAvailable(productId))
            {
                PurchaseFailed?.Invoke(productId, "Store not ready");
                return;
            }
            _store.PurchaseProduct(productId);
        }

        /// <summary>Re-delivers non-consumables (required by App Review: needs a visible Restore button).</summary>
        public void RestorePurchases()
        {
            if (_store == null) return;
            _store.RestoreTransactions((ok, error) =>
            {
                if (!ok) Debug.LogWarning($"[IAP] Restore failed: {error}");
                RestoreFinished?.Invoke(ok, error);
            });
        }

        private void OnStoreConnected()
        {
            var definitions = StoreCatalog.All
                .Select(p => new ProductDefinition(p.ProductId,
                    p.Kind == StoreProductKind.Consumable ? ProductType.Consumable : ProductType.NonConsumable))
                .ToList();
            _store.FetchProducts(definitions);
            _store.FetchPurchases();
        }

        private void OnProductsFetched(List<Product> products)
        {
            IsReady = true;
            ProductsLoaded?.Invoke();
        }

        /// <summary>Re-applies owned non-consumables missing from the save (e.g. after a reinstall).</summary>
        private void OnPurchasesFetched(Orders orders)
        {
            var state = GameServices.Instance.State;
            bool changed = false;
            foreach (var order in orders.ConfirmedOrders)
            {
                string id = ProductIdOf(order);
                var product = StoreCatalog.Find(id);
                if (product == null || product.Kind != StoreProductKind.NonConsumable) continue;
                if (state.Purchases.Grant(id, order.Info.TransactionID) == GrantResult.Granted) changed = true;
            }
            if (changed) GameServices.Instance.SaveNow();
        }

        private void OnPurchasePending(PendingOrder order)
        {
            string id = ProductIdOf(order);
            var result = GameServices.Instance.State.Purchases.Grant(id, order.Info.TransactionID);

            if (result == GrantResult.UnknownProduct)
            {
                Debug.LogError($"[IAP] Pending order for unknown product '{id}' left unconfirmed.");
                return;
            }
            if (result == GrantResult.Granted && !GameServices.Instance.SaveNow())
            {
                // Not confirmed on purpose: the store re-delivers it next launch and the grant is retried.
                PurchaseFailed?.Invoke(id, "Save failed, purchase will be retried");
                return;
            }

            _store.ConfirmPurchase(order);
            if (result == GrantResult.Granted) PurchaseSucceeded?.Invoke(id);
        }

        private void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failed)
                Debug.LogError($"[IAP] Confirmation failed: {failed.FailureReason} - {failed.Details}");
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            PurchaseFailed?.Invoke(ProductIdOf(order), $"{order.FailureReason}: {order.Details}");
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            PurchaseDeferred?.Invoke(ProductIdOf(order));
        }

        private static string ProductIdOf(Order order) =>
            order.CartOrdered.Items().FirstOrDefault()?.Product?.definition.id;
    }
}
