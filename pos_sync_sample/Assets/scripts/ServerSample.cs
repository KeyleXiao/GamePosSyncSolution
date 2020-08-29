/************************
 * @author keyle_xiao
 * @date Saturday, 29 August 2020 20:11:46
 * @mail keyle_xiao@hotmail.com
 * @site https://vrast.cn
 ************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerStatePack
{
    public float speed;
    public Vector3 position;
    public int angle_deg;
    
    public virtual void SetData(PlayerStatePack item)
    {
        speed = item.speed;
        position = item.position;
        angle_deg = item.angle_deg;
    }
}


public class ServerPlayerStatePack : PlayerStatePack
{
    public float currentTime;
    public bool BeSend;

    public override void SetData(PlayerStatePack item)
    {
        base.SetData(item);
        BeSend = false;
    }
}



public class ServerSample : MonoBehaviour
{
    static ServerSample instance;
    private void Awake()
    {
        instance = this;
    }
    
    
    public static ServerSample GetInstance()
    {
        return instance;
    }

    public Action<PlayerStatePack> ReceivePack;
    
    public int Ping = 200;
    public int fluctuation = 30;
    public List<ServerPlayerStatePack> packLst = new List<ServerPlayerStatePack>();
    


    public ServerPlayerStatePack CreatePack(PlayerStatePack pack)
    {
        for (int i = 0; i < packLst.Count; i++)
        {
            if (packLst[i].BeSend)
            {
                packLst[i].currentTime = Time.time + Ping / 1000f * 2 + Random.Range(-fluctuation * 2/1000f , fluctuation * 2/1000f );
                packLst[i].SetData(pack);
                packLst[i].BeSend = false;
                return packLst[i];
            }
        }
        
        var item = new ServerPlayerStatePack();
        item.SetData(pack);
        item.currentTime = Time.time;
        packLst.Add(item);
        
        return item;
    }


    public void SendPack(PlayerStatePack pack)
    {
        CreatePack(pack);
    }

    
    // Time

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < packLst.Count; i++)
        {
            if (packLst[i].BeSend)
                continue;
            
            if (packLst[i].currentTime <= Time.time)
            {
                packLst[i].BeSend = true;
                ReceivePack?.Invoke(packLst[i]);
            }
        }
    }


    private void OnGUI()
    {
        GUILayout.TextField($"模拟服务器延迟：{Ping}*2 = {Ping * 2}");
        GUILayout.TextField($"模拟网络波动：{fluctuation}*2 = {fluctuation * 2}");
    }
}
