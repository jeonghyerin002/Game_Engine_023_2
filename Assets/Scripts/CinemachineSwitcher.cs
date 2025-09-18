using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCam;
    public CinemachineFreeLook freeLookCam;
    public bool usingFreeLook = false;

    void Start()
    {
        virtualCam.Priority = 10;
        freeLookCam.Priority = 0;
    }


    void Update()
    {
        if(Input.GetMouseButtonDown(1)) //¿ìÅ¬¸¯
        {
            usingFreeLook = !usingFreeLook;
            if(usingFreeLook)
            {
                freeLookCam.Priority = 20;
                virtualCam.Priority = 0;
            }
            else
            {
                virtualCam.Priority = 20;
                freeLookCam.Priority = 0;
            }
        }
    }
}
