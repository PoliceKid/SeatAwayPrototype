using Game.Core;
using Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Level.Grid;
using System.Linq;
public class RoomSort2DGameManager : IDisposable
{
    [Inject] private Timer _timer;
    private RoomSort2DGameView _gameView;
    public RoomSort2DGameManager(RoomSort2DGameView gameView)
    {
        _gameView = gameView;
    }
    private List<GridCotainerController> gridCotainerControllers = new List<GridCotainerController>();
    private PlaceholderGridContainerController placeholderGridCotainerControllers = null;
    private GridContainerView _currentGridContainerView;

    public void Initialize()
    {
        _timer.POST_TICK += PostTick;
        _timer.FIXED_TICK += FixedTick;
        InitLevelFromView();
        Debug.Log("Finish init level from view");
    }
    private void PostTick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _gameView.GetMainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                var gridCotainerView = hit.collider.GetComponentInParent<GridContainerView>();
                if(gridCotainerView != null)
                {
                    _currentGridContainerView = gridCotainerView;
                    _currentGridContainerView.OnMove(true);
                }

            }
        }
        if (Input.GetMouseButton(0))
        {
            if (_currentGridContainerView == null) return;
            Vector2 worldPosition = _gameView.GetMainCam.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(worldPosition);
            _currentGridContainerView.Move(worldPosition);
            foreach (var gridView in _currentGridContainerView.GetGridViews)
            {
                Ray gridRay = new Ray(gridView.transform.position, Vector3.forward);
                RaycastHit hit;
                if (Physics.Raycast(gridRay, out hit, 10f))
                {
                    var placeholderGridView = hit.collider.GetComponentInParent<PlaceholderGridView>();
                    if (placeholderGridView != null)
                    {
                        placeholderGridView.CheckRaycast();
                    }

                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if(placeholderCheckOccupierDict.All(x => x.Value == true))
            {
                foreach (var item in placeholderCheckOccupierDict)
                {
                    item.Key.SetOccupier()
                }
            }
            else
            {
                _currentGridContainerView.OnMove(false);
                _currentGridContainerView.ResetPosition();
            }
       

            _currentGridContainerView = null;
            placeholderCheckOccupierDict.Clear();
        }

    }

    private void FixedTick()
    {
        
    }


    #region LEVEL API
    private void InitLevelFromView()
    {
        var gridCotainerControllersTemp = GetgridCntViews(_gameView.GetGridParentCotainer);
        var placeholderGridCotainerControllersTemp = GetPlaceHolderGridCntViews(_gameView.GetPlaceholderGridParentCotainer);
        if (gridCotainerControllersTemp != null && placeholderGridCotainerControllersTemp != null)
        {
            InitLevelFromView(gridCotainerControllersTemp, placeholderGridCotainerControllersTemp.First());
        }
    }
    public void InitPlaceholderFromView(PlaceholderGirdContainerView placeholderGridCntCntViews)
    {
        placeholderGridCntCntViews.Initialize();
        var placeholderGridCntViews = placeholderGridCntCntViews.GetPlaceholderGridViews;
        List<PlaceholderGridController> placeHolderGridControllers = new List<PlaceholderGridController>();

        foreach (var view in placeholderGridCntViews)
        {
            PlaceholderGridController placeholderGridController = new PlaceholderGridController(view);
            placeHolderGridControllers.Add(placeholderGridController);
        }
        placeholderGridCotainerControllers = new PlaceholderGridContainerController(placeHolderGridControllers, placeholderGridCntCntViews);

        foreach (var placeholderGrid in placeholderGridCotainerControllers.GetPlaceholderGrids)
        {
            placeholderGrid.ON_RAYCAST += CheckRaycast;
        }
    }
    public void InitLevelFromView(List<GridContainerView> gridCntViews, PlaceholderGirdContainerView placeholderGridCntCntViews)
    {
        foreach (GridContainerView gridCntView in gridCntViews)
        {
            gridCntView.Initialize();
            var gridViews = gridCntView.GetGridViews;
            List<GridController> gridControllers = new List<GridController>();
            foreach (var gridView in gridViews)
            {
                GridController gridController = new GridController(gridView);
                gridControllers.Add(gridController);
            }
            GridCotainerController gridCotainerController = new GridCotainerController(gridControllers, gridCntView);
            gridCotainerControllers.Add(gridCotainerController);
            
        }

        InitPlaceholderFromView(placeholderGridCntCntViews);
    }

    public void Dispose()
    {
    }
    #endregion
    #region GRID API
    private Dictionary<PlaceholderGridController, bool> placeholderCheckOccupierDict = new Dictionary<PlaceholderGridController, bool>();
    public void CheckRaycast(PlaceholderGridController placeholderController)
    {
        if (_currentGridContainerView == null) return;

        if (!placeholderCheckOccupierDict.ContainsKey(placeholderController))
        {
            if (placeholderController.isOccupier())
            {
                placeholderCheckOccupierDict.Add(placeholderController, false);
            }
            else
            {
                placeholderCheckOccupierDict.Add(placeholderController, true);

            }

        }
        else
        {
            if (placeholderCheckOccupierDict[placeholderController] == true) return;
            if (placeholderController.isOccupier())
            {
                placeholderCheckOccupierDict[placeholderController] = false;
            }
            else
            {
                placeholderCheckOccupierDict[placeholderController] = true;
            }
        }
        if (placeholderCheckOccupierDict.Any(x => x.Value == false)) return;
        foreach (var item in placeholderCheckOccupierDict)
        {
            item.Key.View.OnCorrectPlace();
        }
    }
    public List<GridContainerView> GetgridCntViews(Transform container)
    {
        List<GridContainerView> gridCotainerControllersTemp = new List<GridContainerView>();
        foreach (Transform child in container)
        {
            GridContainerView girdContainerView = child.GetComponent<GridContainerView>();
            if (girdContainerView != null)
            {
                gridCotainerControllersTemp.Add(girdContainerView);
            }
        }
        return gridCotainerControllersTemp;
    }

    #endregion
    #region PLACEHOLDER API
    public List<PlaceholderGirdContainerView> GetPlaceHolderGridCntViews(Transform container)
    {
        List<PlaceholderGirdContainerView> gridCotainerControllersTemp = new List<PlaceholderGirdContainerView>();
        foreach (Transform child in container)
        {
            PlaceholderGirdContainerView placeholderGirdContainerView = child.GetComponent<PlaceholderGirdContainerView>();
            if (placeholderGirdContainerView != null)
            {
                gridCotainerControllersTemp.Add(placeholderGirdContainerView);
            }
        }
        return gridCotainerControllersTemp;
    }
    #endregion
}
