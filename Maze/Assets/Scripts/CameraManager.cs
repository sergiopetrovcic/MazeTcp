using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public List<Camera> cameras = new List<Camera>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DisableAllCameras();
        cameras[0].enabled = true;
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private bool IsKeyDown_W() => Keyboard.current != null && Keyboard.current[Key.W].wasPressedThisFrame;
        private bool IsKeyDown_A() => Keyboard.current != null && Keyboard.current[Key.A].wasPressedThisFrame;
        private bool IsKeyDown_D() => Keyboard.current != null && Keyboard.current[Key.D].wasPressedThisFrame;
        private bool IsKeyDown_R() => Keyboard.current != null && Keyboard.current[Key.R].wasPressedThisFrame;
    #else
        private bool IsKeyDown_W() => Input.GetKeyDown(KeyCode.W);
        private bool IsKeyDown_A() => Input.GetKeyDown(KeyCode.A);
        private bool IsKeyDown_D() => Input.GetKeyDown(KeyCode.D);
    #endif

    private void HandleKeyboardInput()
    {
        // Move para frente
        if (IsKeyDown_W()) MoveToNextCell();//MoveForward();
        // Girar para a esquerda
        if (IsKeyDown_A()) RotateCCW();
        // Girar para a direita
        if (IsKeyDown_D()) RotateCW();
        // Move player para início
        if (IsKeyDown_R()) Restart();
    }

    private void DisableAllCameras()
    {
        foreach (Camera c in cameras)
            c.enabled = false;
    }

    public void Enable(Camera cam)
    {
        foreach (Camera c in cameras)
        {
            if (cam == c)
                c.enabled = true;
            else
                c.enabled = false;
        }
    }

    public void Enable(int c)
    {
        DisableAllCameras();
        cameras[c].enabled = true;
    }
}
