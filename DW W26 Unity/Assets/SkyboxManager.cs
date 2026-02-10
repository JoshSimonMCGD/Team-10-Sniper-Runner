using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    public float skySpeed;

    // Update is called once per frame
    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skySpeed);
    }
}
