using Core;
using Game.Core.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Grid
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;
        public BoxCollider GetCollider => _boxCollider;
        public void Initialize()
        {
            
        }
    }
}
