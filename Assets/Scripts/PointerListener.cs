using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	bool _pressed = false;
	public void OnPointerDown(PointerEventData eventData)
	{
		_pressed = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_pressed = false;
	}

	void Update()
	{
	}

	public bool isPressed() {
		return _pressed;
	}
}
