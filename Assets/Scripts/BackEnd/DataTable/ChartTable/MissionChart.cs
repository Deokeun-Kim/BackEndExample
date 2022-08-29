/* * * * *
 *
 * Auto generated file.
 * Do not edit manually.
 *
 * * * * */

using System.Collections.Generic;

public class MissionChart : ChartTable<MissionTable>
{   
    public MissionChart()
    {
        table = null;
    }

    public void GetChartTableData(LitJson.JsonData _json)
    {
        table = MissionTable_Parser.Parsing(_json);
    }
}