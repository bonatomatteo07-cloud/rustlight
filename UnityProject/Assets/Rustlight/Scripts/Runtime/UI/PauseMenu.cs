using Rustlight.Runtime.Input;
using Rustlight.Runtime.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Rustlight.Runtime.UI
{
    /// <summary>
    /// Esc / Start toggles the pause panel and freezes time. The Steam overlay opening also pauses.
    /// Gameplay input is disabled while paused so clicks on the menu never swing the weapon.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        public bool IsPaused { get; private set; }

        private void OnEnable()
        {
            GameInput.Pause.performed += OnPausePressed;
            GameServices.Instance.PauseRequested += SetPaused;
            resumeButton.onClick.AddListener(() => SetPaused(false));
            quitButton.onClick.AddListener(Quit);
            SetPaused(false);
        }

        private void OnDisable()
        {
            GameInput.Pause.performed -= OnPausePressed;
            if (GameServices.Instance != null) GameServices.Instance.PauseRequested -= SetPaused;
            resumeButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }

        private void OnPausePressed(InputAction.CallbackContext _) => SetPaused(!IsPaused);

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            panel.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
            foreach (var action in GameInput.Gameplay)
            {
                if (action == GameInput.Pause || action == GameInput.Point) continue;
                if (paused) action.Disable(); else action.Enable();
            }
            if (paused) resumeButton.Select();
        }

        private void Quit()
        {
            GameServices.Instance.SaveNow();
            Application.Quit();
        }
    }
}
