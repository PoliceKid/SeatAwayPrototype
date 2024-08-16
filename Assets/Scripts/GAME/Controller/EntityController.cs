using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Injection;
using Core;

namespace Game.Core.Entity
{
    public class EntityModel : Observable
    {
        public float x;
        public float y;
        public EntityModel(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString()
        {
            return $"pos X : {x},pos X : {y} ";
        }
    }
    public class EntityController : IDisposable
    {
        protected Injector _subInjector;
        protected Context _subContext;
        protected EntityView _entityView;

        public EntityController(EntityView entityView, Context context)
        {
            _subContext = new Context(context);
            _subInjector = new Injector(_subContext);
            _subContext.Install(this);
            _subContext.Install(_subInjector);
        }

        public Injector SubInjector => _subInjector;

        public void Dispose()
        {

        }
    }
}

