using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class LevelContainer : MonoBehaviour
{
    [SerializeField] Transform _architectureContainer;
    [SerializeField] Transform _roomContainer;
    [SerializeField] Transform _gateWayContainer;
    [SerializeField] Transform _roomSpawnerPointContainer;
    public Transform GetArchitectureContainer => _architectureContainer;
    public Transform GetRoomContainer => _roomContainer;
    public Transform GetGateWayContainer=> _gateWayContainer;
    public Transform[] GetRoomSpawnerPoints
    {
        get
        {
            Transform[] _roomSpawnerPoints = new Transform[3];
            int count = 0;
            foreach (Transform child in _roomSpawnerPointContainer)
            {
                _roomSpawnerPoints[count] = child;
                count++;
            }
            return _roomSpawnerPoints;
        }
    }
}
