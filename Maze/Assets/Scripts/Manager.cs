using UnityEngine;

public class Manager : MonoBehaviour
{
    public int totalDiscovered = 0;
    public void CellEntered(Cell cell)
    {
        totalDiscovered++;
    }
}
