using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager : MonoBehaviour
{
    [SerializeField]
    //List<ScenarioSaveData> scenarioData = new List<ScenarioSaveData>();
    GameSaveData gsd = new GameSaveData();

    private void Start()
    {
        Debug.Log("Starting thingie.");
        SaveData();
    }

    public void SaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Data.dat");
        Debug.Log($"File created at {Application.persistentDataPath}/Data.dat");
        GameSaveData data = new GameSaveData();
        data = gsd.Clone();
        bf.Serialize(file, data);
        file.Close();
    }
    public void LoadData()
    {
        if (File.Exists(Application.persistentDataPath + "/Data.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath+"/Data.dat", FileMode.Open);
            GameSaveData data = (GameSaveData)bf.Deserialize(file);
            file.Close();

            gsd = data;
        }
        else
        {
            Debug.Log($"File didn't exist ({Application.persistentDataPath}/Data.dat)");
        }

    }
    public void ResetSavedata()
    {
        gsd.scenarioData.Clear();
    }
    public void InitDummySavedata()
    {
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "1-a", grades = new List<string>() { "A", "B" } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "2-b", grades = new List<string>() { "B", "D" } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "3-c", grades = new List<string>() { "B" } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "4-d", grades = new List<string>() { "D" } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "5-e", grades = new List<string>() { "C" } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "6-f", grades = new List<string>() });
    }
}


[Serializable]
public class GameSaveData
{
    public List<ScenarioSaveData> scenarioData;

    public GameSaveData Clone()
    {
        GameSaveData aux = new GameSaveData();
        aux.scenarioData = scenarioData;
        return aux;
    }
}

[Serializable]
public class ScenarioSaveData
{
    public string scenarioID;
    public List<string> grades;

    public ScenarioSaveData Clone()
    {
        ScenarioSaveData aux = new ScenarioSaveData();
        aux.scenarioID = scenarioID;
        aux.grades = grades;
        return aux;
    }
}