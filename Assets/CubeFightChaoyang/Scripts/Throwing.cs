using UnityEngine;
using System.Collections;

//允许玩家可以把游戏对象进行抛掷
//需要把tag设置为"Pickup"或"Pushable"
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMove))]
public class Throwing : MonoBehaviour 
{
	public AudioClip pickUpSound;								//收集物品的声音
	public AudioClip throwSound;								//抛掷物体的声音
	public GameObject grabBox;									//触发的物体范围
	public Vector3 holdOffset;									//position offset from centre of player, to hold the box (used to be "gap" in old version)
	public Vector3 throwForce = new Vector3(0, 5, 7);			//抛掷物体的力坐标
	public float rotateToBlockSpeed = 3;						//推动物体的速度
	public float checkRadius = 0.5f;							//检查半径
	[Range(0.1f, 1f)]											//new weight of a carried object, 1 means no change, 0.1 means 10% of its original weight													
	public float weightChange = 0.3f;							//携带对象的时候不影响运动效果
	[Range(10f, 1000f)]
	public float holdingBreakForce = 45, holdingBreakTorque = 45;//可以破坏对象的力
	public Animator animator;									 //动画控制器
	public int armsAnimationLayer;								 //动画层的下标
	
	[HideInInspector]
	public GameObject heldObj;
	private Vector3 holdPos;
	private FixedJoint joint;
	private float timeOfPickup, timeOfThrow, defRotateSpeed;
	private Color gizmoColor;
	private AudioSource aSource;
	
	private PlayerMove playerMove;
	//private CharacterMotor characterMotor;	line rendererd unnecessary for now. (see line 85)
	private TriggerParent triggerParent;
	private RigidbodyInterpolation objectDefInterpolation;
	
	
	//setup
	void Awake()
	{
		aSource = GetComponent<AudioSource>();
		//create grabBox is none has been assigned
		if(!grabBox)
		{
			grabBox = new GameObject();
			grabBox.AddComponent<BoxCollider>();
			grabBox.GetComponent<Collider>().isTrigger = true;
			grabBox.transform.parent = transform;
			grabBox.transform.localPosition = new Vector3(0f, 0f, 0.5f);
			grabBox.layer = 2;	//ignore raycast by default
			Debug.LogWarning("No grabBox object assigned to 'Throwing' script, one has been created and assigned for you", grabBox);
		}
		
		playerMove = GetComponent<PlayerMove>();
		//characterMotor = GetComponent<CharacterMotor>(); line rendererd unnecessary for now. (see line 85)
		defRotateSpeed = playerMove.rotateSpeed;
		//set arms animation layer to animate with 1 weight (full override)
		if(animator)
			animator.SetLayerWeight(armsAnimationLayer, 1);
	}
	
	//throwing/dropping
	void Update()
	{
		//when we press grab button, throw object if we're holding one
		if (Input.GetButtonDown ("Grab") && heldObj && Time.time > timeOfPickup + 0.1f)
		{
			if(heldObj.tag == "Pickup") 
				ThrowPickup();
		}
		//set animation value for arms layer
		if(animator)
			if(heldObj && heldObj.tag == "Pickup")
				animator.SetBool ("HoldingPickup", true);
			else
				animator.SetBool ("HoldingPickup", false);
		
			if(heldObj && heldObj.tag == "Pushable")
				animator.SetBool ("HoldingPushable", true);
			else
				animator.SetBool ("HoldingPushable", false);
		
		//if we're holding a pushable, rotate to face it
		if (heldObj && heldObj.tag == "Pushable")
		{
			/*the below line would rotate the character to the thing its grabbing. Unity(5.1) physics seems to have broken this
		     * because you cannot create a joint between two objects then rotate them independently.
			 * perhaps it will be fixed in a future update and you can uncomment this and see if it works, but right now its left out
			 * characterMotor.RotateToDirection(heldObj.transform.position, rotateToBlockSpeed, true);
			 */

			//if we let go of grab key, drop the pushable
			if(Input.GetButtonUp ("Grab"))
			{
				DropPushable();
			}
			
			if(!joint)
			{
				DropPushable();
				print ("'Pushable' object dropped because the 'holdingBreakForce' or 'holdingBreakTorque' was exceeded");
			}
		}
	}
	
