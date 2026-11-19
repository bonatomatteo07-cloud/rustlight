using Rustlight.Runtime.Player;
using Rustlight.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rustlight.Runtime.UI
{
    /// <summary>Health bar, level and both currencies. Event-driven: updates only when something changes.</summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private Image healthFill;   // Image Type = Filled
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text scrapText;
        [SerializeField] private TMP_Text coresText;

        private PlayerController _player;

        private void Start()
        {
            _player = PlayerController.Instance;
            var state = GameServices.Instance.State;
            state.Wallet.Changed += RefreshWallet;
            state.LeveledUp += OnLeveledUp;
            if (_player != null) _player.Health.Changed += RefreshHealth;
            RefreshWallet();
            RefreshHealth();
            OnLeveledUp(state.Save.progress.level);
        }

        private void OnDestroy()
        {
            if (GameServices.Instance != null)
            {
                GameServices.Instance.State.Wallet.Changed -= RefreshWallet;
                GameServices.Instance.State.LeveledUp -= OnLeveledUp;
            }
            if (_player != null) _player.Health.Changed -= RefreshHealth;
        }

        private void RefreshWallet()
        {
            var wallet = GameServices.Instance.State.Wallet;
            scrapText.text = wallet.Scrap.ToString("N0");
            coresText.text = wallet.Cores.ToString("N0");
        }

        private void RefreshHealth()
        {
            if (_player == null) return;
            var h = _player.Health;
            healthFill.fillAmount = (float)h.Current / h.Max;
            healthText.text = $"{h.Current}/{h.Max}";
        }

        private void OnLeveledUp(int level) => levelText.text = $"Lv {level}";
    }
}
