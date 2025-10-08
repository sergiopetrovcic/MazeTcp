using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Maze generator com compatibilidade para o novo Input System,
/// salvar/carregar, restart, ajustar X/Z/T pelo teclado
/// e exportação para OBJ.
/// </summary>
public class MazeGenerator_ANTIGO_FUNCIONANDO : MonoBehaviour
{
    [Header("Maze Settings (metros)")]
    [Tooltip("Largura em células (cada célula = 1m)")]
    [Range(5, 200)] public int sizeX = 10;
    [Tooltip("Comprimento em células (cada célula = 1m)")]
    [Range(5, 200)] public int sizeZ = 10;

    [Tooltip("Espessura da parede em metros (T). Ajustes por 0.01 = 1cm)")]
    [Range(0.04f, 0.50f)] public float wallThickness = 0.10f; // 0.04..0.5 m == 4..50 cm

    public float cellSize = 1f; // 1 metro por célula
    public Material wallMaterial;
    [Range(1f, 5f), Tooltip("Altura das paredes em metros")]
    public float wallHeight = 1f;

    private List<GameObject> fogList = new List<GameObject>();
    [SerializeField] private Material fogMaterial;
    [SerializeField, Range(1f, 10f)] private float fogAlpha = 0.8f; // transparência

    private MazeCell[,] grid;
    private Transform mazeRoot;
    private string saveFolder;

    //[Header("Player Settings")]
    //public GameObject playerPrefab;
    //private GameObject currentPlayer;

    private struct MazeCell
    {
        public bool visited;
        public bool northWall;
        public bool southWall;
        public bool eastWall;
        public bool westWall;
    }

    private void Awake()
    {
        saveFolder = Path.Combine(Application.persistentDataPath, "Mazes");
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
    }

