using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
using System.IO;

public partial class CTRL_DataBase : MonoBehaviour
{

    ////////// Getter & Setter  //////////

    ////////// Method           //////////

    ////////// Unity Method     //////////
    // Start is called before the first frame update
    void Start()
    {
        CTRL_Unit_Start();
        CTRL_Skill_Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

partial class CTRL_DataBase
{
    // 유닛의 데이터
    [System.Serializable]
    public class Unit_Data : System.IDisposable
    {
        [SerializeField] string Basic_name;

        [SerializeField] List<Unit_DataLevel> Level_datas;

        ////////// Getter & Setter  //////////
        public string CTRL_Basic_name   { get { return Basic_name;  }   }

        ////////// Method           //////////
        public Unit_Data(Dictionary<string, string> _data)
        {
            Basic_name = _data["Name"].Split('_')[0];
            CTRL_Basic_AddDatas(_data);
        }

        public void CTRL_Basic_AddDatas(Dictionary<string, string> _data)
        {
            if(Level_datas == null)
            {
                Level_datas = new List<Unit_DataLevel>();
            }
            Level_datas.Add(new Unit_DataLevel(_data));
        }

        ////////// Unity Method     //////////
        public void Dispose()
        {

        }
    }

    // 레벨당 할당되는 유닛의 정보
    [System.Serializable]
    public class Unit_DataLevel : System.IDisposable
    {
        [SerializeField] string Basic_class;

        [SerializeField] CTRL_Unit.Info Status_min; // 레벨업 당 받을 수 있는 스탯의 최소치
        [SerializeField] CTRL_Unit.Info Status_max; // 레벨업 당 받을 수 있는 스탯의 최대치(-1일 경우, 최소치가 적용)

        ////////// Getter & Setter  //////////

        ////////// Method           //////////
        public Unit_DataLevel(Dictionary<string, string> _data)
        {
            Status_min = new CTRL_Unit.Info(int.Parse(_data["Attack_Damage_Min"]), float.Parse(_data["Attack_Speed_Min"]), int.Parse(_data["Armor_Min"]), int.Parse(_data["Hp"]), int.Parse(_data["Resilience"]), int.Parse(_data["Evasion_Min"]), float.Parse(_data["Critical_Percent_Min"]), int.Parse(_data["CriDamage_Min"]), _data["Skill"]   );
            Status_max = new CTRL_Unit.Info(int.Parse(_data["Attack_Damage_Max"]), float.Parse(_data["Attack_Speed_Max"]), int.Parse(_data["Armor_Max"]), -1,                     -1,                             int.Parse(_data["Evasion_Max"]), float.Parse(_data["Critical_Percent_Max"]), int.Parse(_data["CriDamage_Max"]), ""               );
        }

        ////////// Unity Method     //////////
        public void Dispose()
        {

        }
    }

    [Header("UNIT ==================================================")]
    [SerializeField] TextAsset          Unit_file;
    [SerializeField] List<Unit_Data>    Unit_datas;

    ////////// Getter & Setter  //////////

    ////////// Method           //////////

    ////////// Unity Method     //////////
    void CTRL_Unit_Start()
    {
        List<Dictionary<string, string>> csvData = CTRL_File_CSVLoad(Unit_file);
        for(int csvDataI = 0; csvDataI < csvData.Count; csvDataI++)
        {
            string csvName = csvData[csvDataI]["Name"];

            // 기존에 데이터가 있는지 찾는다.
            bool isIn = false;
            for (int datasI = 0; datasI < Unit_datas.Count; datasI++)
            {
                if(Unit_datas[datasI].CTRL_Basic_name.Equals(csvName.Split('_')[0]))
                {
                    Unit_datas[datasI].CTRL_Basic_AddDatas(csvData[csvDataI]);
                    isIn = true;
                    break;
                }
            }

            // 데이터가 없다면 생성한다.
            if(!isIn)
            {
                Unit_datas.Add(new Unit_Data(csvData[csvDataI]));
            }
        }
    }
}

partial class CTRL_DataBase
{
    [System.Serializable]
    public class Skill_Data : System.IDisposable
    {
        [SerializeField] string Basic_id;   // 스킬 아이디
        [SerializeField] string Basic_name; // 스킬 이름

        [SerializeField] float Basic_range;                         // 스킬 사거리
        [SerializeField] CTRL_Unit.Active_CAMP  Basic_targetCamp;   // 스킬 목표의 진영
        public enum PRIORITY
        {
            NONE,
            LESS_HP
        }
        [SerializeField] PRIORITY   Basic_priority;                      // 스킬 우선순위

        // AOE
        public enum AOE_CENTER
        {
            TARGET,
            SELF
        }

        public enum AOE_FORM
        {
            NONE,
            LINE,
            CIRCLE
        }

        [SerializeField] AOE_CENTER     Aoe_center; // 범위중심?
        [SerializeField] AOE_FORM       Aoe_form;   // 범위형태
        [SerializeField] List<int>      Aoe_values; // 0 : 범위, 1 : 폭

        //
        public enum RATIO__STATUS_TYPE
        {
            ATTACK_DAMAGE,
            RESILIENCE
        }

        [SerializeField] RATIO__STATUS_TYPE Ratio_statusType;
        [SerializeField] float              Ratio_value;        // 계수

        ////////// Getter & Setter  //////////
        public string   CTRL_Skill_id       { get { return Basic_id;    }   }

        public string   CTRL_Basic_name     { get { return Basic_name;  }   }

        //
        public float                    CTRL_Basic_range        { get { return Basic_range;         }   }

        public CTRL_Unit.Active_CAMP    CTRL_Basic_targetCamp   { get { return Basic_targetCamp;    }   }

        public PRIORITY                 CTRL_Basic_priority     { get { return Basic_priority;      }   }

        //
        public AOE_CENTER   CTRL_Aoe_center { get { return Aoe_center;  }   }

        public AOE_FORM     CTRL_Aoe_form   { get { return Aoe_form;    }   }

        //public string CTRL_Skill_id { get { return Basic_id;    }   }

        //
        public RATIO__STATUS_TYPE   CTRL_Ratio_statusType   { get { return Ratio_statusType;    }   }

        public float                CTRL_Ratio_value        { get { return Ratio_value;         }   }

        ////////// Method           //////////
        public Skill_Data(Dictionary<string, string> _data)
        {
            Basic_id    = _data["Id"];
            Basic_name  = _data["Name"];

            Basic_range         = float.Parse(_data["Range"]);
            Basic_targetCamp    = (CTRL_Unit.Active_CAMP)System.Enum.Parse( typeof(CTRL_Unit.Active_CAMP),  _data["Camp"]       );
            Basic_priority      = (PRIORITY             )System.Enum.Parse( typeof(PRIORITY),               _data["Priority"]   );

            Aoe_center  = (AOE_CENTER   )System.Enum.Parse( typeof(AOE_CENTER), _data["AOE_Center"] );
            Aoe_form    = (AOE_FORM     )System.Enum.Parse( typeof(AOE_FORM),   _data["AOE_Form"]   );
            // 범위 사거리 및 폭 입력.
            // 사거리가 -1이라면 입력하지 않는다.
            int distance = int.Parse(_data["AOE_Range"]);
            if (distance != -1)
            {
                if(Aoe_values == null)
                {
                    Aoe_values = new List<int>();
                }
                Aoe_values.Add(distance);

                //
                int width = int.Parse(_data["AOE_Width"]);
                if(width != -1)
                {
                    Aoe_values.Add(width);
                }
            }

            Ratio_statusType = (RATIO__STATUS_TYPE)System.Enum.Parse(typeof(RATIO__STATUS_TYPE), _data["Status"]);
            Ratio_value = float.Parse(_data["Value"]);
        }

        ////////// Unity Method     //////////
        public void Dispose()
        {

        }
    }

    [Header("SKILL ==================================================")]
    [SerializeField] TextAsset Skill_file;
    [SerializeField] List<Skill_Data> Skill_datas;

    ////////// Getter & Setter  //////////
    object CTRL_Skill_GetSkill(params object[] _args)
    {
        string id = (string)_args[0];

        //
        Skill_Data res = null;

        for (int i = 0; i < Skill_datas.Count; i++)
        {
            if(Skill_datas[i].CTRL_Skill_id.Equals(id))
            {
                res = Skill_datas[i];

                break;
            }
        }

        return res;
    }


    ////////// Method           //////////

    ////////// Unity Method     //////////
    void CTRL_Skill_Start()
    {
        List<Dictionary<string, string>> csvData = CTRL_File_CSVLoad(Skill_file);
        for(int i = 0; i < csvData.Count; i++)
        {
            Skill_datas.Add(new Skill_Data(csvData[i]));
        }

        //
        CTRL_ScriptBridge.CTRL_Basic_instance.CTRL_Event_SetEvent( CTRL_ScriptBridge.Event_TYPE.DATABASE__SKILL__GET_SKILL, CTRL_Skill_GetSkill );
    }
}

partial class CTRL_DataBase
{

    ////////// Getter & Setter  //////////

    ////////// Method           //////////
    List<Dictionary<string, string>> CTRL_File_CSVLoad(TextAsset _file)
    {
        List<Dictionary<string, string>> res = new List<Dictionary<string, string>>();

        // 데이터 추가
        // 0번은 키값
        string[] strs = _file.text.Replace("\r", "").Split('\n');
        string[] strsKey = strs[0].Split(',');
        for (int i = 1; i < strs.Length; i++)
        {
            Dictionary<string, string> element = new Dictionary<string, string>();

            //
            string[] strsElement = strs[i].Split(',');
            for(int j = 0; j < strsKey.Length; j++)
            {
                element.Add(strsKey[j], strsElement[j]);
            }

            //
            res.Add(element);
        }

        return res;
    }

    ////////// Unity Method     //////////
}