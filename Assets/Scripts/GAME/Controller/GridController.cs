using Core;
using Game.Core.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Level.Grid
{
    public class GridController : IDisposable, IOccupier
    {
        protected EntityModel _model;
        protected GridView _view;

        public GridController(GridView view)
        {
            _model = new EntityModel(view.transform.localPosition.x, view.transform.localPosition.y);
             _view = view;
        }

        public void Dispose()
        {

        }
    }
   
}
