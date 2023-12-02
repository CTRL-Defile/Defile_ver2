using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CTRL_ScriptBridge : System.IDisposable
{
    static CTRL_ScriptBridge Basic_instance;

    ////////// Getter & Setter  //////////
    public static CTRL_ScriptBridge CTRL_Basic_instance
    {
        get
        {
            if (Basic_instance == null)
            {
                Basic_instance = new CTRL_ScriptBridge();
            }
            return Basic_instance;
        }
    }

    ////////// Method           //////////

    public CTRL_ScriptBridge()
    {
        Event_Init();
    }

    ////////// Unity Method     //////////
    public void Dispose()
    {

    }
}

#region EVENT

partial class CTRL_ScriptBridge
{
    public enum Event_TYPE
    {
        MASTER__CAMERA__GET_MAIN_CAMERA,

        //
        DATABASE__SKILL__GET_SKILL,

        //
        BATTLE__TILE__GET_TILES,
        BATTLE__TILE__GET_X,
        BATTLE__TILE__GET_Y,
        BATTLE__TILE__UNIT_DATA_UPDATE,
    }
    public delegate object Event_Method(params object[] _args);

    Dictionary<Event_TYPE, Event_Method> Event_methods;

    ////////// Getter & Setter  //////////
    public object CTRL_Event_GetEvent(Event_TYPE _type, params object[] _args)
    {
        object res = null;

        if (Event_methods.ContainsKey(_type))
        {
            res = Event_methods[_type].Invoke(_args);
        }

        return res;
    }

    public void CTRL_Event_SetEvent(Event_TYPE _type, Event_Method _method)
    {
        if (Event_methods.ContainsKey(_type))
        {
            Event_methods[_type] = _method;
        }
        else
        {
            Event_methods.Add(_type, _method);
        }
    }

    ////////// Method           //////////
    void Event_Init()
    {
        Event_methods = new Dictionary<Event_TYPE, Event_Method>();
    }

    ////////// Unity Method     //////////
}

#endregion