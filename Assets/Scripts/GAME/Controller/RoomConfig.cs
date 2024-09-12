using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomConfig : MonoBehaviour
{
    [SerializeField] Transform _BlockContainer;
    public int GetBlockCount() => _BlockContainer.childCount;
}
