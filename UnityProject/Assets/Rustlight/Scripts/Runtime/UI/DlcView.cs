using System.Collections.Generic;
using Rustlight.Core.Dlc;
using Rustlight.Runtime.Platform;
using Rustlight.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rustlight.Runtime.UI
{
    /// <summary>
    /// Lists the DLC with "Posseduto" or a button that opens its Steam store page in the overlay.
    /// Purchases happen entirely in Steam; the list refreshes when Steam reports a new DLC.
    /// </summary>
    public sealed class DlcView : MonoBehaviour
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private DlcRowView rowPrefab;

        private readonly List<DlcRowView> _rows = new List<DlcRowView>();

        private void OnEnable()
        {
            GameServices.Instance.State.Entitlements.Changed += Rebuild;
            Rebuild();
        }

        private void OnDisable()
        {
            if (GameServices.Instance != null) GameServices.Instance.State.Entitlements.Changed -= Rebuild;
        }

        private void Rebuild()
        {
            var entitlements = GameServices.Instance.State.Entitlements;
            for (int i = 0; i < DlcCatalog.All.Count; i++)
            {
                if (i >= _rows.Count) _rows.Add(Instantiate(rowPrefab, listRoot));
                var dlc = DlcCatalog.All[i];
                _rows[i].Bind(dlc.DisplayName, entitlements.OwnsDlc(dlc.Id), () => OpenStore(dlc));
            }
        }

        private static void OpenStore(DlcDefinition dlc)
        {
            if (SteamPlatform.OpenStorePage(dlc)) return;
            if (dlc.SteamAppId != 0) Application.OpenURL($"https://store.steampowered.com/app/{dlc.SteamAppId}/");
        }
    }
}
