using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Core;
using Game.Core.Entity;

namespace Game.Level.Grid
{
    public class GridCotainerModel : EntityModel
    {
        public GridCotainerModel(float x, float y) :base(x, y)
        {

        }
    }
    public class GridCotainerController : IDisposable
    {
        protected List<GridController> _grids;
        protected GridContainerView _view;
        protected GridCotainerModel _model;

        public GridCotainerController(List<GridController> grids, GridContainerView view)
        {
            _grids = grids;
            _view = view;
            _model = new GridCotainerModel(_view.transform.localPosition.x, _view.transform.localPosition.y);
        }

        public void Dispose()
        {

        }
    }

    
}
