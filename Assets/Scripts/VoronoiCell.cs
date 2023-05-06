using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VoronoiCell : MonoBehaviour, IPointerClickHandler,IPointerEnterHandler
{
    public Vector2f Pos;
    public GenerateVoronoi Parent;

    public void Initialize(Vector2f pos, GenerateVoronoi parent, Material material)
    {
        this.Pos = pos;
        this.Parent = parent;
        GetComponent<MeshRenderer>().material = material;
        SetColliderPos(Pos);
    }
    public void SetColliderPos(Vector2f pos)
    {
        GetComponent<BoxCollider>().center = new Vector3(pos.x, pos.y, 0);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log(Pos);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Parent.UpdateVoronoiCell(Pos, CellType.Land);
            Debug.Log(Pos);
        }
    }
}
