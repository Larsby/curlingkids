using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour {

	private const float MUL = 7000000;
	private const int NOF = 16;

	private const float BODYMUL = 70000000;

	public GameObject CapsulePrefab;

	private List<Rigidbody> pieces = new List<Rigidbody>();

	public Rigidbody body;
	public MeshRenderer beast;
	public Rigidbody body2;


	void Start () {

		for (int i = 0; i < NOF; i++) {
			GameObject tmp = Instantiate (CapsulePrefab, new Vector3 (NOF/2 * -2 + i * 2, 1.15f, NOF/2 * -2 + i * 2), Quaternion.identity);
			Rigidbody rb = tmp.GetComponent<Rigidbody> ();
			pieces.Add (rb);
		}

	}
	
	void Update () {
		
		if (Input.GetKeyDown (KeyCode.A)) {
			spreadCapsules ();
		}
		if (Input.GetKeyDown (KeyCode.S)) {
			spreadCapsules (2);
		}

		if (Input.GetKeyDown (KeyCode.N)) {
			tossBody ();
		}
		if (Input.GetKeyDown (KeyCode.M)) {
			tossBody (2);
		}
		if (Input.GetKeyDown (KeyCode.H)) {
			beast.enabled  = !beast.enabled;
		}
			
		if (Input.GetKeyDown (KeyCode.C)) {
			tossBody2 (0.0002f);
		}
		if (Input.GetKeyDown (KeyCode.V)) {
			tossBody2 (0.0004f);
		}
	}

	public void ButtonPress() {
		string selectName = EventSystem.current.currentSelectedGameObject.name;
		string op = selectName.Substring (0, 1);
		int mod = int.Parse(selectName.Substring(1,1));

		if (op == "S")
			spreadCapsules(mod);
		if (op == "T")
			tossBody(mod);
		if (op == "D")
			tossBody2(mod * 0.0002f);

	}

	private void spreadCapsules(float mod = 1) {
		for (int i = 0; i < NOF; i++) {
			pieces [i].AddForce (Random.Range (-1f, 1f) * MUL*mod, 1.15f, Random.Range (-1f, 1f) * MUL*mod);
		}
	}

	private void tossBody(float mod = 1) {
		body.AddForce (Random.Range (-1f, 1f) * BODYMUL*mod, 1.15f, Random.Range (-1f, 1f) * BODYMUL*mod);
	}

	private void tossBody2(float mod = 1) {
		body2.AddForce (Random.Range (-1f, 1f) * BODYMUL*mod, 1.15f, Random.Range (-1f, 1f) * BODYMUL*mod);
//		body2.AddForce (-20000, 1.15f, 10000.6f);
	}
}
