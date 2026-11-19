using UnityEngine.InputSystem;

namespace Rustlight.Runtime.Input
{
    /// <summary>
    /// The game's single set of input actions, built in code so every script and the rebinding menu
    /// share the same bindings. Bindings are grouped by scheme ("Keyboard&amp;Mouse", "Gamepad");
    /// player overrides are stored as JSON in the settings file.
    /// </summary>
    public static class GameInput
    {
        public const string KeyboardMouse = "Keyboard&Mouse";
        public const string Gamepad = "Gamepad";

        private static InputActionAsset _asset;

        public static InputActionAsset Asset => _asset ??= Build();
        public static InputActionMap Gameplay => Asset.FindActionMap("Gameplay", throwIfNotFound: true);
        public static InputAction Move => Gameplay.FindAction("Move", true);
        public static InputAction Attack => Gameplay.FindAction("Attack", true);
        public static InputAction Interact => Gameplay.FindAction("Interact", true);
        public static InputAction Pause => Gameplay.FindAction("Pause", true);
        public static InputAction Point => Gameplay.FindAction("Point", true);

        public static string SaveOverrides() => Asset.SaveBindingOverridesAsJson();

        public static void LoadOverrides(string json)
        {
            Asset.RemoveAllBindingOverrides();
            if (!string.IsNullOrEmpty(json)) Asset.LoadBindingOverridesFromJson(json);
        }

        private static InputActionAsset Build()
        {
            var asset = UnityEngine.ScriptableObject.CreateInstance<InputActionAsset>();
            asset.AddControlScheme(KeyboardMouse).WithRequiredDevice("<Keyboard>").WithOptionalDevice("<Mouse>");
            asset.AddControlScheme(Gamepad).WithRequiredDevice("<Gamepad>");

            var map = asset.AddActionMap("Gameplay");

            var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w", KeyboardMouse)
                .With("Down", "<Keyboard>/s", KeyboardMouse)
                .With("Left", "<Keyboard>/a", KeyboardMouse)
                .With("Right", "<Keyboard>/d", KeyboardMouse);
            move.AddBinding("<Gamepad>/leftStick", groups: Gamepad);

            var attack = map.AddAction("Attack", InputActionType.Button);
            attack.AddBinding("<Mouse>/leftButton", groups: KeyboardMouse);
            attack.AddBinding("<Keyboard>/space", groups: KeyboardMouse);
            attack.AddBinding("<Gamepad>/buttonSouth", groups: Gamepad);

            var interact = map.AddAction("Interact", InputActionType.Button);
            interact.AddBinding("<Keyboard>/e", groups: KeyboardMouse);
            interact.AddBinding("<Gamepad>/buttonWest", groups: Gamepad);

            var pause = map.AddAction("Pause", InputActionType.Button);
            pause.AddBinding("<Keyboard>/escape", groups: KeyboardMouse);
            pause.AddBinding("<Gamepad>/start", groups: Gamepad);

            var point = map.AddAction("Point", InputActionType.PassThrough, expectedControlLayout: "Vector2");
            point.AddBinding("<Pointer>/position", groups: KeyboardMouse);

            map.Enable();
            return asset;
        }
    }
}
