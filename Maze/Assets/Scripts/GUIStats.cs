using UnityEngine;

//[ExecuteInEditMode]
public class GUIStats : MonoBehaviour
{
    //
    public Player player;

    // Windows
    private const int _fs = 18;
    private const int _hi = 10;
    private const int _vi = 25;
    private const int _vs = 22;
    private GUIStyle _styleRed = new GUIStyle();
    private GUIStyle _styleBlue = new GUIStyle();
    private GUIStyle _styleWhite = new GUIStyle();
    private GUIStyle _styleWhiteCenter = new GUIStyle();

    // Stats window
    private Rect _statsWindow = new Rect(20, 20, 300, 650);

    // Shortcuts window
    //private Rect _shortcutsWindow = new Rect(20, 20 + 165 + 20, 300, 165);

    // Statistics
    private MazeGenerator maze;
    private float _timer = 0;

    private void Start()
    {
        _styleRed.normal.textColor = Color.red;
        _styleRed.fontSize = _fs;

        _styleBlue.normal.textColor = Color.blue;
        _styleBlue.fontSize = _fs;

        _styleWhite.normal.textColor = Color.white;
        _styleWhite.fontSize = _fs;

        _styleWhiteCenter.normal.textColor = Color.white;
        _styleWhiteCenter.fontSize = _fs;
        _styleWhiteCenter.alignment = TextAnchor.MiddleCenter;
        maze = GameObject.FindAnyObjectByType<MazeGenerator>();
    }

    private void Update()
    {
        //if (_timerRunning)
        //{
        //    _timer += Time.deltaTime;
        //    UpdateTimerDisplay();
        //}
        //else if (Input.GetKeyDown(KeyCode.S))
        //{
        //    Debug.LogWarning("Start simulation pressed");
        //    //script.StartSim();
        //    _timerRunning = true;
        //    UpdateTimerDisplay();
        //}
        //else if (Input.GetKeyDown(KeyCode.A))
        //{
        //    Debug.LogWarning("Pause simulation pressed");
        //    if (Time.timeScale == 0) Time.timeScale = 1;
        //    else if (Time.timeScale == 1) Time.timeScale = 0;
        //}
        //else if (Input.GetKeyDown(KeyCode.D))
        //{
        //    Debug.LogWarning("Reset simulation pressed");
        //    _timerRunning = false;
        //    _timer = 0;
        //    UpdateTimerDisplay();
        //    //script.DeleteChildren();
        //}
    }

    private void UpdateTimerDisplay()
    {
        //if (_timer < 60)
        //    _timerDisplay = _timer.ToString("0.0") + " sec";
        //else if (_timer > 60 && _timer < 360)
        //    _timerDisplay = (_timer / 60f).ToString("0.0") + " min";
        //else if (_timer >= 360)
        //    _timerDisplay = (_timer / 3600f).ToString("0.0") + " hours";
    }

    void OnGUI()
    {
        _statsWindow = GUI.Window(0, _statsWindow, StatsWindowHandle, "Statistics");//, _styleWhite);
    }

    void StatsWindowHandle(int windowID)
    {
        int _width = 280;
        int _height = 100;
        // Draw any Controls inside the window here
        GUI.Label(new Rect(0, -18, _width, _height), "Current epoch: " + player.epochCurrent, _styleWhiteCenter);//, "box");
        GUI.Label(new Rect(_hi, _vi + 1 * _vs, _width, _height), "- Left rotations: " + player.leftRotationsCurrentEpoch, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 2 * _vs, _width, _height), "- Right rotations: " + player.rightRotationsCurrentEpoch, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 3 * _vs, _width, _height), "- Total rotations (left + right): " + player.rotationsCurrentEpoch, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 4 * _vs, _width, _height), "- Steps: " + player.stepsCurrentEpoch, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 5 * _vs, _width, _height), "- Moves (rotations + steps): " + player.movesCurrentEpoch, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 6 * _vs, _width, _height), "- Cells discovered: " + player.cellsDiscoveredCurrentEpoch, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 7 * _vs, _width, _height), "- Cells visited: " + player.cellsVisitedCurrentEpoch, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 8 * _vs, _width, _height), "- Timer: " + (player.timerCurrentEpoch / 1000).ToString("0.0"), _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 9 * _vs, _width, _height), "- - - - - - - - - - - - - - - - - - - - - - - - - -", _styleWhite);//, "box");
        GUI.Label(new Rect(0, -18 + 10 * _vs, _width, _height), "All epochs", _styleWhiteCenter);//, "box");
        GUI.Label(new Rect(_hi, _vi + 11 * _vs, _width, _height), "- Left rotations: " + player.leftRotations, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 12 * _vs, _width, _height), "- Right rotations: " + player.rightRotations, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 13 * _vs, _width, _height), "- Total rotations (left + right): " + player.rotations, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 14 * _vs, _width, _height), "- Steps: " + player.steps, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 15 * _vs, _width, _height), "- Moves (rotations + steps): " + player.moves, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 16 * _vs, _width, _height), "- Cells discovered: " + player.cellsDiscovered, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 17 * _vs, _width, _height), "- Cells visited: " + player.cellsVisited, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 18 * _vs, _width, _height), "- Timer: " + ((player.timer + player.timerCurrentEpoch) / 1000).ToString("0.0"), _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 19 * _vs, _width, _height), "- - - - - - - - - - - - - - - - - - - - - - - - - -", _styleWhite);//, "box");
        GUI.Label(new Rect(0, -18 + 20 * _vs, _width, _height), "Shortcuts", _styleWhiteCenter);//, "box");
        GUI.Label(new Rect(_hi, _vi + 21 * _vs, _width, _height), "- [W] Move forward", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 22 * _vs, _width, _height), "- [A] Rotate 90° left", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 23 * _vs, _width, _height), "- [D] Rotate 90° right", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 24 * _vs, _width, _height), "- [alt+F4] Exit", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 25 * _vs, _width, _height), "- - - - - - - - - - - - - - - - - - - - - - - - - -", _styleWhite);//, "box");
        GUI.Label(new Rect(0, -18 + 26 * _vs, _width, _height), "Information", _styleWhiteCenter);//, "box");
        GUI.Label(new Rect(_hi, _vi + 27 * _vs, _width, _height), "- Server port: 5005", _styleWhite);//, "box");
        //GUI.Label(new Rect(_hi, _vi + 22 * _vs, _width, _height), "[X] - Save config", _styleWhite);//, "box");
        //GUI.Label(new Rect(_hi, _vi + 23 * _vs, _width, _height), "[Z] - Open config", _styleWhite);//, "box");
        //GUI.Label(new Rect(_hi, _vi + 24 * _vs, _width, _height), "[S] - Start simulation", _styleWhite);//, "box");
        //GUI.Label(new Rect(_hi, _vi + 25 * _vs, _width, _height), "[A] - Pause simulation", _styleWhite);//, "box");
        //GUI.Label(new Rect(_hi, _vi + 26 * _vs, _width, _height), "[D] - Reset simulation", _styleWhite);//, "box");
    }
}