using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public static class Dev
{
    public static bool log_gameState = false;
    public static bool log_inputType = false;
}

public static class Manager
{
    public enum type { input, sound, UI };
}

// A place to store values that are commonly used / shared
public static class Common
{
    // Spawn player a little higher to avoid clipping through plane
    public static Vector3 spawnHeightOffset = new Vector3(0, 0.1f, 0);
}
