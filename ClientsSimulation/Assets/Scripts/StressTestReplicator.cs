using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class StressTestReplicator : MonoBehaviour
{
    private int ClientsNumber = 20;
    public GameObject goParent;
    public GameObject goPrefab;
    public Transform[] SpawnPositions;
    public string ConfigFile;
    private List<GameObject> SimPlayers = new List<GameObject>();
    private int[] GroupIDs = new[]
    {
        1,1,1,1,1,1,
        2,2,2,2,2,2,
        3,3,3,3,3,3,
        4,4,4,4,4,4,
        5,5,5,5,5,5,
        6,6,6,6,6,6,
        7,7,7,7,7,7,
        8,8,8,8,8,8,
        9,9,9,9,9,9,
        10,10,10,10,10,10,
        11,11,11,11,11,11,
        12,12,12,12,12,12,
        13,13,13,13,13,13,
        14,14,14,14,14,14,
        15,15,15,15,15,15,
        16,16,16,16,16,16,16,16,16,16
    };

	// Use this for initialization
	void Start ()
	{
	    ReadConfig();
	    SetupClients();
	}

    private void ReadConfig()
    {
        string filename = Application.streamingAssetsPath + "/" + ConfigFile;
        string[] lines = File.ReadAllLines(filename);
        string[] items = lines[0].Split(':');
        ClientsNumber = int.Parse(items[1]);
    }

    private void SetupClients()
    {
        for (int i = 0; i < ClientsNumber; i++)
        {
            Vector3 pos = SpawnPositions[GroupIDs[i]].position;
            GameObject go = Instantiate(goPrefab, pos, Quaternion.identity, goParent.transform);
            go.name = "Client" + i;
            SimPlayers.Add(go);
            SimpleClient sc = go.GetComponent<SimpleClient>();
            sc.ClientName = "Client" + i;
            sc.GroupID = GroupIDs[i];
            sc.GroupSize = 6;
            sc.Leader = 1;
            sc.Gender = 1;
        }
    }

    public void ClientsStartUp()
    {
        for (int i = 0; i < SimPlayers.Count; i++)
        {
            GameObject player = SimPlayers[i];
            player.GetComponent<SimpleClient>().StartUp();
            Thread.Sleep(20);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
