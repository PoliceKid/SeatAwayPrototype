using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class LevelContainer : MonoBehaviour
{
    [SerializeField] Transform _architectureContainer;
    [SerializeField] Transform _roomConfigContainer;
    [SerializeField] Transform _roomStaticContainer;
    [SerializeField] Transform _gateWayContainer;
    [SerializeField] Transform _roomSpawnerPointContainer;
    [SerializeField] int _lauchCount;
    public Transform GetArchitectureContainer => _architectureContainer;
    public Transform GetRoomConfigContainer => _roomConfigContainer;
    public Transform GetRoomStaticContainer => _roomStaticContainer;
    public Transform GetGateWayContainer=> _gateWayContainer;
    public int GetLauchCount => _lauchCount;
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
