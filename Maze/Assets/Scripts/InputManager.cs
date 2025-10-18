using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private CameraManager cameraManager;
    [SerializeField]
    private MazeGenerator mazeGenerator;

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
    // Maze inputs
    private bool IsKeyDown_Ctrl_F() => Keyboard.current != null && Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.F].wasPressedThisFrame;
    private bool IsKeyDown_Ctrl_R() => Keyboard.current != null && Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.R].wasPressedThisFrame;
#else
    // Camera inputs
    private bool IsKeyDown_NumpadPlus() => Input.GetKeyDown(KeyCode.W);
    private bool IsKeyDown_NumpadMinus() => Input.GetKeyDown(KeyCode.A);
    private bool IsKeyDown_NumpadPeriod() => Input.GetKeyDown(KeyCode.D);
    private bool IsKeyDown_Ctrl_F() => Input.GetKey(KeyCode.LeftCtrl) && Input.GetKeyDown(KeyCode.F);
    // Player inputs
    private bool IsKeyDown_W() => Input.GetKeyDown(KeyCode.W);
    private bool IsKeyDown_A() => Input.GetKeyDown(KeyCode.A);
    private bool IsKeyDown_D() => Input.GetKeyDown(KeyCode.D);
#endif

    private void HandleKeyboardInput()
    {
        if (cameraManager != null)
        {
            if (IsKeyDown_NumpadPlus()) cameraManager.Next();
            if (IsKeyDown_NumpadMinus()) cameraManager.Previous();
            if (IsKeyDown_NumpadPeriod()) cameraManager.ShowAll();
        }

        if (mazeGenerator != null)
        {
            if (IsKeyDown_Ctrl_F()) mazeGenerator.ToggleFog();
            if (IsKeyDown_Ctrl_R()) mazeGenerator.GenerateMaze(mazeGenerator.seed, mazeGenerator.shuffling);
        }

        if (player != null)
        {
            if (IsKeyDown_W()) player.MoveToNextCell();
            if (IsKeyDown_A()) player.RotateCCW();
            if (IsKeyDown_D()) player.RotateCW();
            if (IsKeyDown_R()) player.Restart();
        }
    }

    private void Start()
    {
        if (player == null)
            Debug.LogWarning("Player not initialized!");
        if (cameraManager == null)
            Debug.LogWarning("CameraManager not initialized!");
        if (mazeGenerator == null)
            Debug.LogWarning("MazeGenerator not initialized!");
    }

    void Update()
    {
        HandleKeyboardInput();
    }
}
