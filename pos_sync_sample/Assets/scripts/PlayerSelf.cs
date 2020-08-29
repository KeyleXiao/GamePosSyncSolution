/************************
 * @author keyle_xiao
 * @date Saturday, 29 August 2020 20:11:46
 * @mail keyle_xiao@hotmail.com
 * @site https://vrast.cn
 ************************/

using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStatePackClient : PlayerStatePack
{
	public override void SetData(PlayerStatePack item)
	{
		base.SetData(item);
		CurrentTime = Time.time;
	}

	public float CurrentTime;
}

public class PlayerSelf : MonoBehaviour
{
	private float lastFrameTime;
	private Vector3 lastFramePos;
	private bool LastSyncIdle;
	[FormerlySerializedAs("currentAngle_deg")] public int current_Angle_deg;
	public int Speed = 6;

	private PlayerStatePackClient LastSyncInfo;

	private void Start()
	{
		current_speed = Speed;
	}


	/// <summary>
	/// 上一帧信息
	/// </summary>
	private void RecordLastFrame()
	{
		lastFramePos = transform.position;
		lastFrameTime = Time.time;
	}
	
	/// <summary>
	/// 最后一次同步的信息
	/// </summary>
	private void RecordLastSync(PlayerStatePackClient item)
	{
		LastSyncInfo = item;
		LastSyncIdle = item.speed == 0;
	}

	/// <summary>
	/// 发送位置信息
	/// </summary>
	private void SendPosSync()
	{
		Debug.Log("发送同步包");
		var item = new PlayerStatePackClient();
		item.angle_deg = current_Angle_deg;
		item.position = transform.position;
		item.speed = current_speed;
		item.CurrentTime = Time.time;
		ServerSample.GetInstance().SendPack(item);
		RecordLastSync(item);
	}

	private void Update()
	{
		//这里模拟用户输入
		OnUserInput();
		
		//这里模拟运动
		OnMove();

		NetSyncLogic();
		
		RecordLastFrame();
	}

	private void NetSyncLogic()
	{
		if (LastSyncInfo == null)
		{
			SendPosSync();
			return;
		}
		
		if (lastFramePos == transform.position)
		{
			if (LastSyncIdle) return;

			SendPosSync();
		}

		if (LastSyncIdle)
		{
			SendPosSync(); return;
		}

		var passTime = Time.time - LastSyncInfo.CurrentTime;
		var step = LastSyncInfo.speed * passTime;
		var angle =  LastSyncInfo.angle_deg * Mathf.Deg2Rad;
		var pos = LastSyncInfo.position + new Vector3(step * Mathf.Sin(angle),0,step * Mathf.Cos(angle));

		if (Vector3.Distance(transform.position,pos) > 0.15f)
		{
			SendPosSync();
		}
	}


	int current_speed = 1;
	
	/// <summary>
	/// 主角前进
	/// </summary>
	public void OnMove()
	{
		if (current_speed == 0)
			return;
		
		var passTime = Time.time - lastFrameTime;
		var step = current_speed * passTime;
		var pos = transform.position;
		var angle = current_Angle_deg * Mathf.Deg2Rad;
		
		transform.position = pos + new Vector3(step * Mathf.Sin(angle),0,step * Mathf.Cos(angle));
		transform.transform.eulerAngles = new Vector3(transform.transform.eulerAngles.x,current_Angle_deg,transform.transform.eulerAngles.z);
	}




	RaycastHit hitInfo;
	public bool UpdateDirNow;
	
	private void OnUserInput()
	{
		if (Input.GetKeyUp(KeyCode.Space))
		{
			current_speed = current_speed == 0 ? Speed : 0;
		}
		
		//模拟点击寻路
		if (Input.GetMouseButtonDown(0))
		{
			UpdateDirNow = true;
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			UpdateDirNow = false;
		}

		if (UpdateDirNow)
		{
			UpdateDir();
		}

	}

	public void UpdateDir()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			
		if (Physics.Raycast(ray, out hitInfo))
		{
			var endPoint = hitInfo.point;
				
			var angle = Mathf.Atan2(endPoint.x-transform.position.x,endPoint.z - transform.position.z) * Mathf.Rad2Deg;

			current_Angle_deg = (int)angle;
		}
	}
}