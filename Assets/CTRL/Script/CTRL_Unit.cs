using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CTRL_Unit : MonoBehaviour
{
    enum Basic__UPDATE_PHASE
    {
        INFO_INIT,
        ACTIVE_INIT,
        UI_INIT,
        UPDATE
    }
    [SerializeField] Basic__UPDATE_PHASE Basic_updatePhase;

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
            case Basic__UPDATE_PHASE.INFO_INIT:     { if ( CTRL_Info_Init()     ) { Basic_updatePhase = Basic__UPDATE_PHASE.ACTIVE_INIT;    Update();   }   }   break;
            case Basic__UPDATE_PHASE.ACTIVE_INIT:   { if ( CTRL_Active_Init()   ) { Basic_updatePhase = Basic__UPDATE_PHASE.UI_INIT;        Update();   }   }   break;
            case Basic__UPDATE_PHASE.UI_INIT:       { if ( CTRL_UI_Init()       ) { Basic_updatePhase = Basic__UPDATE_PHASE.UPDATE;         Update();   }   }   break;
            //
            case Basic__UPDATE_PHASE.UPDATE:
                {
                    CTRL_Active_Update();
                    CTRL_UI_Update();
                }
                break;
        }
    }
}

#region STATUS

partial class CTRL_Unit
{
    [System.Serializable]
    public class Info : System.IDisposable
    {
        // 능력치
        [SerializeField] int    Status_atkDamage;   // 공격력
        [SerializeField] float  Status_atkSpeed;    // 공격속도
        [SerializeField] int    Status_armor;       // 방어력
        [SerializeField] int    Status_hp;          // 체력
        [SerializeField] int    Status_resilience;  // 회복력
        [SerializeField] int    Status_evasion;     // 회피
        [SerializeField] float  Status_crtPer;      // 치명타 확률
        [SerializeField] int    Status_crtDamage;   // 치명타 피해
        [SerializeField] string Status_skill;       // 사용하는 스킬

        ////////// Getter & Setter  //////////
        public int      CTRL_Status_atkDamage   { get { return Status_atkDamage;    }                                   }

        public float    CTRL_Status_atkSpeed    { get { return Status_atkSpeed;     }                                   }

        public int      CTRL_Status_armor       { get { return Status_armor;        }                                   }

        public int      CTRL_Status_hp          { get { return Status_hp;           }   set { Status_hp = value;    }   }

        public int      CTRL_Status_resilience  { get { return Status_resilience;   }                                   }
        
        public int      CTRL_Status_evasion     { get { return Status_evasion;      }                                   }

        public float    CTRL_Status_crtPer      { get { return Status_crtPer;       }                                   }

        public int      CTRL_Status_crtDamage   { get { return Status_crtDamage;    }                                   }

        public string   CTRL_Status_skill       { get { return Status_skill;        }                                   }

        ////////// Method           //////////
        public Info(
            //
            int _atkDamage, float _atkSpeed, int _armor, int _hp, int _resilience, int _evasion, float _crtPer, int _crtDamage, string _skill)
        {
            Status_atkDamage    = _atkDamage;
            Status_atkSpeed     = _atkSpeed;
            Status_armor        = _armor;
            Status_hp           = _hp;
            Status_resilience   = _resilience;
            Status_evasion      = _evasion;
            Status_crtPer       = _crtPer;
            Status_crtDamage    = _crtDamage;
            Status_skill        = _skill;
        }

        ////////// Unity Method     //////////
        public void Dispose()
        {

        }
    }

    [Header("INFO ==================================================")]
    [SerializeField] Info Info_data;
    [SerializeField] CTRL_DataBase.Skill_Data Info_skill;

    ////////// Getter & Setter  //////////
    public Info CTRL_Info_data  { get { return Info_data;   }   }

    ////////// Method           //////////

    ////////// Unity Method     //////////
    bool CTRL_Info_Init()
    {
        bool res = true;

        //
        Info_skill = (CTRL_DataBase.Skill_Data)CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_GetEvent(   CTRL_ScriptBridge.Event_TYPE.DATABASE__SKILL__GET_SKILL, Info_data.CTRL_Status_skill);

        //
        return res;
    }
}

#endregion


#region ACTIVE

partial class CTRL_Unit
{
    public enum Active_CAMP
    {
        PLAYER,
        ENEMY
    }

    enum Active_PHASE
    {
        NONE,

        // COMMAND
        COMMAND,

