using System;
using Rustlight.Core;
using Rustlight.Runtime.Platform;
using UnityEngine;
using UnityEngine.Audio;

namespace Rustlight.Runtime.Services
{
    /// <summary>
    /// Persistent root of the game. Starts Steam, loads settings and the save into a <see cref="GameState"/>
    /// before anything else runs, pushes achievements, and saves on quit. Put it in the first scene.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameServices : MonoBehaviour
    {
        public static GameServices Instance { get; private set; }

        [Tooltip("Your Steam App ID. 480 (Spacewar) works for testing with any Steam account.")]
        [SerializeField] private uint steamAppId = SteamPlatform.DevelopmentAppId;
        [SerializeField] private AudioMixer audioMixer;
        [Tooltip("Editor only: DLC ids to treat as owned, e.g. supporter_pack.")]
        [SerializeField] private string[] editorOwnedDlc = Array.Empty<string>();
        [SerializeField] private float autosaveSeconds = 120f;

        public GameState State { get; private set; }
        public SettingsService Settings { get; private set; }

        /// <summary>Raised when the Steam overlay or the pause menu wants the game paused.</summary>
        public event Action<bool> PauseRequested;

        private SaveService _saves;
        private float _nextAchievementCheck;
        private float _nextAutosave;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!SteamPlatform.Initialize(steamAppId))
            {
                Application.Quit(); // relaunching through Steam
                return;
            }
            SteamPlatform.DlcChanged += OnDlcChanged;
            SteamPlatform.OverlayToggled += OnOverlayToggled;

            Settings = new SettingsService(Application.persistentDataPath, audioMixer);
            Settings.Apply();

            _saves = new SaveService(Application.persistentDataPath);
            State = new GameState(_saves.LoadOrCreate(), new SteamDlcOwnership(editorOwnedDlc));

            // Achievements earned offline (or before Steam was set up) are pushed now.
            SteamPlatform.UnlockAchievements(State.Save.unlockedAchievements);
            _nextAutosave = Time.unscaledTime + autosaveSeconds;
        }

        private void Update()
        {
            SteamPlatform.Tick();
            if (State == null) return;

            if (Time.unscaledTime >= _nextAchievementCheck)
            {
                _nextAchievementCheck = Time.unscaledTime + 1f;
                var unlocked = State.Achievements.Evaluate();
                if (unlocked.Count > 0) SteamPlatform.UnlockAchievements(unlocked);
            }
            if (Time.unscaledTime >= _nextAutosave)
            {
                _nextAutosave = Time.unscaledTime + autosaveSeconds;
                SaveNow();
            }
        }

        /// <summary>Writes the current state to disk; returns false if the write failed.</summary>
        public bool SaveNow() => State != null && _saves.TrySave(State.Save);

        public void RequestPause(bool paused) => PauseRequested?.Invoke(paused);

        private void OnDlcChanged() => State?.Entitlements.Refresh();
        private void OnOverlayToggled(bool open) { if (open) PauseRequested?.Invoke(true); }

        private void OnApplicationQuit()
        {
            SaveNow();
            Settings?.Save();
            SteamPlatform.Shutdown();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            SteamPlatform.DlcChanged -= OnDlcChanged;
            SteamPlatform.OverlayToggled -= OnOverlayToggled;
        }
    }
}
