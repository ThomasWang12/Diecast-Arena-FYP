using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [SerializeField] Texture[] colorTextures;
    [SerializeField] Texture[] colorTexturesSupp;
    [SerializeField] GameObject[] colorParts;

    public void ApplyColor(int colorIndex)
    {
        foreach (var part in colorParts)
        {
            // Change texture for "Paint" material (Element 1 in the Materials list)
            part.GetComponent<MeshRenderer>().materials[1].SetTexture("_BaseMap", colorTextures[colorIndex]);
            // Change texture for "Paint Trunk" material (Element 7 in the Materials list)
            part.GetComponent<MeshRenderer>().materials[7].SetTexture("_BaseMap", colorTexturesSupp[colorIndex]);
        }
    }
}
