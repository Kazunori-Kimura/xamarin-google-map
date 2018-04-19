using Newtonsoft.Json.Linq;
using Plugin.Geolocator;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace App1
{
    public partial class MainPage : ContentPage
	{
        /// <summary>
        /// マップの初期位置（東京駅）
        /// </summary>
        private readonly Position MapInitialPosition = new Position(35.681298, 139.766247);

        /// <summary>
        /// 東京タワーの位置
        /// </summary>
        private readonly Position TokyoTowerPosition = new Position(35.658580, 139.743244);

        /// <summary>
        /// マップの表示距離
        /// </summary>
        private readonly Distance MapDistance = Distance.FromMiles(0.3);

        /// <summary>
        /// Google Maps Directions API Key
        /// </summary>
        private readonly string DirectionsApiKey = "===YOUR_API_KEY===";

        /// <summary>
        /// Google Maps Directions API URL base
        /// </summary>
        private readonly string DirectionsApiUrl = @"https://maps.googleapis.com/maps/api/directions/json?"; //origin=%FROM%&destination=%DEST%&key=%YOUR_API_KEY%

        public MainPage()
		{
			InitializeComponent();

            // 東京駅を中央に表示
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(MapInitialPosition, MapDistance));

            // 初期値の設定
            StartLatitude.Text = MapInitialPosition.Latitude.ToString();
            StartLongitude.Text = MapInitialPosition.Longitude.ToString();
            GoalLatitude.Text = TokyoTowerPosition.Latitude.ToString();
            GoalLongitude.Text = TokyoTowerPosition.Longitude.ToString();

            // -- ボタンクリック時の処理 --
            // 現在地
            CurrentLocation.Clicked += async (sender, e) =>
            {
                var location = await this.GetCurrentLocation();
                Position pos = MapInitialPosition;
                if (location.HasValue)
                {
                    pos = location.Value;
                }

                MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(pos, MapDistance));
                StartLatitude.Text = pos.Latitude.ToString();
                StartLongitude.Text = pos.Longitude.ToString();
            };
            // 東京タワー
            TokyoTowerLocation.Clicked += (sender, e) =>
            {
                GoalLatitude.Text = TokyoTowerPosition.Latitude.ToString();
                GoalLongitude.Text = TokyoTowerPosition.Longitude.ToString();
            };
            // ルート検索
            SearchRoute.Clicked += async (sender, e) =>
            {
                Debug.WriteLine("here");
                var positions = await this.GetDirectionsResult(MapInitialPosition, TokyoTowerPosition);
                MyMap.RouteCoordinates.Clear();
                foreach (var position in positions)
                {
                    MyMap.RouteCoordinates.Add(position);
                }

                // 描画
                MyMap.OnRouteUpdated(e);
            };

            // Geolocatorのパーミッションチェック
            ChackLocationPermissionStatusAsync(MyMap).Wait();
		}

        /// <summary>
        /// 現在地を取得
        /// </summary>
        /// <returns></returns>
        private async Task<Position?> GetCurrentLocation()
        {
            try
            {
                // 10秒でタイムアウト
                var location = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(10));
                if (location != null)
                {
                    return new Position(location.Latitude, location.Longitude);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// Google Maps Directions API request
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        private async Task<List<Position>> GetDirectionsResult(Position start, Position goal)
        {
            List<Position> list = new List<Position>();

            // APIリクエスト
            var client = new WebClient();
            string address = DirectionsApiUrl;
            address += $"origin={this.FormatPosition(start)}&destination={this.FormatPosition(goal)}&mode=walking&key={DirectionsApiKey}";
            Debug.WriteLine($"address: {address}");
            string json = await client.DownloadStringTaskAsync(address);
            Debug.WriteLine($"json: {json}");

            // JSONをparse
            var steps = this.GetSteps(json);

            foreach (JObject step in steps)
            {
                // start_location
                JObject startLocation = (JObject)step["start_location"];
                list.Add(this.ParsePosition(startLocation));
                // end_location
                JObject endLocation = (JObject)step["end_location"];
                list.Add(this.ParsePosition(endLocation));
            }

            return list;
        }

        /// <summary>
        /// JSONをパースしてstepを取得
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private JArray GetSteps(string json)
        {
            var data = JObject.Parse(json);
            var routes = (JArray)data["routes"];

            JArray ret = new JArray();

            foreach (JObject route in routes)
            {
                JArray legs = (JArray)route["legs"];
                foreach(JObject leg in legs)
                {
                    JArray steps = (JArray)leg["steps"];
                    foreach(JObject step in steps)
                    {
                        ret.Add(step);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// JSON Object -> Position 変換
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Position ParsePosition(JObject location)
        {
            double lat = (double)location["lat"];
            double lng = (double)location["lng"];

            Debug.WriteLine($"position=({lat},{lng})");

            return new Position(lat, lng);
        }

        /// <summary>
        /// positionを文字列に変換
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private string FormatPosition(Position pos)
        {
            return $"{pos.Latitude},{pos.Longitude}";
        }

        /// <summary>
        /// GeoLocatorのパーミッションチェック
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        private async Task ChackLocationPermissionStatusAsync(Map map)
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Location);
            if (status != PermissionStatus.Granted)
            {
                // 許可されていなければパーミッションをリクエスト
                var permissions = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                status = permissions[Permission.Location];
            }

            if (status == PermissionStatus.Granted)
            {
                map.IsShowingUser = true;
            }
        }
	}
}
