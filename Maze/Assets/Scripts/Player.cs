using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string name = "player01";
    public MazeGenerator maze;
    public Cell cell;
    public float visionRadius = 10f;
    public Cell currentCell;
    private Vector3 targetPosition;
    private List<PlayerEpoch> epochs = new List<PlayerEpoch>();

    [Header("Sensors")]
    public float sensorMaxDistance = 1.0f;     // 30 cm
    public LayerMask obstacleMask = ~0;         // layers considered obstacle
    public Transform sensorsRoot;               // optional root transform for sensors; if null, use robot transform
    private Vector3[] frontDirs;                // sensor directions precomputed
    //private Vector3[] backDirs;                 // sensor directions precomputed
    private Vector3[] sensorOffsets;            // sensor origins (positions relative to robot)
    private bool pendingSensorRead = true;

    [Header("Debug/State")]
    public bool debugDrawRays = true;

    // Statistics
    [HideInInspector] public int epochCurrent = 0;

    [HideInInspector] public int rotations = 0;
    [HideInInspector] public int leftRotations = 0;
    [HideInInspector] public int rightRotations = 0;
    [HideInInspector] public int steps = 0;
    [HideInInspector] public int moves = 0;
    [HideInInspector] public float timer = 0;
    [HideInInspector] public int cellsDiscovered = 0;
    [HideInInspector] public int cellsVisited = 0;

    [HideInInspector] public int rotationsCurrentEpoch = 0;
    [HideInInspector] public int leftRotationsCurrentEpoch = 0;
    [HideInInspector] public int rightRotationsCurrentEpoch = 0;
    [HideInInspector] public int stepsCurrentEpoch = 0;
    [HideInInspector] public int movesCurrentEpoch = 0;
    [HideInInspector] public float timerCurrentEpoch = 0;
    [HideInInspector] public int cellsDiscoveredCurrentEpoch = 0;
    [HideInInspector] public int cellsVisitedCurrentEpoch = 0;

    private bool isPlaying = false;
    private float startTimer;

    // Eventos
    public delegate void PlayerEvent(params object[] args);
    public event PlayerEvent OnStep;
    public event PlayerEvent OnRotateLeft;
    public event PlayerEvent OnRotateRight;
    public event PlayerEvent OnRotate;
    public event PlayerEvent OnMove;
    

    private void Awake()
    {
        maze.OnCellDiscovered += Maze_OnCellDiscovered;
        maze.OnCellVisited += Maze_OnCellVisited;
        maze.OnFinish += Maze_OnFinish;
        this.OnMove += Player_OnMove;

        // default sensors root
        if (sensorsRoot == null) sensorsRoot = transform;

        // prepare directions: front 0, +45, -45 ; back 180, 180+45, 180-45
        frontDirs = new Vector3[3];
        //backDirs = new Vector3[3];

        frontDirs[0] = Quaternion.Euler(0, 0, 0) * Vector3.forward;
        frontDirs[1] = Quaternion.Euler(0, 90f, 0) * Vector3.forward;
        frontDirs[2] = Quaternion.Euler(0, -90f, 0) * Vector3.forward;

        //backDirs[0] = Quaternion.Euler(0, 180f, 0) * Vector3.forward;
        //backDirs[1] = Quaternion.Euler(0, 180f + 45f, 0) * Vector3.forward;
        //backDirs[2] = Quaternion.Euler(0, 180f - 45f, 0) * Vector3.forward;

        // sensor offsets (you can tune these in inspector by editing code or add public transforms)
        sensorOffsets = new Vector3[3];
        float forwardOffset = 0.5f; // half meter in front/back (adjust as robot size)
        float heightOffset = 0.2f;  // sensor height above ground
        // front center, front +45, front -45, back center, back +45, back -45
        sensorOffsets[0] = new Vector3(0f, heightOffset, forwardOffset);
        sensorOffsets[1] = new Vector3(0f, heightOffset, forwardOffset);
        sensorOffsets[2] = new Vector3(0f, heightOffset, forwardOffset);
        //sensorOffsets[3] = new Vector3(0f, heightOffset, -forwardOffset);
        //sensorOffsets[4] = new Vector3(0f, heightOffset, -forwardOffset);
        //sensorOffsets[5] = new Vector3(0f, heightOffset, -forwardOffset);
    }

    private void Player_OnMove(params object[] args)
    {
        pendingSensorRead = true;
    }

    private void Maze_OnCellVisited(params object[] args)
    {
        cellsVisitedCurrentEpoch++;
        cellsVisited++;
    }

    private void Maze_OnCellDiscovered(params object[] args)
    {
        cellsDiscoveredCurrentEpoch++;
        cellsDiscovered++;
    }

    private void Maze_OnFinish(params object[] args)
    {
        isPlaying = false;
        Restart();
    }

    private void OnDestroy()
    {
        maze.OnFinish -= Maze_OnFinish;
        maze.OnCellDiscovered -= Maze_OnCellDiscovered;
        maze.OnCellVisited -= Maze_OnCellVisited;
    }

    void Start()
    {
        if (maze == null)
            maze = FindFirstObjectByType<MazeGenerator>();

        // Define célula inicial
        currentCell = maze.GetCell(maze.playerStart);
        transform.position = currentCell.transform.position + Vector3.up * 1f; // acima do chão
        targetPosition = transform.position;

        float[] dist = GetSensorDistances();
        Debug.Log("Distances: forward = " + dist[0] + " - right = " + dist[1] + " - left = " + dist[2]);
    }

    void Update()
    {
        if (isPlaying)
            timerCurrentEpoch += Time.time - startTimer;

        if (maze != null)
            maze.RevealFog(transform.position, visionRadius);

        if (pendingSensorRead)
        {
            ReadSensors();
            pendingSensorRead = false;
        }
    }

    private void ReadSensors()
    { 
        float[] dist = GetSensorDistances();
        Debug.Log("Distances: forward = " + dist[0] + " - right = " + dist[1] + " - left = " + dist[2]);
    }

    #region Inputs
    public void RotateCCW()
    {
        if (!isPlaying)
            StartTimer();

        transform.Rotate(0f, -90f, 0f, Space.World);
        leftRotationsCurrentEpoch++;
        rotationsCurrentEpoch++;
        movesCurrentEpoch++;
        leftRotations++;
        rotations++;
        moves++;

        try { OnRotateLeft?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnRotateLeft error: " + e); }
        try { OnRotate?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnRotate error: " + e); }
        try { OnMove?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnMove error: " + e); }
    }

    public void RotateCW()
    {
        if (!isPlaying)
            StartTimer();
        transform.Rotate(0f, 90f, 0f, Space.World);
        rightRotationsCurrentEpoch++;
        rotationsCurrentEpoch++;
        movesCurrentEpoch++;
        rightRotations++;
        rotations++;
        moves++;

        try { OnRotateRight?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnRotateRight error: " + e); }
        try { OnRotate?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnRotate error: " + e); }
        try { OnMove?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnMove error: " + e); }
    }

    public void Restart()
    {
        epochs.Add(new PlayerEpoch(
            name,
            epochCurrent,
            rotationsCurrentEpoch,
            leftRotationsCurrentEpoch,
            rightRotationsCurrentEpoch,
            stepsCurrentEpoch,
            movesCurrentEpoch,
            timerCurrentEpoch,
            cellsDiscoveredCurrentEpoch,
            cellsVisitedCurrentEpoch));
        epochCurrent++;
        rotationsCurrentEpoch = 0;
        leftRotationsCurrentEpoch = 0;
        rightRotationsCurrentEpoch = 0;
        stepsCurrentEpoch = 0;
        movesCurrentEpoch = 0;
        timer += timerCurrentEpoch;
        timerCurrentEpoch = 0;
        cellsDiscoveredCurrentEpoch = 0;
        cellsVisitedCurrentEpoch = 0;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        currentCell = maze.GetCell(maze.playerStart);
        maze.NewEpoch();
    }

    public void MoveToNextCell()
    {
        if (!isPlaying)
            StartTimer();

        Cell next = maze.GetNeighbor(currentCell, transform.forward);
        if (next != null)
        {
            currentCell = next;
            stepsCurrentEpoch++;
            movesCurrentEpoch++;
            steps++;
            moves++;

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

    private void StartTimer()
    {
        if (movesCurrentEpoch == 0)
            isPlaying = true;
    }

    private IEnumerator MoveAndTriggerSequence(Cell next, Vector3 novaPosicao)
    {
        transform.position = novaPosicao;

        // Isso força a Unity a processar o OnTriggerEnter (e2) AGORA.
        yield return new WaitForFixedUpdate();

        if (next.Coordinates == maze.targetCell)
            maze.Finish();
    }

    // --- Sensors and safety ---
    /// <summary>
    /// Casts the 6 rays and returns distances (meters). If nothing hit, returns sensorMaxDistance.
    /// Order: front center, front +45, front -45, back center, back +45, back -45
    /// </summary>
    public float[] GetSensorDistances()
    {
        float[] dists = new float[6];
        Vector3 rootPos = sensorsRoot.position;

        // front sensors
        for (int i = 0; i < 3; i++)
        {
            Vector3 origin = sensorsRoot.TransformPoint(sensorOffsets[i]);
            Vector3 dir = sensorsRoot.TransformDirection(frontDirs[i]);
            dists[i] = RayDistance(origin, dir);
            if (debugDrawRays) DebugDrawRay(origin, dir, dists[i], Color.red);
        }

        /*// back sensors (indices 3..5)
        for (int i = 0; i < 3; i++)
        {
            Vector3 origin = sensorsRoot.TransformPoint(sensorOffsets[3 + i]);
            Vector3 dir = sensorsRoot.TransformDirection(backDirs[i]);
            dists[3 + i] = RayDistance(origin, dir);
            if (debugDrawRays) DebugDrawRay(origin, dir, dists[3 + i], Color.blue);
        }*/

        return dists;
    }

    private float RayDistance(Vector3 origin, Vector3 dir)
    {
        if (Physics.Raycast(origin, dir, out RaycastHit hit, sensorMaxDistance, obstacleMask))
        {
            return hit.distance;
        }
        return 0;
    }


    private void DebugDrawRay(Vector3 origin, Vector3 dir, float dist, Color col)
    {
        Vector3 end = origin + dir.normalized * dist;
        Debug.DrawLine(origin, end, col);
    }

    // --- Debug: editor button, not required ---
#if UNITY_EDITOR
    private void OnValidate()
    {
        sensorMaxDistance = Mathf.Max(0.01f, sensorMaxDistance);
        //gpsErrorMeters = Mathf.Max(0f, gpsErrorMeters);
    }
#endif
}