        // MOVE
        MOVE_START = 10000,
        MOVE,

        // ATTACK
        ATTACK_START = 20000,
        ATTACK,
        ATTACK_END,

        //
        DEAD_START = 30000
    }

    public enum Active_ASTAR_TYPE
    {
        RANGE,
        MOVE
    }

    [Header("ACTIVE ==================================================")]
    [SerializeField] Transform Active_models;
    [SerializeField] Animator Active_animator;

    [Header("RUNNING")]
    [SerializeField] Active_CAMP Active_camp;
    [SerializeField] Active_PHASE Active_phase;
    [SerializeField] Active_PHASE Active_prePhase;
    //AStar
    [SerializeField] Active_ASTAR_TYPE  Active_AStarType;
    [SerializeField] List<int>          Active_AStarDatas;   // 계산된 데이터
    [SerializeField] List<int>          Active_AStarCalc;    // 계산해야할 데이터
    // 행동 관련 데이터
    [SerializeField] Vector3    Active_originPos;   // 유닛의 원위치
    [SerializeField] Transform  Active_targetObj;   // 유닛의 목표
    [SerializeField] float      Active_time;
    static float Active_moveSpeed = 2.0f;

    // CTRL_Battle_Manager에서 가져오는 데이터
    List<CTRL_Battle_Tile>  Active_battleTiles;
    int                     Active_battleTileX;
    int                     Active_battleTileY;

    ////////// Getter & Setter  //////////
    public Active_CAMP CTRL_Active_camp { get { return Active_camp; }   }

    public int CTRL_Active_GetAStarData(int _num) { return Active_AStarDatas[_num]; }

    ////////// Method           //////////
    public void CTRL_Active_Damage(float _damage)
    {
        float armor = 100.0f / (100.0f + Info_data.CTRL_Status_armor);
        //
        float damage = _damage * armor;

        Info_data.CTRL_Status_hp = Info_data.CTRL_Status_hp - (int)damage;

        if(Info_data.CTRL_Status_hp <= 0)
        {
            Info_data.CTRL_Status_hp = 0;
            Active_phase = Active_PHASE.DEAD_START;
            CTRL_Active_SettingMotion();

            // 타일에서 유닛을 해지
            CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_GetEvent(
                CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__UNIT_DATA_UPDATE,
                //
                this, -1);
        }
    }

    void CTRL_Active_SettingMotion()
    {
        int motion = ((int)Active_phase) / 10000;
        if (motion != Active_animator.GetInteger("motion"))
        {
            Active_animator.SetInteger("motion", motion);
            Active_animator.SetTrigger("isChange");
        }
    }

    ////////// Unity Method     //////////

    bool CTRL_Active_Init()
    {
        bool res = false;

        //
        if((Active_AStarDatas == null) || (Active_AStarDatas.Count == 0))
        {
            object tilesObj = CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_GetEvent(CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__GET_TILES);
            if(tilesObj != null)
            {
                Active_battleTiles = (List<CTRL_Battle_Tile>)tilesObj;
                Active_battleTileX = (int)CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_GetEvent(CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__GET_X);
                Active_battleTileY = (int)CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_GetEvent(CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__GET_Y);

                Active_AStarDatas = new List<int>();
                Active_AStarCalc = new List<int>();
                while (Active_AStarDatas.Count < Active_battleTiles.Count)
                {
                    Active_AStarDatas.Add(10000);
                }

                res = true;
            }
        }
        else
        {
            res = true;
        }

        //
        return res;
    }


    //////////
    // Update
    void CTRL_Active_Update()
    {
        switch(Active_phase)
        {
            case Active_PHASE.COMMAND:      { CTRL_Active_Update__CAMMAND();    }   break;
            //
            case Active_PHASE.MOVE_START:   { CTRL_Active_Update__MOVE_START(); }   break;
            case Active_PHASE.MOVE:         { CTRL_Active_Update__MOVE();       }   break;
            //
            case Active_PHASE.ATTACK_START: { CTRL_Active_Update__ATTACK_START();   }   break;
            case Active_PHASE.ATTACK:       { CTRL_Active_Update__ATTACK();         }   break;
            //
            case Active_PHASE.DEAD_START: { }break;
        }
    }

    // COMMAND
    #region COMMAND

    void CTRL_Active_Update__CAMMAND()
    {
        Active_originPos = this.transform.position;

        // 목표 설정
        CTRL_Active_Update__CAMMAND__AStar(Active_ASTAR_TYPE.RANGE);
        CTRL_Active_Update__CAMMAND__Targeting();
    }

