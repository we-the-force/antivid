using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warehouse : MonoBehaviour
{
    public List<WarehouseItemObject> StockPile = new List<WarehouseItemObject>();

    private void Start()
    {
        for (int i = 0; i < StockPile.Count; i++)
        {
            StockPile[i].CurrentMaxQty = StockPile[i].BaseMaxQty;
            StockPile[i].CurrentQty = 0;
        }
    }


    /// <summary>
    /// Si la necesidad existe en el stockpile, se agrega la cantidad de items y regresa verdadero, de lo contrario regresa falso
    /// </summary>
    /// <param name="forNeed"></param>
    /// <param name="Qty"></param>
    /// <returns></returns>
    public bool StoreGoods(GlobalObject.NeedScale forNeed, float Qty)
    {
        bool GoodExist = false;

        Debug.LogError(">Almacenando para need " + forNeed.ToString() + " Qty " + Qty);

        for (int i = 0; i < StockPile.Count; i++)
        {
            if (StockPile[i].Need == forNeed)
            {
                GoodExist = true;
                StockPile[i].CurrentQty += Qty;
                if (StockPile[i].CurrentQty > StockPile[i].CurrentMaxQty)
                    StockPile[i].CurrentQty = StockPile[i].CurrentMaxQty;

                break;
            }
        }

        return GoodExist;
    }

    /// <summary>
    /// Si la necesidad puede ser cubierta por los items en el Stockpile, y hay mas de 0
    /// la funcion regresa la cantidad que el stockpile puede manejar, y actualiza el stockpile, de lo contrario regresa 0
    /// </summary>
    /// <param name="forNeed"></param>
    /// <param name="Qty"></param>
    /// <returns></returns>
    public float UseGoods(GlobalObject.NeedScale forNeed, float Qty)
    {
        float canGive = 0;

        for (int i = 0; i < StockPile.Count; i++)
        {
            if (StockPile[i].Need == forNeed)
            {
                if (StockPile[i].CurrentQty > Qty)
                {
                    StockPile[i].CurrentQty -= Qty;
                    canGive = Qty;
                }
                else
                {
                    canGive = StockPile[i].CurrentQty;
                    StockPile[i].CurrentQty = 0;
                }
            }
        }
        return canGive;
    }

    /// <summary>
    /// Expande el Stockpile para una necesidad especifica, asignando una cantidad inicial (Qty) y una Cantidad Base maxima (_maxQty)
    /// Para los casos de que la necesidad ya exista en el Stockpile, actualiza la Cantidad Actual Maxima, sumando el (_maxQty) recibido al BaseMaxQty
    /// </summary>
    /// <param name="withNeed"></param>
    /// <param name="Qty"></param>
    /// <param name="_maxQty"></param>
    public void ExpandStockpile(GlobalObject.NeedScale withNeed, float Qty = 0, float _maxQty = 100)
    {
        bool canExpand = true;

        for (int i = 0; i < StockPile.Count; i++)
        {
            if (StockPile[i].Need == withNeed)
            {
                canExpand = false;
                StockPile[i].CurrentMaxQty = StockPile[i].BaseMaxQty + _maxQty;
                break;
            }
        }

        if (canExpand)
        {
            WarehouseItemObject obj = new WarehouseItemObject();
            obj.Need = withNeed;
            obj.CurrentQty = Qty;
            obj.BaseMaxQty = _maxQty;
            obj.CurrentMaxQty = _maxQty;
        }
    }


    /// <summary>
    /// Si por alguna razon es necesario eliminar el soporte de una necesidad por el warehouse, esta es la funcion para hacerlo, y regresa la cantidad actual de activos
    /// </summary>
    /// <param name="needToRemove"></param>
    /// <returns></returns>
    public float RemoveStockpile(GlobalObject.NeedScale needToRemove)
    {
        float qty = 0;

        for (int i = 0; i < StockPile.Count; i++)
        {
            if (StockPile[i].Need == needToRemove)
            {
                qty = StockPile[i].CurrentQty;
                StockPile.RemoveAt(i);
                break;
            }
        }

        return qty;
    }

}
