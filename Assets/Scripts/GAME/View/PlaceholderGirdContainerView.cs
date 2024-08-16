using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Grid
{
    public class PlaceholderGirdContainerView : MonoBehaviour
    {
        private List<PlaceholderGridView> _placeholderGridViews;
        public List<PlaceholderGridView> GetPlaceholderGridViews => _placeholderGridViews;
        public void Initialize()
        {
            _placeholderGridViews = new List<PlaceholderGridView>();
            foreach (Transform child in transform)
            {
                PlaceholderGridView placeholderGridView = child.GetComponent<PlaceholderGridView>();
                if(placeholderGridView != null)
                {
                    _placeholderGridViews.Add(placeholderGridView);
                }
            }
        }
    }
}
