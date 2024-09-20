using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public class RoomSort2DGameView : MonoBehaviour
{
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Transform _levelContainer;
    [SerializeField] private Button _launchBtn;
    [SerializeField] private Button _launchAllBtn;
    [SerializeField] private Button _jumpBtn;
    [SerializeField] private TextMeshProUGUI _lauchCountText;
    [SerializeField] private TextMeshProUGUI _JumpCountText;
    [SerializeField] private TextMeshProUGUI _unitCountOverView;
    [SerializeField] private TextMeshProUGUI _unitWinCondition;
    [SerializeField] private Image _warningImage;
    [SerializeField] GameObject _gameOverPopup;
    [SerializeField] LayerMask _blockLayerMask;
    [SerializeField] LayerMask _cellLayerMask;
    [SerializeField] private bool _blockDirPathFinding;
    public Camera GetMainCam => _mainCam;
    public LayerMask GetBlockLayerMask => _blockLayerMask;
    public LayerMask GetCellLayerMask => _cellLayerMask;
    public Transform GetLevelContainer => _levelContainer;
    public Button GetLauchBtn => _launchBtn;
    public GameObject GetGameOverPopup => _gameOverPopup;
    public bool GetBlockDirPathFinding => _blockDirPathFinding;
    public TextMeshProUGUI JumpCountText => _JumpCountText;
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
    public void InitlaunchButton(int initLauchCount,System.Func<int> checkReult = null)
    {
        _launchBtn.onClick.RemoveAllListeners();
        UpdateText(_lauchCountText, $"Launch left: {initLauchCount.ToString()}");
        _launchBtn.onClick.AddListener(() =>
        {
            int result = checkReult();
            if (result >=0)
            {        
                UpdateText(_lauchCountText, $"Launch left: {result.ToString()}");
            }
        });
    }
    public void InitlaunchAllButton(int initLauchCount, System.Func<int> checkReult = null)
    {
        _launchAllBtn.onClick.RemoveAllListeners();
        _launchAllBtn.onClick.AddListener(() =>
        {
            int result = checkReult();
            if (result >= 0)
            {
                UpdateText(_lauchCountText, $"Launch left: {result.ToString()}");
            }
        });
    }
    public void InitJumplButton(int initJumpCount,System.Action<int> checkJumpResult, System.Func<bool> performAction = null)
    {
        UpdateText(_JumpCountText, $"Jump x{initJumpCount.ToString()}");
        //checkJumpResult = (result) => { 
        //    UpdateText(_JumpCountText, $"Jump x{result.ToString()}"); 
        //};
        _jumpBtn.onClick.RemoveAllListeners();
        _jumpBtn.onClick.AddListener(() =>
        {
             
            if (performAction != null)
            {
                bool result = performAction();
                if (result)
                {
                    //UpdateText(_JumpCountText, $"Jump x{result.ToString()}");
                    //Active Button canlce here
                }
            }
        });
    }
    public void InitMinUnitWinCondition(int initMinUnit)
    {
        UpdateText(_unitWinCondition, $"Min Unit: {initMinUnit}");
    }
    public void UpdateText(TextMeshProUGUI text,string result)
    {
        text.text = result;
    }
    public void HandleUpdateJumpCount(int jumpCount)
    {
        UpdateText(_JumpCountText, $"Jump x{jumpCount.ToString()}");
    }
    public void HandleUpdateUnitOverviewText(int totalUnitComplete,int initTotalUnit)
    {
        UpdateText(_unitCountOverView, $"Unit: {totalUnitComplete}/{initTotalUnit}");
    }
    public void InitEventUpdateUnitOverviewText(System.Action<int, int> _OnUnitQueueUpdate)
    {
        _OnUnitQueueUpdate += (totalComplete, initTotalUnit) =>
        {
            HandleUpdateUnitOverviewText(totalComplete, initTotalUnit);
        };
    }
    Tween _tween;
    public void HandleShowWarning(bool isActive)
    {
        _warningImage.gameObject.SetActive(isActive);
        if (isActive)
        {
            if (_tween != null)
            {
                _tween.Kill();
            }
            _tween = DotweenAnimationHelper.AnimationScaleLoop(_warningImage.gameObject, 0.8f, 0.5f);
        }
        else
        {
            if (_tween != null)
            {
                _tween.Kill();
            }
        }
    }
}
