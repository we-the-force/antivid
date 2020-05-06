using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyBuildingWindow : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> Titulos;
    public List<string> Descripciones;
    public List<float> TiempoConstruccion;
    public List<int> Costos;

    public Text txtTitulo;
    public Text txtDescripcion;
    public Text txtTiempos;
    public Text txtCostos;

    public int _building; // 0-healthcare 1-food 2-education 3-entertainment

    [SerializeField]
    private BuyablePlot _selectedBuyablePlot;

    public BuyablePlot SelectedBuyablePlot
    {
        get { return _selectedBuyablePlot; }
    }

    public void ShowWindow(BuyablePlot _buyablePlot)
    {
        _selectedBuyablePlot = _buyablePlot;

        SelectBuilding(0);

        gameObject.SetActive(true);
    }

    public void SelectBuilding(int i)
    {
        _building = i;

        Debug.LogError("SELECCIONADO " + i.ToString());
        
        txtTitulo.text = CanvasControl.instance.BuyableBuildingsCollection[i].Name;
        txtTiempos.text = CanvasControl.instance.BuyableBuildingsCollection[i].TimeToBuild.ToString();
        txtCostos.text = CanvasControl.instance.BuyableBuildingsCollection[i].Cost.ToString();
        txtDescripcion.text = CanvasControl.instance.BuyableBuildingsCollection[i].Description;
                
        /*
        txtTiempos.text = Titulos[i];
        txtDescripcion.text = Descripciones[i];
        txtTiempos.text = TiempoConstruccion[i].ToString();
        txtCostos.text = Costos[i].ToString();
        */
    }

    public void BuySelected()
    {
        Debug.LogError("COMPRANDO " + _building.ToString());
        _selectedBuyablePlot.BuyBuilding(_building);
        CloseWindow();
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
        _selectedBuyablePlot.CanvasCancelButton();
        _selectedBuyablePlot = null;
        CanvasControl.instance.ShowHideUI(true);
    }


}
