using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using BackEnd;

public class ChartTable<T>
{
    protected T table;

    public T GetTable()
    {
        return table;
    }
}

public partial class BackEndManager
{
    private ChartManager chartManager;

    public void GetChartList(Action _action = null)
    {
        if(chartManager == null)
            chartManager = ChartManager.Instance;

        chartManager.Initialized();

        Backend.Chart.GetChartList((callback)=> 
        {
            if (callback.IsSuccess() == false)
            {
                Debug.LogError("에러가 발생했습니다 : " + callback.ToString());
                return;
            }

            JsonData json = callback.FlattenRows();

            List<ChartData> testList = new List<ChartData>();
            for (int i = 0; i < json.Count; i++)
			{
                ChartData chartData = new ChartData(json[i]);
                testList.Add(chartData);
            }

			for (int i = 0; i < testList.Count; i++)
                GetLoadChartData(testList[i].chartName, testList[i].selectedChartFileId.ToString(), chartManager.LoadChartData);
        });
    }

    public void GetLoadChartData(string _chartName, string _chartID, Action<string,JsonData> _actionCB)
    {
        Backend.Chart.GetChartContents(_chartID, callback =>
        {
            if (callback.IsSuccess())
                _actionCB.Invoke(_chartName, callback.FlattenRows());
            else
                Debug.LogError(callback.GetMessage());
        });
    }
}

public class ChartData
{
    public bool isChartUpload = true; //차트가 적용되어있는지(returnValue에는 없는 값)
    public string chartName; // 차트이름
    public string chartExplain; // 차트 설명
    public int selectedChartFileId;// 차트 파일 아이디
    public string old; // 신규버전인지

    public ChartData(JsonData _data)
    {
        chartName = _data["chartName"].ToString();
        chartExplain = _data["chartExplain"].ToString();
        int outNum = 0;

        if (Int32.TryParse(_data["selectedChartFileId"].ToString(), out outNum))
        {
            isChartUpload = true;
            selectedChartFileId = outNum;
        }
        else
        {
            isChartUpload = false;
            selectedChartFileId = 0;
        }

        old = _data["old"].ToString();
    }

    public override string ToString()
    {
        return $"chartName: {chartName}\n" +
        $"chartExplain: {chartExplain}\n" +
        $"isChartUpload: {isChartUpload}\n" +
        $"selectedChartFileId: {selectedChartFileId}\n" +
        $"old: {old}\n";
    }
}