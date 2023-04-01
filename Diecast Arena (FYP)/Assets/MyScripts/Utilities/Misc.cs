using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Misc : MonoBehaviour
{
    void Awake()
    {
        // Help static classes to call Awake() for their initialization
        CheckpointCol.Awake();
    }
}
