using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public MazeGenerator maze;
    public float visionRadius = 5f;

    public float rotationDuration = 0.6f; // tempo total da rotação (em segundos)
    private bool isRotating = false;
    void Update()
    {
        //if (maze != null)
        //    maze.RevealFog(transform.position, visionRadius);

        HandleKeyboardInput();
    }

    #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private bool IsKeyDown_W() => Keyboard.current != null && Keyboard.current[Key.W].wasPressedThisFrame;
        private bool IsKeyDown_A() => Keyboard.current != null && Keyboard.current[Key.A].wasPressedThisFrame;
        private bool IsKeyDown_D() => Keyboard.current != null && Keyboard.current[Key.D].wasPressedThisFrame;
#else
        private bool IsKeyDown_W() => Input.GetKeyDown(KeyCode.W);
        private bool IsKeyDown_A() => Input.GetKeyDown(KeyCode.A);
        private bool IsKeyDown_D() => Input.GetKeyDown(KeyCode.D);
#endif

    private void HandleKeyboardInput()
    {
        // Move para frente
        if (IsKeyDown_W()) MoveForward();
        // Girar para a esquerda
        if (IsKeyDown_A()) RotateCCW();
        // Girar para a direita
        if (IsKeyDown_D()) RotateCW();
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

    public void RotateCCW()
    {
        RotateSmooth(-90f);
        //transform.Rotate(0f, -90f, 0f, Space.World);
    }

    public void RotateCW()
    {
        RotateSmooth(90f);
        //transform.Rotate(0f, 90f, 0f, Space.World);
    }

    public void RotateSmooth(float angle)
    {
        if (!isRotating)
            StartCoroutine(RotateSmoothCoroutine(angle));
    }

    private IEnumerator RotateSmoothCoroutine(float angle)
    {
        isRotating = true;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = transform.rotation * Quaternion.Euler(0f, angle, 0f);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            // SmoothStep cria aceleração e desaceleração natural
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.rotation = endRotation;
        isRotating = false;
    }
}
