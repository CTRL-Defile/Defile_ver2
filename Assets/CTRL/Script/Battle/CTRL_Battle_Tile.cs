using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTRL_Battle_Tile : MonoBehaviour
{
    [SerializeField] int Basic_x;
    [SerializeField] int Basic_y;

    [SerializeField] CTRL_Unit Basic_unit;

    ////////// Getter & Setter  //////////
    public int CTRL_Basic_x { get { return Basic_x; }   }
    public int CTRL_Basic_y { get { return Basic_y; }   }

    public CTRL_Unit CTRL_Basic_unit    { get { return Basic_unit;  }   set { Basic_unit = value;   }   }

    ////////// Method           //////////
    public void CTRL_Basic_Setting(int _x, int _y)
    {
        Basic_x = _x;
        Basic_y = _y;
    }

    ////////// Unity Method     //////////
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
