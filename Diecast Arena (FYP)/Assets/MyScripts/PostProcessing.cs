using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PostProcessing : MonoBehaviour
{
    GameMaster master;
    [SerializeField] VolumeProfile profile;
    DepthOfField depthOfField;

    void Awake()
    {
        master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
    }

    public void ToggleDOV(bool state)
    {
        if (profile.TryGet(out depthOfField))
        {
            depthOfField.active = state;
        }
    }
}