    private void Start()
    {
        GenerateMaze();
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    #region Input wrappers (compatível com InputSystem e Legacy Input)
    private void HandleKeyboardInput()
    {
        // Gera novo labirinto aleatório
        if (IsKeyDown_N()) NewMaze();

        // Gerar (regenerar com os parâmetros atuais)
        if (IsKeyDown_G()) GenerateMaze();

        // Salvar
        if (IsKeyDown_S()) SaveMaze();

        // Carregar (lastMaze.json)
        if (IsKeyDown_L()) LoadMaze();

        // Reiniciar (recarrega o arquivo salvo se existir, senão gera)
        if (IsKeyDown_R()) RestartMaze();

        // Exportar OBJ
        if (IsKeyDown_O()) ExportToOBJ();

        // Instancia player
        if (IsKeyDown_P()) SpawnPlayer();

        // Ajustes de tamanho
        if (IsKeyDown_1()) { sizeX = Mathf.Max(5, sizeX - 1); Debug.Log($"sizeX = {sizeX}"); }
        if (IsKeyDown_2()) { sizeX = Mathf.Min(200, sizeX + 1); Debug.Log($"sizeX = {sizeX}"); }
        if (IsKeyDown_3()) { sizeZ = Mathf.Max(5, sizeZ - 1); Debug.Log($"sizeZ = {sizeZ}"); }
        if (IsKeyDown_4()) { sizeZ = Mathf.Min(200, sizeZ + 1); Debug.Log($"sizeZ = {sizeZ}"); }

        // T altera em ±0.01 (1 cm)
        if (IsKeyDown_5()) { wallThickness = Mathf.Max(0.04f, wallThickness - 0.01f); Debug.Log($"T = {wallThickness:F2} m"); }
        if (IsKeyDown_6()) { wallThickness = Mathf.Min(0.50f, wallThickness + 0.01f); Debug.Log($"T = {wallThickness:F2} m"); }

        if (IsKeyDown_7()) { wallHeight = Mathf.Max(1f, wallHeight - 0.1f); Debug.Log($"Altura = {wallHeight:F1} m"); }
        if (IsKeyDown_8()) { wallHeight = Mathf.Min(5f, wallHeight + 0.1f); Debug.Log($"Altura = {wallHeight:F1} m"); }
    }

    // Helpers: cada um mapeia a tecla para o Input System se presente, ou para o Input clássico.
    #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private bool IsKeyDown_N() => Keyboard.current != null && Keyboard.current[Key.N].wasPressedThisFrame;
        private bool IsKeyDown_G() => Keyboard.current != null && Keyboard.current[Key.G].wasPressedThisFrame;
        private bool IsKeyDown_S() => Keyboard.current != null && Keyboard.current[Key.S].wasPressedThisFrame && Keyboard.current[Key.LeftCtrl].wasPressedThisFrame;
        private bool IsKeyDown_L() => Keyboard.current != null && Keyboard.current[Key.L].wasPressedThisFrame;
        private bool IsKeyDown_R() => Keyboard.current != null && Keyboard.current[Key.R].wasPressedThisFrame;
        private bool IsKeyDown_O() => Keyboard.current != null && Keyboard.current[Key.O].wasPressedThisFrame;
        private bool IsKeyDown_P() => Keyboard.current != null && Keyboard.current[Key.P].wasPressedThisFrame;
        private bool IsKeyDown_1() => Keyboard.current != null && Keyboard.current[Key.Digit1].wasPressedThisFrame;
        private bool IsKeyDown_2() => Keyboard.current != null && Keyboard.current[Key.Digit2].wasPressedThisFrame;
        private bool IsKeyDown_3() => Keyboard.current != null && Keyboard.current[Key.Digit3].wasPressedThisFrame;
        private bool IsKeyDown_4() => Keyboard.current != null && Keyboard.current[Key.Digit4].wasPressedThisFrame;
        private bool IsKeyDown_5() => Keyboard.current != null && Keyboard.current[Key.Digit5].wasPressedThisFrame;
        private bool IsKeyDown_6() => Keyboard.current != null && Keyboard.current[Key.Digit6].wasPressedThisFrame;
        private bool IsKeyDown_7() => Keyboard.current != null && Keyboard.current[Key.Digit7].wasPressedThisFrame;
        private bool IsKeyDown_8() => Keyboard.current != null && Keyboard.current[Key.Digit8].wasPressedThisFrame;
#else
        private bool IsKeyDown_N() => Input.GetKeyDown(KeyCode.N);
        private bool IsKeyDown_G() => Input.GetKeyDown(KeyCode.G);
        private bool IsKeyDown_S() => Input.GetKeyDown(KeyCode.S);
        private bool IsKeyDown_L() => Input.GetKeyDown(KeyCode.L);
        private bool IsKeyDown_R() => Input.GetKeyDown(KeyCode.R);
        private bool IsKeyDown_O() => Input.GetKeyDown(KeyCode.O);
        private bool IsKeyDown_P() => Input.GetKeyDown(KeyCode.P);
        private bool IsKeyDown_W() => Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.W);
        private bool IsKeyDown_1() => Input.GetKeyDown(KeyCode.Alpha1);
        private bool IsKeyDown_2() => Input.GetKeyDown(KeyCode.Alpha2);
        private bool IsKeyDown_3() => Input.GetKeyDown(KeyCode.Alpha3);
        private bool IsKeyDown_4() => Input.GetKeyDown(KeyCode.Alpha4);
        private bool IsKeyDown_5() => Input.GetKeyDown(KeyCode.Alpha5);
        private bool IsKeyDown_6() => Input.GetKeyDown(KeyCode.Alpha6);
        private bool IsKeyDown_7() => Input.GetKeyDown(KeyCode.Alpha7);
        private bool IsKeyDown_8() => Input.GetKeyDown(KeyCode.Alpha8);
#endif
    #endregion

    #region Maze generation (Depth-first backtracker)
    private void NewMaze()
    {
        ClearMaze();
        GenerateMaze();
    }

