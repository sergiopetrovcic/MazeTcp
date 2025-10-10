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

    // Stats window
    private Rect _statsWindow = new Rect(20, 20, 300, 335);

    // Shortcuts window
    //private Rect _shortcutsWindow = new Rect(20, 20 + 165 + 20, 300, 165);

    // Statistics
    private MazeGenerator maze;
    private int _totalBodies;
    private int _totalCells;
    private int _redBodies;
    private int _blueBodies;
    private float _starMass;
    private float _starSize;
    private float _timer = 0;
    private string _timerDisplay;
    private bool _timerRunning = false;

    private void Awake()
    {
        player.OnRotateLeft += Player_OnRotateLeft;
    }

    private void Player_OnRotateLeft(params object[] args)
    {
        _totalBodies++;
    }

    private void OnDestroy()
    {
        
    }

    private void Start()
    {
        _styleRed.normal.textColor = Color.red;
        _styleRed.fontSize = _fs;
        _styleBlue.normal.textColor = Color.blue;
        _styleBlue.fontSize = _fs;
        _styleWhite.normal.textColor = Color.white;
        _styleWhite.fontSize = _fs;

        maze = GameObject.FindAnyObjectByType<MazeGenerator>();
    }

    private void Update()
    {
        _totalCells = 10;
        _redBodies = 5;
        _blueBodies = 3;
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
        if (_timer < 60)
            _timerDisplay = _timer.ToString("0.0") + " sec";
        else if (_timer > 60 && _timer < 360)
            _timerDisplay = (_timer / 60f).ToString("0.0") + " min";
        else if (_timer >= 360)
            _timerDisplay = (_timer / 3600f).ToString("0.0") + " hours";
    }

    void OnGUI()
    {
        _statsWindow = GUI.Window(0, _statsWindow, StatsWindowHandle, "Statistics");//, _styleWhite);
    }

    void StatsWindowHandle(int windowID)
    {
        // Draw any Controls inside the window here
        GUI.Label(new Rect(_hi, _vi, 200, 100), "\"Current epoch: " + _totalBodies, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 1 * _vs, 200, 100), "Current epoch: " + _redBodies, _styleRed);//, "box");
        GUI.Label(new Rect(_hi, _vi + 2 * _vs, 200, 100), "Number of blue bodies: " + _blueBodies, _styleBlue);//, "box");
        GUI.Label(new Rect(_hi, _vi + 3 * _vs, 200, 100), "Star mass: " + _starMass.ToString("0"), _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 4 * _vs, 200, 100), "Star size: " + _starSize.ToString("0.00"), _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 5 * _vs, 200, 100), "Duration: " + _timerDisplay, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 6 * _vs, 200, 100), "- - - - - - - - - - - - - - - - - - - - - - - - - -", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 7 * _vs, 200, 100), "All epochs: " + _redBodies, _styleRed);//, "box");
        GUI.Label(new Rect(_hi, _vi + 8 * _vs, 200, 100), "Number of blue bodies: " + _blueBodies, _styleBlue);//, "box");
        GUI.Label(new Rect(_hi, _vi + 9 * _vs, 200, 100), "Star mass: " + _starMass.ToString("0"), _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 10 * _vs, 200, 100), "Star size: " + _starSize.ToString("0.00"), _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 11 * _vs, 200, 100), "Duration: " + _timerDisplay, _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 12 * _vs, 200, 100), "- - - - - - - - - - - - - - - - - - - - - - - - - -", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 13 * _vs, 200, 100), "Shortcuts", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 14 * _vs, 200, 100), "[X] - Save config", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 15 * _vs, 200, 100), "[Z] - Open config", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 16 * _vs, 200, 100), "[S] - Start simulation", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 17 * _vs, 200, 100), "[A] - Pause simulation", _styleWhite);//, "box");
        GUI.Label(new Rect(_hi, _vi + 18 * _vs, 200, 100), "[D] - Reset simulation", _styleWhite);//, "box");
    }
}