using UnityEngine;
using System.Collections;

/// <summary>
/// 销毁游戏对象的基类
/// </summary>
public class DestroyObject : MonoBehaviour 
{
	public AudioClip destroySound;	//当对象被销毁时播放
	public float delay;				//销毁延时值
	public bool destroyChildren;	//是否同时销毁子物体
	public float pushChildAmount;	//push children away from centre of parent
	
	
	void Start()
	{
		//获得子类列表
		Transform[] children = new Transform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++)
			children[i] = transform.GetChild(i);
		
		//分离子类物体
		if (!destroyChildren)
			transform.DetachChildren();
		
		//add force to children (and a bit of spin)
		foreach (Transform child in children)
		{
			Rigidbody rigid = child.GetComponent<Rigidbody>();
			if(rigid && pushChildAmount != 0)
			{
				Vector3 pushDir = child.position - transform.position;
				rigid.AddForce(pushDir * pushChildAmount, ForceMode.Force);
				rigid.AddTorque(Random.insideUnitSphere, ForceMode.Force);
			}
		}
		
		//销毁父类音效
		if (destroySound) {
			AudioSource.PlayClipAtPoint (destroySound, transform.position);
		}
		Destroy (gameObject, delay);
	}
}