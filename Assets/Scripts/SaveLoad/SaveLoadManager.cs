using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager slManager;
    private PlayerController playerRef;
    private AIController aiRef;
    public int SavedScore = 0;
    const string fileName = "/defaultSaveSlot.dat";

    public void Awake()
    {
        if (slManager == null)
        {
            slManager = this;
            playerRef = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
            aiRef = GameObject.FindGameObjectWithTag("Enemy").GetComponentInChildren<AIController>();
        }
    }

    [Serializable]
    class GameSaveData
    {
        public float[] PlayerPosition;
        public float[] EnemyPosition;

        public int Score;
    };

    public void LoadSaveSlot()
    {
        if (File.Exists(Application.persistentDataPath + fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(Application.persistentDataPath + fileName, FileMode.Open, FileAccess.Read);

            GameSaveData data = (GameSaveData)bf.Deserialize(fs);
            fs.Close();

            slManager.SavedScore = data.Score;
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().SetScore(SavedScore);

            if (data.PlayerPosition != null)
            {
                slManager.playerRef.GetComponent<CharacterController>().enabled = false;
                slManager.playerRef.gameObject.transform.position = new Vector3(data.PlayerPosition[0], data.PlayerPosition[1], data.PlayerPosition[2]);
                slManager.playerRef.GetComponent<CharacterController>().enabled = true;
            }

            if (data.EnemyPosition != null)
            {
                slManager.aiRef.GetComponent<CharacterController>().enabled = false;
                slManager.aiRef.gameObject.transform.position = new Vector3(data.EnemyPosition[0], data.EnemyPosition[1], data.EnemyPosition[2]);            
                slManager.aiRef.GetComponent<CharacterController>().enabled = true;
            }
        }
    }

    public void SaveDefaultSlot()
    {
        SavedScore = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().GetScore();

        BinaryFormatter bf = new BinaryFormatter(); //class to help serialize and deserialize data
        FileStream fs = File.Open(Application.persistentDataPath + fileName, FileMode.OpenOrCreate); //open file path for writing

        GameSaveData data = new GameSaveData(); //create new GameData object set high score to be saved
        data.Score = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().GetScore();

        data.PlayerPosition = new float[]{ playerRef.transform.position.x, 1f, playerRef.transform.position.z };
        data.EnemyPosition = new float[]{ aiRef.transform.position.x, 0.5f, aiRef.transform.position.z };

        bf.Serialize(fs, data); //use binary formatter to serialize data at filepath
        fs.Close();
    }

    public void ResetDefaultSlot()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Open(Application.persistentDataPath + fileName, FileMode.Open, FileAccess.Read);

        GameSaveData data = new GameSaveData();
        data.Score = 0;

        bf.Serialize(fs, data);
        fs.Close();
    }

}

