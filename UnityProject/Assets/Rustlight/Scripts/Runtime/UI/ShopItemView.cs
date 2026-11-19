using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rustlight.Runtime.UI
{
    /// <summary>One row of the shop: title, price and a buy button. Lives on a prefab.</summary>
    public sealed class ShopItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;

        public void Bind(string title, string price, bool interactable, Action onBuy)
        {
            titleText.text = title;
            priceText.text = price;
            buyButton.interactable = interactable;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy());
        }

        public void SetInteractable(bool value) => buyButton.interactable = value;
    }
}
