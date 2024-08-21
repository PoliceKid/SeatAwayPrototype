using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSort2DGameView : MonoBehaviour
{
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Transform _levelContainer;
    [SerializeField] private Transform _gridParentCotainer;
    [SerializeField] private Transform _placeholderGridParentCotainer;
    [SerializeField] LayerMask _blockLayerMask;
    [SerializeField] LayerMask _cellLayerMask;
    public Transform GetRoomParentCotainer => _gridParentCotainer;
    public Transform GetArchitectureContainer => _placeholderGridParentCotainer;
    public Camera GetMainCam => _mainCam;
    public LayerMask GetBlockLayerMask => _blockLayerMask;
    public LayerMask GetCellLayerMask => _cellLayerMask;

}
