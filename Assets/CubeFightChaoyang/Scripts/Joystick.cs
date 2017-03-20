using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class Joystick : MonoBehaviour, IDragHandler, IEndDragHandler{

	[SerializeField]
	private Transform orignal;//小球的原点
	[SerializeField]
	private Transform limitPoint;
	private float limitValue;

	private Vector2 direction;//方向

	public static float h, v;//移动量,供输出
	//鼠标拖动的过程中：
	//1:让小球随着鼠标的位置移动
	//2:计算小球移动的边界问题
	//3:控制角色的移动
	public void OnDrag (PointerEventData eventData){
		//起始位置
		Vector2 startPoint = orignal.position;

		limitValue = Vector3.Distance (limitPoint.position , orignal.position);

		//当前鼠标的位置
//		Vector2 endPoint = Input.GetTouch(0).position;
		Vector2 endPoint = Input.mousePosition;

		direction = endPoint - startPoint;
		float distance = Vector2.Distance (startPoint , endPoint);
		//如果鼠标移动的距离超过范围 80-图片的半径
		if (distance > limitValue) {
			endPoint = direction.normalized * limitValue + startPoint;
			if (direction.x > Mathf.Abs(direction.y)) {
				h = 1;
			}
			if (direction.y > Mathf.Abs(direction.x)) {
				v = 1;
			}
			if (-direction.x > Mathf.Abs(direction.y)) {
				h = -1;
			}
			if (-direction.y > Mathf.Abs(direction.x)) {
				v = -1;
			}
		}
		transform.position = endPoint;
	}

	//鼠标松开的时候：
	//1:小球复位
	//2:角色停止移动
	public void OnEndDrag (PointerEventData eventData)
	{
		transform.position = orignal.position;
		Debug.Log ("结束拖拽");
		direction = Vector2.zero;
		h = 0;
		v = 0;
	}
}
