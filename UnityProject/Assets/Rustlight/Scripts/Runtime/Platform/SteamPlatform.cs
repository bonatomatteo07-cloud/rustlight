using System;
using System.Collections.Generic;
using Rustlight.Core.Dlc;
using UnityEngine;
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace Rustlight.Runtime.Platform
{
    /// <summary>
    /// Thin wrapper over Steamworks.NET: init and shutdown, DLC ownership, achievements and the overlay.
    /// Compiles to a no-op (IsReady = false) until Steamworks.NET is installed and the STEAMWORKS_NET
    /// define is set, so the project builds and runs without Steam during development.
    /// </summary>
    public static class SteamPlatform
    {
        /// <summary>480 is Valve's public test app (Spacewar). Replace it with your App ID.</summary>
        public const uint DevelopmentAppId = 480;

        public static bool IsReady { get; private set; }

        /// <summary>Raised when the player installs or buys a DLC while the game runs.</summary>
        public static event Action DlcChanged;

        /// <summary>Raised with true when the Steam overlay opens (pause the game) and false when it closes.</summary>
        public static event Action<bool> OverlayToggled;

#if STEAMWORKS_NET && !DISABLESTEAMWORKS
        private static Callback<DlcInstalled_t> _dlcInstalled;
        private static Callback<GameOverlayActivated_t> _overlay;
#endif

        /// <summary>
        /// Starts Steam. In a build launched outside Steam it relaunches through Steam and returns false,
        /// in which case the caller should quit.
        /// </summary>
        public static bool Initialize(uint appId)
        {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            if (IsReady) return true;
            try
            {
                if (!Application.isEditor && appId != DevelopmentAppId && SteamAPI.RestartAppIfNecessary(new AppId_t(appId)))
                    return false;
            }
            catch (DllNotFoundException e)
            {
                Debug.LogError($"[Steam] steam_api64 library missing: {e.Message}");
                return true; // keep running without Steam
            }

            IsReady = SteamAPI.Init();
            if (!IsReady)
            {
                Debug.LogWarning("[Steam] SteamAPI.Init failed: is the Steam client running and do you own the app?");
                return true;
            }
            _dlcInstalled = Callback<DlcInstalled_t>.Create(_ => DlcChanged?.Invoke());
            _overlay = Callback<GameOverlayActivated_t>.Create(e => OverlayToggled?.Invoke(e.m_bActive != 0));
#endif
            return true;
        }

        /// <summary>Pumps Steam callbacks; call once per frame.</summary>
        public static void Tick()
        {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            if (IsReady) SteamAPI.RunCallbacks();
#endif
        }

        public static void Shutdown()
        {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            if (!IsReady) return;
            SteamAPI.Shutdown();
            IsReady = false;
#endif
        }

        public static bool OwnsDlc(DlcDefinition dlc)
        {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            return IsReady && dlc.SteamAppId != 0 && SteamApps.BIsDlcInstalled(new AppId_t(dlc.SteamAppId));
#else
            return false;
#endif
        }

        /// <summary>Sets each achievement and uploads once. Safe to call with ones already unlocked on Steam.</summary>
        public static void UnlockAchievements(IEnumerable<string> apiNames)
        {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            if (!IsReady) return;
            bool any = false;
            foreach (var name in apiNames) any |= SteamUserStats.SetAchievement(name);
            if (any) SteamUserStats.StoreStats();
#endif
        }

        /// <summary>Opens the DLC's store page in the Steam overlay; returns false if the overlay is unavailable.</summary>
        public static bool OpenStorePage(DlcDefinition dlc)
        {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            if (!IsReady || dlc.SteamAppId == 0 || !SteamUtils.IsOverlayEnabled()) return false;
            SteamFriends.ActivateGameOverlayToStore(new AppId_t(dlc.SteamAppId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
            return true;
#else
            return false;
#endif
        }

        /// <summary>True on Steam Deck, where the UI should default to gamepad prompts and larger text.</summary>
        public static bool IsSteamDeck()
        {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            return IsReady && SteamUtils.IsSteamRunningOnSteamDeck();
#else
            return false;
#endif
        }
    }

    /// <summary>DLC ownership from Steam, plus a list of DLC ids to fake-own while developing in the Editor.</summary>
    public sealed class SteamDlcOwnership : IDlcOwnership
    {
        private readonly HashSet<string> _editorOwned;

        public SteamDlcOwnership(IEnumerable<string> editorOwned) => _editorOwned = new HashSet<string>(editorOwned ?? Array.Empty<string>());

        public bool Owns(DlcDefinition dlc) =>
            SteamPlatform.OwnsDlc(dlc) || (Application.isEditor && _editorOwned.Contains(dlc.Id));
    }
}