    // AStar
    public void CTRL_Active_Update__CAMMAND__AStar(Active_ASTAR_TYPE _AStar_type)
    {
        // 초기화
        for (int i = 0; i < Active_AStarDatas.Count; i++)
        {
            Active_AStarDatas[i] = -1;
        }
        Active_AStarCalc.Clear();

        // 판별
        // 자신의 위치를 찾는다.
        for (int i = 0; i < Active_battleTiles.Count; i++)
        {
            if (Active_battleTiles[i].CTRL_Basic_unit != null)
            {
                if (Active_battleTiles[i].CTRL_Basic_unit.Equals(this))
                {
                    Active_AStarDatas[i] = 0;
                    Active_AStarCalc.Add(i);
                    break;
                }
            }
        }

        // 계산 시작
        while (Active_AStarCalc.Count > 0)
        {
            int distance = Active_AStarDatas[Active_AStarCalc[0]] + 1;

            // 좌우
            CTRL_Active_Update__CAMMAND__AStar_Distance(Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_x - 1 >= 0,                  Active_AStarCalc[0] - 1, distance, _AStar_type);
            CTRL_Active_Update__CAMMAND__AStar_Distance(Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_x + 1 < Active_battleTileX,  Active_AStarCalc[0] + 1, distance, _AStar_type);

            int xMinus = Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_y % 2 - 1;

            // 위
            if (Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_y + 1 < Active_battleTileY)
            {
                CTRL_Active_Update__CAMMAND__AStar_Distance(
                    Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_x + xMinus >= 0,
                    Active_AStarCalc[0] + Active_battleTileX + xMinus,
                    distance,
                    _AStar_type);
                CTRL_Active_Update__CAMMAND__AStar_Distance(
                    Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_x + xMinus + 1 < Active_battleTileX,
                    Active_AStarCalc[0] + Active_battleTileX + xMinus + 1,
                    distance,
                    _AStar_type);
            }

            // 아래
            if (Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_y - 1 >= 0)
            {
                CTRL_Active_Update__CAMMAND__AStar_Distance(
                    Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_x + xMinus > 0,
                    Active_AStarCalc[0] - Active_battleTileX + xMinus,
                    distance,
                    _AStar_type);
                CTRL_Active_Update__CAMMAND__AStar_Distance(
                    Active_battleTiles[Active_AStarCalc[0]].CTRL_Basic_x + xMinus + 1 < Active_battleTileX,
                    Active_AStarCalc[0] - Active_battleTileX + xMinus + 1,
                    distance,
                    _AStar_type);
            }

            Active_AStarCalc.RemoveAt(0);
        }
    }

    void CTRL_Active_Update__CAMMAND__AStar_Distance(bool _isRange, int _target, int _distance, Active_ASTAR_TYPE _AStar_type)
    {
        if (_isRange)
        {
            if ((Active_AStarDatas[_target] == -1) || (Active_AStarDatas[_target] > _distance))
            {
                Active_AStarDatas[_target] = _distance;

                //
                bool isNext = true;
                switch(_AStar_type)
                {
                    case Active_ASTAR_TYPE.MOVE:    { isNext = (Active_battleTiles[_target].CTRL_Basic_unit == null);   }   break;
                }

                if (isNext)
                {
                    Active_AStarCalc.Add(_target);
                }
            }
        }
    }

