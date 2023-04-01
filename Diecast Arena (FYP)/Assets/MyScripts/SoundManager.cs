using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Tooltip("Find the AudioSource component in its children and store them in this list.")]
    public List<AudioSource> audioList;

    void Start()
    {
        List<GameObject> audioObjects = new List<GameObject>();
        Methods.GetChildRecursive(gameObject, audioObjects);

        for (int i = 0; i < audioObjects.Count; i++)
        {
            AudioSource audioSource = audioObjects[i].GetComponent<AudioSource>();
            if (audioSource != null) audioList.Add(audioSource);
        }
    }

    public void Play(string name)
    {
        int index = AudioNameToIndex(name);
        audioList[index].PlayOneShot(audioList[index].clip);
    }

    int AudioNameToIndex(string name)
    {
        for (int i = 0; i < audioList.Count; i++)
        {
            if (audioList[i].gameObject.name == name)
                return i; // The index
        }
        return -1; // It is not in the list
    }
}
