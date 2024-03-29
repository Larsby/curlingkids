using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerLevelHandler : MonoBehaviour {

	Transform pseudoParent = null;
	Vector3 powerIndicatorPos = Vector3.zero;

	void Start () {
	}

	void Update() {
		if (pseudoParent != null) {
			transform.position = pseudoParent.position + powerIndicatorPos;
		}
	}

	public void SetPos(Transform parent, Vector3 indicatorPos) {
		pseudoParent = parent;
		powerIndicatorPos = indicatorPos;

		transform.position = pseudoParent.position + powerIndicatorPos;
		transform.rotation = Quaternion.Euler(90, 0, 180);
	}

	public void Kill(float killTime) {
		Invoke("DestroyMe", killTime);
	}

	public void DestroyMe()
	{
		Destroy(gameObject);
	}
}
