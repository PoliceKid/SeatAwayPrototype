using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class RoomSort2DGameView : MonoBehaviour
{
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Transform _levelContainer;
    [SerializeField] LayerMask _blockLayerMask;
    [SerializeField] LayerMask _cellLayerMask;
    public Camera GetMainCam => _mainCam;
    public LayerMask GetBlockLayerMask => _blockLayerMask;
    public LayerMask GetCellLayerMask => _cellLayerMask;
    public Transform GetLevelContainer => _levelContainer;

    public LevelContainer SpawnLevel(GameObject levelPrefab, Vector3 point, Quaternion quaternion)
    {
        foreach (Transform child in _levelContainer)
        {
            child.gameObject.SetActive(false);
        }
        GameObject LevelGO = Instantiate(levelPrefab, point, quaternion, _levelContainer);
        if(LevelGO != null)
        {
            LevelContainer level = LevelGO.GetComponent<LevelContainer>();
       
            return level;
        }
        return null;
    }
    public Room SpawnRoom(GameObject roomPrefab, Vector3 point, Quaternion quaternion,Transform parent)
    {
        GameObject roomGO = Instantiate(roomPrefab, point, quaternion, parent);
        if (roomGO != null)
        {
            Room room = roomGO.GetComponent<Room>();

            return room;
        }
        return null;
    }
}
