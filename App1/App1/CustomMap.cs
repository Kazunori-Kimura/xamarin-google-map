using System;
using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace App1
{
    /// <summary>
    /// 地図
    /// </summary>
    public class CustomMap : Map
    {
        public List<Position> RouteCoordinates { get; set; }

        public CustomMap()
        {
            RouteCoordinates = new List<Position>();
        }

        public event EventHandler RouteUpdated;

        public virtual void OnRouteUpdated(EventArgs e)
        {
            RouteUpdated?.Invoke(this, e);
        }
    }
}
