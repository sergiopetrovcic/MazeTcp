using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private CameraManager cameraManager;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // Camera inputs
        private bool IsKeyDown_NumpadPlus() => Keyboard.current != null && Keyboard.current[Key.NumpadPlus].wasPressedThisFrame;
        private bool IsKeyDown_NumpadMinus() => Keyboard.current != null && Keyboard.current[Key.NumpadMinus].wasPressedThisFrame;
        private bool IsKeyDown_NumpadPeriod() => Keyboard.current != null && Keyboard.current[Key.NumpadEnter].wasPressedThisFrame;
        // Player inputs
        private bool IsKeyDown_W() => Keyboard.current != null && Keyboard.current[Key.W].wasPressedThisFrame;
        private bool IsKeyDown_A() => Keyboard.current != null && Keyboard.current[Key.A].wasPressedThisFrame;
        private bool IsKeyDown_D() => Keyboard.current != null && Keyboard.current[Key.D].wasPressedThisFrame;
        private bool IsKeyDown_R() => Keyboard.current != null && Keyboard.current[Key.R].wasPressedThisFrame;
#else
        // Camera inputs
        private bool IsKeyDown_NumpadPlus() => Input.GetKeyDown(KeyCode.W);
        private bool IsKeyDown_NumpadMinus() => Input.GetKeyDown(KeyCode.A);
        private bool IsKeyDown_NumpadPeriod() => Input.GetKeyDown(KeyCode.D);
        // Player inputs
        private bool IsKeyDown_W() => Input.GetKeyDown(KeyCode.W);
        private bool IsKeyDown_A() => Input.GetKeyDown(KeyCode.A);
        private bool IsKeyDown_D() => Input.GetKeyDown(KeyCode.D);
#endif

    private void HandleKeyboardInput()
    {
        if (IsKeyDown_NumpadPlus()) cameraManager.Next();
        if (IsKeyDown_NumpadMinus()) cameraManager.Previous();
        if (IsKeyDown_NumpadPeriod()) cameraManager.ShowAll();
        if (IsKeyDown_W()) player.MoveToNextCell();
        if (IsKeyDown_A()) player.RotateCCW();
        if (IsKeyDown_D()) player.RotateCW();
        if (IsKeyDown_R()) player.Restart();
    }

    private void Start()
    {
        if (player == null)
            Debug.LogError("Player not initialized!");
        if (cameraManager == null)
            Debug.LogError("CameraManager not initialized!");
    }

    void Update()
    {
        HandleKeyboardInput();
    }
}
