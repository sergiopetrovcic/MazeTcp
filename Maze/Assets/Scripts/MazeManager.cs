using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public int totalDiscovered = 0;
    public void CellEntered(Cell cell)
    {
        totalDiscovered++;
    }
}
