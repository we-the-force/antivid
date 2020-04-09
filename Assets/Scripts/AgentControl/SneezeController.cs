using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SneezeController : MonoBehaviour
{
    public GameObject sneezeEffectMildCase;
    public GameObject sneezeEffectSeriousCase;

    private GameObject sneezeEffect;

    public GlobalObject.AgentStatus SneezeStatus;
    
    public float SneezeLifespan;

    public int SneezeLifespanInTics;
    public int ticCount;

    public float IllnessPercentageToAdd = 10;

    public Collider myCollider;

    public void Init()
    {
        sneezeEffect = sneezeEffectMildCase;

        if (SneezeStatus == GlobalObject.AgentStatus.Serious_Case)
        {
            //--- Cambia la duracion y la coloracion del estornudo
            SneezeLifespan += SneezeLifespan;
            sneezeEffect = sneezeEffectSeriousCase;
        }

        ticCount = 0;
        WorldManager.TicDelegate += TicReceived;

        StartCoroutine("InitCycle");
    }

    private void TicReceived()
    {
        ticCount++;
    }

    IEnumerator InitCycle()
    {
        yield return new WaitForSeconds(0.2f);

        sneezeEffect.SetActive(true);
        myCollider.enabled = true;

        while (true)
        {
            if(ticCount >= SneezeLifespanInTics)
            {
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        WorldManager.TicDelegate -= TicReceived;
        Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<AgentController>().AddContagion(IllnessPercentageToAdd, true);
        }
    }



}
