using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Item
{
	public int id;
	public string name;
	public int quantity;
	public int stack;
	public sItemMaganer.EItemType type;
	public Sprite icon;

	public Item Copy()
	{
		Item it = new Item();
		it.icon = icon;
		it.id = id;
		it.name = name;
		it.quantity = quantity;
		it.stack = stack;
		it.type = type;
		return it;
	}
}

public class ItemObject
{
	public Item item;
	public string prefab; // scene object
}

public static class sItemMaganer
{
	public enum EItemType {RUBBISH,TRADE_GOOD,FOOD,POTION,WEAPON}
	public static GameObject tooltip;
	public static List<Item> templates = new List<Item>();

	public static ItemContainer playerItemContainer;
	public static ItemContainer targetItemContainer;

	public static void LoadModule()
	{
		tooltip = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/UI/Tooltip"));
		tooltip.SetActive(false);
		tooltip.transform.SetParent(GameObject.Find("Frames").transform);
		tooltip.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200,30);
		GenerateItemTemplates();
	}

	public static Item CreateItem(int _id, EItemType _type, string _name, int _stack, string _icon)
	{
		Item i = new Item();
		i.id = _id;
		i.name = _name;
		i.icon = Resources.Load<Sprite>(_icon);
		i.quantity = 1;
		i.stack = _stack;
		i.type = _type;

		return i;
	}

	public static Item CreateItem(int _id, EItemType _type, string _name,int _stack)
	{
		Item i = new Item();
		i.id = _id;
		i.name = _name;
		i.quantity = 1;
		i.stack = _stack;
		i.type = _type;
		return i;
	}

	public static void GenerateItemTemplates()
	{
		templates.Add(CreateItem(0,EItemType.RUBBISH,"Wooden Shavings",100,"Prefabs/UI/ItemIcons/x"));
		templates.Add(CreateItem(1,EItemType.TRADE_GOOD,"Wax",20,"Prefabs/UI/ItemIcons/x"));
		templates.Add(CreateItem(2,EItemType.TRADE_GOOD,"Wooden Plank",20,"Prefabs/UI/ItemIcons/x"));
		templates.Add(CreateItem(3,EItemType.TRADE_GOOD,"Wooden Trunk",20,"Prefabs/UI/ItemIcons/x"));
		templates.Add(CreateItem(4,EItemType.FOOD,"Roasted Boar Ribs",20,"Prefabs/UI/ItemIcons/x"));
		templates.Add(CreateItem(5,EItemType.POTION,"Healing potion",20,"Prefabs/UI/ItemIcons/x"));
	}

	public static Item GetItemTemplate(int _id)
	{
		Item item = templates.Find(x => x.id == _id);
		if(item == null)
			item = new Item();
		return item.Copy();
	}

	public static void AddItem(ItemContainer ic, Item it)
	{
		while(it.quantity > 0)
		{
			Item i = ic.items.Find(x => x.id == it.id && x.quantity < x.stack);
			if(i != null)
			{
				if(it.quantity < i.stack - i.quantity)
				{
					i.quantity += it.quantity;
					it.quantity = 0;
				}else{
					it.quantity -= i.stack - i.quantity;
					i.quantity = i.stack;
				}
			}else{
				if(ic.items.Count == ic.capacity)
					break;
				Item item = it.Copy();
				item.quantity = 1;
				it.quantity -= 1;
				ic.items.Add(item);
			}
		}
	}
}

