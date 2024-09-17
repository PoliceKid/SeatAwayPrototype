using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
public class RoomSort2DGameView : MonoBehaviour
{
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Transform _levelContainer;
    [SerializeField] private Button _lauchBtn;
    [SerializeField] private TextMeshProUGUI _lauchCountText;
    [SerializeField] GameObject _gameOverPopup;
    [SerializeField] LayerMask _blockLayerMask;
    [SerializeField] LayerMask _cellLayerMask;

    public Camera GetMainCam => _mainCam;
    public LayerMask GetBlockLayerMask => _blockLayerMask;
    public LayerMask GetCellLayerMask => _cellLayerMask;
    public Transform GetLevelContainer => _levelContainer;
    public Button GetLauchBtn => _lauchBtn;
    public GameObject GetGameOverPopup => _gameOverPopup;

    public void Init()
    {
        _gameOverPopup.gameObject.SetActive(false);
    }

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
    public void InitButton(int initLauchCount,System.Func<int> checkReult = null)
    {
        _lauchBtn.onClick.RemoveAllListeners();
        UpdateText(initLauchCount);
        _lauchBtn.onClick.AddListener(() =>
        {
            int result = checkReult();
            if (result >=0)
            {        
                UpdateText(result);
            }
        });
    }
    public void UpdateText(int result)
    {
        _lauchCountText.text = $"x{result.ToString()}";
    }
    
}