    // 목표 설정
    void CTRL_Active_Update__CAMMAND__Targeting()
    {
        // 초기화
        Active_targetObj = null;
        int targetDistance = 10000;

        // 가장 가까운 목표 찾기
        for (int i = 0; i < Active_AStarDatas.Count; i++)
        {
            if ((Active_battleTiles[i].CTRL_Basic_unit != null) && (Active_AStarDatas[i] != -1))
            {
                Active_CAMP targetCamp = Info_skill.CTRL_Basic_targetCamp;
                if(CTRL_Active_camp == Active_CAMP.ENEMY)
                {
                    switch (targetCamp)
                    {
                        case Active_CAMP.PLAYER:    { targetCamp = Active_CAMP.ENEMY;   }   break;
                        case Active_CAMP.ENEMY:     { targetCamp = Active_CAMP.PLAYER;  }   break;
                    }
                }

                //
                if (Active_battleTiles[i].CTRL_Basic_unit.CTRL_Active_camp == targetCamp)
                {
                    if (Active_targetObj == null)
                    {
                        CTRL_Active_Update__CAMMAND__Targeting_Set_Target(i);
                        targetDistance = Active_AStarDatas[i];
                    }
                    else
                    {
                        if (targetDistance > Active_AStarDatas[i])
                        {
                            CTRL_Active_Update__CAMMAND__Targeting_Set_Target(i);
                            targetDistance = Active_AStarDatas[i];
                        }
                    }
                }
            }
        }

        // 가장 가까운 적이 유닛의 사거리에 들어가 있는지 체크
        // TODO: 현재는 사거리에 관련된 기획이 없으므로 1로 통일
        if(targetDistance <= Info_skill.CTRL_Basic_range)
        {
            Active_phase = Active_PHASE.ATTACK_START;
        }
        else
        {
            Active_phase = Active_PHASE.MOVE_START;
        }

        //
        if (Active_phase == Active_PHASE.COMMAND)
        {
            Active_animator.SetInteger("motion", 0);
            Active_animator.SetTrigger("isChange");
        }
        Active_prePhase = Active_phase;
    }

    int CTRL_Active_Update__CAMMAND__Targeting_Set_Target(int _num)
    {
        Active_targetObj = Active_battleTiles[_num].CTRL_Basic_unit.transform;
        return Active_AStarDatas[_num];
    }

    #endregion


    // MOVE
    #region MOVE

    // MOVE_START
    void CTRL_Active_Update__MOVE_START()
    {
        // 유닛의 에이스타를 이동용으로 계산한다.
        CTRL_Active_Update__CAMMAND__AStar(Active_ASTAR_TYPE.MOVE);
        // 목표의 에이스타를 계산시킨다.
        Active_targetObj.GetComponent<CTRL_Unit>().CTRL_Active_Update__CAMMAND__AStar(Active_ASTAR_TYPE.MOVE);

        // 유닛의 에이스타에서 1인 값을 지닌 타일 들 중 상대에게서 가장 작은 수의 타일을 선택한다.
        int targetTile = -1;
        int targetDistance = 10000;
        for (int i = 0; i < Active_AStarDatas.Count; i++)
        {
            if(Active_AStarDatas[i] == 1)
            {
                int enemyDistance = Active_targetObj.GetComponent<CTRL_Unit>().CTRL_Active_GetAStarData(i);
                if (    (enemyDistance !=   -1              ) &&
                        (enemyDistance <    targetDistance  ))
                {
                    targetTile = i;
                    targetDistance = Active_targetObj.GetComponent<CTRL_Unit>().CTRL_Active_GetAStarData(i);
                }
            }
        }

        // 이동할 타일로 목표를 갱신한다.
        if(targetTile != -1)
        {
            // 목표를 타일로 갱신한다.
            Active_targetObj = Active_battleTiles[targetTile].transform;

            // 배틀 매니저에 타일 정보를 업데이트. 타일에 위치한 오브젝트의 위치 셋업.
            CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_GetEvent(
                CTRL_ScriptBridge.Event_TYPE.BATTLE__TILE__UNIT_DATA_UPDATE,
                //
                this, targetTile);

            // 유닛이 이동할 방향을 바라보도록 셋팅.
            Active_models.LookAt(Active_targetObj);
            Active_models.rotation = Quaternion.Euler(0.0f, Active_models.rotation.eulerAngles.y, 0.0f);

            Active_phase = Active_PHASE.MOVE;

            CTRL_Active_SettingMotion();
        }
        // 이동할 타일을 찾지 못한다면 명령으로 이동.
        else
        {
            Active_phase = Active_prePhase = Active_PHASE.COMMAND;
        }
    }

    // MOVE
    void CTRL_Active_Update__MOVE()
    {
        Active_time += Time.deltaTime * Active_moveSpeed;

        if (Active_time >= 1.0f)
        {
            Active_time -= 1.0f;
            this.transform.position = Vector3.Lerp(Active_originPos, Active_targetObj.position, 1.0f);

            Active_phase = Active_PHASE.COMMAND;
        }
        else
        {
            this.transform.position = Vector3.Lerp(Active_originPos, Active_targetObj.position, Active_time);
        }
    }

    #endregion

    // ATTACK
    #region ATTACK

