using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CollisionListener {
	void OnCollisionEnter (Collision collision);
	void OnCollisionExit (Collision collision);
	void OnCollisionStay (Collision collision);
	void OnTriggerEnter (Collider collider);
	void OnTriggerExit (Collider collider);
	void OnTriggerStay (Collider collider);
	void SetAffectedColliderBridgeObject(GameObject go);
}

public class ColliderBridge : MonoBehaviour {

	CollisionListener _listener = null;
	public void Initialize(CollisionListener l)
	{
		_listener = l;
	}
	void OnCollisionEnter(Collision collision)
	{
		if (_listener != null) {
			_listener.SetAffectedColliderBridgeObject (this.gameObject);
			_listener.OnCollisionEnter (collision);
		}
	}
	void OnCollisionExit(Collision collision)
	{
		if (_listener != null) {
			_listener.SetAffectedColliderBridgeObject (this.gameObject);
			_listener.OnCollisionExit (collision);
		}
	}
	void OnCollisionStay(Collision collision)
	{
		if (_listener != null) {
			_listener.SetAffectedColliderBridgeObject (this.gameObject);
			_listener.OnCollisionStay (collision);
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if (_listener != null) {
			_listener.SetAffectedColliderBridgeObject (this.gameObject);
			_listener.OnTriggerEnter (collider);
		}
	}
	void OnTriggerExit(Collider collider)
	{
		if (_listener != null) {
			_listener.SetAffectedColliderBridgeObject (this.gameObject);
			_listener.OnTriggerExit (collider);
		}
	}
	void OnTriggerStay(Collider collider)
	{
		if (_listener != null) {
			_listener.SetAffectedColliderBridgeObject (this.gameObject);
			_listener.OnTriggerStay (collider);
		}
	}
}
