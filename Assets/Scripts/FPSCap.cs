using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FPSCap : MonoBehaviour
{
    private int fps = 60;

    void Start()
    {
        #if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        #endif
    }
}
