using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonCtrl : MonoBehaviour {

	public Button btnBackMainScene;					//返回主场景
	public Button btnJump;							//跳跃键

	void Start(){
		btnBackMainScene.onClick.RemoveAllListeners ();
		btnJump.onClick.RemoveAllListeners ();

		btnBackMainScene.onClick.AddListener (BackMainScene);
//		btnJump.onClick.AddListener (IsJump);
	}

	//按下跳跃
	public void btnJumpDown ()
	{
		PlayerMove.isJump = 1f;
		print ("点击跳跃按钮Button值"+PlayerMove.isJump);
	}

	//抬起停止跳跃
	public void btnJumpUp(){
		PlayerMove.isJump = 0f;
		Debug.Log ("抬起跳跃按钮");
	}

	/// <summary>
	/// 返回主场景
	/// </summary>
	public void BackMainScene ()
	{
		SceneManager.LoadScene (0);
	}
}

