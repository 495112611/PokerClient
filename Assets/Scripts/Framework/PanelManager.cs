using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PanelManager
{
    /// <summary>
    /// 层级
    /// </summary>
    public enum Layer
    {
        Panel,
        Tip
    }
    /// <summary>
    /// 层级列表
    /// </summary>
    private static Dictionary<Layer,Transform> layers=new Dictionary<Layer,Transform>();
    /// <summary>
    /// 面板列表
    /// </summary>
    private static Dictionary<string,BasePanel> panels=new Dictionary<string,BasePanel>();
    /// <summary>
    /// 根目录
    /// </summary>
    private static Transform root;
    /// <summary>
    /// 画布
    /// </summary>
    private static Transform canvas;
    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas");
        layers.Add(Layer.Panel, canvas.Find("Panel"));
        layers.Add(Layer.Tip, canvas.Find("Tip"));
    }
    /// <summary>
    /// 打开面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="para"></param>
    public static void Open<T>(params object[] para)where T:BasePanel
    {
        //是否已经打开
        string name = typeof(T).ToString();
        if (panels.ContainsKey(name))
            return;
        BasePanel panel = root.gameObject.AddComponent<T>();
        panel.OnInit();
        panel.Init();

        Transform layer = layers[panel.layer];
        panel.skin.transform.SetParent(layer, false);
        panels.Add(name, panel);
        panel.OnShow(para);
    }
    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="name"></param>
    public static void Close(string name)
    {
        //是否已经打开
        if (!panels.ContainsKey(name))
            return;
        BasePanel panel=panels[name];
        panel.OnClose();
        panels.Remove(name);
        GameObject.Destroy(panel.skin);
        GameObject.Destroy(panel);
    }
}
