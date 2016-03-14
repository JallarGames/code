using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class Engine : MonoBehaviour 
{
	private Engine(){ }
	private static Engine _Instance;
	public static Engine Instance
	{
		get
		{
			if(_Instance == null)
				_Instance = new Engine();
			return _Instance;
		}
	}

	private float spawn_timer;
	public bool targeted;
	public GameObject targetCircle;
	public bool ShowTooltip;
	public string errorMsg;

	void Start () {
		ShowTooltip = false;
		targeted = false;
		spawn_timer = 0;
		targetCircle =  GameObject.Find("TargetCircle");

		sItemMaganer.LoadModule();
		sCharactersManager.SpawnPlayer();
		sItemMaganer.playerItemContainer = sCharactersManager.characters[0].gameObject.GetComponent<ItemContainer>();
		de_Quickslots.Init();
		SkillsManager.Instance.Generate();

		// temporary code
		Item item;
		item = sItemMaganer.GetItemTemplate(0);
		sItemMaganer.playerItemContainer.items.Add(item.Copy());

		item = sItemMaganer.GetItemTemplate(5);
		item.quantity = 2;
		sItemMaganer.playerItemContainer.items.Add(item.Copy());

		item = sItemMaganer.GetItemTemplate(4);
		item.quantity = 1;
		sItemMaganer.playerItemContainer.items.Add(item.Copy());

		item = sItemMaganer.GetItemTemplate(1);
		item.quantity = 10;
		sItemMaganer.playerItemContainer.items.Add(item.Copy());
	}


	void Update () {
		FindNextTarget();
		UpdateTarget();
		UseSkills();
		Inventory();
		Spawn();
	}
	

	void UpdateTarget()
	{
		if(Input.GetMouseButtonDown(0) && !targeted)
		{
			// uncheck target
			sCharactersManager.characters[0].targetId = -1;
			targetCircle.SetActive(false);
		}else if(Input.GetMouseButtonDown(0) && targeted){
			targetCircle.SetActive(true);
		}

		int targetId = sCharactersManager.characters[0].targetId;
		if(targetId >= 0)
		{
			targetCircle.SetActive(true);
			targetCircle.transform.position = sCharactersManager.characters[targetId].transform.position + new Vector3(0,0.2f,0);
		}else{
			targetCircle.SetActive(false);
		}

		targeted = false;
	}


	void FindNextTarget()
	{
		if(Input.GetKeyDown(KeyCode.Tab))
		{
			if(sCharactersManager.characters.Count>1)
			{
				int a = sCharactersManager.characters[0].targetId;
				for(int i=0; i< sCharactersManager.characters.Count; i++)
				{
					if(a == sCharactersManager.characters.Count-1 || a < 1)
						a = 0;
					a++;
					if(!sCharactersManager.characters[a].isDead)
					{
						float angle =  Vector3.Angle(sCharactersManager.characters[0].transform.forward,sCharactersManager.characters[a].transform.position - sCharactersManager.characters[0].transform.position);
						if(angle<70 && Vector3.Distance(sCharactersManager.characters[0].transform.position,sCharactersManager.characters[a].transform.position) < 10)
						{
							sCharactersManager.characters[0].targetId = a;
							targetCircle.SetActive(true);
							break;
						}
					}
				}
			}
		}
	}

	public void Inventory()
	{
		sItemMaganer.tooltip.SetActive(false);
		sItemMaganer.playerItemContainer.Rebuild();
		if(sItemMaganer.targetItemContainer)
			sItemMaganer.targetItemContainer.Rebuild();

		if(Input.GetKeyDown(KeyCode.B))
		{
			GameObject f = sCharactersManager.characters[0].gameObject.GetComponent<ItemContainer>().frame;
			f.SetActive(!f.activeSelf);
		}

		if(sItemMaganer.targetItemContainer)
		{
			for(int i=0;i<sItemMaganer.targetItemContainer.icons.childCount;i++)
			{
				Vector3 iconPos =  sItemMaganer.targetItemContainer.icons.GetChild(i).position;
				Rect r = new Rect(iconPos.x,iconPos.y-43,43,43);
				if(InRect(r,Input.mousePosition))
				{
					if(Input.GetMouseButtonDown(1))
					{
						if(gameObject.name.Equals("Player1"))
						{
							if(sItemMaganer.targetItemContainer.allowDeposit)
							{
								//de_ItemMaganer.PlayerItemContainer.Items[i].quantity = de_ItemMaganer.TargetItemContainer.AddItem(de_ItemMaganer.PlayerItemContainer.Items[i]);
							}
						}else{
							sItemMaganer.AddItem(sItemMaganer.playerItemContainer,sItemMaganer.targetItemContainer.items[i]);
						}
						break;
					}else{
						sItemMaganer.tooltip.SetActive(true);
						sItemMaganer.tooltip.GetComponentInChildren<Text>().text = sItemMaganer.targetItemContainer.items[i].name;
						break;
					}
				}
			}
		}
		if(sItemMaganer.playerItemContainer.frame.activeSelf)
		{
			for(int i=0;i<sItemMaganer.playerItemContainer.icons.childCount;i++)
			{
				Vector3 iconPos =  sItemMaganer.playerItemContainer.icons.GetChild(i).position;
				Rect r = new Rect(iconPos.x,iconPos.y-43,43,43);
				if(InRect(r,Input.mousePosition))
				{
					sItemMaganer.tooltip.SetActive(true);
					sItemMaganer.tooltip.GetComponentInChildren<Text>().text = sItemMaganer.playerItemContainer.items[i].name;
					break;
				}
			}
		
		}


	}
	
	public bool InRect(Rect r, Vector3 point)
	{
		if(point.x >= r.xMin && point.x < r.xMax && point.y >= r.yMin && point.y < r.yMax)
			return true;
		return false;
	}

	public void UseSkills()
	{
		if(de_Quickslots.globalCD > 0)
			de_Quickslots.globalCD -= Time.deltaTime;
		if(de_Quickslots.globalCD<0)
			de_Quickslots.globalCD = 0;

		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			sCharactersManager.characters[0].UseSkill(0);
		}

		for(int i=0; i<de_Quickslots.Bar1.Count; i++)
		{
			float cd = sCharactersManager.characters[0].skillsCD[de_Quickslots.Bar1[i].skill];
			if(cd == 0)
			{
				int a = i+1;
				GameObject.Find("QuickslotBar1T").transform.FindChild("Text"+a).GetComponent<Text>().text = " ";
			}else{
				int a = i+1;
				GameObject.Find("QuickslotBar1T").transform.FindChild("Text"+a).GetComponent<Text>().text = Mathf.CeilToInt(cd).ToString();
			}
		}
	}

	public void Spawn()
	{
		spawn_timer -= Time.deltaTime;
		if(spawn_timer<0)
		{
			sCharactersManager.SpawnMob();
			spawn_timer = 10f;
		}
	}

	public void CreateFloatingText(Vector2 screenPos, string text)
	{
		GameObject obj = Canvas.Instantiate<GameObject>((GameObject)Resources.Load("Prefabs/UI/flotingText"));
		obj.GetComponent<FloatingText>().Set(screenPos,text,5f,100f);
	}

}
