using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 
/// Dijkstra algoritm implementation.
///  
/// Adaptation of the solution proposed by Mehmet Ali ECER 
/// (mehmetaliecer@gmail.com) as found on site:
/// http://www.codeproject.com/Articles/19919/Shortest-Path-Problem-Dijkstra-s-Algorithm
/// and used under The Code Project Open License (CPOL) (http://www.codeproject.com/info/cpol10.aspx)
/// 
/// The changes made to the original Work, provide the access to the correct path from initial
/// point to every other points in the array
/// 
/// Modifications made by Anton Ortega Rivera on September 11, 2014
/// 
/// </summary>


public class Dijkstra : MonoBehaviour
{
	private int rank = 0;
	private int[,] L;
	private int[] C; 
	private int[] D;

	private List<string> pathResultCollection;

	/*
	private string[] _pathResult; 
	public string[] PathResult
	{
		set { _pathResult = value; }
		get { return _pathResult; }
	}
*/

	private string[] IDArray; 

	private int parent = 0;

	private int trank = 0;

	public static Dijkstra Instance;

	void Awake()
	{
		Instance = this;
	}
	void Start(){}

	public List<string> DijkstraInit(int paramRank, int [,] paramArray, string[] ids)
	{
		L = new int[paramRank, paramRank]; //--- matriz con todas las conexiones del sistema
		C = new int[paramRank]; //--- Node Array
		D = new int[paramRank]; //--- Cost Array


		IDArray = ids;
		rank = paramRank;
		pathResultCollection = new List<string>();


		//_pathResult = new string[paramRank]; 
		for(int i=0;i<rank;i++)
		{
			//_pathResult[i] = IDArray[i];
			pathResultCollection.Add(IDArray[i]);
		}
		parent = 0;

		for (int i = 0; i < rank; i++)
		{
			for (int j = 0; j < rank; j++) {
				L[i, j] = paramArray[i, j];
			}
		}

		//--- Inicializacion estandar del arreglo de los nodos
		for (int i = 0; i < rank; i++)
		{
			C[i] = i;
		}
		C[0] = -1;   
		//------------------------------------------------------

		//--- Inicializa los costos con la primer fila de la matriz de conexiones
		for (int i = 1; i < rank; i++)
		{
			D[i] = L[0, i];
		}
		//-------------------------------------------------------------------------

		for (trank = 1; trank < rank; trank++)
		{
			DijkstraSolving();
		}

		return pathResultCollection;
	}


	public void DijkstraSolving()
	{            
		int minValue = int.MaxValue;
		int minNode = 0;

		for (int i = 0; i < rank; i++)
		{
			if (C[i] == -1)
			{
				continue;
			}
				

			if (D[i] > 0 && D[i] < minValue)
			{
				minValue = D[i];
				minNode = i;

				parent = i;

			}
		}

		C[minNode] = -1;

		for (int i = 0; i < rank; i++)
		{ 
			if (L[minNode, i] < 0)
			{
				continue;
			}
				

			if (D[i] < 0) {
				D[i] = minValue + L[minNode, i];

				//_pathResult[i] += _pathResult[parent];
				pathResultCollection[i] = pathResultCollection[i] + "," + pathResultCollection[parent];
				continue;
			}

			if ((D[minNode] + L[minNode, i]) < D[i])
			{
				D[i] = minValue+ L[minNode, i];

				//_pathResult[i] = IDArray[i] + _pathResult[parent];
				pathResultCollection[i] = IDArray[i] + "," + pathResultCollection[parent];
			}
		}
	}
}

