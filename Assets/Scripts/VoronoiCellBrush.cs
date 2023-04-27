using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiCellBrush : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_layerMask;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
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
            //Debug.Log(hit.collider);
        }
    }
}
