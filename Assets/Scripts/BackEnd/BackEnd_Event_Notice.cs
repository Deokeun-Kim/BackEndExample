using BackEnd;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BackEndManager
{
    /// <summary>
    /// 공지사항 데이터 리스트
    /// </summary>
    public List<NoticeData> noticeDataList { get; private set; } = new List<NoticeData>();

    /// <summary>
    /// 이벤트 데이터 리스트
    /// </summary>
    public List<EventData> eventDataList { get; private set; } = new List<EventData>();

    #region NOTICE_DATA_LOAD
    /// <summary>
    /// 공지사항 데이터를 모두 받아 리스트로 만들어 콜백하는 함수
    /// </summary>
    /// <param name="_action"> 결과값 처리 콜백 함수 </param>
    /// <param name="_loadCount"> 한번에 받아올 갯수 </param>
    public void GetNoticeList(Action<List<NoticeData>> _action = null, int _loadCount = 10)
    {
        noticeDataList.Clear();

        Backend.Notice.NoticeList(_loadCount, callback =>
        {
            if (callback.IsSuccess())
            {
                SettingNoticeList(callback);
                string offset = callback.LastEvaluatedKeyString();
                if (!string.IsNullOrEmpty(offset))
                    LoadLastNotice(_loadCount, offset, _action);
                else
                    _action?.Invoke(noticeDataList);
            }
        });
    }

    /// <summary>
    /// 공지사항 데이터를 리스트로 저장하는 함수.
    /// </summary>
    /// <param name="_loadCount"> 한번에 받아올 데이터 갯수 </param>
    /// <param name="_offset"> 몇번째부터 받아올건지 offset 설정 </param>
    /// <param name="_action"> 비동기 CallBack 처리 </param>
    private void LoadLastNotice(int _loadCount, string _offset , Action<List<NoticeData>> _action = null)
    {
        Backend.Notice.NoticeList(_loadCount, _offset, callback =>
        {
            if (callback.IsSuccess())
            {
                SettingNoticeList(callback);
                string offset = callback.LastEvaluatedKeyString();
                if (!string.IsNullOrEmpty(offset))
                    LoadLastNotice(_loadCount, offset, _action);
                else
                    _action?.Invoke(noticeDataList);
            }
        });
    }

    /// <summary>
    /// 뒤끝 CallBack Object를 사용해 공지사항 데이터를 리스트 객체에 저장하는 함수.
    /// </summary>
    /// <param name="_callback"></param>
    private void SettingNoticeList(BackendReturnObject _callback)
    {
		LitJson.JsonData jsonData = _callback.FlattenRows();
		for (int i = 0; i < jsonData.Count; i++)
		{
            NoticeData tempData = new NoticeData(jsonData[i]);
            noticeDataList.Add(tempData);
        }
    }
    #endregion NOTICE_DATA_LOAD


    #region EVENT_DATA_LOAD
    /// <summary>
    /// 이벤트 데이터를 모두 받아 리스트로 만들어 콜백하는 함수
    /// </summary>
    /// <param name="_action"></param>
    /// <param name="_loadCount"></param>
    public void GetEventList(Action<List<EventData>> _action = null, int _loadCount = 10)
    {
        eventDataList.Clear();

        Backend.Event.EventList(_loadCount, callback =>
        {
            if (callback.IsSuccess())
            {
                SettingEventList(callback);
                string offset = callback.LastEvaluatedKeyString();
                if (!string.IsNullOrEmpty(offset))
                    LoadLastEvent(_loadCount, offset, _action);
                else
                    _action?.Invoke(eventDataList);
            }
        });
    }

    /// <summary>
    /// 이벤트 데이터를 리스트로 저장하는 함수.
    /// </summary>
    /// <param name="_offset"></param>
    /// <param name="_action"></param>
    private void LoadLastEvent(int _loadCount, string _offset, Action<List<EventData>> _action = null)
    {
        Backend.Event.EventList(_loadCount, _offset, callback =>
        {
            if (callback.IsSuccess())
            {
                SettingEventList(callback);
                string offset = callback.LastEvaluatedKeyString();
                if (!string.IsNullOrEmpty(offset))
                    LoadLastEvent(_loadCount, offset, _action);
                else
                    _action?.Invoke(eventDataList);
            }
        });
    }

    /// <summary>
    /// 뒤끝 CallBack Object를 사용해 이벤트 데이터를 리스트 객체에 저장하는 함수.
    /// </summary>
    /// <param name="_callback"></param>
    private void SettingEventList(BackendReturnObject _callback)
    {
        LitJson.JsonData jsonData = _callback.FlattenRows();
        for (int i = 0; i < jsonData.Count; i++)
        {
            EventData tempData = new EventData(jsonData[i]);
            eventDataList.Add(tempData);
        }
    }
    #endregion EVENT_DATA_LOAD

    /// <summary>
    /// 서비스 이용약관 / 개인 정보 처리 방침 데이터 로드
    /// </summary>
    /// <param name="_resultAction"></param>
    public void GetPolicy(Action<PolicyData> _resultAction = null)
    {
        Backend.Policy.GetPolicy((callback) =>
        {
            PolicyData _policyData = new PolicyData();
            _policyData.terms = callback.GetReturnValuetoJSON()["terms"].ToString();
            _policyData.termsURL = callback.GetReturnValuetoJSON()["termsURL"].ToString();
            _policyData.privacy = callback.GetReturnValuetoJSON()["privacy"].ToString();
            _policyData.privacyURL = callback.GetReturnValuetoJSON()["privacyURL"].ToString();
            _resultAction?.Invoke(_policyData);
        });
    }
}

