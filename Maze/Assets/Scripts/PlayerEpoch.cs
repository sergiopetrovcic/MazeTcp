
public class PlayerEpoch
{
    private string player;
    private int epoch;
    private int rotations;
    private int leftRotations;
    private int rightRotations;
    private int steps;
    private int moves;
    private float timer;
    private int cellsDiscovered;
    private int cellsVisited;

    public PlayerEpoch(string player, int epoch, int rotations, int leftRotations, int rightRotations, int steps, int moves, float timer, int cellsDiscovered, int cellsVisited)
    {
        this.player = player;
        this.epoch = epoch;
        this.rotations = rotations;
        this.leftRotations = leftRotations;
        this.rightRotations = rightRotations;
        this.steps = steps;
        this.moves = moves;
        this.timer = timer;
        this.cellsDiscovered = cellsDiscovered;
        this.cellsVisited = cellsVisited;
    }
}