    // ATTACK_START
    void CTRL_Active_Update__ATTACK_START()
    {
        // 공격 관련 변수 초기화 및 공격실행
        Active_phase = Active_PHASE.ATTACK;
        CTRL_Active_SettingMotion();

        // 유닛이 행동할 방향을 바라보도록 셋팅.
        Active_models.LookAt(Active_targetObj);
        Active_models.rotation = Quaternion.Euler(0.0f, Active_models.rotation.eulerAngles.y, 0.0f);

        //
        CTRL_Active_Update();
    }

    // ATTACK
    void CTRL_Active_Update__ATTACK()
    {
        if (Active_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            //Debug.Log(Active_animator.GetCurrentAnimatorStateInfo(0).length);
            string[] clipNames = Active_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Split('_');

            if (clipNames[0].Equals("ATTACK"))
            {
                switch (clipNames[1])
                {
                    case "END":
                        {
                            Active_prePhase = Active_PHASE.COMMAND;
                            Active_phase = Active_PHASE.COMMAND;
                        }
                        break;
                    // 타격을 실행한다.
                    default:
                        {
                            switch (Info_skill.CTRL_Basic_targetCamp)
                            {
                                case Active_CAMP.PLAYER: { } break;
                                case Active_CAMP.ENEMY:
                                    {
                                        float ctrlDamage = 1.0f + ((float)((Random.Range(0.0f, 100.0f) < Info_data.CTRL_Status_crtPer) ? Info_data.CTRL_Status_crtDamage : 0) / 100.0f);
                                        switch (Info_skill.CTRL_Aoe_form)
                                        {
                                            case CTRL_DataBase.Skill_Data.AOE_FORM.NONE:    { CTRL_Active_Update__ATTACK__EnemyStrike(      Active_targetObj,   ctrlDamage);    }   break;
                                            case CTRL_DataBase.Skill_Data.AOE_FORM.CIRCLE:  { CTRL_Active_Update__ATTACK__EnemyAoeCircle();                 }   break;
                                        }
                                    }
                                    break;
                            }

                            Active_animator.SetTrigger("next");
                        }
                        break;
                }
            }
        }
    }

    void CTRL_Active_Update__ATTACK__EnemyStrike(Transform _enemy, float _ctrlDamage)
    {
        // 회피를 계산한다.
        CTRL_Unit enemy = _enemy.GetComponent<CTRL_Unit>();
        // 공격에 성공했을 때
        if (enemy.Info_data.CTRL_Status_evasion < Random.Range(0, 100 + enemy.Info_data.CTRL_Status_evasion))
        {
            float ratioValue = 0.0f;
            switch(Info_skill.CTRL_Ratio_statusType)
            {
                case CTRL_DataBase.Skill_Data.RATIO__STATUS_TYPE.ATTACK_DAMAGE: { ratioValue = Info_data.CTRL_Status_atkDamage;     }   break;
                case CTRL_DataBase.Skill_Data.RATIO__STATUS_TYPE.RESILIENCE:    { ratioValue = Info_data.CTRL_Status_resilience;    }   break;
            }
            float armor = 100.0f / (100.0f + (float)enemy.Info_data.CTRL_Status_armor);
            float totalValue = ratioValue * _ctrlDamage * armor;
            enemy.CTRL_Active_Damage(totalValue);
        }
        // 회피했을 때
        else
        {

        }
    }

    void CTRL_Active_Update__ATTACK__EnemyAoeCircle()
    {

    }
    #endregion
}

#endregion

#region UI



partial class CTRL_Unit
{
    [Header("UI ==================================================")]
    [SerializeField] TMPro.TextMeshPro  UI_tmpDamage;
    [SerializeField] float              UI_tmpDamageTime;

    [Header("RUNNING")]
    [SerializeField] Transform  UI_tmpTarget;
    [SerializeField] float      UI_tmpDamageTimer;

    ////////// Getter & Setter  //////////

    ////////// Method           //////////
    public void CTRL_UI_TmpDamagePrint(int _damage)
    {

    }

    ////////// Unity Method     //////////
    bool CTRL_UI_Init()
    {
        bool res = false;

        //
        object camera = CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_GetEvent(CTRL_ScriptBridge.Event_TYPE.MASTER__CAMERA__GET_MAIN_CAMERA);
        if (camera != null)
        {
            UI_tmpTarget = ((Camera)camera).transform;
            res = true;
        }

        //
        return res;
    }

    void CTRL_UI_Update()
    {
        UI_tmpDamage.transform.LookAt(UI_tmpTarget);

    }
}

#endregion