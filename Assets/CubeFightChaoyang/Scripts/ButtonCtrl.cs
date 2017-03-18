using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonCtrl : MonoBehaviour , IPointerUpHandler , IPointerDownHandler {
	

	public void OnPointerDown (PointerEventData eventData)
	{
		PlayerMove.isJump = 1f;
		print ("点击跳跃按钮Button值"+PlayerMove.isJump);
	}

	public void OnPointerUp (PointerEventData eventData)
	{
		PlayerMove.isJump = 0f;
		print (PlayerMove.isJump);
	}


	void Start () {
		
	}

	void Update(){
		
	}
}
