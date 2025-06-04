using QuickSend.Data;
using QuickSend.Network.Data;
using QuickSend.Network.Udp;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
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
            PacketHandlerRegistry.Subscribe<ConfirmPacket, IPEndPoint>(ConfirmPacketProcessor);
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

        private void ConfirmPacketProcessor(ConfirmPacket packet, IPEndPoint? point) {
            ClientInfo? clientInfo;
            lock (HostList) {
                clientInfo = HostList.FirstOrDefault(x => x.IpEndPoint == point);
            }
            clientInfo?.RemoveConfirmAction(packet.Id, packet.IsConfirmed);
        }

        private void HelloPacketProcessor(HelloPacket packet, IPEndPoint? point) {
            if (point != null) {
                if (packet.Name == DeviceNameEditor.Text) {
                } else {
                    lock (HostList) {
                        ClientInfo? clientInfo = HostList.FirstOrDefault(x => x.IpEndPoint == point);
                        if (clientInfo == null) {
                            clientInfo = new ClientInfo(point) { Name = packet.Name };
                            HostList.Add(clientInfo);
                        }
                    }
                }
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
                IEnumerable<Tuple<ClientInfo, CancellationTokenSource, int>> sendClientInfo;
                lock (HostList) {
                    sendClientInfo = HostList
                    .AsParallel()
                    .Where(x => x.Trusted)
                    .Select(x => {
                        CancellationTokenSource cancellationTokenSource = new();
                        return Tuple.Create(x, cancellationTokenSource, x.AddConfirmAction(isConfirmed => { cancellationTokenSource.Cancel(); }));
                    })
                    .AsEnumerable();
                }
                using Stream stream = await fileResult.OpenReadAsync().ConfigureAwait(false);
                long fileLength = stream.Length;
                using IncrementalHash incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                int bytesRead;
                byte[] buffer = new byte[1024];
                do {
                    bytesRead = await stream.ReadAsync(buffer).ConfigureAwait(false);
                    incrementalHash.AppendData(buffer, 0, bytesRead);
                } while (bytesRead != 0);
                byte[] hash = incrementalHash.GetCurrentHash();
                await Parallel.ForEachAsync(sendClientInfo, async (tuple, token) => {
                    try {
                        while (!tuple.Item2.IsCancellationRequested) {
                            await udpHighPerfServer.SendAsync(PacketHandlerRegistry.Encode(new PreparePacket(fileLength, fileResult.FileName)), tuple.Item1.IpEndPoint).ConfigureAwait(false);
                            await Task.Delay(5000, token).ConfigureAwait(false);
                        }
                    } finally {
                    }
                });
                await Task.Run(() => { }).ConfigureAwait(false);
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
