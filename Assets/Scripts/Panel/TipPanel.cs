using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    private Text text;
    public override void OnInit()
    {
        skinPath = "TipPanel";
        layer = PanelManager.Layer.Tip;
    }
    public override void OnShow(params object[] para)
    {
        text = skin.transform.Find("Text").GetComponent<Text>();

        if (para.Length >= 1)
        {
            text.text = (string)para[0];
        }
    }
    private void Start()
    {
        StartCoroutine(Effect());
    }
    IEnumerator Effect()
    {
        for (int i = 0; i < 100; i++)
        {
            skin.transform.position += Vector3.up *0.2f;
            text.color -= new Color(0, 0, 0, 0.01f);
            yield return new WaitForSeconds(0.02f);
        }
        Close();
    }
}
