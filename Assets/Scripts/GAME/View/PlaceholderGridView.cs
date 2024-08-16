using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Grid
{
    public class PlaceholderGridView : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        public BoxCollider GetBoxCollider => _boxCollider;
        private Color _initColor;
        public System.Action<IOccupier,PlaceholderGridView> ON_RAYCAST = delegate { };
        public void Initialize()
        {
            _initColor = _spriteRenderer.color;
        }
        public void OnCorrectPlace()
        {
            _spriteRenderer.color = Color.green;
        }
        public void ResetColor()
        {
            _spriteRenderer.color = _initColor;
        }
        public void CheckRaycast(IOccupier occupier)
        {
            ON_RAYCAST?.Invoke(occupier,this);
        }
    }
}
