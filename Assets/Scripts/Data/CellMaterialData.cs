using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="CellMaterialData",menuName ="Data/CellMaterial",order =1)]
public class CellMaterialData : ScriptableObject
{
    public List<CellMaterialWrapper> CellMaterials;
    public Material FindMaterial(CellType type)
    {
        foreach (var item in CellMaterials)
        {
            if (item.Type == type)
            {
                return item.Mat;
            }
        }
        Debug.Log("Can't find the CellType in Data");
        return default;
    }
}

[Serializable]
public class CellMaterialWrapper
{
    [SerializeField]
    private CellType m_type;
    public CellType Type { get => m_type; }
    [SerializeField]
    private Material m_material;
    public Material Mat { get => m_material; }
}
