using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphController : MonoBehaviour
{
    public int currentPanel = 0; // 0 -- economico 1 -- population

    public GameObject PanelLegendEconomy;
    public GameObject PanelLegendPopulation;

    public GameObject bgButtonEconomy;
    public GameObject bgButtonPopulation;

    private float xSize;
    public float xSeparation = 35f;
    public float lineWidth = 6f;

    [SerializeField] private Sprite circleSprite;
    public RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;

    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    public RectTransform Grid;
    public RectTransform panelEconomy;
    public RectTransform panelPopulation;

    public int MinPopulation;
    public int MaxPopulation;

    public float MinEconomy;
    public float MaxEconomy;
    
    public RectTransform layerHealtyPop;
    public RectTransform layerSeriousPop;
    public RectTransform layerMildPop;
    public RectTransform layerOutPop;

    public RectTransform layerAgentIncome;
    public RectTransform layerExtraIncome;
    public RectTransform layerBuildingCost;
    public RectTransform layerPolicyCost;
    public RectTransform layerVaccineCost;
    public RectTransform layerTotalIncome;

    public Color colorHealtyPop;
    public Color colorSeriousPop;
    public Color colorMildPop;
    public Color colorOutPop;

    public Color colorAgentIncome;
    public Color colorExtraIncome;
    public Color colorBuildingCost;
    public Color colorPolicyCost;
    public Color colorVaccineCost;
    public Color colorTotalIncome;

    public List<int> HealtyPopList = new List<int>();
    public List<int> MildSicknessList = new List<int>();
    public List<int> SeriousSicknessList = new List<int>();
    public List<int> OutofOrderList = new List<int>();

    public List<int> AgentIncomeList = new List<int>();
    public List<int> ExtraIncomeList = new List<int>();
    public List<int> BuildingCost = new List<int>();
    public List<int> PolicyCosts = new List<int>();
    public List<int> VaccioneCosts = new List<int>();
    public List<int> TotalIncomeList = new List<int>();

    private void Start()
    {
        MaxPopulation = WorldAgentController.instance.AgentCollection.Count + 10;
    }

    public void InitGraph()
    {
        //graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
        
        currentPanel = 0;

        ShowGraph(AgentIncomeList, layerAgentIncome, colorAgentIncome);
        ShowGraph(ExtraIncomeList, layerExtraIncome, colorExtraIncome);
        ShowGraph(BuildingCost, layerBuildingCost, colorBuildingCost);
        ShowGraph(PolicyCosts, layerPolicyCost, colorPolicyCost);
        ShowGraph(VaccioneCosts, layerVaccineCost, colorVaccineCost);
        ShowGraph(TotalIncomeList, layerTotalIncome, colorTotalIncome);

        currentPanel = 1;
        ShowGraph(HealtyPopList, layerHealtyPop, colorHealtyPop);
        ShowGraph(MildSicknessList, layerMildPop, colorMildPop);
        ShowGraph(SeriousSicknessList, layerSeriousPop, colorSeriousPop);
        ShowGraph(OutofOrderList, layerOutPop, colorOutPop);

        ShowPanel(0);
    }

    public void ShowPanel(int _panel)
    {
        currentPanel = _panel;

        RemoveObjects(Grid);
        CreateGrid(AgentIncomeList, Grid);

        bool isEconomy = true;
        if (_panel != 0)
            isEconomy = false;

        bgButtonEconomy.SetActive(isEconomy);
        bgButtonPopulation.SetActive(!isEconomy);
        
        panelEconomy.gameObject.SetActive(isEconomy);
        panelPopulation.gameObject.SetActive(!isEconomy);
        PanelLegendEconomy.SetActive(isEconomy);
        PanelLegendPopulation.SetActive(!isEconomy);
/*
        if (currentPanel == 0)
        {
            panelEconomy.gameObject.SetActive(true);
            panelPopulation.gameObject.SetActive(false);
            PanelLegendEconomy.SetActive(true);
            PanelLegendPopulation.SetActive(false);
        }
        else
        {
            panelEconomy.gameObject.SetActive(false);
            panelPopulation.gameObject.SetActive(true);
        }
        */
    }


    public void DisableWindow()
    {
        RemoveObjects(Grid);
        RemoveObjects(layerAgentIncome);
        RemoveObjects(layerBuildingCost);
        RemoveObjects(layerExtraIncome);
        RemoveObjects(layerHealtyPop);
        RemoveObjects(layerMildPop);
        RemoveObjects(layerOutPop);
        RemoveObjects(layerPolicyCost);
        RemoveObjects(layerSeriousPop);
        RemoveObjects(layerVaccineCost);
        RemoveObjects(layerTotalIncome);

        panelEconomy.gameObject.SetActive(false);
        panelPopulation.gameObject.SetActive(false);
    }

    private void RemoveObjects(RectTransform _transform)
    {
        for (int i = 0; i < _transform.childCount; i++)
        {
            Destroy(_transform.GetChild(i).gameObject);
        }
        _transform.gameObject.SetActive(false);
    }

    private GameObject CreateCircle(Vector2 anchoredPosition, Transform _parent)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(_parent, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    public void LayerCheckbox(Toggle _toggleChange)
    {
        switch (_toggleChange.name)
        {
            case "AgentIncome":
                layerAgentIncome.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "ExtraIncome":
                layerExtraIncome.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "TotalIncome":
                layerTotalIncome.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "BuildingCost":
                layerBuildingCost.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "PolicyCost":
                layerPolicyCost.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "VaccineCost":
                layerVaccineCost.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "HealtyPop":
                layerHealtyPop.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "MildPop":
                layerMildPop.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "SeriousPop":
                layerSeriousPop.gameObject.SetActive(_toggleChange.isOn);
                break;
            case "OutPop":
                layerOutPop.gameObject.SetActive(_toggleChange.isOn);
                break;
        }

        Debug.LogError("El toggle " + _toggleChange.name + " esta en " + _toggleChange.isOn);
    }

    private void CreateGrid(List<int> valueList, Transform _parent)
    {
        float graphHeight = graphContainer.sizeDelta.y;

        float yMaximum = MaxPopulation;
        if (currentPanel == 0)
            yMaximum = MaxEconomy;

        xSize = graphContainer.sizeDelta.x / valueList.Count;
        if(xSize > xSeparation)
            xSize = xSeparation;

        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = xSize + i * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;

            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(_parent, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPosition, -15f);
            labelX.GetComponent<Text>().text = i.ToString();

            RectTransform dashX = Instantiate(dashTemplateY);
            dashX.SetParent(_parent, false);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(xPosition, -15f);
        }

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(_parent, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / separatorCount;
            labelY.anchoredPosition = new Vector2(-6f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = Mathf.RoundToInt(normalizedValue * yMaximum).ToString();

            RectTransform dashY = Instantiate(dashTemplateX);
            dashY.SetParent(_parent, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
        }

        _parent.gameObject.SetActive(true);
    }
    
    private void ShowGraph(List<int> valueList, Transform _parent, Color _color)
    {
        float graphHeight = graphContainer.sizeDelta.y;

        float yMaximum = MaxPopulation;
        if (currentPanel == 0)
            yMaximum = MaxEconomy;

        xSize = graphContainer.sizeDelta.x / valueList.Count;
        if (xSize > xSeparation)
            xSize = xSeparation;

        GameObject lastCircleGameObject = null;

        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = xSize + i * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;

            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), _parent);
            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, 
                                    circleGameObject.GetComponent<RectTransform>().anchoredPosition, 
                                    _parent,  
                                    _color);
            }

            lastCircleGameObject = circleGameObject;
        }

        _parent.gameObject.SetActive(true);
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB, Transform _parent, Color _color)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(_parent, false);

        gameObject.GetComponent<Image>().color = _color;
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, lineWidth);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngle(dir));
    }

    public float GetAngle(Vector2 normalizedDelta)
    {
        float angleRadians = Mathf.Atan2(normalizedDelta.y, normalizedDelta.x);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        if (angleDegrees < 0)
            angleDegrees += 360;

        return angleDegrees;
    }

}
