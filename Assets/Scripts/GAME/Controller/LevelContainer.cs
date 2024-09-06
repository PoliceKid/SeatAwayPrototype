using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelContainer : MonoBehaviour
{
    [SerializeField] Transform _architectureContainer;
    [SerializeField] Transform _roomContainer;
    [SerializeField] Transform _gateWayContainer;
    public Transform GetArchitectureContainer => _architectureContainer;
    public Transform GetRoomContainer => _roomContainer;
    public Transform GetGateWayContainer=> _gateWayContainer;
}
