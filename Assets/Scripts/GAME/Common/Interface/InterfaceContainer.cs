using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceContainer 
{
    
}
public interface IOccupier
{
    public Vector3 GetDirection();
    public BlockType GetBlockType();
}
public interface IPlaceableCondition
{
    public bool IsMatchCondition(IOccupier occupier);
}
