using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageSender : MonoBehaviour {

	public string message = string.Empty;
	public string messageParamOnCollide = string.Empty;
	public bool reCollide = true;
	public List<GameObject> objects;

	void Start () {
	}
	
	void OnTriggerEnter(Collider other) {
		if (message.Length > 0 && objects.Count > 0) {

			foreach (GameObject g in objects) {
				if (g == other.gameObject && message.Length > 0) {

					g.SendMessage (message, messageParamOnCollide, SendMessageOptions.DontRequireReceiver);
					// g.transform.position = GameUtil.AddZ (g.transform.position, 1);
				}
			}
			if (!reCollide)
				objects.Remove (other.gameObject);
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter (collision.collider);
	}

}
