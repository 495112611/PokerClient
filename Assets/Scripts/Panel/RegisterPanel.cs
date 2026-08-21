using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    private InputField idInput;
    private InputField pwInput;
    private InputField repInput;
    private Button registerButton;
    private Button closeButton;
    public override void OnInit()
    {
        skinPath = "RegisterPanel";
        layer = PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] para)
    {
        //Ѱ�����
        idInput = skin.transform.Find("IdInput").GetComponent<InputField>();
        pwInput = skin.transform.Find("PwInput").GetComponent<InputField>();
        repInput = skin.transform.Find("RepInput").GetComponent<InputField>();
        registerButton = skin.transform.Find("RegisterButton").GetComponent<Button>();
        closeButton = skin.transform.Find("CloseButton").GetComponent<Button>();


        registerButton.onClick.AddListener(OnRegisterClick);
        closeButton.onClick.AddListener(OnCloseClick);

        NetManager.AddMsgListener("MsgRegister", OnMsgRegister);
    }
    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgRegister", OnMsgRegister);
    }
    public void OnRegisterClick()
    {
        if (idInput.text == "" || pwInput.text == "" )
        {
            PanelManager.Open<TipPanel>("账号/密码/确认密码为空");
            return;
        }
        if (pwInput.text != repInput.text)
        {
            PanelManager.Open<TipPanel>("密码不一致");
            return;
        }

        //����ע��Э��
        MsgRegister msg =new MsgRegister();
        msg.id = idInput.text;
        msg.pw = pwInput.text;
        NetManager.Send(msg);
    }
    public void OnCloseClick()
    {
        Close();
    }
    public void OnMsgRegister(MsgBase msgBase)
    {
        MsgRegister msg=msgBase as MsgRegister;
        if (msg.result)
        {
            //ע��ɹ�
            PanelManager.Open<TipPanel>("注册成功");
            Close();
        }
        else
        {
            //ע��ʧ��
            PanelManager.Open<TipPanel>("注册失败");
        }
    }
}
