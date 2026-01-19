using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPortalWallPlane : MonoBehaviour
{
    public Transform plane;
    public float fadeWidth = 0.5f;


    void Start()
    {
        Shader.SetGlobalVector("_PortalPlanePos", plane.position);
        Shader.SetGlobalVector("_PortalPlaneNormal", -plane.forward);
        Shader.SetGlobalFloat("_PortalFadeWidth", fadeWidth);
    }

}
