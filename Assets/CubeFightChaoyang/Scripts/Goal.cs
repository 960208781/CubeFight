using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CapsuleCollider))]
public class Goal : MonoBehaviour 
{
	public float lift;				//举起的力量
	public float loadDelay;			//持续时间
	public int nextLevelIndex;		//scene下标
	
	private float counter;
	
	void Awake()
	{
		GetComponent<Collider>().isTrigger = true;
	}
	
	//当主角在触发器内部的时常超过一定时间的时候加载下一个场景
	void OnTriggerStay(Collider other)
	{
		Rigidbody rigid = other.GetComponent<Rigidbody>();
		if(rigid)
			rigid.AddForce(Vector3.up * lift, ForceMode.Force);
		
		if (other.CompareTag("Player"))
		{
			counter += Time.deltaTime;
			if(counter > loadDelay)
			SceneManager.LoadScene (nextLevelIndex);
		}
	}
	
	//如果主角离开触发器，则重置他的计时器时间
	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
			counter = 0f;
	}
}