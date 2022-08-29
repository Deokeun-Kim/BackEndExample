using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChartListEnum
{
    NONE,
    CharacterBalanceChart,
    ItemChart,
}

public class ChartManager : MonoBehaviour
{
    #region SINGLETON
    public static ChartManager chartManager = null;
    public static ChartManager Instance
    {
        get
        {
            if (chartManager == null)
                chartManager = FindObjectOfType<ChartManager>();
            return chartManager;
        }
    }
    #endregion SINGLETON

    public void Initialized()
    {
        itemDataTable = new ItemChart();

    }

    public void LoadChartData(string _chart, LitJson.JsonData _json)
    {
		switch (_chart.EnumParse<ChartListEnum>())
		{
			case ChartListEnum.ItemChart:
                itemDataTable.GetChartTableData(_json);
                break;
			default:
				break;
		}
	}


    public ItemChart itemDataTable { get; private set; }
}