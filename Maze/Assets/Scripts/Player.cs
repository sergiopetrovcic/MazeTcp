using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public MazeGenerator maze;
    public float visionRadius = 10f;

    public float rotationDuration = 0.6f; // tempo total da rotação (em segundos)
    private bool isRotating = false;

    public Cell currentCell;
    public float moveSpeed = 3f;
    private bool isMoving = false;
    private Vector3 targetPosition;

    void Start()
    {
        if (maze == null)
            maze = FindFirstObjectByType<MazeGenerator>();

        // Define célula inicial
        currentCell = maze.GetCell(maze.playerStart);
        transform.position = currentCell.transform.position + Vector3.up * 1f; // acima do chão
        targetPosition = transform.position;
    }

    void Update()
    {
        if (maze != null)
            maze.RevealFog(transform.position, visionRadius);

        //// Movimento suave
        //if (isMoving)
        //{
        //    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        //    if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        //    {
        //        isMoving = false;
        //    }
        //    return;
        //}

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
        //RotateSmooth(-90f);
        transform.Rotate(0f, -90f, 0f, Space.World);
    }

    public void RotateCW()
    {
        //RotateSmooth(90f);
        transform.Rotate(0f, 90f, 0f, Space.World);
    }

    private void Restart()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        currentCell = maze.GetCell(maze.playerStart);
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

    void MoveToNextCell()
    {
        //Cell next = maze.GetNeighborGridAligned(currentCell);
        Cell next = maze.GetNeighbor(currentCell, transform.forward);
        //Cell next = maze.GetNeighborSingleStep(currentCell, transform.forward);
        if (next != null)
        {
            currentCell = next;
            //targetPosition = new Vector3(next.transform.position.x, transform.position.y, next.transform.position.z);
            //isMoving = true;
            transform.position = new Vector3(next.transform.position.x, transform.position.y, next.transform.position.z);
        }
        else
        {
            Debug.Log("Parede bloqueando o caminho ou fora dos limites.");
        }
    }
}
