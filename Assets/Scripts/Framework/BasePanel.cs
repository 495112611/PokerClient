using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    /// <summary>
    /// 加载路径
    /// </summary>
    public string skinPath;
    /// <summary>
    /// 面板
    /// </summary>
    public GameObject skin;
    /// <summary>
    /// 层级
    /// </summary>
    public PanelManager.Layer layer=PanelManager.Layer.Panel;
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        Debug.Log(skinPath);
        skin = Instantiate(Resources.Load<GameObject>(skinPath));
    }
    public virtual void OnInit()
    {

    }
    public virtual void OnShow(params object[] para)
    {

    }
    public virtual void OnClose()
    {

    }
    public void Close()
    {
        string name = GetType().ToString();
        PanelManager.Close(name);
    }
}
