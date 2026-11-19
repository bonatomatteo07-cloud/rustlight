using System.Collections.Generic;
using System.Linq;
using Rustlight.Core.Settings;
using Rustlight.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rustlight.Runtime.UI
{
    /// <summary>
    /// Options screen: resolution, window mode, V-Sync, graphics quality and volumes. Every change
    /// applies immediately and is written to settings.json when the panel closes.
    /// </summary>
    public sealed class SettingsView : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown resolution;
        [SerializeField] private TMP_Dropdown windowMode;
        [SerializeField] private Toggle vSync;
        [SerializeField] private TMP_Dropdown quality;
        [SerializeField] private Slider masterVolume;
        [SerializeField] private Slider musicVolume;
        [SerializeField] private Slider sfxVolume;
        [SerializeField] private Button resetButton;

        private static readonly (WindowMode mode, string label)[] Modes =
        {
            (WindowMode.Borderless, "Schermo intero (senza bordi)"),
            (WindowMode.ExclusiveFullscreen, "Schermo intero (esclusivo)"),
            (WindowMode.Windowed, "Finestra"),
        };

        private List<Vector2Int> _resolutions = new List<Vector2Int>();
        private bool _populating;

        private SettingsService Service => GameServices.Instance.Settings;

        private void OnEnable()
        {
            Populate();
            resolution.onValueChanged.AddListener(OnResolution);
            windowMode.onValueChanged.AddListener(OnWindowMode);
            vSync.onValueChanged.AddListener(OnVSync);
            quality.onValueChanged.AddListener(OnQuality);
            masterVolume.onValueChanged.AddListener(v => SetVolume(ref Service.Current.masterVolume, v));
            musicVolume.onValueChanged.AddListener(v => SetVolume(ref Service.Current.musicVolume, v));
            sfxVolume.onValueChanged.AddListener(v => SetVolume(ref Service.Current.sfxVolume, v));
            resetButton.onClick.AddListener(OnReset);
        }

        private void OnDisable()
        {
            resolution.onValueChanged.RemoveAllListeners();
            windowMode.onValueChanged.RemoveAllListeners();
            vSync.onValueChanged.RemoveAllListeners();
            quality.onValueChanged.RemoveAllListeners();
            masterVolume.onValueChanged.RemoveAllListeners();
            musicVolume.onValueChanged.RemoveAllListeners();
            sfxVolume.onValueChanged.RemoveAllListeners();
            resetButton.onClick.RemoveAllListeners();
            if (GameServices.Instance != null) Service.Save();
        }

        private void Populate()
        {
            _populating = true;
            var s = Service.Current;

            _resolutions = Screen.resolutions
                .Select(r => new Vector2Int(r.width, r.height))
                .Distinct()
                .OrderByDescending(r => r.x * r.y)
                .ToList();
            resolution.ClearOptions();
            resolution.AddOptions(new List<string> { "Nativa" }.Concat(_resolutions.Select(r => $"{r.x} × {r.y}")).ToList());
            int resIndex = _resolutions.FindIndex(r => r.x == s.width && r.y == s.height);
            resolution.SetValueWithoutNotify(resIndex < 0 ? 0 : resIndex + 1);

            windowMode.ClearOptions();
            windowMode.AddOptions(Modes.Select(m => m.label).ToList());
            windowMode.SetValueWithoutNotify(Mathf.Max(0, System.Array.FindIndex(Modes, m => m.mode == s.windowMode)));

            vSync.SetIsOnWithoutNotify(s.vSync);

            quality.ClearOptions();
            quality.AddOptions(QualitySettings.names.ToList());
            quality.SetValueWithoutNotify(s.qualityLevel >= 0 ? s.qualityLevel : QualitySettings.GetQualityLevel());

            masterVolume.SetValueWithoutNotify(s.masterVolume);
            musicVolume.SetValueWithoutNotify(s.musicVolume);
            sfxVolume.SetValueWithoutNotify(s.sfxVolume);
            _populating = false;
        }

        private void OnResolution(int index)
        {
            var s = Service.Current;
            if (index == 0) { s.width = 0; s.height = 0; }
            else { s.width = _resolutions[index - 1].x; s.height = _resolutions[index - 1].y; }
            Service.Apply();
        }

        private void OnWindowMode(int index) { Service.Current.windowMode = Modes[index].mode; Service.Apply(); }
        private void OnVSync(bool on) { Service.Current.vSync = on; Service.Apply(); }
        private void OnQuality(int index) { Service.Current.qualityLevel = index; Service.Apply(); }

        private void SetVolume(ref float field, float value)
        {
            if (_populating) return;
            field = value;
            Service.Apply();
        }

        private void OnReset()
        {
            Service.ResetToDefaults();
            Service.Apply();
            Populate();
        }
    }
}
