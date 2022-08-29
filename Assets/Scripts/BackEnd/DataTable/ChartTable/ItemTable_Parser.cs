/* * * * *
 *
 * Auto generated file.
 * Do not edit manually.
 *
 * * * * */

using UnityEngine;
using LitJson;
using System.IO;

public static class ItemTable_Parser {
	public static ItemTable Parsing(JsonData json = null)
	{
		ItemTable xlsTbAsset = null;
		int i = 1;
		int id = 0;
		try
		{
			xlsTbAsset = new ItemTable();

			JsonData node = json;

			for (; i < node.Count; i++)
			{
				ItemTable.Param p = new ItemTable.Param ();

				if(string.IsNullOrEmpty(node[i]["index"].ToString()) == true)
                    continue;

				
            p.index = int.Parse ( node [i] ["index"].ToString() );
            p.item_name = node [i] ["item_name"].ToString();
            p.item_info = node [i] ["item_info"].ToString();
            p.icon_img = node [i] ["icon_img"].ToString();

				id = p.index;
				xlsTbAsset.param.Add (p.index, p);
			}
		}
		catch(System.Exception ex)
		{
			string error = "ItemTable parsing error.";
			Debug.LogErrorFormat("{0} <color=red>id : {1}    line : {2}</color>", error, id, i);
			Debug.LogException(ex);
		}
        return xlsTbAsset;
	}
}