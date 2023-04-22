using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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

    public static void GetChildRecursive(GameObject obj, List<GameObject> listOfChildren, string findName)
    {
        if (null == obj) return;

        foreach (Transform child in obj.transform)
        {
            if (null == child) continue;
            if (findName == "") listOfChildren.Add(child.gameObject);
            else if (child.gameObject.name.Contains(findName)) listOfChildren.Add(child.gameObject);
            GetChildRecursive(child.gameObject, listOfChildren, findName);
        }
    }

    public static GameObject FindParentWithTagRecursive(GameObject obj, string tag)
    {
        List<GameObject> result = new List<GameObject>();
        RecursiveFind(obj, result, tag);
        static void RecursiveFind(GameObject obj, List<GameObject> result, string tag)
        {
            if (obj == null || obj.transform.parent == null) return;

            if (obj.transform.parent.gameObject.tag == tag) result.Add(obj.transform.parent.gameObject);
            else RecursiveFind(obj.transform.parent.gameObject, result, tag);
        }
        return result[0];
    }

    public static bool IsOwnedPlayer(Collider other)
    {
        if (other.tag == "Player")
            return other.GetComponent<NetworkObject>().IsOwner;
        else return false;
    }

    public static string TimeFormat(float time, bool showDecimal)
    {
        float time3Decimal = Mathf.Round(time * 1000) / 1000;
        float seconds = time3Decimal % 60;
        int minutes = Mathf.FloorToInt(time3Decimal / 60);
        float secondsUnit = (showDecimal) ? seconds : Mathf.Ceil(seconds);
        string colon = (secondsUnit <= 10) ? ":0" : ":";
        string format = minutes + colon + seconds;
        string formatNoDecimal = minutes + colon + Mathf.FloorToInt(seconds);
        return (showDecimal) ? format : formatNoDecimal;
    }

    public static int[] RandomIntArray(int lengthInput, int min, int max)
    {
        int length = (lengthInput < 0) ? 0 : lengthInput; // length cannot be less than 0
        int[] output = new int[length];
        if (length == 0) return output;
        for (int i = 0; i < length; i++) output[i] = -1; // initialize all to -1 so that they are out of index range

        // Flip min/max in case they are flipped
        int actualMin = (max > min) ? min : max;
        int actualMax = (max > min) ? max : min;

        for (int i = 0; i < length; i++)
        {
            int num;
            do
            {
                num = Random.Range(min, max);
            }
            while (output.Contains(num) && Mathf.Abs(actualMax - (actualMin - 1)) >= length); // do it again if this is true
            /// If the min-max range (e.g. 1,2,3) is smaller than the length (e.g. 4), it must have repeating values
            /// To have no repeating values, min-max range needs to be >= length

            output[i] = num;
        }
        return output;
    }

    public static List<int> RandomIntList(int lengthInput, int min, int max)
    {
        int length = (lengthInput < 0) ? 0 : lengthInput; // length cannot be less than 0
        List<int> output = new List<int>(length);
        if (length == 0) return output;
        foreach (var i in output) output[i] = -1; // initialize all to -1 so that they are out of index range

        // Flip min/max in case they are flipped
        int actualMin = (max > min) ? min : max;
        int actualMax = (max > min) ? max : min;

        for (int i = 0; i < length; i++)
        {
            int num;
            do
            {
                num = Random.Range(min, max);
            }
            while (output.Contains(num) && Mathf.Abs(actualMax - (actualMin - 1)) >= length); // do it again if this is true
            /// If the min-max range (e.g. 1,2,3) is smaller than the length (e.g. 4), it must have repeating values
            /// To have no repeating values, min-max range needs to be >= length

            output.Add(num);
        }
        return output;
    }

    public static string IntArrayToString(int[] array)
    {
        string output = "";
        for (int i = 0; i < array.Length; i++)
        {
            if (i > 0) output += ", ";
            output += array[i].ToString();
        }
        return "{ " + output + " }";
    }

    public static string IntListToString(List<int> list)
    {
        string output = "";
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0) output += ", ";
            output += list[i].ToString();
        }
        return "{ " + output + " }";
    }

    public static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180) return angle - 360;
        return angle;
    }

    public static float UnwrapAngle(float angle)
    {
        if (angle >= 0) return angle;
        angle = -angle % 360;
        return 360 - angle;
    }

    public static List<GameObject> GetAllObjectsInScene()
    {
        List<GameObject> objectsInScene = new List<GameObject>();

        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            objectsInScene.Add(go);

        return objectsInScene;
    }

    public static bool IsEmptyOrWhiteSpace(string value) => value.All(char.IsWhiteSpace);

    public static GameObject[] FindAllPlayers()
    {
        return GameObject.FindGameObjectsWithTag("Player");
    }

    public static GameObject FindOwnedPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.TryGetComponent<NetworkObject>(out var networkObject))
            {
                if (networkObject.IsOwner) return player;
            }
        }
        return null;
    }

    public static GameObject FindPlayerById(int id)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.TryGetComponent<NetworkObject>(out var networkObject))
            {
                if ((int)networkObject.OwnerClientId == id) return player;
            }
        }
        return null;
    }

    public static void DefaultPlayerNames()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.TryGetComponent<NetworkObject>(out var networkObject))
            {
                player.name = "Player " + networkObject.OwnerClientId + " Vehicle";
            }
        }
    }

    public static GameObject GetStartPosition(GameObject parent, int ownerPlayerId)
    {
        string findName = "[Player " + ownerPlayerId + " Start Position]";
        return parent.transform.Find(findName).gameObject;
    }
}
