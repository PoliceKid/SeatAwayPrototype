using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceContainer 
{
    
}
public interface IOccupier
{
    public string GetOccupierType();
    public Vector3 GetDirection();
    public void OnPlaceable(bool isValid);
    public void InitOccupier(System.Action callBack = null);
    
}
public interface IOccupierContainer<T> where T : IOccupier
{
    void SetOccupier(T occupier);
    void RemoveOccupier(T occupier);
    void ClearOccupiers();
    T GetLastOccupier();
    bool IsFullOccupier();
    bool IsPlaceable();
    bool IsOccupier();
}
public interface IPlaceableCondition
{
    public bool CheckMatchCondition(IOccupier occupier);
}
