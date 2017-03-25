using UnityEngine;

public class CameraFollow : MonoBehaviour 
{
	public Transform target;									//相机跟随的目标物体
	public Vector3 targetOffset =  new Vector3(0f, 3.5f, 7);	//how far back should camera be from the lookTarget
	public bool lockRotation;									//should the camera be fixed at the offset (for example: following behind the player)
	public float followSpeed = 6;								//相机移动跟随的移动速度
	public float inputRotationSpeed = 100;						//当点击了的相机调整按钮后，相机围绕目标点的旋转速度
	public bool mouseFreelook;									//相机旋转是否跟随鼠标（当相机是非固定的时候）
	public float rotateDamping = 100;							//相机旋转速度
	public GameObject waterFilter;								//当相机在水下的时候，在相机前边显示个贴图模拟水下的颜色
	public string[] avoidClippingTags;							//tags for big objects in your game, which you want to camera to try and avoid clipping with
	
	private Transform followTarget;
	private bool camColliding;									//相机碰撞点检测
	
	//设置对象
	void Awake()
	{
		followTarget = new GameObject().transform;				//自动创建新的游戏对象作为相机的替代
		followTarget.name = "Camera Target";
		if(waterFilter)
			waterFilter.GetComponent<Renderer>().enabled = false;
		if(!target)
			Debug.LogError("'CameraFollow script' 没有设置跟随物体", transform);
		
		//如果是鼠标控制视角，则禁止平滑旋转
		if(mouseFreelook)
			rotateDamping = 0f;
	}
	
	//相机的逻辑方法
	void Update()
	{
		if (!target)		//如果没有获取到跟随目标，则直接return
			return;
		
		SmoothFollow ();
		if(rotateDamping > 0)
			SmoothLookAt();
		else
			transform.LookAt(target.position);
	}
		
	//如果相机进入水池，则相机前边的“片”显示
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag ("Water") && waterFilter)
			waterFilter.GetComponent<Renderer>().enabled = true;
	}
	
	//如果相机离开水池，则相机前边的“片”不显示
	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag ("Water") && waterFilter)
			waterFilter.GetComponent<Renderer>().enabled = false;
	}
	
	//相机向着目标平滑旋转
	void SmoothLookAt()
	{
		Quaternion rotation = Quaternion.LookRotation (target.position - transform.position);
		transform.rotation = Quaternion.Slerp (transform.rotation, rotation, rotateDamping * Time.deltaTime);
	}
		
	//相机朝着目标平滑移动
	void SmoothFollow()
	{
		//move the followTarget (empty gameobject created in awake) to correct position each frame
		followTarget.position = target.position;
		followTarget.Translate(targetOffset, Space.Self);
		if (lockRotation)
			followTarget.rotation = target.rotation;
		
		if(mouseFreelook)
		{
			//mouse look
			float axisX = Input.GetAxis ("Mouse X") * inputRotationSpeed * Time.deltaTime;
			followTarget.RotateAround (target.position,Vector3.up, axisX);
			float axisY = Input.GetAxis ("Mouse Y") * inputRotationSpeed * Time.deltaTime;
			followTarget.RotateAround (target.position, transform.right, -axisY);
		}
		else
		{
			//相机旋转的快捷键
			float axis = Input.GetAxis ("CamHorizontal") * inputRotationSpeed * Time.deltaTime;
			followTarget.RotateAround (target.position, Vector3.up, axis);
		}
		
		//where should the camera be next frame?
		Vector3 nextFramePosition = Vector3.Lerp(transform.position, followTarget.position, followSpeed * Time.deltaTime);
		Vector3 direction = nextFramePosition - target.position;
		//raycast to this position
		RaycastHit hit;
		if(Physics.Raycast (target.position, direction, out hit, direction.magnitude + 0.3f))
		{
			transform.position = nextFramePosition;
			foreach(string tag in avoidClippingTags)
				if(hit.transform.tag == tag)
					transform.position = hit.point - direction.normalized * 0.3f;
		}
		else
		{
			//otherwise, move cam to intended position
			transform.position = nextFramePosition;
		}
	}
}