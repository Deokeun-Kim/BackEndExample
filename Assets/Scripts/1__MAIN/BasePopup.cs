using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum popup_type
{ 
    MAILBOX,
    NOTICES,
    EVENTS,
    FRIEND,
    GUILD,
    MESSAGE,
    PROFILE,
}

public abstract class BasePopup : MonoBehaviour
{
    [Header(">>>EDITOR CONFIRM<<<")]
    public popup_type popupType;

    public abstract void Init();

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
