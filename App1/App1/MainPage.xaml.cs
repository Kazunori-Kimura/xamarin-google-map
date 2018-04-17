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

            // TODO Geolocatorのパーミッションチェック
		}
	}
}
