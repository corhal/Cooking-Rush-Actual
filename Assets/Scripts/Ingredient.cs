﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Ingredient : MonoBehaviour {
	
	public int HP;

	public bool Complex;

	public bool Action;
	public bool Item;

	public string Interaction;

	public bool Correct;
	public bool Active = true;
	public int IngredientType;
	public SpriteRenderer mySprite;
	public Image Fill;
	public Image ComboImage;
	Storage storage;

	public ParticleSystem MyParticles;

	public GameObject ItemsContainer;
	public List<int> Items;
	public List<SpriteRenderer> ItemSprites;

	public Slider mySlider;
	public Slider[] ItemSliders;
	Rigidbody2D myBody;

	public float MaxScore;
	public float CurrentScore;

	public delegate void DestroyedEventHandler (Ingredient ingredient, bool terminated);

	public static event DestroyedEventHandler OnIngredientDestroyed;

	public GameObject MultiplierBlock;
	public Text AdditionalScoreLabel;
	public Text BonusGameLabel;
	public Text TapCountLabel;

	public bool Clicked;

	void Awake() {		
		mySprite = GetComponentInChildren<SpriteRenderer> ();
		mySlider = GetComponentInChildren<Slider> ();
		myBody = GetComponent<Rigidbody2D> ();
		storage = GameObject.FindGameObjectWithTag ("Player").GetComponent<Storage> ();
	}

	void Start() {
		MaxScore = Random.Range (Player.instance.MyMission.MinPointsPerAction, Player.instance.MyMission.MaxPointPerAction);
		CurrentScore = MaxScore;
		Items = new List<int> ();
		if (GameController.instance.dishCount == 0 && GameController.instance.IsFirst == true) {
			//GameController.instance.IsFirst = false;
			GameController.instance.SpawnTime = 20.0f;
			mySlider.maxValue = GameController.instance.SpawnTime;
			GameController.instance.HelperLabel.gameObject.SetActive (true);
			GameController.instance.Advisor.text = "Tap matching ingredients";
			GameController.instance.Advisor.gameObject.SetActive (true);
			GameController.instance.HelperLabel.text = "";
			GameController.instance.PlayAnimation ("FirstHelp");
			//GameController.instance.HelperImage.GetComponent<Animation> ().Play ("FirstHelp");
			int index = Random.Range (0, Dish.instance.Ingredients.Count);
			IngredientType = Dish.instance.Ingredients[index];
			mySprite.sprite = storage.IngredientSprites [IngredientType];
		} else if (GameController.instance.dishCount == 0 && GameController.instance.IsSecond == true) {
			//GameController.instance.IsFirst = false;
			//GameController.instance.HelperImage.transform.position = new Vector2(352.0f, GameController.instance.HelperImage.transform.position.y);
			GameController.instance.SpawnTime = 20.0f;
			mySlider.maxValue = GameController.instance.SpawnTime;
			GameController.instance.HelperLabel.gameObject.SetActive (true);
			GameController.instance.HelperLabel.text = "";
			GameController.instance.Advisor.text = "Discard wrong ingredients";
			GameController.instance.Advisor.gameObject.SetActive (true);
			GameController.instance.Arrow.sprite = storage.ArrowSprites [1];
			GameController.instance.PlayAnimation ("SecondHelp");
			//GameController.instance.HelperImage.GetComponent<Animation> ().Play ("SecondHelp");
			int index = Random.Range (0, GameController.instance.AllowedIngredients.Count);
			while (Dish.instance.Ingredients.Contains(GameController.instance.AllowedIngredients[index])) {
				index = Random.Range (0, GameController.instance.AllowedIngredients.Count);
			}
			IngredientType = GameController.instance.AllowedIngredients [index];
			mySprite.sprite = storage.IngredientSprites [IngredientType];
		} else if (GameController.instance.NextComplex) {
			int index = Random.Range (0, Dish.instance.Ingredients.Count);
			IngredientType = Dish.instance.Ingredients[index];
			mySprite.sprite = storage.IngredientSprites [IngredientType];

			Complex = true;
			Action = true;
			float rand = Random.Range (0.0f, 1.0f);
			if (rand <= 0.33f) {
				Interaction = "slice";
			} else if (rand <= 0.66f) {
				Interaction = "chop";
			} else {
				Interaction = "grate";
			}
		} else if (Dish.instance.IngredientItems.ContainsKey(IngredientType) && Dish.instance.IngredientItems[IngredientType] != -1) {
			Complex = true;
			Item = true;

			int targetItem = Dish.instance.IngredientItems [IngredientType];
			bool hasRelevant = false;
			for (int i = 0; i < ItemSprites.Count; i++) {
				int uniqueItem = Random.Range(0, storage.ItemSprites.Length);
				while (Items.Contains(uniqueItem)) {
					uniqueItem = Random.Range(0, storage.ItemSprites.Length);
				}
				if (uniqueItem == targetItem) {
					hasRelevant = true;
				} else if (!hasRelevant && i == ItemSprites.Count - 1) {
					uniqueItem = targetItem;
					hasRelevant = true;
				}
				Items.Add (uniqueItem);
				ItemSprites [i].sprite = storage.ItemSprites [uniqueItem];
			}
		}

		if (Player.instance.MyMission.Variation == "frenzy") {
			StartAction ();
		}
	}

	void Update () {
		
	}

	public void DestroyIngredient(bool terminated) {
		if (MultiplierBlock.gameObject.activeSelf) {
			MultiplierBlock.gameObject.SetActive(false);
			ComboImage.gameObject.SetActive (false);
		}

		Active = false;
		OnIngredientDestroyed (this, terminated);
	}

	void OnTriggerEnter2D(Collider2D other) {
		
	}

	void OnMouseDown() {
		if (GetComponentInChildren<Button> () != null) {
			GetComponentInChildren<Button> ().gameObject.SetActive (false);	
		}
		if (!Complex) {
			Active = false;
			DestroyIngredient (false);
		} else if (Item) {
			if (!GameController.instance.IsPaused) {
				GameController.instance.IsPaused = true;
				GameController.instance.FirstStep (this);
			}
		} else if (Action) {			
			StartAction ();

			if (GameController.instance.IsPaused && Interaction == "chop") {
				TakeDamage();
			}
		}
	}

	public void StartAction() {
		if (!GameController.instance.IsPaused) {
			GameController.instance.NextComplex = false;
			MultiplierBlock.gameObject.SetActive (true);
			AdditionalScoreLabel.text = "x" + 0;
			BonusGameLabel.gameObject.SetActive (true);
			GameController.instance.IsPaused = true;
			GameController.instance.ShowActionHelp (this);
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (Action && GameController.instance.IsPaused && other.gameObject.GetComponent<GameController>() != null) {
			if (Interaction == "slice") {
				TakeDamage ();
			} else if (Interaction == "grate" && other.transform.position.y >= (GetComponent<Collider2D>().bounds.center + GetComponent<Collider2D>().bounds.extents).y) {
				TakeDamage ();
			}
		}
	}

	public void ForceNext() {		
		Active = false;
		Clicked = true;
		DestroyIngredient (true);
	}

	public void ClickHelper(ItemHelper helper) {
		if (ItemSprites.Contains(helper.gameObject.GetComponent<SpriteRenderer>())) {
			int index = ItemSprites.IndexOf(helper.gameObject.GetComponent<SpriteRenderer> ());
			int item = Items [index];
			int targetItem = Dish.instance.IngredientItems [IngredientType];
			GameController.instance.IsPaused = false;
			if (item == targetItem) {
				Active = false;
				ItemsContainer.SetActive (false);
				DestroyIngredient (false);
			} else {
				ItemsContainer.SetActive (false);
				GameController.instance.AddIncorrect(this);
			}
		}
	}

	public int comboCount;
	int tapCount;
	//string combo;
	void TakeDamage() {
		MyParticles.Play ();
		//CurrentScore += 10.0f;
		//GameController.instance.ComboCount++;
		tapCount++;
		TapCountLabel.text = tapCount.ToString ();
		switch (tapCount) {
		case 1:
			comboCount = 0;
			ComboImage.gameObject.SetActive (true);
			ComboImage.color = new Color (1.0f, 1.0f, 1.0f, 0.0f);
			//combo = " ";
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
			break;
		case 4:
			//combo = "Good!";
			comboCount = 1;
			//ComboImage.gameObject.SetActive (true);
			ComboImage.sprite = storage.ComboSprites [0];
			ComboImage.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.1f, 1.1f, 1.1f);
			break;
		case 8:
			//combo = "Great!";
			comboCount = 2;
			ComboImage.sprite = storage.ComboSprites [1];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.35f, 0.35f, 0.35f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.2f, 1.2f, 1.2f);
			break;
		case 12:
			//combo = "Perfect!";
			comboCount = 3;
			ComboImage.sprite = storage.ComboSprites [2];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.4f, 0.4f, 0.4f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
			break;
		case 16:
			//combo = "Amazing!";
			comboCount = 4;
			ComboImage.sprite = storage.ComboSprites [3];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.45f, 0.45f, 0.45f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.4f, 1.4f, 1.4f);
			break;
		case 20:
			comboCount = 5;
			//combo = "Legendary!";
			ComboImage.sprite = storage.ComboSprites [4];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.45f, 0.45f, 0.45f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
			break;
		default:
			break;
		}
		AdditionalScoreLabel.text = "x" + comboCount;
		//GameController.instance.ShowCombo (this);
		HP--;

	}
}
