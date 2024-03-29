using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
A helper component that enables blending from Mecanim animation to ragdolling and back. 

To use, do the following:

Add "GetUpFromBelly" and "GetUpFromBack" bool inputs to the Animator controller
and corresponding transitions from any state to the get up animations. When the ragdoll mode
is turned on, Mecanim stops where it was and it needs to transition to the get up state immediately
when it is resumed. Therefore, make sure that the blend times of the transitions to the get up animations are set to zero.

TODO:

Make matching the ragdolled and animated root rotation and position more elegant. Now the transition works only if the ragdoll has stopped, as
the blending code will first wait for mecanim to start playing the get up animation to get the animated hip position and rotation. 
Unfortunately Mecanim doesn't (presently) allow one to force an immediate transition in the same frame. 
Perhaps there could be an editor script that precomputes the needed information.

*/

public class RagdollHelper : MonoBehaviour {
	//public property that can be set to toggle between ragdolled and animated character
	public bool ragdolled
	{
		get{
			return state!=RagdollState.animated;
		}
		set{
			if (value==true){
				if (state==RagdollState.animated) {
					//Transition from animated to ragdolled
					setKinematic(false); //allow the ragdoll RigidBodies to react to the environment
//					animator.enabled = false; //disable animation
					state=RagdollState.ragdolled;
				} 
			}
			else {
				if (state==RagdollState.ragdolled) {
					//Transition from ragdolled to animated through the blendToAnim state
					setKinematic(true); //disable gravity etc.
					ragdollingEndTime=Time.time; //store the state change time
					animator.enabled = true; //enable animation
					state=RagdollState.blendToAnim;  
					
					//Store the ragdolled position for blending
					foreach (BodyPart b in bodyParts)
					{
						b.storedRotation=b.transform.rotation;
						b.storedPosition=b.transform.position;
					}
					
					//Remember some key positions
					ragdolledFeetPosition=0.5f*(animator.GetBoneTransform(HumanBodyBones.LeftToes).position + animator.GetBoneTransform(HumanBodyBones.RightToes).position);
					ragdolledHeadPosition=animator.GetBoneTransform(HumanBodyBones.Head).position;
					ragdolledHipPosition=animator.GetBoneTransform(HumanBodyBones.Hips).position;
						
					//Initiate the get up animation
					if (animator.GetBoneTransform(HumanBodyBones.Hips).forward.y>0) //hip hips forward vector pointing upwards, initiate the get up from back animation
					{
						animator.SetBool("GetUpFromBack",true);
					}
					else{
						animator.SetBool("GetUpFromBelly",true);
					}
				} //if (state==RagdollState.ragdolled)
			}	//if value==false	
		} //set
	} 

    //Possible states of the ragdoll
	enum RagdollState
	{
		animated,	 //Mecanim is fully in control
		ragdolled,   //Mecanim turned off, physics controls the ragdoll
		blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
	}

	public Rigidbody head;
	public GameObject neck;
	public Rigidbody spine;
	public Rigidbody lhand, rhand;
	public Rigidbody pelvis;

	public PhysicMaterial physMat;

	public Gauge3d angle = null, force = null;
	private bool init = true;

	public GameObject emptyContainer;

	public Camera swoopCam;

	Component[] rigidComponents;

	//The current state
	RagdollState state=RagdollState.animated;
	
	//How long do we blend when transitioning from ragdolled to animated
	public float ragdollToMecanimBlendTime=0.5f;
	float mecanimToGetUpTransitionTime=0.05f;
	
	//A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
	float ragdollingEndTime=-100;
	
	//Declare a class that will hold useful information for each body part
	public class BodyPart
	{
		public Transform transform;
		public Vector3 storedPosition;
		public Quaternion storedRotation;
	}
	//Additional vectores for storing the pose the ragdoll ended up in.
	Vector3 ragdolledHipPosition,ragdolledHeadPosition,ragdolledFeetPosition;
	
	//Declare a list of body parts, initialized in Start()
	List<BodyPart> bodyParts=new List<BodyPart>();
	
	//Declare an Animator member variable, initialized in Start to point to this gameobject's Animator component.
	Animator animator;
	
	//A helper function to set the isKinematc property of all RigidBodies in the children of the 
	//game object that this script is attached to
	void setKinematic(bool newValue)
	{
		//Get an array of components that are of type Rigidbody
		Component[] components=GetComponentsInChildren(typeof(Rigidbody));

		//For each of the components in the array, treat the component as a Rigidbody and set its isKinematic property
		foreach (Component c in components)
		{
			(c as Rigidbody).isKinematic=newValue;
		}
	}
	
