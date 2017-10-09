using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Util;
using UnityEngine;

public class SimpleClient : MonoBehaviour
{
    private SmartFox sfs;
    private ConfigData cfg;
    
    public string GameHost = "192.168.1.3";
    public int TcpPort = 9933;
    public string ZoneName = "BasicExamples";
    public string RoomName = "Evacuation";
    public string ClientName;
    public float UserVariableSendInterval = 0.25f;

    public int GroupID;
    public int GroupSize;
    public int Leader;
    public int Gender;
    private float DealtaTime = 0.0f;
    private bool HasStarted = false;

    void Start()
    {
//        StartUp();
    }

    void Update()
    {
        if (sfs != null)
            sfs.ProcessEvents();

        if (!HasStarted)
        {
            return;
        }

        //隔断时间更新一次
        DealtaTime += Time.deltaTime;
        if (DealtaTime < UserVariableSendInterval)
        {
            return;
        }
        DealtaTime = 0.0f;

        //更新客户端位置
        Vector3 randomPos = Random.insideUnitSphere;
        randomPos.y = 0f;
        Vector3 newPos = transform.position + randomPos;
        newPos.z = Mathf.Clamp(newPos.z, -3.9f, 3.9f);
        
        transform.position = newPos;
        transform.rotation = Random.rotation;

        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("PosX", (double)transform.position.x));
        userVariables.Add(new SFSUserVariable("PosY", (double)transform.position.y));
        userVariables.Add(new SFSUserVariable("PosZ", (double)transform.position.z));
        userVariables.Add(new SFSUserVariable("RotY",
            (double)transform.localRotation.eulerAngles.y));
        userVariables.Add(new SFSUserVariable("RotX",
            (double)Camera.main.transform.localRotation.eulerAngles.x));
            userVariables.Add(new SFSUserVariable("Animation", "Run"));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    public void StartUp()
    {
        sfs = new SmartFox();
        cfg = new ConfigData();
        cfg.Host = GameHost;
        cfg.Port = TcpPort;
        cfg.Zone = ZoneName;
        cfg.UseBlueBox = false;

        sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
        sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);

        sfs.Connect(cfg);
    }

    public void Disconnect()
    {
        // Disconnect
        sfs.Disconnect();

        // Remove SFS2X listeners and re-enable interface
        reset();
    }

    private void reset()
    {
        // Remove SFS2X listeners
        sfs.RemoveAllEventListeners();
    }

    private void OnConnection(BaseEvent evt)
    {
        if ((bool)evt.Params["success"])
        {
            sfs.Send(new LoginRequest(ClientName, "", ZoneName));
        }
        else
        {
            // Remove SFS2X listeners and re-enable interface
            reset();
            Debug.Log(ClientName + " Connection failed. Server is running?");
        }
    }

    private void OnConnectionLost(BaseEvent evt)
    {
        // Remove SFS2X listeners and re-enable interface
        reset();

        string reason = (string)evt.Params["reason"];

        if (reason != ClientDisconnectionReason.MANUAL)
        {
            // Show error message
            Debug.Log(ClientName + " Connection failed because : " + reason);
        }
    }

    private void OnLogin(BaseEvent evt)
    {
        sfs.Send(new JoinRoomRequest(RoomName));
    }

    private void OnLoginError(BaseEvent evt)
    {
        // Disconnect
        sfs.Disconnect();

        // Remove SFS2X listeners and re-enable interface
        reset();

        // Show error message
        Debug.Log(ClientName + " Login failed: " + (string)evt.Params["errorMessage"]);
    }

    private void OnRoomJoin(BaseEvent evt)
    {
        SFSObject sfsObj = SFSObject.NewInstance();
        //需要给设置
        sfsObj.PutInt("GroupID", GroupID);
        sfsObj.PutInt("GroupSize", GroupSize);
        sfsObj.PutInt("Leader", Leader);
        sfsObj.PutInt("Gender", Gender);
        sfsObj.PutFloatArray("GroupColor", new float[] { Random.value, Random.value, Random.value });
        sfs.Send(new ExtensionRequest("Ready", sfsObj, sfs.LastJoinedRoom));
    }

    private void OnRoomJoinError(BaseEvent evt)
    {
        // Disconnect
        sfs.Disconnect();

        // Remove SFS2X listeners and re-enable interface
        reset();

        // Show error message
        Debug.Log(ClientName + "Join room failed: " + (string)evt.Params["errorMessage"]);
    }

    private void OnExtensionResponse(BaseEvent evt)
    {
        string cmd = (string)evt.Params["cmd"];
        SFSObject dataObject = (SFSObject)evt.Params["params"];

        switch (cmd)
        {
            case "Start":
            {
                HasStarted = true;
                    break;
                }
            case "Wait":
                {
                    int curUserCount = dataObject.GetInt("UserCount");
                    Debug.Log( string.Format(ClientName + "正在等待其他用户......\n已加入 {0} 名用户\n", curUserCount));
                    string preparedUsernames = dataObject.GetUtfString("Names");
//                    Debug.Log(preparedUsernames);
                    break;
                }
            case "HasStarted":
                {
                    Debug.Log("疏散已经开始，请等待下次机会！");
                    break;
                }
            default:
                break;
        }
    }
}
