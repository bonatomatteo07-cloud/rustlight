using System;

namespace Rustlight.Core.Settings
{
    /// <summary>Window modes, mirroring Unity's FullScreenMode values that make sense on PC.</summary>
    public enum WindowMode { ExclusiveFullscreen = 0, Borderless = 1, Windowed = 3 }

    /// <summary>
    /// Player options, stored apart from the save (settings.json) so a new game or a cloud-synced
    /// save never changes someone's resolution or key bindings. Width/height 0 means "native".
    /// </summary>
    [Serializable]
    public sealed class GameSettings
    {
        public int version = 1;
        public int width;
        public int height;
        public WindowMode windowMode = WindowMode.Borderless;
        public bool vSync = true;
        public int targetFrameRate = 0;     // 0 = unlimited (only used when vSync is off)
        public int qualityLevel = -1;       // -1 = keep the project default
        public float masterVolume = 0.8f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.9f;
        public string bindingOverridesJson = "";

        /// <summary>Repairs out-of-range values from hand-edited or older files; returns this.</summary>
        public GameSettings Sanitize()
        {
            masterVolume = Clamp01(masterVolume);
            musicVolume = Clamp01(musicVolume);
            sfxVolume = Clamp01(sfxVolume);
            if (width < 0 || height < 0 || (width == 0) != (height == 0)) { width = 0; height = 0; }
            if (!Enum.IsDefined(typeof(WindowMode), windowMode)) windowMode = WindowMode.Borderless;
            if (targetFrameRate < 0) targetFrameRate = 0;
            bindingOverridesJson ??= "";
            return this;
        }

        /// <summary>Converts a 0–1 slider to mixer decibels: 0 is silent (-80 dB), 1 is 0 dB.</summary>
        public static float ToDecibels(float linear) =>
            linear <= 0.0001f ? -80f : (float)(20.0 * Math.Log10(linear));

        private static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : (float.IsNaN(v) ? 1f : v);
    }
}