    private void GenerateMaze()
    {
        ClearMaze();
        grid = new MazeCell[sizeX, sizeZ];

        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                grid[x, z].northWall = true;
                grid[x, z].southWall = true;
                grid[x, z].eastWall = true;
                grid[x, z].westWall = true;
                grid[x, z].visited = false;
            }
        }

        StartCoroutine(GenerateMazeRoutine());


    }

    private IEnumerator GenerateMazeRoutine()
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(Random.Range(0, sizeX), Random.Range(0, sizeZ));
        grid[current.x, current.y].visited = true;

        while (true)
        {
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);
            if (neighbors.Count > 0)
            {
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWall(current, chosen);
                stack.Push(current);
                current = chosen;
                grid[current.x, current.y].visited = true;
            }
            else if (stack.Count > 0)
            {
                current = stack.Pop();
            }
            else
            {
                break;
            }

            // permite que Unity não trave se o labirinto for grande
            yield return null;
        }

        BuildMazeGeometry();
        CreateGuaranteedExit();
        PositionCameraAbove();
        //SpawnPlayer();
    }

    private void PositionCameraAbove()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MazeCamera");
            cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
        }

        cam.orthographic = true;
        float maxSize = Mathf.Max(sizeX, sizeZ);
        cam.orthographicSize = maxSize * 0.55f;
        cam.transform.position = new Vector3(0f, maxSize, 0f);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 500f;
    }

    private void CreateGuaranteedExit()
    {
        // Tenta encontrar uma célula na borda que seja acessível (visitada)
        List<(Vector2Int pos, string dir)> borderCells = new List<(Vector2Int, string)>();

        for (int x = 0; x < sizeX; x++)
        {
            if (grid[x, 0].visited) borderCells.Add((new Vector2Int(x, 0), "south"));
            if (grid[x, sizeZ - 1].visited) borderCells.Add((new Vector2Int(x, sizeZ - 1), "north"));
        }
        for (int z = 0; z < sizeZ; z++)
        {
            if (grid[0, z].visited) borderCells.Add((new Vector2Int(0, z), "west"));
            if (grid[sizeX - 1, z].visited) borderCells.Add((new Vector2Int(sizeX - 1, z), "east"));
        }

        if (borderCells.Count == 0) return;

        var chosen = borderCells[Random.Range(0, borderCells.Count)];

        switch (chosen.dir)
        {
            case "north": grid[chosen.pos.x, chosen.pos.y].northWall = false; break;
            case "south": grid[chosen.pos.x, chosen.pos.y].southWall = false; break;
            case "east": grid[chosen.pos.x, chosen.pos.y].eastWall = false; break;
            case "west": grid[chosen.pos.x, chosen.pos.y].westWall = false; break;
        }

        // Reconstrói o labirinto com a abertura aplicada
        ClearMaze();
        BuildMazeGeometry();

        Debug.Log($"Saída criada em {chosen.pos} na borda {chosen.dir.ToUpper()}.");
    }

    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> list = new List<Vector2Int>();

        if (cell.x > 0 && !grid[cell.x - 1, cell.y].visited)
            list.Add(new Vector2Int(cell.x - 1, cell.y));
        if (cell.x < sizeX - 1 && !grid[cell.x + 1, cell.y].visited)
            list.Add(new Vector2Int(cell.x + 1, cell.y));
        if (cell.y > 0 && !grid[cell.x, cell.y - 1].visited)
            list.Add(new Vector2Int(cell.x, cell.y - 1));
        if (cell.y < sizeZ - 1 && !grid[cell.x, cell.y + 1].visited)
            list.Add(new Vector2Int(cell.x, cell.y + 1));

        return list;
    }

    private void RemoveWall(Vector2Int a, Vector2Int b)
    {
        if (a.x == b.x)
        {
            if (a.y < b.y)
            {
                grid[a.x, a.y].northWall = false;
                grid[b.x, b.y].southWall = false;
            }
            else
            {
                grid[a.x, a.y].southWall = false;
                grid[b.x, b.y].northWall = false;
            }
        }
        else if (a.y == b.y)
        {
            if (a.x < b.x)
            {
                grid[a.x, a.y].eastWall = false;
                grid[b.x, b.y].westWall = false;
            }
            else
            {
                grid[a.x, a.y].westWall = false;
                grid[b.x, b.y].eastWall = false;
            }
        }
    }
    #endregion

    #region Geometry building
    private void BuildMazeGeometry()
    {
        ClearMaze();

        mazeRoot = new GameObject("MazeRoot").transform;
        mazeRoot.SetParent(transform, false);
        mazeRoot.gameObject.isStatic = true;

        // Corrredor width = 1m - T (equivalente a cellSize - wallThickness)
        float corridorWidth = cellSize - wallThickness;
        float halfSizeX = sizeX * cellSize * 0.5f - cellSize * 0.5f;
        float halfSizeZ = sizeZ * cellSize * 0.5f - cellSize * 0.5f;

        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                // As paredes começam em Y=0, topo em Y=wallHeight, alinhadas ao terreno z=0.
                // centro da célula
                Vector3 basePos = new Vector3(x * cellSize - halfSizeX, wallHeight * 0.5f, z * cellSize - halfSizeZ);

                // North wall (ao lado positivo Z) => orientada ao longo de X
                if (grid[x, z].northWall)
                {
                    Vector3 pos = basePos + new Vector3(0f, 0f, (corridorWidth * 0.5f));
                    Vector3 scale = new Vector3(cellSize - 0f, wallHeight, wallThickness); // comprimento X = cellSize
                    CreateWall(pos, scale, Quaternion.identity);
                }

                // South wall (lado -Z)
                if (grid[x, z].southWall)
                {
                    Vector3 pos = basePos + new Vector3(0f, 0f, -(corridorWidth * 0.5f));
                    Vector3 scale = new Vector3(cellSize - 0f, wallHeight, wallThickness);
                    CreateWall(pos, scale, Quaternion.identity);
                }

                // East wall (lado +X) => orientada ao longo de Z
                if (grid[x, z].eastWall)
                {
                    Vector3 pos = basePos + new Vector3((corridorWidth * 0.5f), 0f, 0f);
                    Vector3 scale = new Vector3(wallThickness, wallHeight, cellSize - 0f);
                    CreateWall(pos, scale, Quaternion.identity);
                }

                // West wall (lado -X)
                if (grid[x, z].westWall)
                {
                    Vector3 pos = basePos + new Vector3(-(corridorWidth * 0.5f), 0f, 0f);
                    Vector3 scale = new Vector3(wallThickness, wallHeight, cellSize - 0f);
                    CreateWall(pos, scale, Quaternion.identity);
                }

                // === Criação do fog sobre cada célula ===
                Vector3 fogPos = new Vector3(
                    x * cellSize - halfSizeX,
                    wallHeight + 0.01f,
                    z * cellSize - halfSizeZ
                );
                GameObject fog = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fog.transform.SetParent(mazeRoot.transform, true);
                fog.transform.localScale = new Vector3(cellSize, cellSize, 1);
                fog.transform.position = fogPos;
                fog.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // deitado sobre o labirinto

                if (fogMaterial != null)
                {
                    var matInstance = new Material(fogMaterial);
                    Color c = matInstance.color;
                    c.a = fogAlpha;
                    matInstance.color = c;
                    fog.GetComponent<Renderer>().material = matInstance;
                }
                else
                {
                    fog.GetComponent<Renderer>().material.color = new Color(0, 0, 0, fogAlpha);
                }

                fogList.Add(fog);
            }
        }
    }

    public void RevealFog(Vector3 playerPosition, float visionRadius)
    {
        Vector2 playerXZ = new Vector2(playerPosition.x, playerPosition.z);

        foreach (var fog in fogList)
        {
            if (fog == null) continue;

            Vector2 fogXZ = new Vector2(fog.transform.position.x, fog.transform.position.z);
            float dist = Vector2.Distance(playerXZ, fogXZ);

            if (dist < visionRadius)
                fog.SetActive(false);
        }
    }


    private void CreateWall(Vector3 localPos, Vector3 localScale, Quaternion rot)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(mazeRoot, false);
        wall.transform.localPosition = localPos;
        wall.transform.localRotation = rot;
        wall.transform.localScale = localScale;

        if (wallMaterial != null)
            wall.GetComponent<Renderer>().material = wallMaterial;

        // BoxCollider já criado por CreatePrimitive, manter
        // Se quiser, ajuste aqui propriedades do collider (isTrigger etc.)
    }

    private void ClearMaze()
    {
        if (mazeRoot != null)
            DestroyImmediate(mazeRoot.gameObject);
    }
    #endregion

    #region Save / Load / Restart
    [System.Serializable]
    private class MazeData
    {
        public int sizeX, sizeZ;
        public float wallThickness;
        public List<string> walls;
    }

    private string currentFile => Path.Combine(saveFolder, "lastMaze.json");

    private void SpawnPlayer()
    {
        //if (playerPrefab == null)
        //{
        //    Debug.LogWarning("Nenhum playerPrefab definido no MazeGenerator.");
        //    return;
        //}

        //// Remove player anterior, se houver
        //if (currentPlayer != null)
        //{
        //    Destroy(currentPlayer);
        //}

        // Escolhe uma célula aleatória válida dentro do labirinto
        int x = Random.Range(0, sizeX);
        int z = Random.Range(0, sizeZ);

        Vector3 spawnPos = new Vector3(
            (x * cellSize - (sizeX * cellSize * 0.5f)) + (cellSize * 0.5f),
            wallHeight, // ligeiramente acima do chão
            (z * cellSize - (sizeZ * cellSize * 0.5f)) + (cellSize * 0.5f)
        );

        //currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        //Debug.Log($"Player instanciado em ({x}, {z})");
    }


    private void SaveMaze()
    {
        if (grid == null)
        {
            Debug.LogWarning("Nada para salvar (grid é null). Gere o labirinto primeiro.");
            return;
        }

        MazeData data = new MazeData
        {
            sizeX = sizeX,
            sizeZ = sizeZ,
            wallThickness = wallThickness,
            walls = new List<string>()
        };

        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                data.walls.Add($"{x},{z},{grid[x, z].northWall},{grid[x, z].southWall},{grid[x, z].eastWall},{grid[x, z].westWall}");
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(currentFile, json);
        Debug.Log($"Maze salvo em: {currentFile}");
    }

    private void LoadMaze()
    {
        if (!File.Exists(currentFile))
        {
            Debug.LogWarning("Arquivo de labirinto não encontrado.");
            return;
        }

        string json = File.ReadAllText(currentFile);
        MazeData data = JsonUtility.FromJson<MazeData>(json);
        sizeX = data.sizeX;
        sizeZ = data.sizeZ;
        wallThickness = data.wallThickness;

        ClearMaze();

        grid = new MazeCell[sizeX, sizeZ];
        foreach (string s in data.walls)
        {
            string[] p = s.Split(',');
            int x = int.Parse(p[0]);
            int z = int.Parse(p[1]);
            grid[x, z].northWall = bool.Parse(p[2]);
            grid[x, z].southWall = bool.Parse(p[3]);
            grid[x, z].eastWall = bool.Parse(p[4]);
            grid[x, z].westWall = bool.Parse(p[5]);
        }

        BuildMazeGeometry();
        Debug.Log("Maze carregado com sucesso.");
    }

    private void RestartMaze()
    {
        if (File.Exists(currentFile))
            LoadMaze();
        else
            GenerateMaze();
    }
    #endregion

    #region OBJ Exporter
    /// <summary>
    /// Combina as malhas dos filhos de mazeRoot e escreve um OBJ em Application.persistentDataPath/Mazes.
    /// </summary>
    public void ExportToOBJ()
    {
        if (mazeRoot == null)
        {
            Debug.LogWarning("Não há labirinto para exportar. Gere primeiro.");
            return;
        }

        MeshFilter[] meshFilters = mazeRoot.GetComponentsInChildren<MeshFilter>();
        if (meshFilters == null || meshFilters.Length == 0)
        {
            Debug.LogWarning("Nenhuma malha encontrada para exportar.");
            return;
        }

        // Combine em uma única malha
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            Mesh m = meshFilters[i].sharedMesh;
            if (m == null) continue;
            combine[i].mesh = m;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix * mazeRoot.transform.worldToLocalMatrix;
        }

        Mesh combined = new Mesh();
        combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // previne problemas com muitos vértices
        combined.CombineMeshes(combine, true, true);

        // Converte malha para OBJ text
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Maze OBJ exported " + System.DateTime.Now.ToString("s"));
        foreach (Vector3 v in combined.vertices)
            sb.AppendLine($"v {v.x} {v.y} {v.z}");
        foreach (Vector3 n in combined.normals)
            sb.AppendLine($"vn {n.x} {n.y} {n.z}");
        foreach (Vector2 uv in combined.uv)
            sb.AppendLine($"vt {uv.x} {uv.y}");

        int[] tris = combined.triangles;
        // OBJ é 1-based
        for (int i = 0; i < tris.Length; i += 3)
        {
            int a = tris[i] + 1;
            int b = tris[i + 1] + 1;
            int c = tris[i + 2] + 1;
            // v/vt/vn : usamos mesmo índice para todos
            sb.AppendLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");
        }

        string fileName = $"maze_{sizeX}x{sizeZ}_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.obj";
        string path = Path.Combine(saveFolder, fileName);
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

        Debug.Log($"Maze exportado para OBJ: {path}");
    }
    #endregion
}
