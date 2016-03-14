using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcComponent
{
	public IAiIdle AiIdle;
	public IAiMoving AiMoving;
	public IAiVoices AiVoices;
	public IAiFighting AiFighting;
	public IAiQuesting AiQuesting;
	public IAiTrading AiTrading;

	public Engine.NpcType type;
	public int count;
}

public class NpcManager
{
	private NpcManager(){}
	private NpcManager _Instance;
	public NpcManager Instance
	{
		get
		{
			if(_Instance == null)
				_Instance = new NpcManager();
			return _Instance;
		}
	}

	public List<NpcComponent> components;
}

public class Npc
{
	public int npcId;
	public string name;
	public NpcComponent npcComponent;
	public Transform transform;
}

public class Player
{
	public string name;
}

public class Character
{
	public Character(Player module)
	{
		playerModule = module;
	}

	public Character(Npc module)
	{
		npcModule = module;
	}

	private Player playerModule;
	private Npc npcModule;

	public string GetName()
	{
		if(playerModule != null)
			return playerModule.name;
		else if(npcModule != null)
			return npcModule.name;
		return "unknown";
	}
}



