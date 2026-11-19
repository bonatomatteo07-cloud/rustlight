using System;
using System.Collections.Generic;

namespace Rustlight.Core.Dlc
{
    /// <summary>A paid DLC on Steam and what owning it unlocks. Nothing here affects stats.</summary>
    public sealed class DlcDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        /// <summary>The DLC's own App ID from Steamworks (0 until it is created there).</summary>
        public uint SteamAppId { get; }
        public IReadOnlyList<string> Cosmetics { get; }
        public int InventorySlots { get; }
        public IReadOnlyList<string> Zones { get; }

        public DlcDefinition(string id, string displayName, uint steamAppId,
            IReadOnlyList<string> cosmetics = null, int inventorySlots = 0, IReadOnlyList<string> zones = null)
        {
            Id = id;
            DisplayName = displayName;
            SteamAppId = steamAppId;
            Cosmetics = cosmetics ?? new string[0];
            InventorySlots = inventorySlots;
            Zones = zones ?? new string[0];
        }
    }

    /// <summary>
    /// The one list of DLC. Replace the App IDs with the ones Steamworks assigns
    /// (App Admin → All Associated Packages, DLC, Demos and Tools → Add new DLC).
    /// </summary>
    public static class DlcCatalog
    {
        public static readonly DlcDefinition SupporterPack = new DlcDefinition(
            "supporter_pack", "Pacchetto Sostenitore", steamAppId: 0,
            cosmetics: new[] { "suit_vanguard", "drone_halo" }, inventorySlots: 10);

        public static readonly DlcDefinition SunkenCity = new DlcDefinition(
            "sunken_city", "Capitolo II: La Città Sommersa", steamAppId: 0,
            cosmetics: new[] { "suit_diver" }, zones: new[] { "zone_sunken_city" });

        public static readonly IReadOnlyList<DlcDefinition> All = new[] { SupporterPack, SunkenCity };
    }

    /// <summary>Answers "does this player own this DLC?" (Steam in the game, a fake in tests and the Editor).</summary>
    public interface IDlcOwnership
    {
        bool Owns(DlcDefinition dlc);
    }

    /// <summary>
    /// Derives everything DLC unlocks from current ownership, never from the save, so a refund
    /// or a family-shared library is reflected on the next <see cref="Refresh"/>.
    /// </summary>
    public sealed class Entitlements
    {
        private readonly IDlcOwnership _ownership;
        private readonly IReadOnlyList<DlcDefinition> _catalog;
        private readonly HashSet<string> _ownedDlc = new HashSet<string>();
        private readonly HashSet<string> _cosmetics = new HashSet<string>();
        private readonly HashSet<string> _zones = new HashSet<string>();

        public int ExtraInventorySlots { get; private set; }
        public event Action Changed;

        public Entitlements(IDlcOwnership ownership, IReadOnlyList<DlcDefinition> catalog = null)
        {
            _ownership = ownership;
            _catalog = catalog ?? DlcCatalog.All;
            Refresh();
        }

        public bool OwnsDlc(string dlcId) => _ownedDlc.Contains(dlcId);
        public bool GrantsCosmetic(string cosmeticId) => _cosmetics.Contains(cosmeticId);

        /// <summary>Zones that no DLC lists are part of the base game and always unlocked.</summary>
        public bool IsZoneUnlocked(string zoneId)
        {
            foreach (var dlc in _catalog)
                foreach (var z in dlc.Zones)
                    if (z == zoneId) return _zones.Contains(zoneId);
            return true;
        }

        /// <summary>Re-reads ownership for every DLC; raises <see cref="Changed"/> if anything differs.</summary>
        public void Refresh()
        {
            var before = new HashSet<string>(_ownedDlc);
            _ownedDlc.Clear(); _cosmetics.Clear(); _zones.Clear();
            ExtraInventorySlots = 0;
            foreach (var dlc in _catalog)
            {
                if (!_ownership.Owns(dlc)) continue;
                _ownedDlc.Add(dlc.Id);
                foreach (var c in dlc.Cosmetics) _cosmetics.Add(c);
                foreach (var z in dlc.Zones) _zones.Add(z);
                ExtraInventorySlots += dlc.InventorySlots;
            }
            if (!before.SetEquals(_ownedDlc)) Changed?.Invoke();
        }
    }
}
