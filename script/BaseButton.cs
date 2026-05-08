using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// 一个BaseButton
/// </summary>
#region
public class BaseButton : Skin 
{
    public BtnEventTriggerListener __listener;
    private int _soundType = 1;
    private int _effectType = 1;
    Button btn;

    private float beginScale;
    private float endScale;

    public BaseButton(Transform trans, int effectType = 1, int soundType = 1)
        : base(trans)
    {
        _soundType = soundType;
        _effectType = effectType;

        beginScale = recTransform.localScale.x;
        endScale = beginScale * 0.92f;
        btn = trans.GetComponent<Button>();
        addEvent();
    }

    public bool interactable
    {

        set { if (btn != null) btn.interactable = value; }
        get { if (btn == null) return false; return btn.interactable; }
    }

    public bool active
    {
        get { return gameObject.activeSelf; }
    }

    private Action<GameObject> onClickhandle;
    public Action<GameObject> onClick
    {
        set
        {
            if (value == null && onClickhandle != null)
            {
                if (__listener != null)
                    __listener.onClick = null;
            }

            onClickhandle = value;
        }
        get
        {
            return onClickhandle;
        }
    }

    private Action<GameObject> onClickFalseHandle;
    public Action<GameObject> onClickFalse
    {
        set
        {
            onClickFalseHandle = value;
        }
        get
        {
            return onClickFalseHandle;
        }
    }

    private void doClick(GameObject go)
    {
        if (go.transform.GetComponent<Button>())
        {
            if (go.transform.GetComponent<Button>().interactable == false)
            {
                if (onClickFalseHandle != null)
                    onClickFalseHandle(gameObject);
                return;
            }
        }
        if (_soundType == 1)
        {
            //MediaClient.instance.PlaySoundUrl("media/ui/button", false, null);
            //MediaClient.instance.PlaySoundUrl("audio/common/click_button", false, null);
        }
        //debug.Log("播放音效");

        if (onClickhandle != null)
            onClickhandle(gameObject);
    }


    private void doDown(GameObject go)
    {
        if (go.transform.GetComponent<Button>())
        {
            if (go.transform.GetComponent<Button>().interactable == false)
            {
                return;
            }
        }

        //if (_effectType == 1)
        //    recTransform.DOScale(endScale, 0.15f);
    }

    private void doUp(GameObject go)
    {
        if (go.transform.GetComponent<Button>())
        {
            if (go.transform.GetComponent<Button>().interactable == false)
            {
                return;
            }
        }

        //if (_effectType == 1)
        //    recTransform.DOScale(beginScale, 0.2f);
    }

    public void removeAllListener()
    {
        if (__listener != null)
        {
            __listener.onDown = null;
            __listener.onUp = null;
            __listener.onExit = null;
            __listener.onClick = null;
        }
        recTransform.localScale = Vector3.one;
    }

    public void addEvent()
    {
        __listener = BtnEventTriggerListener.Get(gameObject);
        if (_effectType == 1)
        {
            __listener.onDown = doDown;
            __listener.onUp = doUp;
            __listener.onExit = doUp;
        }
        __listener.onClick = doClick;
    }

    public void dispose()
    {
        if (__listener != null)
        {
            __listener.clearAllListener();
            __listener = null;
        }

        GameObject.Destroy(gameObject);
    }
}
public class Skin
{
    public object data;
    public Transform __mainTrans;
    public RectTransform recTransform;
    public Skin(Transform trans)
    {
        __mainTrans = trans;
        recTransform = trans.GetComponent<RectTransform>();
    }


    public Vector3 pos
    {
        get { return recTransform.position; }
        set { recTransform.position = value; }
    }

    public Transform getTransformByPath(string path)
    {
        string[] arr = path.Split(new char[] { '.' });
        Transform trans = this.__mainTrans;
        for (int i = 0; i < arr.Length; i++)
        {
            trans = trans.Find(arr[i]);
        }
        return trans;
    }

    public GameObject getGameObjectByPath(string path)
    {
        return getTransformByPath(path).gameObject;
    }

    public Button getButtonByPath(string path)
    {
        return getComponentByPath<Button>(path);
    }

    public GameObject gameObject
    {
        get { return __mainTrans.gameObject; }
    }

    public Transform transform
    {
        get { return __mainTrans.transform; }
    }

    public void setPerent(Transform p)
    {
        __mainTrans.transform.SetParent(p, false);
    }


    protected bool __visiable = true;
    virtual public bool visiable
    {
        get { return __visiable; }
        set { if (__mainTrans.gameObject.activeSelf == value) return; __visiable = value; __mainTrans.gameObject.SetActive(value); }
    }

    public T getComponentByPath<T>(string path) where T : Component
    {
        Transform trans = this.__mainTrans;

        string[] arr = path.Split(new char[] { '.' });
        for (int i = 0; i < arr.Length; i++)
        {
            trans = trans.Find(arr[i]);
        }


        return trans.GetComponent<T>();

    }

