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

    private BuyablePlot SelectedBuyablePlot;

    public void ShowWindow(BuyablePlot _buyablePlot)
    {
        SelectedBuyablePlot = _buyablePlot;

        SelectBuilding(0);

        gameObject.SetActive(true);
    }

    public void SelectBuilding(int i)
    {
        _building = i;

        Debug.LogError("SELECCIONADO " + i.ToString());

        txtTiempos.text = Titulos[i];
        txtDescripcion.text = Descripciones[i];
        txtTiempos.text = TiempoConstruccion[i].ToString();
        txtCostos.text = Costos[i].ToString();
    }

    public void BuySelected()
    {
        SelectedBuyablePlot.BuyBuilding(_building);
        CloseWindow();
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);

        SelectedBuyablePlot.CanvasCancelButton();

        CanvasControl.instance.EconomicPanel.SetActive(true);
        CanvasControl.instance.ControlsPanel.SetActive(true);
        CanvasControl.instance.InfoPanel.SetActive(true);
    }


}
