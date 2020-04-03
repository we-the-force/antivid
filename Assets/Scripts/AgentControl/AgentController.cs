using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public float Speed;

    public int AgentID;

    public GlobalObject.AgentStatus myStatus;

    public List<NeedPercentage> myNeedList;

    public Rigidbody myRigidBody;

    public PathFindingNode myCurrentNode;
    public int myDestinationNodeID;

    public BuildingController myHouse;

    public GlobalObject.AgentPerk myPerk;

    public int currentNodeId;

    /// <summary>
    /// La necesidad que en este momento se esta atendiendo
    /// </summary>
    public GlobalObject.NeedScale NeedTakenCare;
    public bool TakingCareOfNeed = false;


    private AgentMovement movementController;

    //--- Propiedades
    public float hungerResistance = 1.0f;
    public float healthResistance = 1.0f;
    public float wanderResistance = 1.0f;
    public float educationResistance = 1.0f;
    public float entertainmentResistance = 1.0f;
    public float sleepResistance = 1.0f;
    public float resourceProduction = 1.0f;

    public void InitAgent()
    {
        movementController = gameObject.GetComponent<AgentMovement>();

        movementController.Speed = Speed;

        int rndPerk = Random.Range(1, 5);
        switch (rndPerk)
        {
            case 1:
                myPerk = GlobalObject.AgentPerk.Athletic;
                break;
            case 2:
                myPerk = GlobalObject.AgentPerk.Workaholic;
                break;
            case 3:
                myPerk = GlobalObject.AgentPerk.Extrovert;
                break;
            case 4:
                myPerk = GlobalObject.AgentPerk.Introvert;
                break;
            default:
                myPerk = GlobalObject.AgentPerk.None;
                break;
        }

        WorldManager.instance.GetPerkValues(myPerk, out hungerResistance, out healthResistance, out wanderResistance, out educationResistance, out entertainmentResistance, out sleepResistance, out resourceProduction);

        NeedPercentage _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Hunger;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * hungerResistance);
        _need.TicValue = 2;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Education;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * educationResistance);
        _need.TicValue = 1;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Entertainment;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * entertainmentResistance);
        _need.TicValue = 1;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.HealtCare;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * healthResistance);
        _need.TicValue = 0;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Sleep;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * sleepResistance);
        _need.TicValue = 2;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Wander;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * wanderResistance);
        _need.TicValue = 3;

        StartCoroutine("Life");
    }

    IEnumerator Life()
    {
        float seconds = 0;

        while (true)
        {
            //--- Cada Tik va a ser de 1/4 de segundo
            if (seconds >= 0.25f)
            {
                //--- Al cumplirse un tik, se suman los porcentajes de acuerdo a las resistencias y habilidades del agente
                for (int i = 0; i < myNeedList.Count; i++)
                {
                    if(myNeedList[i].MaxPercentage())
                    {
                        TakeCareOfNeed(myNeedList[i].Need);
                    }
                }

                seconds = 0;
            }

            yield return new WaitForFixedUpdate();
            seconds += Time.fixedDeltaTime;
        }
    }

    private void TakeCareOfNeed(GlobalObject.NeedScale _need)
    {
        //--- Si ya se esta atendiendo una necesidad, no cambia de necesidad, pero si va 
        //--- sumando porcentaje de necesidad
        if (TakingCareOfNeed)
            return;

        TakingCareOfNeed = true;
        NeedTakenCare = _need;

        myDestinationNodeID = WorldAgentController.instance.GetBuildingPathfindingNodeID(_need);

        if (myDestinationNodeID < 0)
        {
            //--- No hay edificio disponible para atender la necesidad; debera atender la siguiente necesidad mas alta
            //--- si no tiene necesidad que haya sobrepasado el limite, comienza a diambular, luego pregunta de nuevo
            Debug.LogError("NO HAY EDIFICIO CON NECESIDAD A CUBRIR");
            TakingCareOfNeed = false;
            return;
        }

        movementController.MoveAgent(myDestinationNodeID);
    }


    public void DestinyReached()
    {
        Debug.LogError(" SI YA LLEGO A ALGUN LADO");

        
    }


    IEnumerator Moving()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
        }
    }

}