    public EventTriggerListener getEventTrigerByPath(string path = "")
    {
        if (path == "")
            return EventTriggerListener.Get(__mainTrans.gameObject);
        else
            return EventTriggerListener.Get(getGameObjectByPath(path));
    }



    public void clearListenersPath(string path = "")
    {
        GameObject go;
        if (path == "")
            go = __mainTrans.gameObject;
        else
            go = getGameObjectByPath(path);
        EventTriggerListener.Get(go).clearAllListener();
    }

    public void destoryGo()
    {
        GameObject.Destroy(__mainTrans.gameObject);
        __mainTrans = null;
        recTransform = null;
    }

}
public class EventTriggerListener : UnityEngine.EventSystems.EventTrigger
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void VectorDelegate(GameObject go, Vector2 delta);
    public delegate void relayDelegate(GameObject go, PointerEventData eventData);
    public VoidDelegate onClick;
    public VectorDelegate onPointClick;
    public VectorDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VectorDelegate onUp;
    public VectorDelegate onMove;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    public VoidDelegate onDragIn;
    public VectorDelegate onDrag;
    public VoidDelegate onDragOut;
    public VectorDelegate onDragEnd;
    public VectorDelegate onInPoDrag;
    public VoidDelegate OnBeingDrag;
    public relayDelegate relayScrollIn;
    public relayDelegate relayDragIn;
    public relayDelegate relayBeingDrag;
    public relayDelegate relayEndDrag;

    public Action<PointerEventData> newInPoDrag;
    public Action<PointerEventData> newOnDrag, newEndDrag, newOnDown, newOnUp,newOnBeingDrag,newOnScroll;
    static public EventTriggerListener Get(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("EventTriggerListener_go_is_NULL");
            return null;
        }
        else
        {
            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener == null) listener = go.AddComponent<EventTriggerListener>();
            return listener;
        }
    }
    public override void OnScroll(PointerEventData eventData)
    {
        if (newOnScroll != null) newOnScroll(eventData);
        if (relayScrollIn != null) relayScrollIn(gameObject, eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (newOnBeingDrag != null) newOnBeingDrag(eventData);
        if (onDragIn != null) onDragIn(gameObject);
        if (relayDragIn != null) relayDragIn(gameObject, eventData);
    }

    //public override void OnScroll(PointerEventData eventData)
    //{
    //    base.OnScroll(eventData);
    //}

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (newInPoDrag != null) newInPoDrag(eventData);
        if (onInPoDrag != null) onInPoDrag(gameObject, eventData.position);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (newOnDrag != null) newOnDrag(eventData);
        if (onDrag != null) onDrag(gameObject, eventData.position );
     //   if (onDrag != null) OnBeingDrag(gameObject);
        if (relayBeingDrag != null) relayBeingDrag(gameObject, eventData);
    }
    
    public override void OnEndDrag(PointerEventData eventData)
    {
        if (newEndDrag != null) newEndDrag(eventData);
        if (onDragOut != null) onDragOut(gameObject);
        if (onDragEnd != null) onDragEnd(gameObject, eventData.position);
        if (relayEndDrag != null) relayEndDrag(gameObject, eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null) onClick(gameObject);
        if (onPointClick != null) onPointClick(gameObject, eventData.position);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (newOnDown != null) newOnDown(eventData);
        if (onDown != null) onDown(gameObject, eventData.position);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
        if (relayBeingDrag != null) relayBeingDrag(gameObject, eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (newOnUp != null) newOnUp(eventData);
        if (onUp != null) onUp(gameObject, eventData.position);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }
   
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (onMove != null) onMove(gameObject, eventData.moveVector);
    }

    public void clearAllListener()
    {
        onClick = null;
        onDown = null;
        onEnter = null;
        onExit = null;
        onUp = null;
        onSelect = null;
        onUpdateSelect = null;
        onDrag = null;
        onDragOut = null;
        onDragIn = null;
        onMove = null;
        onInPoDrag = null;
        onDragEnd = null;
        Destroy(gameObject.GetComponent<EventTriggerListener>());
    }
}
public class BtnEventTriggerListener :
MonoBehaviour,
IPointerExitHandler,
IPointerDownHandler,
IPointerUpHandler,
IPointerClickHandler
{
    ArrayList m_arr;
    public delegate void VoidDelegate(GameObject go);
    public delegate void VectorDelegate(GameObject go, Vector2 delta);
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onExit;
    public VoidDelegate onUp;

    static public BtnEventTriggerListener Get(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("BtnEventTriggerListener_go_is_NULL");
            return null;
        }
        else
        {
            BtnEventTriggerListener listener = go.GetComponent<BtnEventTriggerListener>();
            if (listener == null) listener = go.AddComponent<BtnEventTriggerListener>();
            return listener;
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null) onClick(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
    }
    public void clearAllListener()
    {
        onClick = null;
        onDown = null;
        onExit = null;
        onUp = null;
        Destroy(gameObject.GetComponent<BtnEventTriggerListener>());
    }
}
#endregion
