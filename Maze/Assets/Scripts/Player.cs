using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public MazeGenerator maze;
    public float visionRadius = 10f;
    public Cell currentCell;
    private Vector3 targetPosition;

    // Statistics
    public int leftRotations = 0, rightRotations = 0, steps = 0, leftRotationsThisEpoch = 0, rightRotationsThisEpoch = 0, stepsThisEpoch = 0;

    // Eventos
    public delegate void PlayerEvent(params object[] args);
    public event PlayerEvent OnStep;
    public event PlayerEvent OnRotateLeft;
    public event PlayerEvent OnRotateRight;
    public event PlayerEvent OnRotate;
    public event PlayerEvent OnMove;

    private void Awake()
    {
        maze.OnFinish += Maze_OnFinish;
    }

    private void Maze_OnFinish(params object[] args)
    {
        Debug.Log(Time.time.ToString("F3") + " - Época " + args[3] + " encerrada em " + args[0] + " segundos com " + args[2] + " células visitadas! Total de células visitadas é " + args[1] + ".");
        Restart();
    }

    private void OnDestroy()
    {
        maze.OnFinish -= Maze_OnFinish;
    }

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
    }

    #region Inputs
    public void RotateCCW()
    {
        transform.Rotate(0f, -90f, 0f, Space.World);
        leftRotations++;
        try { OnRotateLeft?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnRotateLeft error: " + e); }
        try { OnRotate?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnRotate error: " + e); }
        try { OnMove?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnMove error: " + e); }
    }

    public void RotateCW()
    {
        transform.Rotate(0f, 90f, 0f, Space.World);
        rightRotations++;
        try { OnRotateRight?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnRotateRight error: " + e); }
        try { OnRotate?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnRotate error: " + e); }
        try { OnMove?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnMove error: " + e); }
    }

    public void Restart()
    {
        leftRotationsThisEpoch = 0; rightRotationsThisEpoch = 0; stepsThisEpoch = 0;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        currentCell = maze.GetCell(maze.playerStart);
        maze.NewEpoch();
    }

    public void MoveToNextCell()
    {
        Cell next = maze.GetNeighbor(currentCell, transform.forward);
        if (next != null)
        {
            currentCell = next;
            steps++;
            try { OnStep?.Invoke(); }
            catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > MoveToNextCell() > OnStep error: " + e); }
            try { OnMove?.Invoke(); }
            catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnMove error: " + e); }
            // Manobra necessária para garantir a ordem dos eventos:
            StartCoroutine(MoveAndTriggerSequence(next, new Vector3(next.transform.position.x, transform.position.y, next.transform.position.z)));
        }
        else
            Debug.Log("Parede bloqueando o caminho ou fora dos limites.");
    }
    #endregion

    private IEnumerator MoveAndTriggerSequence(Cell next, Vector3 novaPosicao)
    {
        transform.position = novaPosicao;

        // Isso força a Unity a processar o OnTriggerEnter (e2) AGORA.
        yield return new WaitForFixedUpdate();

        if (next.Coordinates == maze.targetCell)
            maze.Finish();
    }
}
