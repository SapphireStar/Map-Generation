using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiCellBrush : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_layerMask;
    [SerializeField]
    private GenerateVoronoi m_generator;

    private CellType m_cellType = CellType.Land;
    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Raycast();
        }
    }
    void Raycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray,out RaycastHit hit,100, m_layerMask);
        if (hit.collider != null)
        {
            VoronoiCell cell = hit.transform.GetComponent<VoronoiCell>();
            m_generator.UpdateVoronoiCell(cell.Pos, m_cellType);
        }
    }

    public void ChooseLand()
    {
        m_cellType = CellType.Land;
    }

    public void ChooseWater()
    {
        m_cellType = CellType.Water;
    }
    public void ChooseOcean()
    {
        m_cellType = CellType.Water | CellType.Ocean;
    }
}
