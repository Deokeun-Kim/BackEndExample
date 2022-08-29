using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using LitJson;

public partial class BackEndManager
{
    #region SENT MESSAGE 
    public void SendMessage(string _userinDate, string _Message, Action<bool> _action = null)
	{
		Backend.Message.SendMessage(_userinDate, _Message, (callback) =>
		{
            Debug.Log(ShowDebugLog($"SendMessage ({_userinDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
	}

    public void DeleteSendMessage(string _messageinDate, Action<bool> _action = null)
    {
        Backend.Message.DeleteSentMessage(_messageinDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"DeleteSentMessage ({_messageinDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }
        
    public void GetSendMessage(string _messageinDate, Action<bool> _action = null)
    {
        Backend.Message.GetSentMessage(_messageinDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"GetSentMessage ({_messageinDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    public void GetSendMessageList(Action<List<MessageData>> _action = null)
    {
        Backend.Message.GetSentMessageList((callback) =>
        {
            Debug.Log(ShowDebugLog($"GetSentMessageList", callback));
            JsonData json = callback.FlattenRows();
            List<MessageData> resultList = new List<MessageData>();
            for (int i = 0; i < json.Count; i++)
            {
                MessageData messateEntity = new MessageData(json[i]);
                resultList.Add(messateEntity);
            }
            _action?.Invoke(resultList);
        });
    }
    #endregion SENT MESSAGE 

    #region RECV MESSAGE 
    public void RecvMessage(string _userinDate, Action<bool> _action = null)
    {
        Backend.Message.GetReceivedMessage(_userinDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"RecvMessage ({_userinDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    public void DeleteRecvMessage(string _messageinDate, Action<bool> _action = null)
    {
        Backend.Message.DeleteReceivedMessage(_messageinDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"DeleteRecvMessage ({_messageinDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    public void GetRecvMessageList(Action<List<MessageData>> _action = null)
    {
        Backend.Message.GetReceivedMessageList((callback) =>
        {
            Debug.Log(ShowDebugLog($"GetRecvMessageList", callback));
            JsonData json = callback.FlattenRows();
            List<MessageData> resultList = new List<MessageData>();
            for (int i = 0; i < json.Count; i++)
            {
                MessageData messateEntity = new MessageData(json[i]);
                resultList.Add(messateEntity);
            }
            _action?.Invoke(resultList);

        });
    }
    #endregion RECV MESSAGE 
}

public class MessageData
{
    public string receiver;
    public string sender;
    public string content;
    public string inDate;
    public string senderNickname;
    public bool isRead;
    public string receiverNickname;
    public bool isReceiverDelete;
    public bool isSenderDelete;

    public MessageData(JsonData _Data)
    {
        receiver = _Data["receiver"].ToString();
        sender = _Data["sender"].ToString();
        content = _Data["content"].ToString();
        inDate = _Data["inDate"].ToString();
        senderNickname = _Data["senderNickname"].ToString();
        isRead = _Data["isRead"].ToString() == "true" ? true : false;
        receiverNickname = _Data["receiverNickname"].ToString();
        isReceiverDelete = _Data["isReceiverDelete"].ToString() == "true" ? true : false;
        isSenderDelete = _Data["isSenderDelete"].ToString() == "true" ? true : false;
    }

    public override string ToString()
    {
        return $"receiver : {receiver}\n" +
        $"sender : {sender}\n" +
        $"content : {content}\n" +
        $"inDate : {inDate}\n" +
        $"senderNickname : {senderNickname}\n" +
        $"isRead : {isRead}\n" +
        $"receiverNickname : {receiverNickname}\n" +
        $"isReceiverDelete : {isReceiverDelete}\n" +
        $"isSenderDelete : {isSenderDelete}\n";
    }
};