	//pickup/grab
	void OnTriggerStay(Collider other)
	{
		//if grab is pressed and an object is inside the players "grabBox" trigger
		if(Input.GetButton("Grab"))
		{
			//pickup
			if(other.tag == "Pickup" && heldObj == null && timeOfThrow + 0.2f < Time.time)
				LiftPickup(other);
			//grab
			if(other.tag == "Pushable" && heldObj == null && timeOfThrow + 0.2f < Time.time)
				GrabPushable(other);
		}
	}
			
	private void GrabPushable(Collider other)
	{
		heldObj = other.gameObject;
		objectDefInterpolation = heldObj.GetComponent<Rigidbody>().interpolation;
		heldObj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		AddJoint ();
		//no breakForce limit for pushables anymore because Unity 5's new physics system broke this. Perhaps it'll be fixed in future
		joint.breakForce = Mathf.Infinity;
		joint.breakTorque = Mathf.Infinity;
		//stop player rotating in direction of movement, so they can face the block theyre pulling
		playerMove.rotateSpeed = 0;
	}
	
	private void LiftPickup(Collider other)
	{
		//get where to move item once its picked up
		Mesh otherMesh = other.GetComponent<MeshFilter>().mesh;
		holdPos = transform.position + transform.forward * holdOffset.z + transform.right * holdOffset.x + transform.up * holdOffset.y;
		holdPos.y += (GetComponent<Collider>().bounds.extents.y) + (otherMesh.bounds.extents.y);
		
		//if there is space above our head, pick up item (this uses the defaul CheckSphere layers, you can add a layerMask parameter here if you need to though!)
		if(!Physics.CheckSphere(holdPos, checkRadius))
		{
			gizmoColor = Color.green;
			heldObj = other.gameObject;
			objectDefInterpolation = heldObj.GetComponent<Rigidbody>().interpolation;
			heldObj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
			heldObj.transform.position = holdPos;
			heldObj.transform.rotation = transform.rotation;
			AddJoint();
			//here we adjust the mass of the object, so it can seem heavy, but not effect player movement whilst were holding it
			heldObj.GetComponent<Rigidbody>().mass *= weightChange;
			//make sure we don't immediately throw object after picking it up
			timeOfPickup = Time.time;
		}
		//if not print to console (look in scene view for sphere gizmo to see whats stopping the pickup)
		else
		{
			gizmoColor = Color.red;
			print ("Can't lift object here. If nothing is above the player, perhaps you need to add a layerMask parameter to line 136 of the code in this script," +
				"the CheckSphere function, in order to make sure it isn't detecting something above the players head that is invisible");
		}
	}
	
	private void DropPushable()
	{
		heldObj.GetComponent<Rigidbody>().interpolation = objectDefInterpolation;
		Destroy (joint);
		playerMove.rotateSpeed = defRotateSpeed;
		heldObj = null;
		timeOfThrow = Time.time;
	}
	
	public void ThrowPickup()
	{
		if(throwSound)
		{
			aSource.volume = 1;
			aSource.clip = throwSound;
			aSource.Play ();
		}
		Destroy (joint);

		Rigidbody r = heldObj.GetComponent<Rigidbody>();
		r.interpolation = objectDefInterpolation;
		r.mass /= weightChange;
		r.AddRelativeForce (throwForce, ForceMode.VelocityChange);

		heldObj = null;
		timeOfThrow = Time.time;
	}
	
	//connect player and pickup/pushable object via a physics joint
	private void AddJoint()
	{
		if (heldObj)
		{
			if(pickUpSound)
			{
				aSource.volume = 1;
				aSource.clip = pickUpSound;
				aSource.Play ();
			}
			joint = heldObj.AddComponent<FixedJoint>();
			joint.connectedBody = GetComponent<Rigidbody>();
		}
	}
	
	//draws red sphere if something is in way of pickup (select player in scene view to see)
	void OnDrawGizmosSelected()
	{
		Gizmos.color = gizmoColor;
		Gizmos.DrawSphere (holdPos, checkRadius);
	}
}

/*注意:检查如果球员能举起一个对象,并没有在他们的头上,sphereCheck使用。(第136行)
*这个函数使用默认layermask,但是如果你发现它不是适合你的游戏的层次,你可能需要创建你的
*自己的公共LayerMask变量,将其分配给函数作为最后一个参数*/