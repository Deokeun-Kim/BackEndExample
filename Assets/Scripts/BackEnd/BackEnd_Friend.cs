using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using LitJson;

public partial class BackEndManager
{
    /// <summary>
    /// 친구 리스트 조회.
    /// </summary>
    /// <param name="_action"></param>
	public void GetFriendList(Action<List<FriendData>> _action = null)
	{
		Backend.Friend.GetFriendList((callback) =>
		{
            Debug.Log(ShowDebugLog("GetFriendList", callback));
            List<FriendData> freindList = new List<FriendData>();
            if (callback.IsSuccess() == false)
            {
                _action?.Invoke(freindList);
                return;
            }
            JsonData json = callback.FlattenRows();
            for (int i = 0; i < json.Count; i++)
            {
                FriendData friendItem = new FriendData(json[i],FriendDataType.MyFriendList);
                freindList.Add(friendItem);
            }
            _action?.Invoke(freindList);
        });
	}

    /// <summary>
    /// 친구 제거 
    /// </summary>
    /// <param name="_inDate"></param>
    /// <param name="_action"></param>
    public void DeleteFriend(string _inDate, Action<bool> _action = null)
    {
        Backend.Friend.BreakFriend(_inDate, (callback) =>
        {
            Debug.Log(ShowDebugLog("DeleteFriend", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 친구 신청 보내기.
    /// </summary>
    /// <param name="_inDate"></param>
    /// <param name="_action"></param>
    public void RequestFriend(string _inDate, Action<BackendReturnObject> _action = null)
    {
        Backend.Friend.RequestFriend(_inDate, (callback) =>
        {
            Debug.Log(ShowDebugLog("RequestFriend", callback));
            _action?.Invoke(callback);
        });
    }

    /// <summary>
    /// 친구 신청 요청 취소.
    /// </summary>
    /// <param name="_inDate"></param>
    /// <param name="_action"></param>
    public void CancelFriendRequest(string _inDate, Action<BackendReturnObject> _action = null)
    {
        Backend.Friend.RevokeSentRequest(_inDate, (callback) =>
        {
            Debug.Log(ShowDebugLog("CancelFriendRequest", callback));
            _action?.Invoke(callback);
        });
    }
    
    /// <summary>
    /// 친구 요청 수락.
    /// </summary>
    /// <param name="_inDate"></param>
    /// <param name="_action"></param>
    public void AcceptFriend(string _inDate, Action<BackendReturnObject> _action = null)
    {
        Backend.Friend.AcceptFriend(_inDate, (callback) =>
        {
            Debug.Log(ShowDebugLog("AcceptFriend", callback));
            _action?.Invoke(callback);
        });
    }

    /// <summary>
    /// 친구 요청 거절
    /// </summary>
    /// <param name="_inDate"></param>
    /// <param name="_action"></param>
    public void RejectFriend(string _inDate, Action<BackendReturnObject> _action = null)
    {
        Backend.Friend.RejectFriend(_inDate, (callback) =>
        {
            Debug.Log(ShowDebugLog("RejectFriend", callback));
            _action?.Invoke(callback);
        });
    }

    /// <summary>
    /// 보낸 친구 신청 전체 리스트 조회.
    /// </summary>
    /// <param name="_friendRequestList"></param>
    public void GetSendFriendRequestList(Action<List<FriendData>> _friendRequestList = null)
    {
        Backend.Friend.GetSentRequestList((callback) =>
        {
            Debug.Log(ShowDebugLog("GetSendFriendRequestList", callback));
            JsonData json = callback.FlattenRows();
            List<FriendData> freindList = new List<FriendData>();
            for (int i = 0; i < json.Count; i++)
            {
                FriendData friendItem = new FriendData(json[i],FriendDataType.SendRequestList);
                freindList.Add(friendItem);
                Debug.Log(friendItem.ToString());
            }
            _friendRequestList?.Invoke(freindList);
        });
    }

    /// <summary>
    /// 받은 친구 신청 전체 리스트 조회. 
    /// </summary>
    /// <param name="_friendRequestList"></param>
    public void GetRecvFriendRequestList(Action<List<FriendData>> _friendRequestList = null)
    {
        Backend.Friend.GetReceivedRequestList((callback) =>
        {
            Debug.Log(ShowDebugLog("GetRecvFriendRequestList", callback));
            JsonData json = callback.FlattenRows();
            List<FriendData> freindList = new List<FriendData>();
            for (int i = 0; i < json.Count; i++)
            {
                FriendData friendItem = new FriendData(json[i],FriendDataType.RecvRequestList);
                freindList.Add(friendItem);
                Debug.Log(friendItem.ToString());
            }
            _friendRequestList?.Invoke(freindList);
        });
    }

    /// <summary>
    /// 친구 추천 리스트 조회.
    /// </summary>
    /// <param name="_action"></param>
    public void GetRandUserInfo(Action<List<FriendData>> _action = null)
    {
        Backend.Social.GetRandomUserInfo(10, (callback) =>
        {
            if (callback.IsSuccess() == false)
            {
                return;
            }

            List<FriendData> freindList = new List<FriendData>();
            for (int i = 0; i < callback.Rows().Count; i++)
            {
                FriendData _addData = new FriendData(callback.Rows()[i],FriendDataType.RandFriendList);
                freindList.Add(_addData);
            }
            _action?.Invoke(freindList);
        });
    }
}

public enum FriendDataType
{
    SendRequestList,
    RecvRequestList,
    MyFriendList,
    RandFriendList,
}

public class FriendData
{
    public string nickname;
    public string inDate;
    public string lastLogin;
    public string createdAt;
    public string guildName;

    public FriendDataType typeData;

    public override string ToString()
    {
        return $"nickname : {nickname}\ninDate : {inDate}\nlastLogin : {lastLogin}\ncreatedAt : {createdAt}\n guildName : {guildName}\n";
    }

    public FriendData(JsonData _data, FriendDataType _type)
    {
        typeData = _type;
        inDate = _data["inDate"].ToString();
        if (_data.ContainsKey("nickname") && _data["nickname"] != null)
            nickname = _data["nickname"].ToString();
        if (_data.ContainsKey("lastLogin") && _data["lastLogin"] != null)
            lastLogin = _data["lastLogin"].ToString();
        if (_data.ContainsKey("createdAt") && _data["createdAt"] != null)
            createdAt = _data["createdAt"].ToString();
        if (_data.ContainsKey("guildName") && _data["guildName"] != null)
            guildName = _data["guildName"].ToString();
    }
};