using System.Collections.Generic;
using Rustlight.Core.Monetization;
using Rustlight.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rustlight.Runtime.UI
{
    /// <summary>
    /// Real-money shop. Prices always come from the App Store (localized), never hardcoded.
    /// Owned packs show as owned; the Restore button is mandatory for App Review.
    /// </summary>
    public sealed class ShopView : MonoBehaviour
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private ShopItemView itemPrefab;
        [SerializeField] private Button restoreButton;
        [SerializeField] private TMP_Text statusText;

        private static readonly Dictionary<string, string> Titles = new Dictionary<string, string>
        {
            { StoreCatalog.CoresSmall, "100 Nuclei" },
            { StoreCatalog.CoresMedium, "550 Nuclei" },
            { StoreCatalog.CoresLarge, "1200 Nuclei" },
            { StoreCatalog.StarterPack, "Pacchetto Iniziale" },
            { StoreCatalog.SupporterPack, "Pacchetto Sostenitore" },
        };

        private readonly Dictionary<string, ShopItemView> _rows = new Dictionary<string, ShopItemView>();
        private bool _busy;

        private void OnEnable()
        {
            var iap = IAPManager.Instance;
            if (iap == null) { statusText.text = "Negozio non disponibile."; return; }
            iap.ProductsLoaded += Rebuild;
            iap.PurchaseSucceeded += OnSucceeded;
            iap.PurchaseFailed += OnFailed;
            iap.PurchaseDeferred += OnDeferred;
            iap.RestoreFinished += OnRestoreFinished;
            restoreButton.onClick.AddListener(OnRestoreClicked);
            Rebuild();
        }

        private void OnDisable()
        {
            var iap = IAPManager.Instance;
            restoreButton.onClick.RemoveListener(OnRestoreClicked);
            if (iap == null) return;
            iap.ProductsLoaded -= Rebuild;
            iap.PurchaseSucceeded -= OnSucceeded;
            iap.PurchaseFailed -= OnFailed;
            iap.PurchaseDeferred -= OnDeferred;
            iap.RestoreFinished -= OnRestoreFinished;
        }

        private void Rebuild()
        {
            var iap = IAPManager.Instance;
            statusText.text = iap.IsReady ? "" : "Connessione all'App Store…";
            var purchases = GameServices.Instance.State.Purchases;

            foreach (var product in StoreCatalog.All)
            {
                if (!_rows.TryGetValue(product.ProductId, out var row))
                {
                    row = Instantiate(itemPrefab, listRoot);
                    _rows[product.ProductId] = row;
                }
                bool owned = product.Kind == StoreProductKind.NonConsumable && purchases.IsEntitled(product.ProductId);
                string price = owned ? "Acquistato" : iap.GetPrice(product.ProductId) ?? "—";
                bool canBuy = !owned && !_busy && iap.IsAvailable(product.ProductId);
                string id = product.ProductId;
                row.Bind(Titles.TryGetValue(id, out var t) ? t : id, price, canBuy, () => Buy(id));
            }
        }

        private void Buy(string productId)
        {
            _busy = true;
            statusText.text = "Acquisto in corso…";
            Rebuild();
            IAPManager.Instance.Buy(productId);
        }

        private void OnSucceeded(string productId) { _busy = false; statusText.text = "Acquisto completato!"; Rebuild(); }
        private void OnFailed(string productId, string reason) { _busy = false; statusText.text = "Acquisto non riuscito."; Debug.LogWarning(reason); Rebuild(); }
        private void OnDeferred(string productId) { _busy = false; statusText.text = "In attesa di approvazione."; Rebuild(); }

        private void OnRestoreClicked()
        {
            statusText.text = "Ripristino in corso…";
            IAPManager.Instance.RestorePurchases();
        }

        private void OnRestoreFinished(bool ok, string error)
        {
            statusText.text = ok ? "Acquisti ripristinati." : "Ripristino non riuscito.";
            Rebuild();
        }
    }
}
