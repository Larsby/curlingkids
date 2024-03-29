using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyCreditManager : MonoBehaviour {

	public Text creditsText;
	public Sprite zombieIcon;
	public GameObject buyButtonPrefab;
	public GameObject buttonContainer;
	public GameObject []packButtons;
	Purchaser purchase;
	public GameObject loadProgress;
	private bool updated = false;

	private void CreateConsumableButton(int index,string price, string credits, string preText = "", string postText = "", string currencySymbol="") {

		string buttontext = preText + price + currencySymbol +" - " + credits + postText;

		GameObject g = Instantiate (buyButtonPrefab, buttonContainer.transform, false);
		Text t = g.GetComponentInChildren<Text> ();
		t.text = buttontext;
		Button b = g.GetComponentInChildren<Button> ();
		b.onClick.AddListener (() =>
			{
			BuyConsumable ( index);
			});
	}
	void CreateNonConsumableButton(int index) 
	{
		//string buttontext = preText + price + currencySymbol + " - " + credits + postText;
		string buttontext = purchase.GetLocalTitleForNonConsumable(index) + " "+ purchase.GetLocalPriceForNonConsumable(index); //+ purchase.GetLocalDescriptionForNonConsumable(0);
		GameObject g = Instantiate(packButtons[index-1], buttonContainer.transform, false);
		Text t = g.GetComponentInChildren<Text>();
		t.text = buttontext;
		Image i = g.transform.GetChild(0).GetComponent<Image>();
		i.sprite = zombieIcon;
		Button b = g.GetComponentInChildren<Button>();
		b.onClick.AddListener(() =>
		{
			PurchaseNonConsumable(index);
		});
	}
	public void PurchaseNonConsumable(int index) {
		if (StaticManager.WorldPurchased(index) == false)
		{
			purchase.BuyNonConsumable();
		}	
	}
	public void RestorePurchase() {

		purchase.RestorePurchases();
	}

	void Awake() {
		SoundManager.Create ();
		purchase = GetComponent<Purchaser>();
	}

	void CreateLocalizedConsumeButtons() {
		int numberOfConsumables = purchase.GetNumberOfConsumables();
		for (int i = 0; i < numberOfConsumables; i++)
		{
			CreateConsumableButton(i,purchase.GetLocalPriceForConsumable(i),""+ purchase.GetAmountOfGoldFromConsumable(i));
		}
		if (StaticManager.WorldPurchased(1) == false)
		{
			CreateNonConsumableButton(1);
		}

	}
	void Start () {

		updated = false;
	
	}
	
	void Update () {
		creditsText.text = "" + StaticManager.GetNumberOfCredits();
		if(!updated && purchase.IsInitialized()) {
			updated = true;
			loadProgress.SetActive(false);
			CreateLocalizedConsumeButtons();
		}
	}


	public void BackButton() {
		StaticManager.PopScene ();
	}

	public void BuyConsumable(int index) {

		// Debug.Log (prize + " " + credits);

		purchase.BuyConsumable(index);
	//	StaticManager.AddCredits (credits);
	//	creditsText.text = "" + StaticManager.GetNumberOfCredits ();

	}

}
