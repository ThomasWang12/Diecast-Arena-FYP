using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Misc : MonoBehaviour
{
    GameMaster master;
    InputManager input;

    bool toggleConsole = false;

    void Awake()
    {
        master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
        input = master.input;

        // Help static classes to call Awake() for their initialization
        Methods.Awake();
        CheckpointCol.Awake();
    }

    void Start()
    {
        GameObject.Find("IngameDebugConsole").GetComponent<Canvas>().enabled = toggleConsole;
    }

    void Update()
    {
        // Debug console visibility
        if (input.ToggleConsole())
        {
            toggleConsole = !toggleConsole;
            GameObject.Find("IngameDebugConsole").GetComponent<Canvas>().enabled = toggleConsole;
        }
    }
}
