using System;
using System.IO;
using Rustlight.Core.Settings;
using Rustlight.Runtime.Input;
using UnityEngine;
using UnityEngine.Audio;

namespace Rustlight.Runtime.Services
{
    /// <summary>
    /// Loads, applies and saves <see cref="GameSettings"/> (settings.json, kept out of Steam Cloud so
    /// resolution and key bindings stay per machine). The AudioMixer must expose the parameters
    /// "MasterVolume", "MusicVolume" and "SfxVolume".
    /// </summary>
    public sealed class SettingsService
    {
        private readonly string _path;
        private readonly AudioMixer _mixer;

        public GameSettings Current { get; private set; }

        public SettingsService(string directory, AudioMixer mixer)
        {
            _path = Path.Combine(directory, "settings.json");
            _mixer = mixer;
            Current = Load();
        }

        /// <summary>Applies every setting to the engine. Call after changing <see cref="Current"/>.</summary>
        public void Apply()
        {
            var s = Current.Sanitize();

            int w = s.width > 0 ? s.width : Display.main.systemWidth;
            int h = s.height > 0 ? s.height : Display.main.systemHeight;
            Screen.SetResolution(w, h, (FullScreenMode)s.windowMode);

            QualitySettings.vSyncCount = s.vSync ? 1 : 0;
            Application.targetFrameRate = s.vSync || s.targetFrameRate == 0 ? -1 : s.targetFrameRate;
            if (s.qualityLevel >= 0 && s.qualityLevel < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(s.qualityLevel, applyExpensiveChanges: true);

            if (_mixer != null)
            {
                _mixer.SetFloat("MasterVolume", GameSettings.ToDecibels(s.masterVolume));
                _mixer.SetFloat("MusicVolume", GameSettings.ToDecibels(s.musicVolume));
                _mixer.SetFloat("SfxVolume", GameSettings.ToDecibels(s.sfxVolume));
            }
            else
            {
                AudioListener.volume = s.masterVolume;
            }

            GameInput.LoadOverrides(s.bindingOverridesJson);
        }

        /// <summary>Stores the current bindings into the settings and writes the file.</summary>
        public void Save()
        {
            Current.bindingOverridesJson = GameInput.SaveOverrides();
            try
            {
                string tmp = _path + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(Current, prettyPrint: true));
                if (File.Exists(_path)) File.Delete(_path);
                File.Move(tmp, _path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Settings] Write failed: {e.Message}");
            }
        }

        public void ResetToDefaults()
        {
            Current = new GameSettings();
            GameInput.LoadOverrides("");
        }

        private GameSettings Load()
        {
            try
            {
                if (File.Exists(_path)) return JsonUtility.FromJson<GameSettings>(File.ReadAllText(_path)).Sanitize();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Settings] Could not read settings, using defaults: {e.Message}");
            }
            return new GameSettings();
        }
    }
}
