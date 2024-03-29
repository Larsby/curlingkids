using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokeWithObject : MonoBehaviour {

	private List<Coroutine> IWOCOS = new List<Coroutine> ();

	protected Coroutine InvokeWO(System.Action<System.Object> callback, float delay, System.Object objectParam) {
		Coroutine cr = StartCoroutine(IWO(callback, delay, objectParam));
		IWOCOS.Add (cr);
		return cr;
	}
	protected Coroutine InvokeWA(System.Action callback, float delay) {
		Coroutine cr = StartCoroutine(IWA(callback, delay));
		IWOCOS.Add (cr);
		return cr;
	}

	private IEnumerator IWO(System.Action<System.Object> callback, float delay, System.Object objectParam)
	{
		yield return new WaitForSeconds(delay);
		callback (objectParam);
	}
	private IEnumerator IWA(System.Action callback, float delay)
	{
		yield return new WaitForSeconds(delay);
		callback ();
	}

	protected void CancelAllInvokeW() {
		// Debug.Log ("Nof: " + IWOCOS.Count);
		foreach (Coroutine cr in IWOCOS) {
			StopCoroutine (cr);
		}
		IWOCOS.Clear ();
	}

	protected void CancelInvokeW(Coroutine cr) {
		if (cr != null)
			StopCoroutine (cr);
	}

}
