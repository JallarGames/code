using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public interface IExecutableEffect
{
	void Execute(List<object> param,float timer);
}

public class Effect
{
	private float timer;
	private IExecutableEffect effect;
	private List<object> param;
	/* 0-user
	 * 1-target
	 * 2-duration
	 * 3-name
	 * 4-cast time 
	 * 5-range
	 * 6-cd
	 */
	public Effect(IExecutableEffect e, List<object> param)
	{
		this.param = param;
		timer = (float)param[2];
		effect = e;
	}
	public bool Execute()
	{	
		timer -= Time.deltaTime;
		if(timer <= 0)
		{
			return false;
		}else{
			effect.Execute(param,timer);
		}

		return true;
	}
	public Effect Copy()
	{
		List<object> p = new List<object>(param);
		return new Effect(effect,p);
	}

	public void SetParam(int id, object o)
	{
		param[id] = o;
	}
}

public class Effect1 : IExecutableEffect
{
	// 7-tick time
	// 8-tick dmg
	// 9-ticks
	public void Execute(List<object> param,float timer){
		if(timer <= (float)param[2]-((int)param[9]*(float)param[7]))
		{
			sCharactersManager.characters[(int)param[1]].moraleCurrent -= (float)param[8];
			param[9] = (int)param[9]+1;
		}
	}
}


public class Skill
{
	private Effect effect;
	private List<object> param = new List<object>();

	public void SetEffect(IExecutableEffect e)
	{
		effect = new Effect(e,param);
	}

	public bool CanExecute(int user, int target){
		Character T = sCharactersManager.characters[target];
		Character U = sCharactersManager.characters[user];
		if(Vector3.Distance(U.transform.position,T.transform.position) <= (float)param[5])
		{
			return true;
		}
		return false;
	}
	public void ApplyEffect(int user, int target){
		Effect e = effect.Copy();
		e.SetParam(0,user);
		e.SetParam(1,target);
		// ew. zmiana parametow
		//...
		sCharactersManager.characters[target].activeEffects.Add(e);
	}

	public object GetParam(int id)
	{
		return param[id];
	}
	public void SetParam(int id, object o){
		param[id] = o;
	}
	public void AddParam(object o){
		param.Add(o);
	}

	public string GetName(){
		return (string)param[4];
	}
}

public class SkillsManager
{
	private SkillsManager(){}
	private static SkillsManager _Instance;
	public static SkillsManager Instance
	{
		get
		{
			if(_Instance == null)
				_Instance = new SkillsManager();
			return _Instance;
		}
	}

	public List<Skill> skills = new List<Skill>();
	public bool CanExecute(int id, int user, int target)
	{
		return skills[id].CanExecute(user,target);
	}

	public void Execute(int id, int user, int target,bool casted){
		if((float)skills[id].GetParam(4) > 0 && !casted)
			sCharactersManager.characters[user].caster.Set(id,user,target);
		else
			skills[id].ApplyEffect(user,target);
	}

	public void AddSkill(Skill skill)
	{
		skills.Add(skill);
	}

	public void Generate()
	{
		Skill s;
		s = new Skill();
		s.AddParam(-1);
		s.AddParam(-1);
		s.AddParam(10f);
		s.AddParam("Dotka");
		s.AddParam(0f);
		s.AddParam(50f);
		s.AddParam(10);
		s.AddParam(1f);
		s.AddParam(100f);
		s.AddParam(0);
		s.SetEffect(new Effect1());
		skills.Add(s);
	}
}


public class Caster
{
	private bool casting;
	private int skill;
	public float timer;
	private float timerMax;
	private int user;
	private int target;

	public void Update(){
		timer -= Time.deltaTime;
		if(timer<=0)
		{
			casting = false;
			sCharactersManager.characters[user].UseCastedSkill(skill,user,target);
		}
	}

	public void Set(int _skill, int _user, int _target)
	{
		casting = true;
		timer = (float)SkillsManager.Instance.skills[_skill].GetParam(4);
		timerMax = (float)SkillsManager.Instance.skills[_skill].GetParam(4);
		skill = _skill;
		user = _user;
		target = _target;
	}

	public bool Casting(){
		return casting;
	}

	public string GetName()
	{
		return SkillsManager.Instance.skills[skill].GetName();
	}
	public float GetProgress()
	{
		return 1-(timer / timerMax);
	}
}
