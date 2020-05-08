using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultWindow : MonoBehaviour
{
    [SerializeField, Range(0.1f, 10f), Tooltip("The seconds it takes for the score text to reach the final score")]
    float scoreSpeed = 1f;

    [SerializeField, Range(10, 1000), Tooltip("The positive points per each healthy citizen")]
    int pointPerHealthyCitizen = 100;
    [SerializeField, Range(10, 1000), Tooltip("The negative points per each incapacitated citizen")]
    int pointPerIncapCitizen = 150;
    [SerializeField, Range(1, 100), Tooltip("Used in plain score calculation; ")]
    int pointPerMoney = 1;
    [SerializeField, Range(0.001f, 2f), Tooltip("Used in plain score calculation; if CurrentCurrency is negative, pointPerMoney is multiplied by this")]
    float weightPerNegativeMoney = 0.25f;

    [SerializeField, Range(0.01f, 5f), Tooltip("Used in percentual score calculation; the percent that each incapacitated citizen subtracts from the score")]
    float incapWeight = 2.5f;
    [SerializeField, Range(0.005f, 0.5f), Tooltip("Used in percentual score calculation; the percent each unit of currency adds/subtracts to the score")]
    float moneyWeight = 0.005f;

    [SerializeField]
    Text scoreText;
    [SerializeField]
    Text rankText;

    [SerializeField]
    float normalPoints;
    [SerializeField]
    float percentPoints;
    [SerializeField]
    string rank;


    public void CalcScores()
    {
        normalPoints = CalcPoints();
        percentPoints = CalcScorePercentage();
        rank = GetRank(percentPoints);

        WriteTempData(rank);

        StartCoroutine(WriteScores());
    }

    IEnumerator WriteScores()
    {
        yield return new WaitForSeconds(1f);
        yield return WritePoints();
        rankText.text = rank;
        AudioManager.Instance.Play(AudioManager.Instance.EventNormal);
        yield return null;
    }
    IEnumerator WritePoints()
    {
        float currentTime = 0;
        float currentProgress = 0;
        float currentValue = 0;

        do
        {
            currentProgress = currentProgress > 1 ? 1 : currentTime / scoreSpeed;
            currentValue = (int)Mathf.SmoothStep(currentValue, normalPoints, currentProgress);

            scoreText.text = ((int)currentValue).ToString();

            currentTime += Time.unscaledDeltaTime;
            yield return null;
        } while (currentTime < scoreSpeed + 0.01f);
    }

    float CalcPoints()
    {
        List<AgentController> auxAgentList = WorldAgentController.instance.AgentCollection;
        int totalPop = auxAgentList.Count;
        int incap = auxAgentList.FindAll(x => x.myStatus == GlobalObject.AgentStatus.Out_of_circulation).Count;

        int validPop = totalPop - incap;
        float currentMoney = CurrencyManager.Instance.CurrentCurrency;
        float currencyValue = (currentMoney * (currentMoney > 0 ? pointPerMoney : pointPerMoney * weightPerNegativeMoney));

        float positivePoints = (validPop * pointPerHealthyCitizen) + currencyValue;
        float negativePoints = (incap * pointPerIncapCitizen);

        float totalPoints = (positivePoints - negativePoints);

        Debug.Log($"Normal Score:\r\nTotPop: {totalPop} ({validPop}/{incap})\r\n\r\nPositive points: {positivePoints} ({validPop * pointPerHealthyCitizen} + {currencyValue})\r\nNegative Points: {negativePoints}\r\nTotal Points: {totalPoints}");

        return totalPoints;
    }
    float CalcScorePercentage()
    {
        List<AgentController> auxAgentList = WorldAgentController.instance.AgentCollection;
        int totalPop = auxAgentList.Count;
        int incap = auxAgentList.FindAll(x => x.myStatus == GlobalObject.AgentStatus.Out_of_circulation).Count;
        float currentCurrency = CurrencyManager.Instance.CurrentCurrency;

        float weightPerDude = 100f / totalPop;
        float negativeScore = incap * weightPerDude * incapWeight;
        float moneyScore = currentCurrency * (currentCurrency > 0 ? moneyWeight : moneyWeight * weightPerNegativeMoney);

        float totalScore = 100f + moneyScore - negativeScore;

        Debug.Log($"Percentual score:\r\nTotPop: {totalPop} ({totalPop - incap}/{incap})\r\n\r\nNegative score: {negativeScore}\r\nMoney score: {moneyScore}\r\nTotal Score: {totalScore}");
        return totalScore;
    }
    string GetRank(float percentScore)
    {
        //50  60  70  80  90  95  100 >100
        //F,  E,  D,  C,  B,  A,  S,  SS,
        //F     [  0,  50)
        //E     [ 50,  60)
        //D     [ 60,  70)
        //C     [ 70,  80)
        //B     [ 80,  90)
        //A     [ 90,  98)
        //S     [ 98, 102.5)
        //SS    100+

        if (percentScore <= 50f)
        {
            return "F";
        }
        else if (percentScore < 60f)
        {
            return "E";
        }
        else if (percentScore < 70f)
        {
            return "D";
        }
        else if (percentScore < 80f)
        {
            return "C";
        }
        else if (percentScore < 90f)
        {
            return "B";
        }
        else if (percentScore < 98f)
        {
            return "A";
        }
        else if (percentScore < 102.5f)
        {
            return "S";
        }
        else
        {
            return "SS";
        }
    }
    public void WriteTempData(string rank)
    {
        string id = SceneManager.GetActiveScene().name;
        ScenarioSaveData auxData = new ScenarioSaveData() { scenarioID = id, scores = new List<RunScore> { new RunScore() { score = percentPoints, rank = this.rank } } };
        Debug.LogError($"Writing to tempData\r\n{auxData.ToString()}");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Data/Temp/result.dat");
        Debug.Log($"File created at {Application.persistentDataPath}/Data/Temp/Data.dat");
        bf.Serialize(file, auxData);
        file.Close();
    }
}
