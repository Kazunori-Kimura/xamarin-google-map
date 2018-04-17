using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
        /// マップの表示距離
        /// </summary>
        private readonly Distance MapDistance = Distance.FromMiles(0.3);

        public MainPage()
		{
			InitializeComponent();

            // 東京駅を表示
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(MapInitialPosition, MapDistance));

            StartLatitude.Text = "35.681298";
            StartLongitude.Text = "139.766247";

            // Geolocatorのパーミッションチェック
            ChackLocationPermissionStatusAsync(MyMap);
		}

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
