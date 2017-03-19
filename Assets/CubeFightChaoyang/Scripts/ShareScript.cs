using UnityEngine;
using System.Collections;
using cn.sharesdk.unity3d;
using UnityEngine.UI;

public class ShareScript : MonoBehaviour {

	public ShareSDK ssdk;
	private string objName;
	[SerializeField]
	private Text MessageView;
	public Button shareBtn;

	void Start () {
		ssdk = gameObject.GetComponent<ShareSDK> ();
		ssdk.shareHandler = OnShareResultHandler;

		shareBtn.onClick.RemoveAllListeners ();
		shareBtn.onClick.AddListener (btnScreenCapOnClick);
	}
	/// <summary>
	/// 分享回调
	/// </summary>
	/// <param name="reqID">Req I.</param>
	/// <param name="state">State.</param>
	/// <param name="type">Type.</param>
	/// <param name="result">Result.</param>
	void OnShareResultHandler (int reqID , ResponseState state , PlatformType type , Hashtable result) {
		if (state == ResponseState.Success) {
			objName = "分享成功"+MiniJSON.jsonEncode(result);
			print (objName);
		}else if (state == ResponseState.Fail){
			print ("分享失败");
		}else if (state == ResponseState.Cancel){
			print ("Cancel");
		}
	}

	public void btnScreenCapOnClick(){
		MessageView.text = "开始截屏";
		Application.CaptureScreenshot ("ScreenShot.png");
		StartCoroutine (ScreenshotTime(2));
		MessageView.text = "截屏中···";
		MessageView.text = "截屏完成";
		DestroyObject (MessageView , 2f);
	}

	private IEnumerator ScreenshotTime(float a){
		yield return new WaitForSeconds (a);
		string imagePath = Application.persistentDataPath + "/ScreenShot.png";

		ShareContent content = new ShareContent ();
		content.SetText ("朝阳-分享测试");
		content.SetImagePath (imagePath);
//		content.SetImagePath ("http://www.ceeger.com/forum/attachment/1703/thread/14_140593_046c4f378b99d74.png");
		content.SetTitle ("测试");
		content.SetShareType (ContentType.Image);
		ssdk.ShowPlatformList (null , content , 100 , 100);
//		ssdk.ShowShareContentEditor (PlatformType.QQ , content);
	}
}
