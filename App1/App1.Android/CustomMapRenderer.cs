using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using App1;
using App1.Droid;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace App1.Droid
{
    class CustomMapRenderer : MapRenderer
    {
        List<Position> routeCoordinates;

        public CustomMapRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                // unsubscribe
            }

            if (e.NewElement != null)
            {
                var map = (CustomMap)e.NewElement;
                routeCoordinates = map.RouteCoordinates;
                Control.GetMapAsync(this);

                map.RouteUpdated += CustomMap_RouteUpdated;
            }
        }

        private void CustomMap_RouteUpdated(object sender, EventArgs e)
        {
            // ルート描画処理
            this.RenderPolylines();
        }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

            this.RenderPolylines();
        }

        private void RenderPolylines()
        {
            System.Diagnostics.Debug.WriteLine("Render Polylines");

            // 色指定
            var polylineOptions = new PolylineOptions();
            polylineOptions.InvokeColor(0x66FF0000);

            foreach (var pos in routeCoordinates)
            {
                polylineOptions.Add(new LatLng(pos.Latitude, pos.Longitude));
            }

            // 描画
            NativeMap.AddPolyline(polylineOptions);
        }
    }
}