	// Initialization, first frame of game
	void Start ()
	{
		//Set all RigidBodies to kinematic so that they can be controlled with Mecanim
		//and there will be no glitches when transitioning to a ragdoll
		setKinematic(true);
		
		//Find all the transforms in the character, assuming that this script is attached to the root
		Component[] components=GetComponentsInChildren(typeof(Transform));
		
		//For each of the transforms, create a BodyPart instance and store the transform 
		foreach (Component c in components)
		{
			BodyPart bodyPart=new BodyPart();
			bodyPart.transform=c as Transform;
			bodyParts.Add(bodyPart);
		}
		
		//Store the Animator component
		animator=GetComponent<Animator>();

		rigidComponents = GetComponentsInChildren(typeof(Rigidbody));
		foreach (Component c in rigidComponents)
		{
			(c as Rigidbody).collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			(c as Rigidbody).interpolation = RigidbodyInterpolation.Interpolate;
		}


		Component[] colliderComponents=GetComponentsInChildren(typeof(BoxCollider));
		foreach (Component c in colliderComponents)
		{
			(c as BoxCollider).material = physMat;
		}


	}

	int animatorOffWaitFrames = -1;
	bool readyForNext = false;
	bool wasTossed = false;
	float tossCamTimer = 0;

	void ReadyForNext() {
		readyForNext = true;
	}

	private void MoveCam1() {
		float zForce = Mathf.Max (0.33f, force.pos);
		float animTime = 11 + (1 - force.pos) * 5;
		Invoke ("ReadyForNext", animTime);
		iTween.MoveTo(swoopCam.gameObject, iTween.Hash("path", new Vector3[]{swoopCam.transform.position, new Vector3(0, 2f, swoopCam.transform.position.z + 45 * zForce), new Vector3(0,1.7f,64f)}, "time", animTime, "easetype", iTween.EaseType.easeOutBack));	
//		iTween.MoveTo(swoopCam.gameObject, iTween.Hash("path", new Vector3[]{swoopCam.transform.position, new Vector3(0, 2f, swoopCam.transform.position.z + 45 * zForce), new Vector3(0,7.1f,64.18f)}, "time", animTime, "easetype", iTween.EaseType.easeOutBack));	
	}

	private void RotateCam1() {
		iTween.RotateTo(swoopCam.gameObject, new Vector3(30, 180, 0), 3.5f);
//		iTween.RotateTo(swoopCam.gameObject, new Vector3(44, 180, 0), 2.5f);
	}


	public void Toss() {
		ragdolled = true;
//		animator.enabled = false;
		animatorOffWaitFrames = 2;

		float orgForce = force.pos;
		force.pos = 0.5f + force.pos / 1.8f;

		float div = 30;
		float zForce = force.pos * 600/div;
		if (zForce < 200/div)
			zForce = 200/div;
		foreach (Component c in rigidComponents)
		{
			if (c!=head && c!=spine)
//				(c as Rigidbody).AddForce (0,50,800);
//				(c as Rigidbody).AddForce ((angle.pos - 0.5f) * 300 * force.pos, 0, zForce);
				(c as Rigidbody).velocity = new Vector3 ((angle.pos - 0.5f) * 220 * orgForce/div, -3, zForce);

			(c as Rigidbody).drag = 0.14f;
			(c as Rigidbody).angularDrag = 0.1f;
//			(c as Rigidbody).mass = 10f;
		}
//		spine.velocity = new Vector3 ((angle.pos - 0.5f) * 150 * force.pos/div, -3, zForce);

//		Invoke ("MoveCam1", 0.7f);
//		Invoke ("RotateCam1", 1.2f);

		wasTossed = true;
	}

	public bool IsReadyForNext() {
		return readyForNext;
	}


