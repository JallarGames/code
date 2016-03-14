using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class Cell
{
	public Vector2 position = new Vector2();
	public GameObject cellObject;
}

public class Way
{
	public List<Cell> cells = new List<Cell>();
}

public class Main : MonoBehaviour {

	public GameObject cellPrefab;
	public GameObject player;
	private int mapSize;
	private List<Cell> cells = new List<Cell>();
	private List<Way> ways = new List<Way>();

	private bool[] directionsUsed = new bool[4];
	private List<Vector2> directions = new List<Vector2>(4);
	private Vector2 startCell = new Vector2();

	private int minutes;
	private float timer;
	private enum GAME_STATE {MENU,PLAY,SCORE};
	private GAME_STATE gameState;
	public Text timerText;

	public GameObject UiMenu;
	public GameObject UiPlay;
	public GameObject UiScore;
	public GameObject end;
	public Text score;

	private float speed = 0.02f;

	void Start () {
		directions.Add(new Vector2(-1,0));
		directions.Add(new Vector2(1,0));
		directions.Add(new Vector2(0,1));
		directions.Add(new Vector2(0,-1));
		gameState = GAME_STATE.MENU;
		UiMenu.SetActive(true);
		UiPlay.SetActive(false);
		UiScore.SetActive(false);

	}

	void Update () {
		if(gameState == GAME_STATE.PLAY)
		{
			timer += Time.deltaTime;
			if(timer >= 60)
			{
				timer -= 60;
				minutes++;
			}
			timerText.text = string.Format("{0}:{1}",minutes,Mathf.CeilToInt(timer));

			MovePlayer();
		}
	}

	public void EndGame()
	{
		ClearScene();
		gameState = GAME_STATE.SCORE;
		UiMenu.SetActive(false);
		UiPlay.SetActive(false);
		UiScore.SetActive(true);
		score.text = string.Format("!! Congratulations !! \n\nYour time is {0} minutes {1} seconds",minutes,Mathf.CeilToInt(timer));
	}


