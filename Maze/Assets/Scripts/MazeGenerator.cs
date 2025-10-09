using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MazeMode
{
    Goal,
    Exit
}

public class MazeGenerator : MonoBehaviour
{
    [Header("Configuração do Labirinto")]
    public int sizeX = 10;
    public int sizeZ = 10;
    public float cellWidth = 2f;
    public float cellLength = 2f;
    public float wallHeight = 2f;

    public MazeMode mode = MazeMode.Goal;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject wallPrefab;

    public Cell[,] grid;

    // Posicionamento do player e célula alvo
    public Vector2Int playerStart;
    public Vector2Int targetCell; // usado no Goal ou Exit

    [Header("Fog Reveal Settings")]
    private List<GameObject> fogList = new List<GameObject>();
    [SerializeField] private Material fogMaterial;
    [SerializeField, Range(1f, 10f)] private float fogAlpha = 0.8f; // transparência
    public bool allowRefog = false; // se verdadeiro, a neblina volta quando o player sai da área

    void Start()
    {
        GenerateMaze();
    }

    #region Geração do Labirinto
    public void GenerateMaze()
    {
        grid = new Cell[sizeZ, sizeX]; // linhas x colunas (Z x X)

        // Instancia células
        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                Vector3 pos = new Vector3(x * cellWidth, 0, z * cellLength);
                GameObject obj = Instantiate(cellPrefab, pos, Quaternion.identity, transform);

                Cell cell = obj.GetComponent<Cell>();
                cell.width = cellWidth;
                cell.length = cellLength;
                cell.wallHeight = wallHeight;

                cell.UpdateFloor();
                cell.UpdateBoxCollider();

                cell.CreateWalls(wallPrefab);

                grid[z, x] = cell;
            }
        }

        // Remove aberturas agora **após criar todas as células**
        RemoveWallForEdge(playerStart);
        if (mode == MazeMode.Exit)
            RemoveWallForEdge(targetCell);

        // Escolhe entrada(s)
        if (mode == MazeMode.Goal)
            playerStart = new Vector2Int(0, 0);
        else if (mode == MazeMode.Exit)
        {
            playerStart = new Vector2Int(0, 0);
            targetCell = new Vector2Int(sizeZ - 1, sizeX - 1);
        }

        // Gera labirinto via DFS
        GenerateMazeDFS(playerStart.x, playerStart.y, new bool[sizeZ, sizeX]);

        // Para modo Goal, seleciona célula alvo mais distante
        if (mode == MazeMode.Goal)
            targetCell = FindFurthestCell(playerStart);

        PositionCameraAbove();
    }

    #endregion

    #region Input
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    //private bool IsKeyDown_W() => Keyboard.current != null && Keyboard.current[Key.W].isPressed;
    //private bool IsKeyDown_R() => Keyboard.current != null && Keyboard.current[Key.R].wasPressedThisFrame;
    //private bool IsKeyDown_D() => Keyboard.current != null && Keyboard.current[Key.D].wasPressedThisFrame;
#else
        //private bool IsKeyDown_W() => Input.GetKeyDown(KeyCode.W);
        private bool IsKeyDown_R() => Input.GetKeyDown(KeyCode.R);
        private bool IsKeyDown_D() => Input.GetKeyDown(KeyCode.D);
