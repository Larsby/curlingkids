using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeReader : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler {

	private Vector2 startPos;
	private float startTime;
	private bool wasFlicked = false;
	private bool isOngoing = false;
	private Vector2 endForce;
	private float endSpeed;

	private bool useLineRenderer = true;
	private LineRenderer lineRenderer;
	private List<Vector3> drawPoints = new List<Vector3> ();

	private float maxX = 1;
	private float maxTime = 0.3f;

	private Vector3 dollPos = Vector3.zero;
	private Vector3 corrPos = Vector3.zero;

	public float pinModZ = 0.25f;

	private bool mixEndTimeWithHold = false;

	private bool reverseForce = false;
	private bool hideWrongDirectionLine = false;

	private float minYDist = 0.4f;
    float incCounter = 0.0f;

    public Material lineMaterial;

	void Awake() {
		lineRenderer = GetComponent<LineRenderer> ();
		lineRenderer.startWidth = 0.05f;
		lineRenderer.endWidth = 0.04f;
		Reset ();
	}

	void Start () {}

	private float textureOffsetPos = 0;
	private float textureOffsetSpeed = 1f;
	private float curveSpeed = 0.05f;

    void perlinLineRenderer()
    {
		incCounter += curveSpeed * Time.deltaTime;
  
        AnimationCurve curve = new AnimationCurve();

        for (int i = 0; i < 10;i++)
        {
            curve.AddKey(i/9, Mathf.PerlinNoise(0, i*(Mathf.Sin(incCounter * 1) * 2)) * 0.25f );
        }
		textureOffsetPos -= textureOffsetSpeed * Time.deltaTime;

        lineRenderer.widthCurve = curve;
		float offset = textureOffsetPos;
         lineMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }

	void Update () {

        //todo, add check for length here
       
        perlinLineRenderer();
  
	}

	void UpdateLineRenderer(Vector2 point) {

		if ((Time.time - startTime > maxTime && StaticManager.useMaxTime) || wasFlicked || useLineRenderer == false) {
			lineRenderer.positionCount = 0;
			return;
		}

		if (drawPoints.Count >= 2)
			drawPoints.RemoveAt (1);
		
		Vector3 tempV;
		if (!reverseForce) {
			tempV.x = point.x;
			tempV.y = point.y;

			if (point.y <= startPos.y && hideWrongDirectionLine)
				tempV = startPos;
		} else {
			tempV.x = Screen.width - point.x;
			tempV.y = point.y;
			if (drawPoints.Count > 0) {

				tempV.y = startPos.y - (point.y - startPos.y);

				// Double line length; works so-so and accentuates errors in 2d to 3d conversion
				//tempV.x = tempV.x * 2 - Screen.width / 2;
				//stempV.y = startPos.y - (point.y - startPos.y) * 2;

				if (point.y >= startPos.y && hideWrongDirectionLine) {
					tempV.x = Screen.width - startPos.x;
					tempV.y = startPos.y;
				}
			}

		}
		tempV.z = 2;
    
		drawPoints.Add (Camera.main.ScreenToWorldPoint(tempV + corrPos));

		if (drawPoints.Count < 2) {
			lineRenderer.positionCount = 0;
			return;
		}
        Gradient gradient = new Gradient();
        float alpha = 0.9f;
        float endAplha = 0.0f;
            float length = Vector2.Distance(point, startPos) / 600;

       
		Color startcol = GameUtil.IntColor(0, 255, 0);

		Color endcol = GameUtil.IntColor(255, 0, 0);
		//Color endcol = GameUtil.IntColor(130, 214, 255);
		//Color endcol = GameUtil.IntColor(255, 255, 255);


        Color shootCOl = Color.Lerp(startcol, endcol, length  );
     
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(shootCOl, 0.0f), new GradientColorKey(shootCOl, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(endAplha, 1.0f) }
            );
        lineRenderer.colorGradient = gradient; 

		lineRenderer.positionCount = drawPoints.Count;
       		for(var i = 0; i < drawPoints.Count; i++){
                    lineRenderer.SetPosition(i,drawPoints[i]);    
                }
        
       //     lineRenderer.SetPosition(0, drawPoints[1]);
      //  lineRenderer.SetPosition(1, drawPoints[0]);

	}


	private void CalculateEndForce(PointerEventData aEventData, bool endCalc = false) {

		float xDist = 0, yDist = 0;

		if (!reverseForce && (int)Camera.main.transform.rotation.eulerAngles.z != 180) {
			xDist = ((aEventData.position.x - startPos.x) / Screen.dpi) * 2.5f;
			yDist = ((aEventData.position.y - startPos.y) / Screen.dpi) * 2.5f;
		} else {
			xDist = ((startPos.x - aEventData.position.x) / Screen.dpi) * 2.5f;
			yDist = ((startPos.y - aEventData.position.y) / Screen.dpi) * 2.5f;
		}
		float deltaTime = Time.time - startTime;

		float tossAngle = Mathf.Atan2 (yDist, xDist);

		yDist -= minYDist;

		tossAngle -= (Mathf.PI / 2);
		tossAngle = -tossAngle;
		//Debug.Log ("ANGLE: " + tossAngle + "   Dist: " + xDist + " " + yDist + " over s: " + deltaTime);

		if ((deltaTime > maxTime && StaticManager.useMaxTime) || (yDist < 0.02f && endCalc)) {
			lineRenderer.positionCount = 0;
			isOngoing = false;
			return;
		}

		float forceSpeed = 0;

		if (StaticManager.useMaxTime || (mixEndTimeWithHold && deltaTime <= maxTime)) {
			float deltaAdd = deltaTime * 0.25f;
			forceSpeed = (yDist * 1.2f) * ((maxTime + deltaAdd) - deltaTime);
		} else {
			forceSpeed = (yDist * 0.99f) * 0.3f;
		}

		endSpeed = Mathf.Clamp01 (forceSpeed); // reported indicator speed clamped to 0-1
		//forceSpeed = Mathf.Clamp (forceSpeed, 0.3f, 1); // real speed clamped to 0.3-1
		forceSpeed = 0.3333f + endSpeed * 0.6666f; // real speed clamped to 0.33-1 but wider range of 0.3333-1 than line above

		// 0-45(PI/4) degrees: increase speed linearly approaching 45, 45-90(PI/2) degrees decrease speed linearly approaching 90
		float absAngle = Mathf.Abs (tossAngle);
		float addedForce = Mathf.Clamp (absAngle, 0, Mathf.PI / 4);
		if (absAngle < Mathf.PI / 4f)
			addedForce /= 2f;
		else
			addedForce /= 2f + ((absAngle - Mathf.PI / 4f) * 10f);
		addedForce += 1f;

		// 33-90 degrees = decrease speed linearly stage 2 (after adding 1 above, so we can go below 1 here)
/*		if (absAngle > Mathf.PI / 6f)
			addedForce -= ((absAngle - Mathf.PI / 6f) * 0.8f);
		addedForce = Mathf.Clamp (addedForce, 0.7f, 10); */

		//Debug.Log (addedForce + "  " + forceSpeed + "");

		endForce.x = forceSpeed * (tossAngle / (Mathf.PI / 2)) * addedForce;
		endForce.y = forceSpeed * ((Mathf.PI/2 - Mathf.Abs(tossAngle)) / (Mathf.PI / 2)) * addedForce;

		if (forceSpeed <= 0.334)
			endForce.x = endForce.y = 0;

		// Debug.Log (endForce.ToString("F4"));
	}


	private int firstTouchId = -1;

	public void OnBeginDrag(PointerEventData aEventData) {
//		Debug.Log("BEGIN: " + aEventData.position);

		if (firstTouchId != -1)
			return;

		DiscardHelp();

		firstTouchId = aEventData.pointerId;

		startPos = aEventData.position;

		corrPos = Vector3.zero;

		Vector3 fixedPos = startPos;
		if (reverseForce)
			fixedPos.x = Screen.width - fixedPos.x;
		
		if (StaticManager.fixedSwipeStartPoint && dollPos != Vector3.zero) {
			corrPos.x = dollPos.x - fixedPos.x;
			corrPos.y = dollPos.y - fixedPos.y;
			corrPos.z = 0;
		}

		startTime = Time.time;

		isOngoing = true;

		drawPoints.Clear ();
		UpdateLineRenderer (startPos);
	}


	public void OnEndDrag(PointerEventData aEventData) {
//		Debug.Log("END: " + aEventData.position);

		if (aEventData.pointerId != firstTouchId)
			return;
		firstTouchId = -1;
		
		if (StaticManager.newEndforceCalculation)
			CalculateEndForce (aEventData, true);
		else
			OLD_CalculateEndForce (aEventData, true);

		if (isOngoing) {
			wasFlicked = true;
			UpdateLineRenderer (aEventData.position);
		}
		isOngoing = false;
	}
		
	public void OnDrag(PointerEventData aEventData) {
		
		if (aEventData.pointerId != firstTouchId)
			return;

		if (StaticManager.newEndforceCalculation)
			CalculateEndForce (aEventData);
		else
			OLD_CalculateEndForce (aEventData);

		UpdateLineRenderer (aEventData.position);

//		Debug.Log("" + aEventData.position + " "+ Screen.width+"x"+Screen.height + " "+Screen.dpi);
	}

	public void Reset() {
		endForce = Vector2.zero;
		wasFlicked = false;
		drawPoints.Clear ();
		if (lineRenderer != null)
			lineRenderer.positionCount = 0;
	}

	public bool WasFlicked() {
		return wasFlicked;
	}

	public bool IsOnGoing() {
		return isOngoing;
	}

	public float GetXForce() {
		return endForce.x;
	}
	public float GetYForce() {
		return endForce.y;
	}
	public float GetSpeed() {
		return endSpeed;
	}

	public void SetDollPos(Vector3 dollStartPos, Vector3 camPos = default(Vector3), Quaternion camRot = default(Quaternion)) {

		if (camPos != Vector3.zero) {
			Vector3 oldCamPos = Camera.main.transform.position;
			Quaternion oldCamRot = Camera.main.transform.rotation;
			Camera.main.transform.position = camPos;
			Camera.main.transform.rotation = camRot;
			dollPos = Camera.main.WorldToScreenPoint (GameUtil.AddZ(dollStartPos, pinModZ));
			Camera.main.transform.position = oldCamPos;
			Camera.main.transform.rotation = oldCamRot;
		}
		else
			dollPos = Camera.main.WorldToScreenPoint (GameUtil.AddZ(dollStartPos, pinModZ));
		// Debug.Log (dollPos);
	}

	// Old version of endforce calculation
	private void OLD_CalculateEndForce(PointerEventData aEventData, bool endCalc = false) {

		float xDist = ((aEventData.position.x - startPos.x) / Screen.dpi) * 2.5f;
		float yDist = ((aEventData.position.y - startPos.y) / Screen.dpi) * 2.5f;
		float deltaTime = Time.time - startTime;

		float tossAngle = Mathf.Atan2 (yDist, xDist);

		tossAngle -= (Mathf.PI / 2);
		tossAngle = -tossAngle;
		//Debug.Log ("ANGLE: " + tossAngle + "   Dist: " + xDist + " " + yDist + " over s: " + deltaTime);

		if ((deltaTime > maxTime && StaticManager.useMaxTime) || (yDist < 0.2f && endCalc)) {
			lineRenderer.positionCount = 0;
			isOngoing = false;
			return;
		}

		endForce.x = (xDist * 2) * 0.15f;
		//endForce.y = (yDist * 0.99f) * 0.15f;

		endForce.x = (tossAngle) * 1.3f;


		if (StaticManager.useMaxTime) {
			//endForce.y = (yDist * 0.85f) * (deltaTime * 0.75f); // = increase if swiping long time rather than decrease
			float deltaAdd = deltaTime * 0.25f;
			endForce.y = (yDist * 1.2f) * ((maxTime + deltaAdd) - deltaTime);
		} else {
			endForce.y = (yDist * 0.99f) * 0.3f;
		}

		endForce.x = Mathf.Clamp (endForce.x, -maxX, maxX);
		endForce.y = Mathf.Clamp (endForce.y, 0f, 1f);

		// Debug.Log (endForce.ToString("F4"));
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		DiscardHelp();
	}

	private void DiscardHelp()
	{
		HelpDisplayer helpDiscard = GameObject.FindObjectOfType<HelpDisplayer>();
		if (helpDiscard) helpDiscard.End();
	}

}
