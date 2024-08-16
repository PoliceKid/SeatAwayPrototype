using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSort2DGameView : MonoBehaviour
{
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Transform _levelContainer;
    [SerializeField] private Transform _gridParentCotainer;
    [SerializeField] private Transform _placeholderGridParentCotainer;
    public Transform GetGridParentCotainer => _gridParentCotainer;
    public Transform GetPlaceholderGridParentCotainer => _placeholderGridParentCotainer;
    public Camera GetMainCam => _mainCam;
}
