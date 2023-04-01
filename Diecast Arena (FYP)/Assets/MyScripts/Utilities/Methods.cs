using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public static class Methods
{
    public static float Map(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static List<GameObject> GetChildren(GameObject go)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        return children;
    }

    public static GameObject GetChildContainsName(GameObject go, string name)
    {
        foreach (Transform tran in go.transform)
        {
            if (tran.gameObject.name.Contains(name))
                return tran.gameObject;
        }
        return null;
    }

    public static GameObject FindParentWithTag(GameObject childObject, string tag)
    {
        Transform t = childObject.transform;
        while (t.parent != null)
        {
            if (t.parent.tag == tag)
            {
                return t.parent.gameObject;
            }
            t = t.parent.transform;
        }
        return null;
    }

    public static void GetChildRecursive(GameObject obj, List<GameObject> listOfChildren)
    {
        if (null == obj) return;

        foreach (Transform child in obj.transform)
        {
            if (null == child) continue;
            listOfChildren.Add(child.gameObject);
            GetChildRecursive(child.gameObject, listOfChildren);
        }
    }

    public static bool IsPlayer(Collider other)
    {
        // Detects for the box collider of 'chassis' in player vehicle
        if (other.transform.root.gameObject.tag != "Player"
            || other.gameObject.name != "chassis") return false;

        return true;
    }

    public static string TimeFormat(float time)
    {
        float time3Decimal = Mathf.Round(time * 1000) / 1000;
        float seconds = time3Decimal % 60;
        int minutes = Mathf.FloorToInt(time3Decimal / 60);
        string colon = (seconds < 10) ? ":0" : ":";
        return minutes + colon + seconds;
    }
}
