using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class agentAnimController : MonoBehaviour
{
    public Text txtContagion;
    public Text txtInfection;

    public AgentController myController;

    public Canvas myCanvas;

    public void endAnim()
    {
        gameObject.SetActive(false);
    }

    public void StartAnim()
    {
        if (myController.TotalContagionPerCycle == 0 && myController.totalInfectedCellsThisCycle == 0)
            return;

        int _contagion = Mathf.CeilToInt(myController.TotalContagionPerCycle);
        int _infection = Mathf.CeilToInt(myController.totalInfectedCellsThisCycle);

        myController.TotalContagionPerCycle = 0;
        myController.totalInfectedCellsThisCycle = 0;

        if (_contagion == 0)
            txtContagion.text = "";
        else
            txtContagion.text = _contagion.ToString();

        if (_infection == 0)
            txtInfection.text = "";
        else
            txtInfection.text = _infection.ToString();

       // transform.LookAt(CameraController.Instance.Cam.transform);

        gameObject.SetActive(true);
    }

}
