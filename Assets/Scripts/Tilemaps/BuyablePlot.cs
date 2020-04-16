using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PlotSize { TwoByTwo, TwoByThree, ThreeByThree, ThreeByFour };
public class BuyablePlot : MonoBehaviour
{
    [SerializeField]
    Image barraProgreso;

    public float TicsToBuild = 15;

    public float Cost = 5f;
    public float UpkeepCost = 1000;
    public bool IsBuyable = true;
    public bool IsBuildable = true;
    public bool IsBought = false;
    public PlotSize Size;
    public List<NodeType> InvalidNodeTypes = new List<NodeType>();
    public List<GameObject> Models;
    public List<Material> Materials;
    public NodeType AssignedNode = NodeType.None;
    public GameObject AssignedModel;
    [SerializeField]
    PathFindingNode pfnode;
    [SerializeField]
    BuildingController buildCont;

    GameObject canvas;
    [SerializeField]
    bool interactingWithCanvas = false;
    //A list of what can be built depending on the dimensions.
    private void Awake()
    {
        pfnode = GetComponent<PathFindingNode>();
        buildCont = GetComponent<BuildingController>();
        canvas = transform.GetChild(0).gameObject;
        IsBought = false;
        IsBuyable = true;
        UpdateCost();
      //  ShowCanvas();
    }
    void Start()
    {
        HandleModel();
        canvas.GetComponent<RectTransform>().rotation = Quaternion.Euler(90f, 0, 0);

    }

    private void TicReceived()
    {
        currentTics++;
        barraProgreso.fillAmount = currentTics / TicsToBuild;

        if (currentTics == TicsToBuild)
        {
            Buy(auxNode);
        }
    }


    public void Buy(NodeType type)
    {
        WorldManager.TicDelegate -= TicReceived;
        UnderConstruction = false;

        canvas.SetActive(false);

        IsBuyable = false;
        IsBought = true;
        AssignedNode = type;
        UpdateCost();
        HandleModel();
        interactingWithCanvas = false;
        //ShowCanvas();
        buildCont.TicsToCoverNeed = type == NodeType.HealthCare ? 40 : 20;
        WorldAgentController.instance.CalculateBuildingUpkeepCost();
    }
    public void UnBuy()
    {
        IsBuyable = true;
        IsBought = false;
        AssignedNode = NodeType.Buyable;
        UpdateCost();
        HandleModel();
        WorldAgentController.instance.CalculateBuildingUpkeepCost();
    }
    public void HandleModel()
    {
        if (IsBought)
        {
            if (!InvalidNodeTypes.Contains(AssignedNode))
            {
                DestroyAssignedModel();
                AssignedModel = Instantiate(Models[(int)Size], transform.GetChild(1));
                ChangeAssignedNodeType();
            }
            else
            {
                DestroyAssignedModel();
                AssignedModel = Instantiate(Models[4], transform.GetChild(1));
                ChangeAssignedNodeType();
            }
        }
        else
        {
            DestroyAssignedModel();
            AssignedModel = Instantiate(Models[4], transform.GetChild(1));
            ChangeAssignedNodeType();
        }
    }
    void ChangeAssignedNodeType()
    {
        ChangeAssignedModelRoof();
        if (pfnode == null)
        {
            pfnode = GetComponent<PathFindingNode>();
        }
        pfnode.nodeType = AssignedNode;
        ChangeNeedCovered();
    }
    void ChangeNeedCovered()
    {
        if (buildCont == null)
        {
            buildCont = GetComponent<BuildingController>();
        }
        buildCont.MainNeedCovered = (TranslateNodeTypeToNeed(AssignedNode));
    }
    GlobalObject.NeedScale TranslateNodeTypeToNeed(NodeType type)
    {
        switch (type)
        {
            case NodeType.Food:
                return GlobalObject.NeedScale.Hunger;
            case NodeType.HealthCare:
                return GlobalObject.NeedScale.HealtCare;
            case NodeType.Education:
                return GlobalObject.NeedScale.Education;
            case NodeType.Entertainment:
                return GlobalObject.NeedScale.Entertainment;
        }
        return GlobalObject.NeedScale.None;
    }
    void ChangeAssignedModelRoof()
    {
        if (AssignedModel != null && AssignedModel != Models[4])
        {
            switch (AssignedNode)
            {
                case NodeType.Entertainment:
                    AssignedModel.transform.GetChild(1).GetComponent<Renderer>().material = Materials[0];
                    break;
                case NodeType.HealthCare:
                    AssignedModel.transform.GetChild(1).GetComponent<Renderer>().material = Materials[1];
                    break;
                case NodeType.Food:
                    AssignedModel.transform.GetChild(1).GetComponent<Renderer>().material = Materials[2];
                    break;
                case NodeType.Education:
                    AssignedModel.transform.GetChild(1).GetComponent<Renderer>().material = Materials[3];
                    break;
            }
        }
    }
    void DestroyAssignedModel()
    {
        if (AssignedModel != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(AssignedModel);
#else
            Destroy(AssignedModel);
#endif
        }
    }
    void UpdateCost()
    {
        buildCont.UpkeepCost = IsBought ? UpkeepCost : 0;
    }

    //0: Health, 1: Food, 2 Entertainment
    private NodeType auxNode = NodeType.None;
    private bool UnderConstruction;
    private int currentTics = 0;
    public void BuyBuilding(int type)
    {
        Debug.LogError("Comprar el tipo  " + type.ToString());

        if (CurrencyManager.Instance.HasEnoughCurrency(Cost))
        {
            canvas.SetActive(true);

            UnderConstruction = true;
            currentTics = 0;
            Debug.Log("Buying building!");
            CurrencyManager.Instance.CurrentCurrency -= Cost;
            //Buy(nodeType);

            switch (type)
            {
                case 0:
                    auxNode = NodeType.HealthCare;
                    break;
                case 1:
                    auxNode = NodeType.Food;
                    break;
                case 2:
                    auxNode = NodeType.Education;
                    break;
                case 3:
                    auxNode = NodeType.Entertainment;
                    break;
            }
                        
            WorldManager.TicDelegate += TicReceived;
            //Buy(type == 0 ? NodeType.HealthCare : type == 1 ? NodeType.Food : NodeType.Entertainment);           
        }
        else
        {
            Debug.LogError("No tiene dinero " + Cost.ToString());
        }
        
    }
    public void CanvasCancelButton()
    {
        WorldManager.instance.ChangeTimeScale(1);
        interactingWithCanvas = false;
        //ShowCanvas();
    }

    void ShowCanvas()
    {
        if (interactingWithCanvas)
        {
            WorldManager.instance.ChangeTimeScale(0);
            CanvasControl.instance.ShowBuildWindow(this);
        }


        //canvas.SetActive(interactingWithCanvas);
    }
    public void LeftClick()
    {
        if (UnderConstruction)
            return;

        interactingWithCanvas = true;
        ShowCanvas();
        Debug.Log("I was clicked!");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BuyablePlot), true)]
[CanEditMultipleObjects]
public class BuyablePlotEditor : Editor
{
    private BuyablePlot BPComp { get { return (target as BuyablePlot); } }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        if (GUILayout.Button("Do the thing with the models"))
        {
            BPComp.HandleModel();
        }
    }
}
#endif
