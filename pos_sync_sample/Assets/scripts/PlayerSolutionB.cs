/************************
 * @author keyle_xiao
 * @date Saturday, 29 August 2020 20:11:46
 * @mail keyle_xiao@hotmail.com
 * @site https://vrast.cn
 ************************/

using System;
using UnityEngine;

public class PlayerSolutionB : MonoBehaviour
{
	public Vector3 modifyOffset = Vector3.zero;
	public float smoothSpeed = 0.1f;
	
	public void Start()
	{
		ServerSample.GetInstance().ReceivePack += ReceivePack;
	}

	private PlayerStatePackClient currentPack;

	private Vector3 RuntimePos;
	
	public void OnMove(float passTime,float step,Vector3 pos,float angle)
	{
		if (currentPack.speed == 0)
			return;

		RuntimePos = pos + new Vector3(step * Mathf.Sin(angle),0,step * Mathf.Cos(angle));
	}

	private Vector3 localPositon;
	private float localAnlge;
	
	private void ReceivePack(PlayerStatePack item)
	{
		Debug.Log("其他玩家收到同步包");

		modifyOffset = item.position - transform.position;
		localPositon = transform.position;
		localAnlge = transform.eulerAngles.y;
		var pack = new PlayerStatePackClient();
		pack.SetData(item);

		currentPack = pack;
	}

	public float timeTakenDuringLerp = .2f;
	
	private void Update()
	{
		if (currentPack == null ||currentPack.speed == 0)
			return;
		
		var passTime = Time.time - currentPack.CurrentTime;
		var step = currentPack.speed * passTime;
		var pos = localPositon;//currentPack.position;
		var angle =  currentPack.angle_deg * Mathf.Deg2Rad;
		
		OnMove(passTime,step,pos,angle);

		ApplyPoint(passTime);
	}

	private void ApplyPoint(float passTime)
	{
		float percentageComplete = passTime / timeTakenDuringLerp;
		
		if(percentageComplete >= 1.0f)
		{
			transform.position = RuntimePos + modifyOffset;
			return;
		}

		var offset = Vector3.Lerp(Vector3.zero, modifyOffset,percentageComplete); 
		transform.position = RuntimePos + offset;

		if(percentageComplete*2 >= 1.0f)
		{
			return;
		}
		var yoffset = Mathf.LerpAngle(localAnlge, currentPack.angle_deg,percentageComplete*2);
		transform.eulerAngles = new Vector3(transform.transform.eulerAngles.x,yoffset,transform.transform.eulerAngles.z);
	}
}