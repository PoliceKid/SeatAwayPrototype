using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
public class LevelContainer : MonoBehaviour
{
    [SerializeField] Transform _architectureContainer;
    [SerializeField] Transform _roomConfigContainer;
    [SerializeField] Transform _roomStaticContainer;
    [SerializeField] Transform _gateWayContainer;
    [SerializeField] Transform _roomSpawnerPointContainer;
    [SerializeField] Transform _unitCotainer;
    [SerializeField] int _lauchCount;
    [SerializeField] int _jumpCount;
    [SerializeField] int _minUnitCheckGameOver;
    public Transform GetArchitectureContainer => _architectureContainer;
    public Transform GetRoomConfigContainer => _roomConfigContainer;
    public Transform GetRoomStaticContainer => _roomStaticContainer;
    public Transform GetGateWayContainer=> _gateWayContainer;
    public int GetLauchCount => _lauchCount;
    public int GetJumpCount => _jumpCount;
    public int GetMinUnitWinCondition => _minUnitCheckGameOver;
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
    [ContextMenu("Test")]
    public void TestCreateUnit()
    {
        foreach (Transform child in _unitCotainer)
        {
            Unit unit = child.GetComponent<Unit>();
            Instantiate(unit, _unitCotainer);
        }
    }
}
