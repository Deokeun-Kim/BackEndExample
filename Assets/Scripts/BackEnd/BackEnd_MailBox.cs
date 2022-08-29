using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using LitJson;

public partial class BackEndManager
{
    #region GET MAIL POST LIST
    public void GetAllPostList(Action<List<MailPostData>> _action = null)
    {
        StopCoroutine(nameof(R_GetAllPost));
        StartCoroutine(nameof(R_GetAllPost), _action);
    }

    private IEnumerator R_GetAllPost(Action<List<MailPostData>> _action = null)
    {
        List<MailPostData> mailDataList = new List<MailPostData>();

        bool isLoad = false;
        GetPostListByType(mailDataList, PostType.Admin, () => isLoad = true);
        while (isLoad == false)
            yield return null;

        isLoad = false;
        GetPostListByType(mailDataList,PostType.Rank, () => isLoad = true);
        while (isLoad == false)
            yield return null;

        _action?.Invoke(mailDataList);
    }

    private void GetPostListByType(List<MailPostData> mailDataList, PostType _type, Action _action)
    {
        Backend.UPost.GetPostList(_type, 100, callback =>
        {
            Debug.Log(ShowDebugLog($"GetPostListByType ({_type})", callback));
            JsonData json = callback.GetReturnValuetoJSON()["postList"];
            for (int i = 0; i < json.Count; i++)
            {
                MailPostData tempData = new MailPostData(json[i]);
                mailDataList.Add(tempData);
            }
            _action.Invoke();
        });
    }
    #endregion GET MAIL POST LIST

    public void GetRecvMail(MailPostData _data, Action<bool> _action = null)
	{
		PostType recvType = PostType.Rank;
        if (_data.type.Equals("admin") == true)
            recvType = PostType.Admin;

        Backend.UPost.ReceivePostItem(recvType, _data.inDate, (callback) =>
        {
            Debug.Log(ShowDebugLog("GetRecvMail", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    #region GET RECV ALL MAIL
    public void GetAllRecvMail(Action<bool> _action = null)
    {
        StopCoroutine(nameof(R_GetAllRecvMail));
        StartCoroutine(nameof(R_GetAllRecvMail), _action);
    }

    public IEnumerator R_GetAllRecvMail(Action<bool> _action = null)
    {
        bool isLoad = false;
        Backend.UPost.ReceivePostItemAll(PostType.Admin, callback =>
        {
            Debug.Log(ShowDebugLog($"GetAllRecvMail ({PostType.Admin})", callback));
            isLoad = true;
        });
		while (isLoad == false)
            yield return null;

        isLoad = false;
        Backend.UPost.ReceivePostItemAll(PostType.Rank, callback =>
        {
            Debug.Log(ShowDebugLog($"GetAllRecvMail ({PostType.Rank})", callback));
            isLoad = true;
        });
        while (isLoad == false)
            yield return null;

        _action?.Invoke(true);
    }
    #endregion GET RECV ALL MAIL 
}

public class MailPostData
{
    public string type;
    public string title;
    public string author;
    public string content;
    public DateTime expirationDate;
    public DateTime reservationDate;
    public DateTime sentDate;
    public string nickname;
    public string inDate;
    public List<MailPostItemData> items = new List<MailPostItemData>();
    public bool isItem;


    public MailPostData(JsonData _data)
    {
        content = _data["content"].ToString();
        expirationDate = DateTime.Parse(_data["expirationDate"].ToString());
        reservationDate = DateTime.Parse(_data["reservationDate"].ToString());
        nickname = _data["nickname"].ToString();
        inDate = _data["inDate"].ToString();
        title = _data["title"].ToString();
        sentDate = DateTime.Parse(_data["sentDate"].ToString());

        if (_data.ContainsKey("author"))
            author = _data["author"].ToString();

        if (_data.ContainsKey("rankType"))
            type = _data["rankType"].ToString();
        else
            type = "admin";

        isItem = false;
        if (_data.ContainsKey("items") && _data["items"].Count > 0)
        {
            isItem = true;
            for (int i = 0; i < _data["items"].Count; i++)
            {
                MailPostItemData tempData = new MailPostItemData(_data["items"][i]);
                items.Add(tempData);
            }
        }
    }

    public override string ToString()
    {
        string totalString = string.Empty;
        string itemList = "";
        for (int i = 0; i < items.Count; i++)
        {
            itemList += items[i].ToString();
        }
        totalString = $"{title} / {author} / {content} / {expirationDate} / {reservationDate} / {sentDate} / {nickname} / {inDate}";
        totalString += "\nItemList : \n";
        totalString += itemList;
        return totalString;
    }
}

public class MailPostItemData
{
    public string chartFileName;
    public int itemCount;

    // MailBox Item Chart 참조 -> 메일로 보내는 아이템 차트 데이터의 컬럼에 따라 추가 / 제거 필요
    public string itemKey;
    public string memo;
    public string chartName;
    // MailBox Item Chart 참조 -> 메일로 보내는 아이템 차트 데이터의 컬럼에 따라 추가 / 제거 필요

    public MailPostItemData(JsonData _data)
    {
        chartFileName = _data["item"]["chartFileName"].ToString();
        itemCount = int.Parse(_data["itemCount"].ToString());

        // MailBox Data에 따라 참조 키가 달라짐 -> 메일로 보내는 아이템 차트 데이터의 컬럼에 따라 추가 / 제거 필요
        itemKey = _data["item"]["index"].ToString();
        memo = _data["item"]["memo"].ToString();
        chartName = _data["chartName"].ToString();
    }

    public override string ToString()
    {
        return $"item({chartFileName} / {itemKey} / {memo} / {chartName} / {itemCount})";
    }
}