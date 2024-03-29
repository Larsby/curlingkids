using UnityEngine;

public class UIModder : MonoBehaviour {

	public RectTransform[] WHobjects = null;
	public float WH_yMove = 50f;
	public RectTransform[] TBobjects = null;
	public float TB_yMove = 50f;

	void Start () {

		if (!(Screen.width == 1125 && Screen.height == 2436)) { // Not Iphone X
			foreach (RectTransform rt in WHobjects)
			{
				rt.anchoredPosition = GameUtil.AddY(rt.anchoredPosition, WH_yMove);
			}

			foreach (RectTransform rt in TBobjects)
			{
				Vector2 oldOffsetMin = rt.offsetMin;
				rt.anchoredPosition = GameUtil.AddY(rt.anchoredPosition, TB_yMove);
				rt.offsetMin = oldOffsetMin;
			}

		}
	}
	
}
