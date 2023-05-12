using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorScreen : MonoBehaviour
{
    [SerializeField]
    private VoronoiCellBrush m_brush;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChooseLand()
    {
        m_brush.ChooseLand();
    }

    public void ChooseWater()
    {
        m_brush.ChooseWater();
    }

    public void ChooseOcean()
    {
        m_brush.ChooseOcean();
    }
}
