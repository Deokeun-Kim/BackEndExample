/* * * * *
 *
 * Auto generated file.
 * Do not edit manually.
 *
 * * * * */

using UnityEngine;
using LitJson;
using System.IO;

public static class MissionTable_Parser {
	public static MissionTable Parsing(JsonData json = null)
	{
		MissionTable xlsTbAsset = null;
		int i = 1;
		int id = 0;
		try
		{
			xlsTbAsset = new MissionTable();

			JsonData node = json;

			for (; i < node.Count; i++)
			{
				MissionTable.Param p = new MissionTable.Param ();

				if(string.IsNullOrEmpty(node[i]["index"].ToString()) == true)
                    continue;

				
            p.index = int.Parse ( node [i] ["index"].ToString() );
            p.mission_type = node [i] ["mission_type"].ToString();
            p.missionplay_type = node [i] ["missionplay_type"].ToString();
            p.mission_tag = node [i] ["mission_tag"].ToString();
            p.goal_value = int.Parse ( node [i] ["goal_value"].ToString() );
            p.reward_id = node [i] ["reward_id"].ToString();
            p.bp_point = int.Parse ( node [i] ["bp_point"].ToString() );
            p.mission_name = node [i] ["mission_name"].ToString();
            p.mission_info = node [i] ["mission_info"].ToString();
            p.priority = int.Parse ( node [i] ["priority"].ToString() );
            p.navi_page = int.Parse ( node [i] ["navi_page"].ToString() );
            p.display = bool.Parse ( node [i] ["display"].ToString() );

				id = p.index;
				xlsTbAsset.param.Add (p.index, p);
			}
		}
		catch(System.Exception ex)
		{
			string error = "MissionTable parsing error.";
			Debug.LogErrorFormat("{0} <color=red>id : {1}    line : {2}</color>", error, id, i);
			Debug.LogException(ex);
		}
        return xlsTbAsset;
	}
}