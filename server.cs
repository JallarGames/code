using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;

using MySql;
using MySql.Data.MySqlClient;


public class MysqlHandler
{
	public int queries;
	private MySqlConnection con = null;
	private MySqlCommand cmd = null;
	public MySqlDataReader data = null;
	private string lastQuery;

	public void Connect()
	{
		con = new MySqlConnection("Server=127.0.0.1;Database=DC;User=root;Password=;Pooling=true;");
		try{
			con.Open();
		}catch(Exception e){
			GameObject.Find("LAST_SEND").GetComponent<Text>().text = e.Message;
		}
	}

	public void Select(string target, string table, string conditions)
	{
		lastQuery = String.Format("Select {0} From {1}",target,table);
		if(conditions != null)
			lastQuery += " Where "+conditions;
		
		cmd = new MySqlCommand(lastQuery,con);
		data = cmd.ExecuteReader();
		queries++;
	}

	public void Update(string target, string table, string conditions)
	{
		lastQuery = String.Format("Update {0} Set {1}",table,target);
		if(conditions != null)
			lastQuery += " Where "+conditions;

		cmd = new MySqlCommand(lastQuery,con);
		data = cmd.ExecuteReader();
		queries++;
	}
}


public class server : MonoBehaviour
{
	public Text textConnections;
	private MysqlHandler mysql;

	private enum CODE_RECV{
		PING,
		LOGIN_REQUEST,
		ENTER_WORLD_REQUEST,
		SEND_POSITION
	}

	private enum CODE_SEND{
		PING,
		LOGIN_ACCEPTED, 
		LOGIN_DECLAINED,
		ENTER_WORLD
	}
	void Start () {
		NetworkTransport.Init();
		NetServer.Instance.CreateHost(6010,100);
		mysql = new MysqlHandler();
		mysql.Connect();


	}

	void Update () {
		NetServer.Instance.Receive();
		HandleLastMsg();
		textConnections.text = "CONNECTIONS: "+NetServer.Instance.ConnectionsCount.ToString();
	}

	private bool LoginRequest(string login, string pass, out int count, out string characters)
	{
		int playerid;
		characters = null;
		count = 0;
		mysql.Select("playerid","accounts","login = '"+login+"'"); //  AND password = '"+pass+"'"
		if(mysql.data.Read())
		{
			playerid = mysql.data.GetInt32(0);
			mysql.data.Close();
			mysql.Select("characterid,name","players","playerid = '"+playerid+"'");
			while(mysql.data.Read())
			{
				count++;
				characters += string.Format(";{0};{1}",mysql.data.GetInt32(0),mysql.data.GetString(1));
			}
			Debug.Log(characters);
			mysql.data.Close();
			return true;
		}else{
			mysql.data.Close();
			return false;
		}
	}
		
	private void HandleLastMsg()
	{
		string msg = System.Text.Encoding.UTF8.GetString(NetServer.Instance.buffer);
		string[] msgParams = msg.Split(';');
		int i ;
		int.TryParse(msgParams[0],out i);
		switch((CODE_RECV)i)
		{
		case CODE_RECV.LOGIN_REQUEST:
			Debug.Log("request");
			string characters;
			int count;
			if(LoginRequest(msgParams[1],msgParams[2],out count, out characters))
			{
				NetServer.Instance.Send(string.Format("{0};{1}{2}",(int)CODE_SEND.LOGIN_ACCEPTED, count, characters),NetServer.Instance.client);
			}else{
				NetServer.Instance.Send(string.Format("{0}",(int)CODE_SEND.LOGIN_DECLAINED),NetServer.Instance.client);
			}
			break;
		}
	}

}

public class Client
{
	public int hostId; 
	public int connectionId; 
	public int channelId; 

	public Client()
	{
		
	}
	public Client(int host, int connection, int channnel)
	{
		hostId = host;
		connectionId = connection;
		channelId = channnel;
	}
}

public class NetServer
{
	private NetServer(){}
	private static NetServer _Instance;
	public static NetServer Instance{
		get{
			if(_Instance == null)
				_Instance = new NetServer();
			return _Instance;
		}
	}

	private int hostId;
	private int channelId;

	private byte error;
	private int bufferSize = 1024;
	public byte[] buffer = new byte[1024]; 
	private int dataSize;

	public Client client = new Client();
	private List<Client> clients = new List<Client>();

	public void CreateHost(int port,int maxConnections)
	{
		ConnectionConfig config = new ConnectionConfig();
		channelId = config.AddChannel(QosType.Reliable);

		HostTopology topology = new HostTopology(config,maxConnections);
		hostId = NetworkTransport.AddHost(topology,port);
	}
		
	public void Receive()
	{
		buffer = new byte[1024];
		NetworkEventType recData = NetworkTransport.Receive(out client.hostId, out client.connectionId, out client.channelId, buffer, bufferSize, out dataSize, out error);
		switch (recData)
		{
		case NetworkEventType.ConnectEvent:
			Debug.Log("Connected");
			clients.Add(new Client(client.hostId,client.connectionId,client.channelId));
			break;
		case NetworkEventType.DataEvent:
			GameObject.Find("LAST_RECV").GetComponent<Text>().text = System.Text.Encoding.UTF8.GetString(buffer);
			break;
		case NetworkEventType.DisconnectEvent:
			Debug.Log("DC");
			clients.Remove(clients.Find(x => x.connectionId == client.connectionId));
			break;
		}

	}

	public void Send(string msg, Client client)
	{
		buffer = System.Text.Encoding.UTF8.GetBytes(msg);
		NetworkTransport.Send(client.hostId,client.connectionId,client.channelId,buffer,bufferSize,out error);
		GameObject.Find("LAST_SEND").GetComponent<Text>().text = msg;
	}

	public int ConnectionsCount{ get{ return clients.Count; } }

}
