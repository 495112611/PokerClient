using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    //���
    private InputField idInput;
    private InputField pwInput;
    private Button loginButton;
    private Button registerButton;
    public override void OnInit()
    {
        skinPath = "LoginPanel";
        layer=PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] para)
    {
        //Ѱ�����
        idInput = skin.transform.Find("IdInput").GetComponent<InputField>();
        pwInput = skin.transform.Find("PwInput").GetComponent<InputField>();
        loginButton = skin.transform.Find("LoginButton").GetComponent<Button>();
        registerButton = skin.transform.Find("RegisterButton").GetComponent<Button>();

        //�����¼�
        loginButton.onClick.AddListener(OnLoginClick);
        registerButton.onClick.AddListener(OnRegisterClick);

        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        //����Э�����
        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
        NetManager.Connect("127.0.0.1",8888);
    }
    public override void OnClose()
    {
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        
        NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);
    }
    public void OnLoginClick()
    {
        if(idInput.text==""||pwInput.text=="")
        {
            PanelManager.Open<TipPanel>("用户名或密码不能为空");
            return;
        }
        MsgLogin msgLogin = new MsgLogin();
        msgLogin.id=idInput.text;
        msgLogin.pw = pwInput.text;
        NetManager.Send(msgLogin);
    }
    public void OnRegisterClick()
    {
        //��ע�����
        PanelManager.Open<RegisterPanel>();
    }
    public void OnMsgLogin(MsgBase msgBase)
    {
        MsgLogin msg = msgBase as MsgLogin;
        if (msg.result)
        {
            //��½�ɹ�
            PanelManager.Open<TipPanel>("登录成功");
            GameManager.id=msg.id;
            PanelManager.Open<RoomListPanel>();


            Close();
        }
        else
        {
            //��¼ʧ��
            PanelManager.Open<TipPanel>("登录失败");
        }
    }
    public void OnConnectSucc(string err)
    {
        Debug.Log("登录成功");
    }
    public void OnConnectFail(string err)
    {
        PanelManager.Open<TipPanel>(err);
    }
}