	private bool modX = false;
	void modXer() {
		modX = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (angle == null)
			return;

		if (init) {
			if (angle.state == Gauge3d.Process.Idle)
				angle.Enable ();
			if (angle.state == Gauge3d.Process.Completed && force.state == Gauge3d.Process.Idle) {

				angle.gameObject.SetActive (false);
				force.gameObject.SetActive (true);
				force.gameObject.transform.rotation = Quaternion.Euler (90, 180 - (angle.pos * 2 - 1) * -angle.rotationRange, 0);
				force.gameObject.transform.localScale = new Vector3 (0.1f, 0.0033f + (force.pos / 10), 0.1f);
				//Debug.Break ();

				iTween.RotateTo (emptyContainer.gameObject, new Vector3 (0, (angle.pos - 0.5f) * 15, 0), 0.6f); 
				force.Enable ();
			}
			if (force.state == Gauge3d.Process.Completed) {
				angle.gameObject.SetActive (false);
				force.gameObject.SetActive (false);

				float animSpeed = (0.5f + force.pos / 1.8f) * 1.9f;
				if (animSpeed < 0.5f)
					animSpeed = 0.5f;
				animator.speed = 1 * animSpeed;
				animator.enabled = true;
				animator.SetTrigger ("Jump");
				Invoke ("Toss", 0.7f / animSpeed);
				Invoke ("modXer", 0.55f / animSpeed);
				init = false;
			}
		}

		if (wasTossed) {
			tossCamTimer += Time.deltaTime;

			float camLerp = Mathf.Clamp01 (tossCamTimer / 3.0f);

			swoopCam.transform.position = new Vector3(Mathf.Lerp(swoopCam.transform.position.x, head.transform.position.x, camLerp), Mathf.Lerp(swoopCam.transform.position.y, head.transform.position.y + 1.7f, camLerp), Mathf.Lerp(swoopCam.transform.position.z, head.transform.position.z - 5f, camLerp));
			swoopCam.transform.rotation = Quaternion.Euler(new Vector3 (Mathf.Lerp(33.56f, 30, camLerp), 0, 0));
//			swoopCam.transform.rotation = Quaternion.Euler(new Vector3 (30, 0, 0));


			bool inMotion = false;
			foreach (Component c in rigidComponents)
			{
				Rigidbody rb = (Rigidbody)c;

				if (Mathf.Abs(rb.velocity.x) > 0.2f || Mathf.Abs(rb.velocity.y) > 0.2f || Mathf.Abs(rb.velocity.z) > 0.2f)
					inMotion = true;
			}
			if (!inMotion) {
				readyForNext = true;
				wasTossed = false;
			}

		}

		if (modX && animator.enabled) {
//			emptyContainer.transform.Translate (0.02f, 0, 0);
			emptyContainer.transform.Translate ((angle.pos - 0.5f) * 0.05f, 0, 0);
		}

		animatorOffWaitFrames--;
		if (animatorOffWaitFrames == 0)
			animator.enabled = false;

		if (ragdolled) {
//			head.isKinematic = true;

//			iTween.MoveTo(swoopCam.gameObject, iTween.Hash("position", new Vector3(0,1.7f,88.5f), "time", 4.5f, "easetype", iTween.EaseType.easeOutSine));
//			if (head.useGravity)
//				iTween.RotateTo(neck.gameObject, iTween.Hash("rotation", new Vector3(0, 0, 15), "time", 2.5f, "islocal", true));

//			head.useGravity = false;

//			iTween.RotateTo (neck.gameObject, new Vector3(0,0,15), 3.0f);
//			neck.transform.localRotation = Quaternion.Euler (0, 0, 15);
//			Debug.Break ();


			foreach (Component c in rigidComponents)
			{
				Rigidbody rb = (Rigidbody)c;
				if (rb.drag < 0.14f) {
					rb.drag += 0.01f;
				}
			}

			if (Input.anyKeyDown ) {

				foreach (Component c in rigidComponents)
				{
					Rigidbody rb = (Rigidbody)c;
						rb.drag -= 0.1f;
					if (rb.drag < 0.02f) {
						rb.drag = 0.02f;
					}
				}
			}
		}


	}


	public void SetOutOfBounds () {
		readyForNext = true;
		wasTossed = false;
		this.gameObject.SetActive (false);
	}

	public void SetInactive () {
		readyForNext = true;
		wasTossed = false;
	}

	public bool WasTossed() {
		return wasTossed;
	}


	void LateUpdate()
	{
		//Clear the get up animation controls so that we don't end up repeating the animations indefinitely
//		animator.SetBool("GetUpFromBelly",false);
//		animator.SetBool("GetUpFromBack",false);

		//Blending from ragdoll back to animated
		if (state==RagdollState.blendToAnim)
		{
			if (Time.time<=ragdollingEndTime+mecanimToGetUpTransitionTime)
			{
				//If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
				//character to the best match with the ragdoll
				Vector3 animatedToRagdolled=ragdolledHipPosition-animator.GetBoneTransform(HumanBodyBones.Hips).position;
				Vector3 newRootPosition=transform.position + animatedToRagdolled;
					
				//Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
				RaycastHit[] hits=Physics.RaycastAll(new Ray(newRootPosition,Vector3.down)); 
				newRootPosition.y=0;
				foreach(RaycastHit hit in hits)
				{
					if (!hit.transform.IsChildOf(transform))
					{
						newRootPosition.y=Mathf.Max(newRootPosition.y, hit.point.y);
					}
				}
				transform.position=newRootPosition;
				
				//Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
				Vector3 ragdolledDirection=ragdolledHeadPosition-ragdolledFeetPosition;
				ragdolledDirection.y=0;

				Vector3 meanFeetPosition=0.5f*(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
				Vector3 animatedDirection=animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
				animatedDirection.y=0;
										
				//Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
				//hence setting the y components of the vectors to zero. 
				transform.rotation*=Quaternion.FromToRotation(animatedDirection.normalized,ragdolledDirection.normalized);
			}
			//compute the ragdoll blend amount in the range 0...1
			float ragdollBlendAmount=1.0f-(Time.time-ragdollingEndTime-mecanimToGetUpTransitionTime)/ragdollToMecanimBlendTime;
			ragdollBlendAmount=Mathf.Clamp01(ragdollBlendAmount);
			
			//In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
			//To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
			//and slerp all the rotations towards the ones stored when ending the ragdolling
			foreach (BodyPart b in bodyParts)
			{
				if (b.transform!=transform){ //this if is to prevent us from modifying the root of the character, only the actual body parts
					//position is only interpolated for the hips
					if (b.transform==animator.GetBoneTransform(HumanBodyBones.Hips))
						b.transform.position=Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
					//rotation is interpolated for all body parts
					b.transform.rotation=Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
				}
			}
			
			//if the ragdoll blend amount has decreased to zero, move to animated state
			if (ragdollBlendAmount==0)
			{
				state=RagdollState.animated;
				return;
			}
		}
	}

}
