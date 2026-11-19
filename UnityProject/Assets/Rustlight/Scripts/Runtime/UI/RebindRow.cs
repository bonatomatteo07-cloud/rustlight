using Rustlight.Runtime.Input;
using Rustlight.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Rustlight.Runtime.UI
{
    /// <summary>
    /// One line of the controls menu: shows the current key for an action and rebinds it on click
    /// (Esc cancels). Use one row per action and scheme, plus one per direction for "Move".
    /// </summary>
    public sealed class RebindRow : MonoBehaviour
    {
        [SerializeField] private string actionName = "Attack";
        [Tooltip("For composite actions such as Move: Up, Down, Left or Right. Leave empty otherwise.")]
        [SerializeField] private string compositePart = "";
        [SerializeField] private string group = GameInput.KeyboardMouse;
        [SerializeField] private TMP_Text bindingText;
        [SerializeField] private Button rebindButton;

        private InputActionRebindingExtensions.RebindingOperation _operation;

        private InputAction Action => GameInput.Gameplay.FindAction(actionName, throwIfNotFound: true);

        private void OnEnable()
        {
            rebindButton.onClick.AddListener(StartRebind);
            Refresh();
        }

        private void OnDisable()
        {
            rebindButton.onClick.RemoveListener(StartRebind);
            _operation?.Cancel();
        }

        private int BindingIndex()
        {
            var bindings = Action.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                var b = bindings[i];
                bool inGroup = b.groups != null && b.groups.Contains(group);
                if (string.IsNullOrEmpty(compositePart))
                {
                    if (!b.isComposite && !b.isPartOfComposite && inGroup) return i;
                }
                else if (b.isPartOfComposite && inGroup &&
                         string.Equals(b.name, compositePart, System.StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        private void Refresh()
        {
            int index = BindingIndex();
            bindingText.text = index < 0 ? "—" : Action.GetBindingDisplayString(index);
        }

        private void StartRebind()
        {
            int index = BindingIndex();
            if (index < 0) return;

            var action = Action;
            action.Disable();
            bindingText.text = "Premi un tasto…";
            _operation = action.PerformInteractiveRebinding(index)
                .WithControlsExcluding("<Pointer>/position")
                .WithControlsExcluding("<Pointer>/delta")
                .WithCancelingThrough("<Keyboard>/escape")
                .WithBindingGroup(group)
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(_ => Finish(action, save: true))
                .OnCancel(_ => Finish(action, save: false))
                .Start();
        }

        private void Finish(InputAction action, bool save)
        {
            _operation?.Dispose();
            _operation = null;
            action.Enable();
            Refresh();
            if (save) GameServices.Instance.Settings.Save();
        }
    }
}
