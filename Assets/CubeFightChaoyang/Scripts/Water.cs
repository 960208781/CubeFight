using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//水的浮力效果的实现
[RequireComponent(typeof(BoxCollider))]
public class Water : MonoBehaviour 
{
	public AudioClip splashSound;						//当对象进入水的音效
	public Vector3 force = new Vector3(0, 16.5f, 0);	//在水中所受的浮力
	public bool effectPlayerDrag;						//主角的rigidbody是否影响水的浮力值
	public float resistance = 0.4f;						//在水中实施拖拽效果(but not player)
	public float angularResistance = 0.2f;				//在水中的旋转阻力but not player)
	
	private Dictionary<GameObject, float> dragStore = new Dictionary<GameObject, float>();
	private Dictionary<GameObject, float> angularStore = new Dictionary<GameObject, float>();
	
	void Awake()
	{
		if(tag != "Water")
		{
			tag = "Water";
			Debug.LogWarning("'Water' 请将 'Water'tag 'Water'重新赋值！", transform);
		}
		GetComponent<Collider>().isTrigger = true;		//将刚体设置为触发器
	}
	
	//浮力处理
	void OnTriggerStay(Collider other)
	{
		//get surface position
		float surface = transform.position.y + GetComponent<Collider>().bounds.extents.y;
		Rigidbody rigid = other.GetComponent<Rigidbody>();
		if(rigid)
		{
			//get object depth
			float depth = surface - other.transform.position.y;
			//if below surface, push object
			if(depth > 0.4f)
				rigid.AddForce(force, ForceMode.Force);
			//if we are near the surface, add less force, this prevents objects from "jittering" up and down on the surface
			else
				rigid.AddForce (force * (depth * 2), ForceMode.Force);
		}
	}
	
	//sets drag on objects entering water
	void OnTriggerEnter(Collider other)
	{
		//rigidbody entered water?
		Rigidbody r = other.GetComponent<Rigidbody>();
		if(r)
		{
			if(splashSound)
			{
				float volume = other.GetComponent<Rigidbody>().velocity.magnitude/5;
				AudioSource.PlayClipAtPoint(splashSound, other.transform.position, volume);
			}
			//stop if we arent effecting player
			if (r.tag == "Player" && !effectPlayerDrag)
				return;
	
			//store objects default drag values
			dragStore.Add (r.gameObject, r.drag);
			angularStore.Add(r.gameObject, r.angularDrag);
			
			//apply new drag values to object
			r.drag = resistance;
			r.angularDrag = angularResistance;
		}
		else if(splashSound)
			AudioSource.PlayClipAtPoint(splashSound, other.transform.position);
	}
	
	//reset drag on objects leaving water
	void OnTriggerExit(Collider other)
	{
		//rigidbody entered water?
		Rigidbody r = other.GetComponent<Rigidbody>();
		if(r)
		{
			//stop if we arent effecting player
			if(r.tag == "Player" && !effectPlayerDrag)
				return;
			
			//see if we've stored this objects default drag values
			if (dragStore.ContainsKey(r.gameObject) && angularStore.ContainsKey(r.gameObject))
			{
				//restore values
				r.drag = dragStore[r.gameObject];
				r.angularDrag = angularStore[r.gameObject];
				//remove stored values for this object
				dragStore.Remove(r.gameObject);
				angularStore.Remove (r.gameObject);
			}
			else
			{
				//restore default values incase we cant find it in list (for whatever reason)
				r.drag = 0f;
				r.angularDrag = 0.05f;
				print ("Object left water: couldn't get drag values, restored to defaults");
			}
		}
	}
}