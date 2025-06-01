using QuickSend.Data;
using QuickSend.Network.Data;
using QuickSend.Network.Udp;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace QuickSend {
    public partial class MainPage : ContentPage {
        private readonly UdpHighPerfServer udpHighPerfServer = new(7749);
        readonly System.Timers.Timer timer;
        private readonly ObservableCollection<ClientInfo> HostList = [];

        public MainPage() {
            InitializeComponent();
            ClientCollectionView.ItemsSource = HostList;
            PacketHandlerRegistry.Initialize();
            PacketHandlerRegistry.Subscribe<HelloPacket, IPEndPoint>(HelloPacketProcessor);
            DeviceNameEditor.Text = Preferences.Get("DeviceID", $"{DeviceInfo.Current.Name} - {DeviceInfo.Current.Idiom}");
            timer = new() {
                Interval = 3000,
                AutoReset = false
            };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            udpHighPerfServer.OnReceived += UdpHighPerfServer_OnReceived;
            udpHighPerfServer.Start();
        }

        private void HelloPacketProcessor(HelloPacket packet, IPEndPoint? point) {
            if (packet.Name == DeviceNameEditor.Text) {

            } else {
                ClientInfo? clientInfo = HostList.FirstOrDefault(x => x.IpEndPoint == point);
                if (clientInfo == null) {
                    clientInfo = new ClientInfo() { IpEndPoint = point, Name = packet.Name };
                    HostList.Add(clientInfo);
                }
            }
        }

        private void SwitchCell_OnChanged(object? sender, ToggledEventArgs e) {
            if (sender is SwitchCell switchCell) {

            }
        }

        private void UdpHighPerfServer_OnReceived(object? sender, UdpInfo e) {
            PacketHandlerRegistry.Decode(e.Data.AsSpan(0, e.DataLength), e.RemoteEndPoint);
        }

        private async void Timer_Elapsed(object? sender, ElapsedEventArgs e) {
            await udpHighPerfServer.SendAsync(PacketHandlerRegistry.Encode(new HelloPacket(DeviceNameEditor.Text)), new IPEndPoint(IPAddress.Broadcast, 7749)).ConfigureAwait(false);
        }

        private async void QuickSendButton_Clicked(object sender, EventArgs e) {
            FileResult? fileResult = await FilePicker.PickAsync().ConfigureAwait(false);
            if (fileResult != null) {
                using var stream = await fileResult.OpenReadAsync().ConfigureAwait(false);
                byte[] data = new byte[1024];
                int dataLength = await stream.ReadAsync(data).ConfigureAwait(false);
            }
        }

        private void DeviceNameEditor_TextChanged(object sender, TextChangedEventArgs e) {
            if (DeviceNameEditor.Text == "") {
                Preferences.Remove("DeviceID");
            } else {
                Preferences.Set("DeviceID", DeviceNameEditor.Text);
            }
        }
    }
}
