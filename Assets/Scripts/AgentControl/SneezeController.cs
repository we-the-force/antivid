using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SneezeController : MonoBehaviour
{
    public GameObject sneezeEffect;

    public float SneezeLifespan;

    public Collider myCollider;

    private void Start()
    {
        StartCoroutine("Init");
    }

    IEnumerator Init()
    { 
        yield return new WaitForSeconds(0.2f);

        sneezeEffect.SetActive(true);
        myCollider.enabled = true;

        Debug.LogError(">>> YA ESTORNUDO");

        yield return new WaitForSeconds(SneezeLifespan);

        Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<AgentController>().AddContagion();
        }
    }



}
