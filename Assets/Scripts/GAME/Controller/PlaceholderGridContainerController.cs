using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Core.Entity;

namespace Game.Level.Grid
{
    public class PlaceholderGridContainerModel : EntityModel
    {
        public PlaceholderGridContainerModel(float x, float y) : base(x, y)
        {

        }
    }
    public class PlaceholderGridContainerController : IDisposable
    {
        protected List<PlaceholderGridController> _placeholderGrids;
        protected PlaceholderGridContainerModel _model;
        protected PlaceholderGirdContainerView _view;
        public List<PlaceholderGridController> GetPlaceholderGrids => _placeholderGrids;
        public PlaceholderGridContainerController(List<PlaceholderGridController> placeholderGrids, PlaceholderGirdContainerView view)
        {
            _placeholderGrids = placeholderGrids;
            _view = view;
            _model = new PlaceholderGridContainerModel(_view.transform.localPosition.x, _view.transform.localPosition.y);

        }

        public void Initialize()
        {
            
        }
        public void Dispose()
        {
            
        }
    }
}
