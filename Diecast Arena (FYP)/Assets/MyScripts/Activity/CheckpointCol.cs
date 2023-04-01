using UnityEngine;
using UnityEngine.Timeline;

public static class CheckpointCol
{
    static GameMaster master;

    /* Tunables */
    static float fadeDist = 30f;
    static float alphaMax = 0.8f;
    static float alphaMin = 0.25f;
    static float fadeStartAlpha = 0.5f;
    static float fadeDuration = 0.3f;

    [HideInInspector] public static float inactiveAfter = 2.0f;

    public static void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
    }

    public static float FadeCloserValue(Vector3 userPos, float colliderRadius)
    {
        float dist = Vector3.Distance(master.playerPos, userPos) - colliderRadius;
        float distClamped = Mathf.Clamp(dist, 0, fadeDist);
        return Methods.Map(distClamped, 0, fadeDist, alphaMin, alphaMax);
    }

    public static float FadeCollidedValue(float collideTime)
    {
        float k = Mathf.Clamp((collideTime + fadeDuration) - Time.time, 0, fadeDuration);
        return Easing.Type.SineEaseOut(k, 0, fadeStartAlpha, fadeDuration);
    }

    public static void FadeCollided(GameObject user, Material userMat, float fade)
    {
        if (user == null) return;
        userMat.SetColor("_EmissionColor", Color.white);
        Color color = new Color(Color.white.r, Color.white.g, Color.white.b, fade);
        userMat.SetColor("_BaseColor", color);
    }
}
