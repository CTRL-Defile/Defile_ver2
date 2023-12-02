using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CTRL_Battle_Manager : CTRL_Scene_Manager
{
    enum Basic_PHASE
    {
        INIT,
        UPDATE
    }

    [SerializeField] Basic_PHASE Basic_phase;

    ////////// Getter & Setter  //////////

    ////////// Method           //////////

    ////////// Unity Method     //////////
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        CTRL_Tile_Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        switch(Basic_phase)
        {
            case Basic_PHASE.INIT:
                {
                }
                break;
            case Basic_PHASE.UPDATE:
                {
                    Active_Update();
                }
                break;
        }
    }
}

#region TILE

partial class CTRL_Battle_Manager
{
    [Header("TILE ==================================================")]
    [SerializeField] Transform              Tile_parent;
    [SerializeField] List<CTRL_Battle_Tile> Tile_tiles;

    [SerializeField] int    Tile_x;
    [SerializeField] int    Tile_y;

    ////////// Getter & Setter  //////////
    object CTRL_TILE_GetTiles(params object[] _args)    { return Tile_tiles;    }

    object CTRL_TILE_GetX(params object[] _args)        { return Tile_x;        }

    object CTRL_TILE_GetY(params object[] _args)        { return Tile_y;        }

    ////////// Method           //////////
    object CTRL_TILE_UnitDataUpdate(params object[] _args)
    {
        CTRL_Unit   unit    = (CTRL_Unit)_args[0];
        int         tileNum = (int)_args[1];

        //////////
        // 유닛을 타일에서 해지
        for (int i = 0; i < Tile_tiles.Count; i++)
        {
            if(Tile_tiles[i].CTRL_Basic_unit != null)
            {
                if(Tile_tiles[i].CTRL_Basic_unit.Equals(unit))
                {
                    Tile_tiles[i].CTRL_Basic_unit = null;
                    break;
                }
            }
        }

        // 유닛을 해당 타일에 배치
        // -1일 때, 유닛을 필드에서 해지.
        if (tileNum != -1)
        {
            Tile_tiles[tileNum].CTRL_Basic_unit = unit;
        }

        //////////
        return true;
    }

    ////////// Unity Method     //////////
    void CTRL_Tile_Start()
    {
        Vector3 pos = Tile_tiles[0].transform.position;
        Vector3 posValue = Tile_tiles[1].transform.position - pos;

        while(Tile_tiles.Count < Tile_x * Tile_y)
        {
            CTRL_Battle_Tile obj = Instantiate(Tile_tiles[0], Tile_parent);
            obj.transform.localScale = Vector3.one;

            Tile_tiles.Add(obj);
        }

        for(int i = 0; i < Tile_tiles.Count; i++)
        {
            Tile_tiles[i].transform.position
                = new Vector3(
                    pos.x + (posValue.x * ((2.0f * (i % Tile_x)) + ((i / Tile_x) % 2))),
                    0,
                    pos.z + (posValue.z * (i / Tile_x)));
            Tile_tiles[i].CTRL_Basic_Setting(i % Tile_x, i / Tile_x);
        }
        
        CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_SetEvent(  CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__GET_TILES,           CTRL_TILE_GetTiles          );
        CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_SetEvent(  CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__GET_X,               CTRL_TILE_GetX              );
        CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_SetEvent(  CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__GET_Y,               CTRL_TILE_GetY              );

        CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_SetEvent(  CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__UNIT_DATA_UPDATE,    CTRL_TILE_UnitDataUpdate    );
    }
}

#endregion

#region ACTIVE

partial class CTRL_Battle_Manager
{
    enum Active_PHASE
    {
        //
        PREPARE__INIT,
        PREPARE,
        //
        BATTLE__INIT,
        BATTLE,
        BATTLE_PLUS__INIT,
        BATTLE_PLUS,
        //
        END
    }

    [Header("ACTIVE ==================================================")]
    [SerializeField] Active_PHASE Active_phase;

    [SerializeField] float Active_prepareTime;

    [SerializeField] float Active_battleTime;
    [SerializeField] float Active_battleTimePlus;

    [Header("RUNNING")]
    [SerializeField] float Active_timer;

    ////////// Getter & Setter  //////////

    ////////// Method           //////////

    ////////// Unity Method     //////////
    
    //////////
    // Update
    //
    void Active_Update()
    {
        switch(Active_phase)
        {
            case Active_PHASE.PREPARE__INIT:    {       Active_Update___PREPARE__INIT();    Active_phase = Active_PHASE.PREPARE;            }   break;
            case Active_PHASE.PREPARE:          { if (  Active_Update___PREPARE()       )   { Active_phase = Active_PHASE.BATTLE__INIT; }   }   break;
            //
            case Active_PHASE.BATTLE__INIT: {       Active_Update___BATTLE_INIT();      Active_phase = Active_PHASE.BATTLE;                     }   break;
            case Active_PHASE.BATTLE:       { if(   Active_Update___BATTLE()        )   { Active_phase = Active_PHASE.BATTLE_PLUS__INIT;    }   }   break;
                
            case Active_PHASE.BATTLE_PLUS__INIT:    {       Active_Update___BATTLE_PLUS__INIT();    Active_phase = Active_PHASE.BATTLE_PLUS;    }   break;
            case Active_PHASE.BATTLE_PLUS:          { if(   Active_Update___BATTLE_PLUS()       )   { Active_phase = Active_PHASE.END;      }   }   break;
            //
            case Active_PHASE.END:      {   }   break;
        }
    }

    // PREPARE
    void Active_Update___PREPARE__INIT()
    {
        Active_timer = 0.0f;
    }

    bool Active_Update___PREPARE()
    {
        bool res = false;

        //
        Active_timer += Time.deltaTime;
        if (Active_timer >= Active_prepareTime)
        {
            res = true;
        }

        //
        return res;
    }

    // BATTLE
    void Active_Update___BATTLE_INIT()
    {
        Active_timer = 0.0f;
    }

    bool Active_Update___BATTLE()
    {
        bool res = false;

        //
        Active_timer += Time.deltaTime;
        if (Active_timer >= Active_battleTime)
        {
            res = true;
        }

        //
        return res;
    }

    // BATTLE_PLUS
    void Active_Update___BATTLE_PLUS__INIT()
    {
        Active_timer = 0.0f;
    }

    bool Active_Update___BATTLE_PLUS()
    {
        bool res = false;

        //
        Active_timer += Time.deltaTime;
        if (Active_timer >= Active_battleTimePlus)
        {
            res = true;
        }

        //
        return res;
    }
}

#endregion