#endif

    private void HandleKeyboardInput()
    {
        // Move para frente
        //if (IsKeyDown_W()) MoveToNextCell();//MoveForward();
        // Girar para a esquerda
        //if (IsKeyDown_R()) Restart();
        // Girar para a direita
        //if (IsKeyDown_D()) RotateCW();
    }
    #endregion

    #region DFS Recursivo
    private void GenerateMazeDFS(int row, int col, bool[,] visited)
    {
        visited[row, col] = true;

        List<Vector2Int> neighbors = new List<Vector2Int>
    {
        new Vector2Int(row - 1, col), // cima
        new Vector2Int(row + 1, col), // baixo
        new Vector2Int(row, col - 1), // esquerda
        new Vector2Int(row, col + 1)  // direita
    };

        // Embaralha os vizinhos
        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector2Int temp = neighbors[i];
            int rand = Random.Range(0, neighbors.Count);
            neighbors[i] = neighbors[rand];
            neighbors[rand] = temp;
        }

        foreach (var n in neighbors)
        {
            int nRow = n.x;
            int nCol = n.y;

            // Checa limites usando sizeZ (linhas) e sizeX (colunas)
            if (nRow >= 0 && nRow < sizeZ && nCol >= 0 && nCol < sizeX && !visited[nRow, nCol])
            {
                RemoveWallBetween(grid[row, col], grid[nRow, nCol]);
                GenerateMazeDFS(nRow, nCol, visited);
            }
        }

        CreateFog();
    }


    public void CreateFog()
    {
        // Remove fog antigo se houver
        foreach (var f in fogList)
        {
            if (f != null)
                Destroy(f);
        }
        fogList.Clear();

        float halfSizeX = (sizeX * cellWidth) / 2f;
        float halfSizeZ = (sizeZ * cellLength) / 2f;


        // Cria um quad de fog sobre cada célula
        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                Vector3 fogPos = new Vector3(
                    x * cellWidth,
                    wallHeight + 0.01f, // levemente acima das paredes
                    z * cellLength
                );

                GameObject fog = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fog.name = $"Fog_{x}_{z}";
                fog.transform.SetParent(transform, true);
                fog.transform.localScale = new Vector3(cellWidth, cellLength, 1);
                fog.transform.position = fogPos;
                fog.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // deitado sobre o labirinto

                // Aplica material ou cor padrão
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
        if (fogList == null || fogList.Count == 0)
            return;

        Vector2 playerXZ = new Vector2(playerPosition.x, playerPosition.z);

        foreach (var fog in fogList)
        {
            if (fog == null) continue;

            Vector2 fogXZ = new Vector2(fog.transform.position.x, fog.transform.position.z);
            float dist = Vector2.Distance(playerXZ, fogXZ);

            // Se estiver dentro do raio de visão
            if (dist < visionRadius)
            {
                if (fog.activeSelf)
                    fog.SetActive(false); // revela
            }
            else if (allowRefog)
            {
                if (!fog.activeSelf)
                    fog.SetActive(true); // reaplica neblina se o jogador se afastar
            }
        }
    }
    #endregion

    #region Remoção de paredes
    private void RemoveWallBetween(Cell a, Cell b)
    {
        Vector2Int diff = new Vector2Int(
            Mathf.RoundToInt(b.transform.localPosition.x - a.transform.localPosition.x),
            Mathf.RoundToInt(b.transform.localPosition.z - a.transform.localPosition.z)
        );

        if (diff.x > 0) { Destroy(a.wallEast); Destroy(b.wallWest); } // b está à direita
        else if (diff.x < 0) { Destroy(a.wallWest); Destroy(b.wallEast); } // b está à esquerda
        else if (diff.y > 0) { Destroy(a.wallNorth); Destroy(b.wallSouth); } // b está acima
        else if (diff.y < 0) { Destroy(a.wallSouth); Destroy(b.wallNorth); } // b está abaixo
    }

    private void RemoveWallForEntry(Vector2Int pos)
    {
        Cell c = grid[pos.x, pos.y];
        if (pos.x == 0) Destroy(c.wallWest);
        else if (pos.x == sizeX - 1) Destroy(c.wallEast);
        else if (pos.y == 0) Destroy(c.wallSouth);
        else if (pos.y == sizeZ - 1) Destroy(c.wallNorth);
    }

    private void RemoveWallForEdge(Vector2Int pos)
    {
        Cell c = grid[pos.x, pos.y]; // pos.x = linha (Z), pos.y = coluna (X)


        if (pos.x == 0) Destroy(c.wallWest);
        else if (pos.x == sizeX - 1) Destroy(c.wallEast);
        else if (pos.y == 0) Destroy(c.wallSouth);
        else if (pos.y == sizeZ - 1) Destroy(c.wallNorth);
    }

    #endregion

    #region Encontrar célula mais distante
    private Vector2Int FindFurthestCell(Vector2Int start)
    {
        // BFS simples para encontrar célula mais distante
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[sizeZ, sizeX];
        Vector2Int furthest = start;

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            furthest = curr;

            List<Vector2Int> neighbors = new List<Vector2Int>
            {
                new Vector2Int(curr.x - 1, curr.y),
                new Vector2Int(curr.x + 1, curr.y),
                new Vector2Int(curr.x, curr.y - 1),
                new Vector2Int(curr.x, curr.y + 1)
            };

            foreach (var n in neighbors)
            {
                if (n.x >= 0 && n.x < sizeX && n.y >= 0 && n.y < sizeZ && !visited[n.x, n.y])
                {
                    // Só considera vizinhos com parede removida
                    if (HasNoWallBetween(grid[curr.x, curr.y], grid[n.x, n.y]))
                    {
                        visited[n.x, n.y] = true;
                        queue.Enqueue(n);
                    }
                }
            }
        }

        return furthest;
    }

    private bool HasNoWallBetween(Cell a, Cell b)
    {
        Vector2Int diff = new Vector2Int(
            Mathf.RoundToInt(b.transform.localPosition.x - a.transform.localPosition.x),
            Mathf.RoundToInt(b.transform.localPosition.z - a.transform.localPosition.z)
        );

        if (diff.x > 0) return a.wallEast == null;
        if (diff.x < 0) return a.wallWest == null;
        if (diff.y > 0) return a.wallNorth == null;
        if (diff.y < 0) return a.wallSouth == null;

        return false;
    }

    public Cell GetCell(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= sizeZ || pos.y < 0 || pos.y >= sizeX)
            return null;
        return grid[pos.x, pos.y]; // x = linha, y = coluna
    }


    public Cell GetNeighbor(Cell current, Vector3 direction)
    {
        Vector2Int currentPos = GetCellPosition(current);
        Vector2Int nextPos = currentPos;

        Vector3 dir = direction.normalized;

        if (Mathf.Abs(dir.z) > Mathf.Abs(dir.x))
        {
            // eixo Z → linha (primeiro índice do grid)
            if (dir.z > 0)
                nextPos.x += 1; // norte
            else
                nextPos.x -= 1; // sul
        }
        else
        {
            // eixo X → coluna (segundo índice do grid)
            if (dir.x > 0)
                nextPos.y += 1; // leste
            else
                nextPos.y -= 1; // oeste
        }

        // Verifica limites (linhas = sizeZ, colunas = sizeX)
        if (nextPos.x < 0 || nextPos.x >= sizeZ || nextPos.y < 0 || nextPos.y >= sizeX)
            return null;

        Cell next = grid[nextPos.x, nextPos.y];

        if (!HasNoWallBetween(current, next))
            return null;

        return next;
    }

    public Cell GetNeighborSingleStep(Cell current, Vector3 direction)
    {
        Vector2Int pos = GetCellPosition(current);
        Vector2Int nextPos = pos;

        if (Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
        {
            if (direction.z > 0) nextPos.x += 1; // norte
            else nextPos.x -= 1; // sul
        }
        else
        {
            if (direction.x > 0) nextPos.y += 1; // leste
            else nextPos.y -= 1; // oeste
        }

        // Limites
        if (nextPos.x < 0 || nextPos.x >= sizeZ || nextPos.y < 0 || nextPos.y >= sizeX)
            return null;

        Cell next = grid[nextPos.x, nextPos.y];

        // Só retorna se não houver parede entre as duas células
        if (!HasNoWallBetween(current, next))
            return null;

        return next;
    }

    public Cell GetNeighborGridAligned(Cell current)
    {
        Vector2Int pos = GetCellPosition(current);
        Vector2Int nextPos = pos;

        // Determina direção baseada na rotação do player
        float yRot = Mathf.Round(transform.eulerAngles.y) % 360;

        if (yRot == 0f) nextPos.x += 1;      // Norte
        else if (yRot == 90f) nextPos.y += 1; // Leste
        else if (yRot == 180f) nextPos.x -= 1; // Sul
        else if (yRot == 270f) nextPos.y -= 1; // Oeste
        else return null; // ângulo inválido

        // Limites do grid
        if (nextPos.x < 0 || nextPos.x >= sizeZ || nextPos.y < 0 || nextPos.y >= sizeX)
            return null;

        Cell next = grid[nextPos.x, nextPos.y];

        if (!HasNoWallBetween(current, next))
            return null;

        return next;
    }


    // linha = z (primeiro índice do grid)
    // coluna = x (segundo índice do grid)
    Vector2Int GetCellPosition(Cell c)
    {
        for (int z = 0; z < sizeZ; z++)
            for (int x = 0; x < sizeX; x++)
                if (grid[z, x] == c)
                    return new Vector2Int(z, x); // linha = x = z, coluna = y = x
        return Vector2Int.zero;
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
        cam.orthographicSize = 16;// maxSize * 0.55f;
        cam.transform.position = new Vector3(sizeX - 1, maxSize, sizeZ - 1);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 500f;
    }

    #endregion
}
