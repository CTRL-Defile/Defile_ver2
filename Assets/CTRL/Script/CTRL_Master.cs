using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CTRL_Master : MonoBehaviour
{
    public enum Basic_UPDATE_PHASE
    {
        CAMERA,
        UPDATE
    }
    [SerializeField] Basic_UPDATE_PHASE Basic_updatePhase;

    ////////// Getter & Setter  //////////

    ////////// Method           //////////

    ////////// Unity Method     //////////
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch(Basic_updatePhase)
        {
            case Basic_UPDATE_PHASE.CAMERA: {   CTRL_Camera_Init();     Basic_updatePhase = Basic_UPDATE_PHASE.UPDATE;  }   break;
            case Basic_UPDATE_PHASE.UPDATE: {                                                                           }   break;
        }
    }
}

partial class CTRL_Master
{
    [Header("CAMERA ==================================================")]
    [SerializeField] List<Camera> Camera_cameras;

    ////////// Getter & Setter  //////////
    object CTRL_Camera_GetMainCamera(params object[] _args) { return Camera_cameras[0]; }

    ////////// Method           //////////

    ////////// Unity Method     //////////
    void CTRL_Camera_Init()
    {
        CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_SetEvent(  CTRL_ScriptBridge.Event_TYPE.MASTER__CAMERA__GET_MAIN_CAMERA,   CTRL_Camera_GetMainCamera   );
    }
}
