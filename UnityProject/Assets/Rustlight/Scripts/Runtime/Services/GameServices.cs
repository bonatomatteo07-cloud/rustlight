using Rustlight.Core;
using UnityEngine;

namespace Rustlight.Runtime.Services
{
    /// <summary>
    /// Persistent root of the game: loads the save into a <see cref="GameState"/> before anything else
    /// runs and saves whenever the app goes to the background (iOS may kill it without warning after that).
    /// Put it on a GameObject in the first scene.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameServices : MonoBehaviour
    {
        public static GameServices Instance { get; private set; }

        public GameState State { get; private set; }
        private SaveService _saves;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _saves = new SaveService(Application.persistentDataPath);
            State = new GameState(_saves.LoadOrCreate());
        }

        /// <summary>Writes the current state to disk; returns false if the write failed.</summary>
        public bool SaveNow() => State != null && _saves.TrySave(State.Save);

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        private void OnApplicationQuit() => SaveNow();
    }
}
