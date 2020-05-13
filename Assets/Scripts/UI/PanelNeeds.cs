using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelNeeds : MonoBehaviour
{
    public static PanelNeeds instance;

    public Text txtHungerTot;
    public Text txtEducaTot;
    public Text txtSleepTot;
    public Text txtViajeTot;
    public Text txtEntrTot;

    public Text txtHungerDiff;
    public Text txtEducaDiff;
    public Text txtSleepDiff;
    public Text txtViajeDiff;
    public Text txtEntrDiff;

    private void Awake()
    {
        instance = this;
    }

    public void UpdateAllNeeds()
    {
        float hungerTot = 0;
        float educaTot = 0;
        float sleepTot = 0;
        float viajeTot = 0;
        float entrTot = 0;

        float hungerdiff = 0;
        float educadiff = 0;
        float sleepdiff = 0;
        float viajediff = 0;
        float entrdiff = 0;

        for (int i = 0; i < WorldAgentController.instance.AgentCollection.Count; i++)
        {
            AgentController _agent = WorldAgentController.instance.AgentCollection[i];

            if (_agent.myStatus != GlobalObject.AgentStatus.Out_of_circulation)
            {
                for (int n = 0; n < _agent.myNeedList.Count; n++)
                {
                    NeedPercentage _need = _agent.myNeedList[n];

                    switch (_need.Need)
                    {
                        case GlobalObject.NeedScale.Hunger:
                            if (_need.CurrentPercentage > (_need.PercentageToCompare * 1.5f))
                                hungerTot += 2;
                            else if (_need.CurrentPercentage > _need.PercentageToCompare)
                                hungerTot += 1;
                            else
                                hungerdiff += 1;

                           // hungerTot += _need.CurrentPercentage;
                          //  hungerdiff += _need.PercentageToCompare;// - _need.CurrentPercentage;
                            break;
                        case GlobalObject.NeedScale.Education:
                            if (_need.CurrentPercentage > (_need.PercentageToCompare * 1.5f))
                                educaTot += 2;
                            else if (_need.CurrentPercentage > _need.PercentageToCompare)
                                educaTot += 1;
                            else
                                educadiff += 1;

                            //educaTot += _need.CurrentPercentage;
                            //educadiff += _need.PercentageToCompare;// - _need.CurrentPercentage;
                            break;
                        case GlobalObject.NeedScale.Entertainment:
                            if (_need.CurrentPercentage > (_need.PercentageToCompare * 1.5f))
                                entrTot += 2;
                            else if (_need.CurrentPercentage > _need.PercentageToCompare)
                                entrTot += 1;
                            else
                                entrdiff += 1;

                            //entrTot += _need.CurrentPercentage;
                            //entrdiff += _need.PercentageToCompare;// - _need.CurrentPercentage;
                            break;
                        case GlobalObject.NeedScale.Sleep:
                            if (_need.CurrentPercentage > (_need.PercentageToCompare * 1.5f))
                                sleepTot += 2;
                            else if (_need.CurrentPercentage > _need.PercentageToCompare)
                                sleepTot += 1;
                            else
                                sleepdiff += 1;

                            //sleepTot += _need.CurrentPercentage;
                            //sleepdiff += _need.PercentageToCompare;// - _need.CurrentPercentage;
                            break;
                        case GlobalObject.NeedScale.Travel:

                            if (_need.CurrentPercentage > (_need.PercentageToCompare * 1.5f))
                                viajeTot += 2;
                            else if (_need.CurrentPercentage > _need.PercentageToCompare)
                                viajeTot += 1;
                            else
                                viajediff += 1;

                            //viajeTot += _need.CurrentPercentage;
                            //viajediff += _need.PercentageToCompare;// - _need.CurrentPercentage;
                            break;
                    }
                }
            }
        }

        txtHungerTot.text = hungerTot.ToString();
        txtEducaTot.text = educaTot.ToString();
        txtSleepTot.text = sleepTot.ToString();
        txtViajeTot.text = viajeTot.ToString();
        txtEntrTot.text = entrTot.ToString();

        txtHungerDiff.text = hungerdiff.ToString();
        txtEducaDiff.text = educadiff.ToString();
        txtSleepDiff.text = sleepdiff.ToString();
        txtViajeDiff.text = viajediff.ToString();
        txtEntrDiff.text = entrdiff.ToString();
    }
}

/*
public class HappinessCoeficient
{
    public GlobalObject.NeedScale Need;

    public float 
}*/