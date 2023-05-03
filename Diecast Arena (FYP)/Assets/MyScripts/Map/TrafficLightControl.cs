using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TrafficLight;

public static class TrafficLight
{
    public enum mode { Changing, AlwaysRed, AlwaysGreen }
    public enum currentLight { Red, RedYellow, Green, Yellow }
}

public class TrafficLightControl : MonoBehaviour
{
    Animator animator;

    public bool toggleRed;
    public bool toggleYellow;
    public bool toggleGreen;
    public currentLight current;

    [System.Serializable]
    public struct trafficLight
    {
        public int set;
        public currentLight current;
        public List<GameObject> lights;
    }

    [Space(10)]

    public trafficLight[] trafficLights;

    [Space(10)]

    [SerializeField] Material lightOff;
    [SerializeField] Material lightRed;
    [SerializeField] Material lightYellow;
    [SerializeField] Material lightGreen;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        foreach (var trafficLight in trafficLights)
        {
            trafficLight.lights.Add(Methods.GetChildContainsName(trafficLight.lights[0], "Red"));
            trafficLight.lights.Add(Methods.GetChildContainsName(trafficLight.lights[0], "Yellow"));
            trafficLight.lights.Add(Methods.GetChildContainsName(trafficLight.lights[0], "Green"));
        }

        SetLightMode(mode.Changing);
    }

    void Update()
    {
        // Determine current light
        if (toggleRed && !toggleYellow && !toggleGreen) current = currentLight.Red;
        if (toggleRed && toggleYellow && !toggleGreen) current = currentLight.RedYellow;
        if (!toggleRed && !toggleYellow && toggleGreen) current = currentLight.Green;
        if (!toggleRed && toggleYellow && !toggleGreen) current = currentLight.Yellow;

        for (int i = 0; i < trafficLights.Length; i++)
            trafficLights[i].current = current;

        foreach (var trafficLight in trafficLights)
        {
            if (trafficLight.current == currentLight.Red)
            {
                SetLight(trafficLight.lights[1], lightRed);
                SetLight(trafficLight.lights[2], lightOff);
                SetLight(trafficLight.lights[3], lightOff);
            }

            if (trafficLight.current == currentLight.RedYellow)
            {
                SetLight(trafficLight.lights[1], lightRed);
                SetLight(trafficLight.lights[2], lightYellow);
                SetLight(trafficLight.lights[3], lightOff);
            }

            if (trafficLight.current == currentLight.Green)
            {
                SetLight(trafficLight.lights[1], lightOff);
                SetLight(trafficLight.lights[2], lightOff);
                SetLight(trafficLight.lights[3], lightGreen);
            }

            if (trafficLight.current == currentLight.Yellow)
            {
                SetLight(trafficLight.lights[1], lightOff);
                SetLight(trafficLight.lights[2], lightYellow);
                SetLight(trafficLight.lights[3], lightOff);
            }
        }
    }

    void SetLight(GameObject user, Material mat)
    {
        user.GetComponent<MeshRenderer>().material = mat;
    }

    public void SetLightMode(mode mode)
    {
        if (mode == mode.Changing)
        {
            animator.enabled = true;
            animator.Play("Changing", 0, 0.0f);
        }

        if (mode == mode.AlwaysRed)
        {
            animator.enabled = false;
            toggleRed = true;
            toggleYellow = false;
            toggleGreen = false;
}

        if (mode == mode.AlwaysGreen)
        {
            animator.enabled = false;
            toggleRed = false;
            toggleYellow = false;
            toggleGreen = true;
        }
    }
}
