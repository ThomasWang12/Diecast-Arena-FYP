using RVP;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameMaster;

public class InputManager : MonoBehaviour
{
    GameMaster master;
    UIManager UI;

    public enum inputType { Initial, MouseKeyboard, Gamepad }
    inputType defaultInputType = inputType.MouseKeyboard;
    public inputType currentInputType;
    inputType prevInputType = inputType.Initial;

    public bool allowInput = true;
    public bool allowDrive = true;
    public bool forceBrake = false;

    string key = null;
    bool padAxis6Pressed = false; // Gamepad Left/Right Buttons (left = -1, right = 1)
    bool padAxis7Pressed = false; // Gamepad Up/Down Buttons (up = 1, down = -1)

    [HideInInspector] public bool allowExitActivity = false;

    /* Tunables */
    float deadzone = 0.19f;
    float deadzero = 0.001f; // float below this value ~= 0 (prevent floating-point precision issue)

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        UI = master.ManagerObject(Manager.type.UI).GetComponent<UIManager>();
    }

    void Start()
    {
        currentInputType = defaultInputType;
    }

    void Update()
    {
        currentInputType = CheckInputType();
        OnInputTypeChange(currentInputType);

        allowExitActivity = master.currentState.ToString().Contains("Activity") && !master.currentState.ToString().Contains("_");
        if (allowExitActivity)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Gamepad Select"))
                master.ExitActivity(master.activeActivityIndex);
        }

        if (master.currentState == gameState.Menu || master.currentState == gameState.Session)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }
    }

    void LateUpdate()
    {
        if (Mathf.Abs(Input.GetAxisRaw("Gamepad Left/Right Buttons")) < deadzero)
            padAxis6Pressed = false;

        if (Mathf.Abs(Input.GetAxisRaw("Gamepad Up/Down Buttons")) < deadzero)
            padAxis7Pressed = false;
    }

    void OnGUI()
    {
        // Detects all key inputs from keyboard
        if (Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            // A key input is detected, its KeyCode is stored in Event.current.keyCode
            key = Event.current.keyCode.ToString();
        }
    }

    inputType CheckInputType()
    {
        // The 'key' reseult from OnGUI()
        if (key != null)
        {
            key = null;
            return inputType.MouseKeyboard;
        }

        // Detect mouse input
        if (currentInputType == inputType.Gamepad)
        {
            // Left click / Middle click / Right click
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                return inputType.MouseKeyboard;
        }

        // Detect gamepad input
        if (currentInputType == inputType.MouseKeyboard)
        {
            // Left joystick
            if (Mathf.Abs(Input.GetAxisRaw("Joystick Horizontal")) > deadzone ||
                Mathf.Abs(Input.GetAxisRaw("Joystick Vertical")) > deadzone)
                return inputType.Gamepad;

            // Right joystick
            if (Mathf.Abs(Input.GetAxisRaw("Joystick Look X")) > deadzone ||
                Mathf.Abs(Input.GetAxisRaw("Joystick Look Y")) > deadzone)
                return inputType.Gamepad;

            // LB / RB / L3 / R3
            if (Input.GetButtonDown("Gamepad LB") || Input.GetButtonDown("Gamepad RB") ||
                Input.GetButtonDown("Gamepad L3") || Input.GetButtonDown("Gamepad R3"))
                return inputType.Gamepad;

            // Shoulder triggers: LT, RT
            if (Mathf.Abs(Input.GetAxisRaw("Gamepad LT")) > deadzone ||
                Mathf.Abs(Input.GetAxisRaw("Gamepad RT")) > deadzone)
                return inputType.Gamepad;

            // Buttons: Select, Start
            if (Input.GetButtonDown("Gamepad Select") || Input.GetButtonDown("Gamepad Start"))
                return inputType.Gamepad;

            // Buttons: Up, Down, Left, Right
            if (Mathf.Abs(Input.GetAxisRaw("Gamepad Up/Down Buttons")) > deadzero ||
                Mathf.Abs(Input.GetAxisRaw("Gamepad Left/Right Buttons")) > deadzero)
                return inputType.Gamepad;

            // Buttons: Fire1 = A (X), Fire2 = B (O), Fire3 = X (Square), Jump = Y (Triangle)
            if (Input.GetButtonDown("Gamepad Fire1") || Input.GetButtonDown("Gamepad Fire2") ||
                Input.GetButtonDown("Gamepad Fire3") || Input.GetButtonDown("Gamepad Jump"))
                return inputType.Gamepad;
        }

        return currentInputType;
    }

    void OnInputTypeChange(inputType type)
    {
        if (prevInputType != type)
        {
            if (Dev.log_inputType)
                Debug.Log("Input Type: " + type.ToString());

            if (UI.promptsInitialized) UI.ChangeButtonType(type);
            prevInputType = type;
        }
    }

    public void ForceBrake()
    {
        allowDrive = false;
        forceBrake = true;
    }

    public void EnableDrive()
    {
        allowDrive = true;
        forceBrake = false;
    }

    public bool ToggleHUD()
    {
        if (!allowInput) return false;

        if (Input.GetKeyDown(KeyCode.F2)) return true;
        return false;
    }

    public bool EnterActivity()
    {
        if (!allowInput) return false;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Gamepad Fire3")) return true;
        return false;
    }

    #region Gamepad Buttons (Run once upon pressed)

    public bool GamepadLeftButton()
    {
        if (Input.GetAxisRaw("Gamepad Left/Right Buttons") < 0)
        {
            if (!padAxis6Pressed)
            {
                padAxis6Pressed = true;
                return true;
            }
        }
        return false;
    }

    public bool GamepadRightButton()
    {
        if (Input.GetAxisRaw("Gamepad Left/Right Buttons") > 0)
        {
            if (!padAxis6Pressed)
            {
                padAxis6Pressed = true;
                return true;
            }
        }
        return false;
    }

    public bool GamepadUpButton()
    {
        if (Input.GetAxisRaw("Gamepad Up/Down Buttons") > 0)
        {
            if (!padAxis7Pressed)
            {
                padAxis7Pressed = true;
                return true;
            }
        }
        return false;
    }

    public bool GamepadDownButton()
    {
        if (Input.GetAxisRaw("Gamepad Up/Down Buttons") < 0)
        {
            if (!padAxis7Pressed)
            {
                padAxis7Pressed = true;
                return true;
            }
        }
        return false;
    }

    #endregion
}
