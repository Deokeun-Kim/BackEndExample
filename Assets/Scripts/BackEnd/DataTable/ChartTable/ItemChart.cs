/* * * * *
 *
 * Auto generated file.
 * Do not edit manually.
 *
 * * * * */

using System.Collections.Generic;

public class ItemChart : ChartTable<ItemTable>
{   
    public ItemChart()
    {
        table = null;
    }

    public void GetChartTableData(LitJson.JsonData _json)
    {
        table = ItemTable_Parser.Parsing(_json);
    }
}