[Serializable]
public class NoticeData
{
    public string title;
    public string contents;
    public DateTime postingDate;
    public string imageKey;
    public string inDate;
    public string uuid;
    public string linkUrl;
    public bool isPublic;
    public string linkButtonName;
    public string author;

    public override string ToString()
    {
        return $"title : {title}\n" +
        $"contents : {contents}\n" +
        $"postingDate : {postingDate}\n" +
        $"imageKey : {imageKey}\n" +
        $"inDate : {inDate}\n" +
        $"uuid : {uuid}\n" +
        $"linkUrl : {linkUrl}\n" +
        $"isPublic : {isPublic}\n" +
        $"linkButtonName : {linkButtonName}\n" +
        $"author : {author}\n";
    }

    public NoticeData(LitJson.JsonData _jsonData)
    {
       title = _jsonData["title"].ToString();
       contents = _jsonData["content"].ToString();
       postingDate = DateTime.Parse(_jsonData["postingDate"].ToString());
       inDate = _jsonData["inDate"].ToString();
       uuid = _jsonData["uuid"].ToString();
       isPublic = _jsonData["isPublic"].ToString() == "y" ? true : false;
       author = _jsonData["author"].ToString();

        if (_jsonData.ContainsKey("imageKey"))
        {
            imageKey = "http://upload-console.thebackend.io" + _jsonData["imageKey"].ToString();
        }
        if (_jsonData.ContainsKey("linkUrl"))
        {
           linkUrl = _jsonData["linkUrl"].ToString();
        }
        if (_jsonData.ContainsKey("linkButtonName"))
        {
           linkButtonName = _jsonData["linkButtonName"].ToString();
        }

    }
}

[Serializable]
public class EventData
{
    public string uuid;
    public string content;
    public string contentImageKey;
    public string popUpImageKey;
    public DateTime postingDate;
    public DateTime startDate;
    public DateTime endDate;
    public string inDate;
    public string linkUrl;
    public string author;
    public bool isPublic;
    public string linkButtonName;
    public string title;

    public EventData(LitJson.JsonData _jsonData)
    {
        title = _jsonData["title"].ToString();
        content = _jsonData["content"].ToString();
        postingDate = DateTime.Parse(_jsonData["postingDate"].ToString());
        startDate = DateTime.Parse(_jsonData["startDate"].ToString());
        endDate = DateTime.Parse(_jsonData["endDate"].ToString());
        inDate = _jsonData["inDate"].ToString();
        uuid = _jsonData["uuid"].ToString();
        isPublic = _jsonData["isPublic"].ToString() == "y" ? true : false;
        author = _jsonData["author"].ToString();

        if (_jsonData.ContainsKey("contentImageKey"))
        {
            contentImageKey = "http://upload-console.thebackend.io" + _jsonData["contentImageKey"].ToString();
        }
        if (_jsonData.ContainsKey("popUpImageKey"))
        {
            popUpImageKey = "http://upload-console.thebackend.io" + _jsonData["popUpImageKey"].ToString();
        }
        if (_jsonData.ContainsKey("linkUrl"))
        {
            linkUrl = _jsonData["linkUrl"].ToString();
        }
        if (_jsonData.ContainsKey("linkButtonName"))
        {
            linkButtonName = _jsonData["linkButtonName"].ToString();
        }
    }

    public override string ToString()
    {
        return $"uuid : {uuid}\n" +
        $"content : {content}\n" +
        $"contentImageKey : {contentImageKey}\n" +
        $"popUpImageKey : {popUpImageKey}\n" +
        $"postingDate : {postingDate}\n" +
        $"startDate : {startDate}\n" +
        $"endDate : {endDate}\n" +
        $"inDate : {inDate}\n" +
        $"linkUrl : {linkUrl}\n" +
        $"author : {author}\n" +
        $"isPublic : {isPublic}\n" +
        $"linkButtonName : {linkButtonName}\n" +
        $"title : {title}\n";
    }
}

[Serializable]
public class PolicyData
{
    public string terms;
    public string termsURL;
    public string privacy;
    public string privacyURL;
    public override string ToString()
    {
        string str = $"terms : {terms}\n" +
        $"termsURL : {termsURL}\n" +
        $"privacy : {privacy}\n" +
        $"privacyURL : {privacyURL}\n";
        return str;
    }
}