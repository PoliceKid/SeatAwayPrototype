using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelContainer : MonoBehaviour
{
    [SerializeField] Transform _architectureContainer;
    [SerializeField] Transform _roomContainer;

    public Transform GetArchitectureContainer => _architectureContainer;
    public Transform GetRoomContainer => _roomContainer;
}
