using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;
    
    /// <summary>
    /// Determina la cantidad de porcentage de contagio que se aumenta en todos los agentes de un edificio
    /// cuando un agente cotagiado entra en ese edificio
    /// </summary>
    public float buildingInfectionPercentage;

    /// <summary>
    /// Determina que tan infeccioso es el virus, para los agentes que viajan fuera de la ciudad
    /// es la probabilidad de que alguien regrese contagiado
    /// </summary>
    public float TravelInfectionPercentage;

    /// <summary>
    /// Determina despues de cuantos viajes un agente se contagia; si no ha recibido contagio de manera natural
    /// </summary>
    public int InfectionGuaranteedAfterNumberOfTravel;

    //--- World Time (tic) logic declaration
    public float SecondsPerTic;
    public float TicScale = 1.0f;

    public delegate void OnTicCall();
    public static OnTicCall TicDelegate;

    public bool FirstInfectionDetected;

    [SerializeField]
    public List<StatisticMinMaxObject> StatisticMinMaxCollection;
    [SerializeField]
    public List<StatisticObject> StatisticCollection;

    public int currentTimeCycle = 0;

    IEnumerator TICManager()
    {
        float _seconds = 1;

        while (true)
        {
            if (TicScale == 0)
            {
                //--- es pausa
                _seconds = 0;
                yield return new WaitForFixedUpdate();
            }
            else
            {
                _seconds = SecondsPerTic / TicScale;
                yield return new WaitForSeconds(_seconds);

                TicDelegate();

                CanvasControl.instance.Statistic(WorldAgentController.instance.AgentCollection);
            }
        }
    }

    public void ChangeTimeScale(float _scale)
    {
        //--- Si la _scale es igual a 0; ahi se hace la pausa;
        //---
        TicScale = _scale;

        int idx = Mathf.RoundToInt(_scale * 2);
        CanvasControl.instance.SpeedActiveButton(idx);
    }
    //-----
    
    private void Awake()
    {
        instance = this;
    }

    public Transform NodeParent;
    public List<PathFindingNode> NodeCollection;


    // Start is called before the first frame update
    void Start()
    {
        FirstInfectionDetected = false;
        StartCoroutine(DelayStart());

    }

    IEnumerator DelayStart()
    {
        yield return null;
        NodeCollection = new List<PathFindingNode>();

        for (int i = 0; i < NodeParent.childCount; i++)
        {
            PathFindingNode _node = NodeParent.GetChild(i).GetComponent<PathFindingNode>();
            _node.NodeID = i;
            _node.InitConnections();
            NodeCollection.Add(_node);
        }
        InitPathCollection();
    }


    public List<PerkPercentages> NeedPercentageBaseCollection;
    /*public List<NeedPercentage> GetPerkValues(GlobalObject.AgentPerk perk, out float resourceProduction)
    {
        List<NeedPercentage> _needCollection = NeedPercentageBaseCollection;
        resourceProduction = 1;

        switch (perk)
        {
            case GlobalObject.AgentPerk.Gamer:
                break;
            case GlobalObject.AgentPerk.Executive:
                break;
            case GlobalObject.AgentPerk.LeasureTraveler:
                break;
            case GlobalObject.AgentPerk.Athletic:
                hungerResistance = 0.7f;
                healthResistance = 1.3f;
                break;
            case GlobalObject.AgentPerk.Workaholic:
                resourceProduction = 1.25f;
                healthResistance = 0.7f;
                sleepResistance = 0.85f;
                break;
            case GlobalObject.AgentPerk.Introvert:
                educationResistance = 1.6f;
                wanderResistance = 1.6f;
                healthResistance = 0.6f;
                entertainmentResistance = 1.6f;
                resourceProduction = 1.1f;
                hungerResistance = 1.3f;
                break;
            case GlobalObject.AgentPerk.Extrovert:
                entertainmentResistance = 0.65f;
                healthResistance = 1.2f;
                wanderResistance = 0.65f;
                break;
        }


        return _needCollection;
    }*/

    public void GetPerkValues(GlobalObject.AgentPerk perk,
            out float hungerResistance,
            out float healthResistance,
            out float wanderResistance,
            out float educationResistance,
            out float entertainmentResistance,
            out float sleepResistance,
            out float travelResistance,
            out float resourceProduction
)
    {
        hungerResistance = 1;
        healthResistance = 1;
        wanderResistance = 1;
        educationResistance = 1;
        entertainmentResistance = 1;
        sleepResistance = 1;
        resourceProduction = 1;
        travelResistance = 1;

        switch (perk)
        {
            case GlobalObject.AgentPerk.Gamer:
                break;
            case GlobalObject.AgentPerk.Executive:
                break;
            case GlobalObject.AgentPerk.LeasureTraveler:
                break;
            case GlobalObject.AgentPerk.Athletic:
                hungerResistance = 0.7f;
                healthResistance = 1.3f;
                break;
            case GlobalObject.AgentPerk.Workaholic:
                resourceProduction = 1.25f;
                healthResistance = 0.7f;
                sleepResistance = 0.85f;
                break;
            case GlobalObject.AgentPerk.Introvert:
                educationResistance = 1.6f;
                wanderResistance = 1.6f;
                healthResistance = 0.6f;
                entertainmentResistance = 1.6f;
                resourceProduction = 1.1f;
                hungerResistance = 1.3f;
                break;
            case GlobalObject.AgentPerk.Extrovert:
                entertainmentResistance = 0.65f;
                healthResistance = 1.2f;
                wanderResistance = 0.65f;
                break;
        }
    }


    public void InitPathCollection()
    {
        //--- Ordena tanto los nodos, como las conexiones entre ellos en cada nodo
        NodeCollection = NodeCollection.OrderBy(x => x.NodeID).ToList();

        foreach (PathFindingNode obj in NodeCollection)
        {
            obj.NodeConnection = obj.NodeConnection.OrderBy(x => x.ConnectedNode.NodeID).ToList();
        }
        //---------------------------------------------------------------------------

        int rank = NodeCollection.Count;

        //  Debug.LogError(" >>> TOTAL DE NODOS >>> " + rank);

        int[,] connectionArray = new int[rank, rank];
        string[] ids = new string[rank];

        int row = 0;
        int colDisplacement = 0;
        PathFindingNode rowObj;
        PathFindingNode colObj;

        for (int node = 0; node < rank; node++)
        {
            //--- Este ciclo recorre cada nodo del sistema de busqueda
            //--- y le asigna el camino mas corto a los otros nodos 
            row = node;

            colDisplacement = node;

            //--- Resetear el connection array para llenarlo nuevamente
            ResetConnectionArray(out connectionArray, rank);

            //--- RECORRE NUEVAMENTE TODOS LOS NODOS
            //--- EMPEZANDO DESDE EL NODO SIGUIENTE SELECCIONADO
            for (int i = 0; i < rank; i++)
            {
                rowObj = NodeCollection[i];

                for (int k = 0; k < rowObj.ConnectedNodes.Count; k++)
                {
                    colObj = rowObj.NodeConnection[k].ConnectedNode;

                    int currentRow = rowObj.NodeID - colDisplacement;
                    int currentCol = colObj.NodeID - colDisplacement;
                    if (currentRow < 0)
                    {
                        currentRow = rank + currentRow;  //--- al ser negativo, se resta
                    }
                    if (currentCol < 0)
                    {
                        currentCol = rank + currentCol;  //--- al ser negativo, se resta
                    }

                    //     Debug.LogError("COL: " + currentCol + " ROW: " + currentRow);

                    connectionArray[currentRow, currentCol] = 1;
                }

                ids[i] = row.ToString();

                row++;
                if (row == rank)
                {
                    row = 0;
                }
            }

            List<string> lista = Dijkstra.Instance.DijkstraInit(rank, connectionArray, ids);

            rowObj = NodeCollection[row];
            for (int i = 0; i < lista.Count; i++)
            {
                rowObj.ShortestPathCollection.Add(lista[i]);

                string[] pathIdCollection = lista[i].Split(',');

                int _id = 0;
                int.TryParse(pathIdCollection[0], out _id);
                rowObj.SP_Destinations.Add(_id);

                PathCost _pathCost = new PathCost();

                _pathCost.TileID = int.Parse(pathIdCollection[0]);
                _pathCost.Cost = pathIdCollection.Length;

                rowObj.PathCostCollection.Add(_pathCost);
            }
        }

        
        StartCoroutine("TICManager");
    }

    private void ResetConnectionArray(out int[,] connectionArray, int rank)
    {
        //  Debug.LogError(" >>> Reseteando arreglo: " + rank);

        connectionArray = new int[rank, rank];

        for (int i = 0; i < rank; i++)
        {
            for (int k = 0; k < rank; k++)
            {
                connectionArray[i, k] = -1;
            }
        }
    }


    /// <summary>
    /// ANTON ::
    /// Este metodo se usara para que los agentes puedan conocer cual es el siguiente punto en el 
    /// arreglo de tiles al que deben moverse, regresara un valor de Vector2, que representa el 
    /// punto exacto en el espacio local a donde se movera el agente.
    /// 
    /// De igual forma el calculo por default lo hace buscando al personaje, pero tiene  la opcion
    /// de recibir un segundo valor en forma de Vector2, con lo cual lo utilizara para encontrar la
    /// ruta mas corta hacia esa segunda posicion
    /// </summary>
    /// <param name="myPosition">La posicion del agente que solicita una ruta</param>
    /// <param name="findPlayer">TRUE: Por default busca al jugador ; FALSE : busca en referencia a somePosition</param>
    /// <param name="somePosition">Posicion asignada a partir de la cual realiza la busqueda</param>
    /// <returns></returns>
    //public Transform GetNextTileInRoute(Transform myPosition, Transform somePosition = null)
    public PathFindingNode GetNextTileInRoute(int myNodeID, int nextNodeID, PathFindingNode _tileOrigin = null)
    {
        PathFindingNode result = null;
        PathFindingNode tileInfoOrigin = null;
       // PathFindingNode tileInfoDestination = null;
        string destinationId;
        string[] pathIdCollection;

        if (_tileOrigin != null)
        {
            tileInfoOrigin = _tileOrigin;
        }
        else
        {
            for (int i = 0; i < NodeCollection.Count; i++)
            {
                if (NodeCollection[i].NodeID == myNodeID)
                    tileInfoOrigin = NodeCollection[i];

                //if (NodeCollection[i].NodeID == nextNodeID)
                //    tileInfoDestination = NodeCollection[i];
            }
        }

        destinationId = nextNodeID.ToString();
        for (int i = 0; i < tileInfoOrigin.ShortestPathCollection.Count; i++)
        {
            pathIdCollection = tileInfoOrigin.ShortestPathCollection[i].Split(',');
            if (pathIdCollection[0] == destinationId)
            {
                destinationId = pathIdCollection[pathIdCollection.Length - 1];
                break;
            }
        }

        for (int i = 0; i < NodeCollection.Count; i++)
        {
            if (NodeCollection[i].NodeID.ToString() == destinationId)
            {
                result = NodeCollection[i];
                break;
            }
        }

        return result;
    }

    public string GetCompletePath(int destinationID, PathFindingNode _tileOrigin)
    {
        string _result = "";

        for (int i = 0; i < _tileOrigin.SP_Destinations.Count; i++)
        {
            if (_tileOrigin.SP_Destinations[i] == destinationID)
            {
                _result = _tileOrigin.ShortestPathCollection[i];
                break;
            }
        }

        return _result;
    }

    public PathFindingNode GetNodeFromID(int pathNodeId)
    {
        PathFindingNode _result = null;

        for (int i = 0; i < NodeCollection.Count; i++)
        {
            if (NodeCollection[i].NodeID == pathNodeId)
            {
                _result = NodeCollection[i];
                break;
            }
        }

        return _result;
    }


}
