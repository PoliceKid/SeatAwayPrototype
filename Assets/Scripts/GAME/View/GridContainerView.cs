using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Game.Level.Grid
{
    public class GridContainerView : MonoBehaviour
    {
        [SerializeField] private SortingGroup _sortingGroup;
        private List<GridView> _gridViews;
        public List<GridView> GetGridViews => _gridViews;
        private Vector3 _initPosition;
        public void Initialize()
        {
            _gridViews = new List<GridView>();
            foreach (Transform child in transform)
            {
                GridView gridView = child.GetComponent<GridView>();
                if (gridView != null)
                {
                    _gridViews.Add(gridView);
                }
            }
            _initPosition = transform.localPosition;
        }
        public void Move(Vector3 targetPoint)
        {
            transform.localPosition = targetPoint;
        }
        public void OnMove(bool onMove)
        {
            _sortingGroup.sortingOrder = onMove?10:1;          
        }
        public void ResetPosition()
        {
            Move(_initPosition);
        }

    }
}
