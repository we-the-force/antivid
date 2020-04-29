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

        LoadData();
    }
    private void Start()
    {
        Debug.Log("Starting thingie.");
        //LoadData
        LoadTempData();
        //SaveData();

        Debug.LogError("Printing scenarios");
        foreach (ScenarioSaveData ssd in gsd.scenarioData)
        {
            Debug.LogError(ssd.ToString());
        }
    }
    public void WriteAuxTempData()
    {
        string id = gsd.scenarioData[UnityEngine.Random.Range(0, gsd.scenarioData.Count - 1)].scenarioID;
        ScenarioSaveData auxData = new ScenarioSaveData() { scenarioID = id, grades = new List<string> { "X" } };
        Debug.LogError($"Writing to tempData\r\n{auxData.ToString()}");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(dataPath + "/Temp/result.dat");
        Debug.Log($"File created at {dataPath}/Data.dat");
        bf.Serialize(file, auxData);
        file.Close();
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
            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Open(dataPath + "/Data.dat", FileMode.Open);
            //GameSaveData data = (GameSaveData)bf.Deserialize(file);
            //file.Close();
            GameSaveData data = (GameSaveData)ReadFile(dataPath + "/Data.dat");

            gsd = data;
        }
        else
        {
            Debug.Log($"File didn't exist ({dataPath}/Data.dat)");
        }
    }
    public void LoadTempData()
    {
        if (File.Exists(dataPath + "/Temp/result.dat"))
        {
            ScenarioSaveData data = (ScenarioSaveData)ReadFile(dataPath + "/Temp/result.dat");

            AddResultToScenario(data);

            File.Delete(dataPath + "/Temp/result.dat");

            SaveData();
        }
    }

    void AddResultToScenario(ScenarioSaveData toAdd)
    {
        if (gsd.ContainsScenario(toAdd.scenarioID))
        {
            gsd.scenarioData.Find(x => x.scenarioID == toAdd.scenarioID).grades.Add(toAdd.grades[0]);
        }
        else
        {
            Debug.LogError($"Temp scenario result (id: {toAdd.scenarioID} doesn't exist in local scenarios!)");
        }
    }
    object ReadFile(string path)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(path, FileMode.Open);
        object aux = bf.Deserialize(file);
        file.Close();

        return aux;
    }
    bool AuxFolderExists()
    {
        return Directory.Exists(dataPath + "/Data");
    }
    void CreateFileStructure()
    {
        Directory.CreateDirectory(dataPath + "/Data");

        if (!Directory.Exists(dataPath + "/Data/Temp"))
        {
            Debug.Log($"Directory 'Temp' doesn't exist");
            Directory.CreateDirectory(dataPath + "/Data" + "/Temp");
        }
        else
        {
            Debug.Log($"Directory 'Temp' exists");
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

    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
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
    public bool ContainsScenario(string id)
    {
        return scenarioData.Find(x => x.scenarioID == id) != null;
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

    public override string ToString()
    {
        string aux = $"Scenario '{scenarioID}'    (";
        aux += " ";
        for (int i = 0; i < grades.Count; i++)
        {
            aux += i < grades.Count - 1 ? $"{grades[i]}." : $"{grades[i]}, ";
        }
        aux += ")";
        return aux;
    }
}