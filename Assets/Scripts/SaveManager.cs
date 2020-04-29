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

    string dataPath;
    private void Awake()
    {
        dataPath = Application.persistentDataPath;

        if (!AuxFolderExists())
        {
            Debug.Log($"Folder didn't exist  ({dataPath}/Data)");
            CreateFileStructure();
            //Directory.CreateDirectory(Application.persistentDataPath + "/Data");
        }
        dataPath += "/Data";
    }
    private void Start()
    {
        Debug.Log("Starting thingie.");
        SaveData();
    }

    public void SaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(dataPath + "/Data.dat");
        Debug.Log($"File created at {dataPath}/Data.dat");
        GameSaveData data = new GameSaveData();
        data = gsd.Clone();
        bf.Serialize(file, data);
        file.Close();
    }
    public void LoadData()
    {
        if (File.Exists(dataPath + "/Data.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(dataPath + "/Data.dat", FileMode.Open);
            GameSaveData data = (GameSaveData)bf.Deserialize(file);
            file.Close();

            gsd = data;
        }
        else
        {
            Debug.Log($"File didn't exist ({dataPath}/Data.dat)");
        }
    }
    bool AuxFolderExists()
    {
        return Directory.Exists(dataPath + "/Data");
    }
    void CreateFileStructure()
    {
        string initialPath = dataPath;
        Directory.CreateDirectory(initialPath + "/Data");
        Directory.CreateDirectory(initialPath + "/Data" + "/Temp");
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