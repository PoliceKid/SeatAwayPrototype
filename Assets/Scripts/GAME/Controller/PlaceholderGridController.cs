using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Core.Entity;
using Injection;

namespace Game.Level.Grid
{
    public class PlaceholderGridModel: EntityModel
    {
        public IOccupier Occupier;
        public PlaceholderGridModel(float x, float y) : base(x, y)
        {

        }

    }
    public class PlaceholderGridController : IDisposable
    {
        protected PlaceholderGridModel _model;
        protected PlaceholderGridView _view;
        public PlaceholderGridView View => _view;
        public System.Action<IOccupier,PlaceholderGridController> ON_RAYCAST = delegate { };
        public PlaceholderGridController( PlaceholderGridView view)
        {
            _model = new PlaceholderGridModel(view.transform.localPosition.x, view.transform.localPosition.y);
            _view = view;
            _view.ON_RAYCAST += OnRaycast;
        }

        private void OnRaycast(IOccupier occupier, PlaceholderGridView view)
        {
            ON_RAYCAST?.Invoke(occupier,this);
        }

        public bool isOccupier() => _model.Occupier != null;
        public void SetOccupier(IOccupier occupier)
        {
            _model.Occupier = occupier;
        }
        public void ResetOccupier()
        {
            _model.Occupier = null;
        }
        public void Dispose()
        {
            
        }
    }
}
