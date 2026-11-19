using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rustlight.Runtime.UI
{
    /// <summary>One DLC row: name, and either an "owned" label or a store button.</summary>
    public sealed class DlcRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text ownedLabel;
        [SerializeField] private Button storeButton;

        public void Bind(string displayName, bool owned, Action openStore)
        {
            nameText.text = displayName;
            ownedLabel.gameObject.SetActive(owned);
            storeButton.gameObject.SetActive(!owned);
            storeButton.onClick.RemoveAllListeners();
            storeButton.onClick.AddListener(() => openStore());
        }
    }
}
