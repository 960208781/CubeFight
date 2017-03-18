using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// 开始屏幕
/// </summary>
public class StartScreen : MonoBehaviour {

	[SerializeField]
	private Button startBtn;
	[SerializeField]
	private Button settingBtn;
	[SerializeField]
	private GameObject settingWindow;
	[SerializeField]
	private Button moveCtrl;
	[SerializeField]
	private Text moveCtrlText;
	[SerializeField]
	private Text messageText; //通知消息

	void Start () {
		settingWindow.SetActive (false);
		startBtn.onClick.RemoveAllListeners ();
		settingBtn.onClick.RemoveAllListeners ();
		moveCtrl.onClick.RemoveAllListeners ();
		startBtn.onClick.AddListener (LoadScene);
		settingBtn.onClick.AddListener (SettingWindow);
		moveCtrl.onClick.AddListener (MoveCtrl);
	}
	
	void LoadScene () {
		SceneManager.LoadScene ("2D Demo Scene");
	}

	void SettingWindow(){
		settingWindow.SetActive (true);
		//测试提示文字
//		messageText.gameObject.SetActive(messageText.gameObject.activeSelf ? false : true);
	}

	void MoveCtrl(){
		if (settingWindow.activeSelf) {
			PlayerMove.ctrlWay = PlayerMove.ctrlWay ? false : true;
			Debug.Log (PlayerMove.ctrlWay);
			moveCtrlText.text = moveCtrlText.text.Equals ("当前移动方式\n重力感应") ? "当前移动方式\n摇杆控制" : "当前移动方式\n重力感应";
		}
	}

	void Update(){
//		if (!Input.GetTouch(0).Equals(settingWindow)) {
//			settingWindow.SetActive (false);
//		}
	}
}
