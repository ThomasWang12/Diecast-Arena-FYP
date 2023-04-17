using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A place to store values that are commonly used / shared
public class Common : MonoBehaviour
{
    public static readonly string mainSceneName = "Demo Network";

    // Spawn player a little higher to avoid clipping through plane
    public static Vector3 spawnHeightOffset = new Vector3(0, 0.25f, 0);

    public Color raceCheckpoint = Color.black;
    public Color raceCheckpointFinish = Color.black;
    public Color collectCheckpoint = Color.black;
    public Color collectCheckpointDiamond = Color.black;
    public Color huntCheckpoint = Color.black;
}
