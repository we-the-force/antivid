﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

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

    public bool ContainsScenario(string id)
    {
        return gsd.ContainsScenario(id);
    }
    public ScenarioSaveData GetScenario(string id)
    {
        return gsd.GetScenario(id);
    }
    public void WriteAuxTempData()
    {
        string id = gsd.scenarioData[UnityEngine.Random.Range(0, gsd.scenarioData.Count - 1)].scenarioID;
        ScenarioSaveData auxData = new ScenarioSaveData() { scenarioID = id, scores = new List<RunScore> { new RunScore() { score = 150f, rank = "X" } } };
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
            Debug.Log("Loading data!");
            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Open(dataPath + "/Data.dat", FileMode.Open);
            //GameSaveData data = (GameSaveData)bf.Deserialize(file);
            //file.Close();
            try
            {
                GameSaveData data = (GameSaveData)ReadFile(dataPath + "/Data.dat");
                gsd = data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading data:\r\n{e.Message}");

            }

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
            //gsd.scenarioData.Find(x => x.scenarioID == toAdd.scenarioID).scores.Add(toAdd.scores[0]);
            gsd.scenarioData.Find(x => x.scenarioID == toAdd.scenarioID).AddRunResult(toAdd.scores[0]);
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
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "1-a", scores = new List<RunScore>() { new RunScore() { score = 100f, rank = "A" }, new RunScore() { score = 90f, rank = "B" } } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "2-b", scores = new List<RunScore>() { new RunScore() { score = 80f, rank = "C" }, new RunScore() { score = 75.4f, rank = "D" } } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "3-c", scores = new List<RunScore>() { new RunScore() { score = 78f, rank = "B" } } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "4-d", scores = new List<RunScore>() { new RunScore() { score = 95f, rank = "A" } } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "5-e", scores = new List<RunScore>() { new RunScore() { score = 110f, rank = "SS" } } });
        gsd.scenarioData.Add(new ScenarioSaveData() { scenarioID = "6-f", scores = new List<RunScore>() });
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
    public ScenarioSaveData GetScenario(string id)
    {
        return scenarioData.Find(x => x.scenarioID == id);
    }
}

[Serializable]
public class ScenarioSaveData
{
    public string scenarioID;
    public List<RunScore> scores = new List<RunScore>();
    //public List<string> grades;

    public bool Cleared()
    {
        return scores.Count > 0;
    }
    public void AddRunResult(RunScore toAdd)
    {
        scores.Add(toAdd);
        ReOrderScores();
    }
    public RunScore GetBestRun()
    {
        if (Cleared())
        {
            return scores[0];
        }
        else
        {
            //return new RunScore() { score = 0, rank = "-" }       //Por si quieres regresar un resultado a fuerzas
            return null;
        }
    }
    public void ReOrderScores()
    {
        scores = scores.OrderByDescending(x => x.score).ToList();
    }

    public ScenarioSaveData Clone()
    {
        return new ScenarioSaveData()
        {
            scenarioID = this.scenarioID,
            scores = this.scores
        };
    }

    public override string ToString()
    {
        string aux = $"Scenario '{scenarioID}'    (";
        aux += " ";
        for (int i = 0; i < scores.Count; i++)
        {
            aux += i < scores.Count - 1 ? $"{scores[i].ToString()}." : $"{scores[i].ToString()}, ";
        }
        aux += ")";
        return aux;
    }
}

[Serializable]
public class RunScore
{
    public float score;
    public string rank;

    public RunScore Clone()
    {
        return new RunScore()
        {
            score = this.score,
            rank = this.rank
        };
    }

    public override string ToString()
    {
        return $"Score: {score}, rank: {rank}";
    }
}