	public void MovePlayer()
	{
		Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));
		moveDirection = player.transform.TransformDirection(moveDirection);
		moveDirection *= speed;

		player.GetComponent<CharacterController>().Move(moveDirection);

	}

	public void StartGame(int mapsize)
	{
		ClearScene();
		mapSize = mapsize;
		int a = Mathf.RoundToInt(mapSize/2);
		startCell.Set(a,a);
		GenerateCells();
		FindWay(cells.Find(x => x.position == startCell));
		GenerateExit();
		HideWalls();
		player.transform.position= new Vector3(a+0.5f,0.15f,a+0.5f);
		timer = 0;
		minutes = 0;
		gameState = GAME_STATE.PLAY;
		UiMenu.SetActive(false);
		UiPlay.SetActive(true);
	}

	private void ClearScene()
	{
		end.transform.position = new Vector3(-1,0,-1);
		player.transform.rotation = new Quaternion(0,0,0,0);
		player.transform.position= new Vector3(0.5f,0.15f,0.5f);
		ways.Clear();
		foreach(Cell c in cells)
		{
			Destroy(c.cellObject);
		}
		cells.Clear();
	}

	public void ReturnToMenu()
	{
		gameState = GAME_STATE.PLAY;
		ClearScene();
		UiMenu.SetActive(true);
		UiPlay.SetActive(false);
		UiScore.SetActive(false);
	}

	private void GenerateExit()
	{
		bool found = false;
		foreach(Cell c in cells)
		{
			if(c.position.x == 0 || c.position.y == 0 || c.position.x == mapSize-1 || c.position.y == mapSize-1)
			{
				foreach(Way w in ways)
				{
					if(w.cells.Exists(x => x.position == c.position) && w.cells.Exists(x => x.position == startCell))
					{
						end.transform.position = c.cellObject.transform.position;
						if(c.position.x == 0){
							c.cellObject.transform.FindChild("Wall2").gameObject.SetActive(false);
							end.transform.Translate(new Vector3(-1,0,0));
						}else if(c.position.x == mapSize-1){
							c.cellObject.transform.FindChild("Wall4").gameObject.SetActive(false);
							end.transform.Translate(new Vector3(1,0,0));
						}else if(c.position.y == 0){
							c.cellObject.transform.FindChild("Wall1").gameObject.SetActive(false);
							end.transform.Translate(new Vector3(0,0,-1));
						}else if(c.position.y == mapSize-1){
							c.cellObject.transform.FindChild("Wall3").gameObject.SetActive(false);
							end.transform.Translate(new Vector3(0,0,1));
						}
						found = true;
						break;
					}
				}
			}
			if(found)
				break;
		}
		if(!found)
		{
			ways.Clear();
			FindWay(cells.Find(x => x.position == startCell));
			GenerateExit();
		}
	}

	private void GenerateCells()
	{
		for(int i=0; i<mapSize; i++)
		{
			for(int j=0; j<mapSize; j++)
			{
				Cell cell = new Cell();
				cell.position = new Vector2(i,j);
				cell.cellObject = GameObject.Instantiate(cellPrefab);
				cell.cellObject.transform.position += new Vector3(i,0,j);
				cells.Add(cell);
			}
		}
	}

	private void FindWay(Cell start)
	{
		Way way = new Way();
		way.cells.Add(start);
		ways.Add(way);
		FindNextCellInWay(way);
		for(int i =0; i< 4 ; i++)
			directionsUsed[i] = false;
		
		for(int i =0; i < way.cells.Count-1; i++)
		{
			FindWay(way.cells[i]);
		}
	}

	private int GetDirection()
	{
		int dir;
		do
		{
			dir = Random.Range(0,4);
		}while(directionsUsed[dir]);
		return dir;
	}

	private bool IsCorrectCell(Way way,Vector2 dir, out Cell nextCell)
	{
		Vector2 nextPosition = new Vector2(way.cells[way.cells.Count-1].position.x + dir.x, way.cells[way.cells.Count-1].position.y + dir.y);
		if(cells.Exists(x => x.position == nextPosition))
		{
			nextCell = cells.Find(x => x.position == nextPosition);
			return true;
		}else{
			nextCell = new Cell();
			return false;
		}
	}

	private bool IsFreeCell(Cell cell)
	{
		foreach(Way w in ways)
		{
			if(w.cells.Exists(x => x.position == cell.position))
			{
				return false;
			}
		}
		return true;
	}

	private void FindNextCellInWay(Way way)
	{
		if(!directionsUsed[0] || !directionsUsed[1] || !directionsUsed[2] || !directionsUsed[3])
		{
			
			Cell nextCell = new Cell();

			int dir = GetDirection();
			directionsUsed[dir] = true;
			if(IsCorrectCell(way,directions[dir],out nextCell))
			{
				if(IsFreeCell(nextCell))
				{
					way.cells.Add(nextCell);
					for(int i =0; i< 4 ; i++)
						directionsUsed[i] = false;
				}
			}
			FindNextCellInWay(way);
		}
	}

	private void HideWalls()
	{
		foreach(Way w in ways)
		{
			for(int i = 0 ; i < w.cells.Count-1; i++)
			{
				Vector2 dir = w.cells[i].position - w.cells[i+1].position;
				if(dir == new Vector2(0,-1)){
					//up for i
					w.cells[i].cellObject.transform.FindChild("Wall3").gameObject.SetActive(false);
					w.cells[i+1].cellObject.transform.FindChild("Wall1").gameObject.SetActive(false);
				}else if(dir == new Vector2(0,1)){
					//down
					w.cells[i].cellObject.transform.FindChild("Wall1").gameObject.SetActive(false);
					w.cells[i+1].cellObject.transform.FindChild("Wall3").gameObject.SetActive(false);
				}else if(dir == new Vector2(-1,0)){
					//right
					w.cells[i].cellObject.transform.FindChild("Wall4").gameObject.SetActive(false);
					w.cells[i+1].cellObject.transform.FindChild("Wall2").gameObject.SetActive(false);
				}else if(dir == new Vector2(1,0)){
					//left
					w.cells[i].cellObject.transform.FindChild("Wall2").gameObject.SetActive(false);
					w.cells[i+1].cellObject.transform.FindChild("Wall4").gameObject.SetActive(false);
				}
			}
		}
	}
}
