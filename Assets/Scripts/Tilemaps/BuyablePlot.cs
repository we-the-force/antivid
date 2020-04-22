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

    private int modelId;

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



    BuyBuildingWindow buyBuildingWindow;
    //BuyBuildingWindow

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
        //buyBuildingWindow = GameObject.Find("BuyBuildingWindow").GetComponent<BuyBuildingWindow>();
        buyBuildingWindow = GameObject.Find("Canvas").transform.Find("BuyBuildingWindow").GetComponent<BuyBuildingWindow>();
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
        //UnderConstruction = false;

        canvas.SetActive(false);

        IsBuyable = false;
        IsBought = true;
        AssignedNode = type;
        UpdateCost();

        HandleModel();

        interactingWithCanvas = false;
        buildCont.BaseTicsToCoverNeed = type == NodeType.HealthCare ? 200 : 100;
        WorldAgentController.instance.CalculateBuildingUpkeepCost();
        WorldAgentController.instance.RefreshBuildingsPolicies();
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

    public Transform newBuildingAnchor;
    public Transform constructionAnchor;

    /*IEnumerator rotateModel()
    {
        yield return new WaitForFixedUpdate();
        AssignedModel.transform.localPosition = Vector3.zero;
        AssignedModel.transform.localEulerAngles = Vector3.zero;
        AssignedModel.SetActive(true);
    }*/

    public void HandleModel()
    {
        if (IsBought)
        {
            if (!InvalidNodeTypes.Contains(AssignedNode))
            {
                DestroyAssignedModel();

                Debug.LogError("MODEL ID " + modelId);

                //AssignedModel = Instantiate(Models[modelId], newBuildingAnchor);
                AssignedModel = Instantiate(CanvasControl.instance.BuyableBuildingsCollection[modelId].objPrefab, newBuildingAnchor);

                AssignedModel.transform.localPosition = Vector3.zero;
                AssignedModel.transform.localEulerAngles = Vector3.zero;
                AssignedModel.SetActive(true);

                BuildingController BC = AssignedModel.GetComponent<BuildingController>();
                buildCont.UpkeepCost = BC.UpkeepCost;
                buildCont.BaseNeedCovered = BC.BaseNeedCovered;
                buildCont.TimeToCoverNeed = BC.TimeToCoverNeed;
                buildCont.BaseTicsToCoverNeed = BC.BaseTicsToCoverNeed;
                buildCont.BasePercentageRestored = BC.BasePercentageRestored;
                buildCont.BaseAgentCapacity = BC.BaseAgentCapacity;
                buildCont.NewBuilding();

                BC.enabled = false;
                AssignedModel.GetComponent<PathFindingNode>().enabled = false;

                //StartCoroutine("rotateModel");

                constructionAnchor.gameObject.SetActive(false);
                //AssignedModel = Instantiate(Models[(int)Size], transform.GetChild(1));

                ChangeAssignedNodeType();
            }
            else
            {
                DestroyAssignedModel();
                AssignedModel = Instantiate(Models[4], newBuildingAnchor);

                StartCoroutine("rotateModel");

                constructionAnchor.gameObject.SetActive(false);
                //AssignedModel = Instantiate(Models[4], transform.GetChild(1));
                ChangeAssignedNodeType();
            }
        }
        else
        {
            DestroyAssignedModel();
            AssignedModel = Instantiate(Models[4], newBuildingAnchor);
           // constructionAnchor.gameObject.SetActive(false);
            //AssignedModel = Instantiate(Models[4], transform.GetChild(1));
            ChangeAssignedNodeType();
        }
    }
    void ChangeAssignedNodeType()
    {
        //ChangeAssignedModelRoof();
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
        buildCont.BaseNeedCovered = (TranslateNodeTypeToNeed(AssignedNode));
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
    
    
    //--- ya no debe hacer eso por que ya son modelos finales
    /*
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
    */
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

        Cost = CanvasControl.instance.BuyableBuildingsCollection[type].Cost;
        TicsToBuild = CanvasControl.instance.BuyableBuildingsCollection[type].TimeToBuild;

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
                    modelId = 0;
                    break;
                case 1:
                    auxNode = NodeType.Food;
                    modelId = 1;
                    break;
                case 2:
                    auxNode = NodeType.Education;
                    modelId = 2;
                    break;
                case 3:
                    auxNode = NodeType.Entertainment;
                    modelId = 3;
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
        if (interactingWithCanvas && buyBuildingWindow.SelectedBuyablePlot == null)
        {
            WorldManager.instance.ChangeTimeScale(0);
            CanvasControl.instance.ShowBuildWindow(this);
        }
        Debug.Log($"Tried to do the thing but couldn do it lmao ({interactingWithCanvas} && {(buyBuildingWindow.SelectedBuyablePlot == null)})");

        //canvas.SetActive(interactingWithCanvas);
    }
    public void LeftClick()
    {
        if (UnderConstruction)
            return;

        interactingWithCanvas = true;
        ShowCanvas();
        Debug.Log("I was clicked!");
        //if (!IsBought)
        //{
        //    interactingWithCanvas = true;
        //    ShowCanvas();
        //    Debug.Log("I was clicked!");
        //}
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
