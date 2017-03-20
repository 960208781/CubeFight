using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 利用CharacterMotor类处理主角的运动逻辑
/// </summary>
[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(DealDamage))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour 
{
	//控制
	public bool sidescroller;									//if true, won't apply vertical input
	public Transform mainCam, floorChecks;						//外部传入主相机和主角身体监测点
	public Animator animator;									//主角的动画控制器
	public AudioClip jumpSound;									//跳跃音
	public AudioClip landSound;									//落地音效
	public static bool ctrlWay = true;							//是否重力感应控制
	public static float isJump = 0f;							//外部传入控制跳跃(0为不跳，值越大力越大)

	//移动
	public float accel = 70f;									//运动时状态的减速度值
	public float airAccel = 18f;			
	public float decel = 7.6f;
	public float airDecel = 1.1f;
	[Range(0f, 5f)]
	public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f;		//空中或者地面的快速旋转量
	public float maxSpeed = 9;									//X/Z 轴的最大速度
	public float slopeLimit = 40, slideAmount = 35;				//最大可以走的斜坡
	public float movingPlatformFriction = 7.7f;					//在移动的方块上玩家受到的摩擦力
	private float h , v;

	//跳跃
	public Vector3 jumpForce =  new Vector3(0, 13, 0);			//标准跳跃的力
	public Vector3 secondJumpForce = new Vector3(0, 13, 0); 	//连跳力
	public Vector3 thirdJumpForce = new Vector3(0, 13, 0);		//三连跳
	public float jumpDelay = 0.1f;								//跳跃的间隔时间
	public float jumpLeniancy = 0.17f;							//how early before hitting the ground you can press jump, and still have it work
	[HideInInspector]
	public int onEnemyBounce;									//敌人的弹跳力
	
	private int onJump;
	private bool grounded;
	private Transform[] floorCheckers;
	private Quaternion screenMovementSpace;
	private float airPressTime, groundedCount, curAccel, curDecel, curRotateSpeed, slope;
	private Vector3 direction, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;
	
	private CharacterMotor characterMotor;
	private EnemyAI enemyAI;
	private DealDamage dealDamage;
	private Rigidbody rigid;
	private AudioSource aSource;

	void Awake()
	{
		//控制方式的切换，如果是重力感应输入，则隐藏掉摇杆
		if (ctrlWay) {
			GameObject.Find ("Joystick").SetActive(false);
		}

		//在主角中心创建一个于地面接触参考点
		if(!floorChecks)
		{
			floorChecks = new GameObject().transform;
			floorChecks.name = "FloorChecks";
			floorChecks.parent = transform;
			floorChecks.position = transform.position;
			GameObject check = new GameObject();
			check.name = "Check1";
			check.transform.parent = floorChecks;
			check.transform.position = transform.position;
			Debug.LogWarning("没有分配给PlayerMove floorChecks的脚本,所以创建了一个主角于地面的接触监测点", floorChecks);
		}
		//给主角分配一个tag
		if(tag != "Player")
		{
			tag = "Player";
			Debug.LogWarning ("PlayerMove脚本分配给对象没有标记“Player”,tag已被自动分配", transform);
		}

		mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
		dealDamage = GetComponent<DealDamage>();
		characterMotor = GetComponent<CharacterMotor>();
		rigid = GetComponent<Rigidbody>();
		aSource = GetComponent<AudioSource>();

		//获取floorcheckers的子物体放在数组中
		//later these are used to raycast downward and see if we are on the ground
		floorCheckers = new Transform[floorChecks.childCount];
		for (int i=0; i < floorCheckers.Length; i++)
			floorCheckers[i] = floorChecks.GetChild(i);
	}
	
	//主角的状态，接收输入值
	void Update()
	{	
		//唤醒刚体组件
		rigid.WakeUp();
		//手势跳跃
		JumpCalculations ();
		//在空中或者地面的时候调整相对的运动值
		curAccel = (grounded) ? accel : airAccel;
		curDecel = (grounded) ? decel : airDecel;
		curRotateSpeed = (grounded) ? rotateSpeed : airRotateSpeed;
				
		//获得相对于相机的运动轴
		screenMovementSpace = Quaternion.Euler (0, mainCam.eulerAngles.y, 0);
		screenMovementForward = screenMovementSpace * Vector3.forward;
		screenMovementRight = screenMovementSpace * Vector3.right;

		//人物移动方式判断
		if (ctrlWay) {
			if (Application.platform.Equals (RuntimePlatform.WindowsEditor) || Application.platform.Equals (RuntimePlatform.WindowsPlayer)) {
				h = Input.GetAxis ("Horizontal");
				v = Input.GetAxis ("Vertical");
				print ("INput输出");
			}else if (Application.platform.Equals (RuntimePlatform.Android) || Application.platform.Equals (RuntimePlatform.IPhonePlayer)) {
				h = Input.acceleration.x * 4;
				v = Input.acceleration.y * 4;
				print ("zhongliganying ");
			}
		} else {
				h = Joystick.h;
				v = Joystick.v;
			print ("anniuyaogan");
		}

		//only apply vertical input to movemement, if player is not sidescroller
		if(!sidescroller)
			direction = (screenMovementForward * v) + (screenMovementRight * h);
		else
			direction = Vector3.right * h;
		moveDirection = transform.position + direction;
	}
	
	//使用fixedUpdate进行物理计算
	void FixedUpdate() 
	{
		//是否接触地面
		grounded = IsGrounded ();
		//移动旋转速度
		characterMotor.MoveTo (moveDirection, curAccel, 0.7f, true);
		if (rotateSpeed != 0 && direction.magnitude != 0)
			characterMotor.RotateToDirection (moveDirection , curRotateSpeed * 5, true);
		characterMotor.ManageSpeed (curDecel, maxSpeed + movingObjSpeed.magnitude, true);
		//set animation values
		if(animator)
		{
			animator.SetFloat("DistanceToTarget", characterMotor.DistanceToTarget);
			animator.SetBool("Grounded", grounded);
			animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
		}
	}
	
	//prevents rigidbody from sliding down slight slopes (read notes in characterMotor class for more info on friction)
	void OnCollisionStay(Collision other)
	{
		//only stop movement on slight slopes if we aren't being touched by anything else
		if (other.collider.tag != "Untagged" || grounded == false)
			return;
		//if no movement should be happening, stop player moving in Z/X axis
		if(direction.magnitude == 0 && slope < slopeLimit && rigid.velocity.magnitude < 2)
		{
			//it's usually not a good idea to alter a rigidbodies velocity every frame
			//but this is the cleanest way i could think of, and we have a lot of checks beforehand, so it should be ok
			rigid.velocity = Vector3.zero;
		}
	}
	
	//returns whether we are on the ground or not
	//also: bouncing on enemies, keeping player on moving platforms and slope checking
	private bool IsGrounded() 
	{
		//get distance to ground, from centre of collider (where floorcheckers should be)
		float dist = GetComponent<Collider>().bounds.extents.y;
		//check whats at players feet, at each floorcheckers position
		foreach (Transform check in floorCheckers)
		{
			RaycastHit hit;
			if(Physics.Raycast(check.position, Vector3.down, out hit, dist + 0.05f))
			{
				if(!hit.transform.GetComponent<Collider>().isTrigger)
				{
					//slope control
					slope = Vector3.Angle (hit.normal, Vector3.up);
					//slide down slopes
					if(slope > slopeLimit && hit.transform.tag != "Pushable")
					{
						Vector3 slide = new Vector3(0f, -slideAmount, 0f);
						rigid.AddForce (slide, ForceMode.Force);
					}
					//enemy bouncing
					if (hit.transform.tag == "Enemy" && rigid.velocity.y < 0)
					{
						enemyAI = hit.transform.GetComponent<EnemyAI>();
						enemyAI.BouncedOn();
						onEnemyBounce ++;
						dealDamage.Attack(hit.transform.gameObject, 1, 0f, 0f);
					}
					else
						onEnemyBounce = 0;
					//moving platforms
					if (hit.transform.tag == "MovingPlatform" || hit.transform.tag == "Pushable")
					{
						movingObjSpeed = hit.transform.GetComponent<Rigidbody>().velocity;
						movingObjSpeed.y = 0f;
						//9.5f is a magic number, if youre not moving properly on platforms, experiment with this number
						rigid.AddForce(movingObjSpeed * movingPlatformFriction * Time.fixedDeltaTime, ForceMode.VelocityChange);
					}
					else
					{
						movingObjSpeed = Vector3.zero;
					}
					//yes our feet are on something
					return true;
				}
			}
		}
		movingObjSpeed = Vector3.zero;
		//no none of the floorchecks hit anything, we must be in the air (or water)
		return false;
	}

	/// <summary>
	/// 跳跃的运算
	/// </summary>
	private void JumpCalculations()
	{
		//keep how long we have been on the ground
		groundedCount = (grounded) ? groundedCount += Time.deltaTime : 0f;
		
		//落地声音
		if(groundedCount < 0.25 && groundedCount != 0 && !GetComponent<AudioSource>().isPlaying && landSound && GetComponent<Rigidbody>().velocity.y < 1)
		{
			aSource.volume = Mathf.Abs(GetComponent<Rigidbody>().velocity.y)/40;
			aSource.clip = landSound;
			aSource.Play ();
		}

		//如在空中按下了跳跃则记录一下时间

		//跳跃的值
//		if (Application.platform.Equals(RuntimePlatform.WindowsEditor)) {
//			isJump = Input.GetAxis ("Jump");
//		}
//			Debug.Log("是否跳跃："+isJump);
//			isJump = Input.touchCount;

		//////////////////////////////////////跳跃脚本//////////////////////////////////////////////////////	
		if (isJump > 0f && !grounded)
			airPressTime = Time.time;
		
		//if were on ground within slope limit
		if (grounded && slope < slopeLimit) {
			//and we press jump, or we pressed jump justt before hitting the ground
				if (isJump > 0f || airPressTime + jumpLeniancy > Time.time) {	
				//increment our jump type if we haven't been on the ground for long
				onJump = (groundedCount < jumpDelay) ? Mathf.Min (2, onJump + 1) : 0;
				//execute the correct jump (like in mario64, jumping 3 times quickly will do higher jumps)
				if (onJump == 0)
					Jump (jumpForce);
				else if (onJump == 1)
					Jump (secondJumpForce);
				else if (onJump == 2) {
					Jump (thirdJumpForce);
					onJump--;
				}
			}
		}
	}
	
	//施加到主角身上的推力
	public void Jump(Vector3 jumpVelocity)
	{
		if(jumpSound)
		{
			aSource.volume = 1;
			aSource.clip = jumpSound;
			aSource.Play ();
		}
		rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
		rigid.AddRelativeForce (jumpVelocity, ForceMode.Impulse);
		airPressTime = 0f;
	}
}