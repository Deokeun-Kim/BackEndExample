/* * * * *
 *
 * Auto generated file.
 * Do not edit manually.
 *
 * * * * */

using System.Collections.Generic;


public class MissionTable
{   
    public Dictionary<int, Param> param = new Dictionary<int, Param> ();

    [System.Serializable]
    public class Param
    {
        
      public int index;
      public string mission_type;
      public string missionplay_type;
      public string mission_tag;
      public int goal_value;
      public string reward_id;
      public int bp_point;
      public string mission_name;
      public string mission_info;
      public int priority;
      public int navi_page;
      public bool display;
    }
}