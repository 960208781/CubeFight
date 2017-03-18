using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour 
{	
	public GUISkin guiSkin;					//assign the skin for GUI display
	[HideInInspector]
	public int coinsCollected;

	private int coinsInLevel;
	private Health health;
	
	//setup, get how many coins are in this level
	void Start()
	{
		coinsInLevel = GameObject.FindGameObjectsWithTag("Coin").Length;		
		health = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
	}
	
	//show current health and how many coins you've collected
	void OnGUI()
	{
		GUI.skin = guiSkin;
		GUILayout.Space(5f);
		
		if(health)
			GUILayout.Label ("血量: " + health.currentHealth);
		if(coinsInLevel > 0)
			GUILayout.Label ("当前敌人: " + coinsCollected + " / " + coinsInLevel);
		
		GUILayout.Label ("陀螺仪X"+Input.acceleration.x.ToString());
		GUILayout.Label ("陀螺仪Y"+Input.acceleration.y.ToString());

//		if (GUILayout.Button("分享")) {
//			Debug.Log("分享按钮");
//		}
	}
}