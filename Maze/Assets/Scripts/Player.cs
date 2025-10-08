using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public MazeGenerator maze;
    public float visionRadius = 5f;

    void Update()
    {
        if (maze != null)
            maze.RevealFog(transform.position, visionRadius);

        HandleKeyboardInput();
    }

    #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private bool IsKeyDown_W() => Keyboard.current != null && Keyboard.current[Key.W].isPressed;
#else
        private bool IsKeyDown_W() => Input.GetKeyDown(KeyCode.W);
#endif

    private void HandleKeyboardInput()
    {
        // Move para frente
        if (IsKeyDown_W()) MoveForward();
    }

    public void MoveForward()
    {
        Vector3 direction = Vector3.zero;
        direction += transform.forward; // move na direção que o player está olhando

        // Normaliza e aplica movimento
        if (direction != Vector3.zero)
        {
            direction.Normalize();
            transform.position += direction * 1 * Time.deltaTime;
        }
    }
}
