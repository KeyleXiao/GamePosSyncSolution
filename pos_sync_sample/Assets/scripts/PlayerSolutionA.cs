/************************
 * @author keyle_xiao
 * @date Saturday, 29 August 2020 20:11:46
 * @mail keyle_xiao@hotmail.com
 * @site https://vrast.cn
 ************************/

using System;
using UnityEngine;

public class PlayerSolutionA : MonoBehaviour
{
	public void Start()
	{
		ServerSample.GetInstance().ReceivePack += ReceivePack;
	}

	private PlayerStatePackClient currentPack;
	
	/// <summary>
	/// 主角前进
	/// </summary>
	public void OnMove()
	{
		if (currentPack.speed == 0)
			return;
		
		var passTime = Time.time - currentPack.CurrentTime;
		var step = currentPack.speed * passTime;
		var pos = currentPack.position;
		var angle =  currentPack.angle_deg * Mathf.Deg2Rad;
		
		transform.position = pos + new Vector3(step * Mathf.Sin(angle),0,step * Mathf.Cos(angle));
		transform.transform.eulerAngles = new Vector3(transform.transform.eulerAngles.x,currentPack.angle_deg,transform.transform.eulerAngles.z);
	}


	private void ReceivePack(PlayerStatePack item)
	{
		Debug.Log("其他玩家收到同步包");
		transform.position = item.position;
		
		var pack = new PlayerStatePackClient();
		pack.SetData(item);

		currentPack = pack;
	}

	private void Update()
	{
		if (currentPack == null ||currentPack.speed == 0)
			return;
		
		OnMove();
	}
}