using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace QuickSend.Network.Udp {
    internal class UdpHighPerfServer {
        private readonly Socket _socket;
        private readonly ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
        private bool _isRunning = false;
        private readonly Channel<UdpInfo> channel = Channel.CreateUnbounded<UdpInfo>();
        private readonly int port;

        public int Port => port;

        public event EventHandler<UdpInfo>? OnReceived;
        public UdpHighPerfServer(int port) {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bool success = false;
            while (!success) {
                try {
                    _socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    success = true;
                } catch (SocketException) {
                    // 端口已占用，尝试下一个
                    port++;
                }
            }
            _socket.DontFragment = true;
            _socket.EnableBroadcast = true;
            this.port = port;
        }

        public void Start() {
            _isRunning = true;
            Task.Run(ReceiveLoop);
            Task.Run(ProcessLoop);
        }

        private async Task ReceiveLoop() {
            while (_socket.Poll(-1, SelectMode.SelectRead) && _socket.Available > 0) {
                // 从内存池租用缓冲区
                byte[] buffer = arrayPool.Rent(_socket.Available);
                // 零拷贝接收
                var result = await _socket.ReceiveFromAsync(
                    buffer,
                    SocketFlags.None,
                    new IPEndPoint(IPAddress.Any, 0)
                ).ConfigureAwait(false);

                // 直接处理数据（示例：原样回传）
                await channel.Writer.WriteAsync(new UdpInfo(buffer, result.ReceivedBytes, result.RemoteEndPoint)).ConfigureAwait(false);
            }
            Debug.WriteLine("ReceiveLoop stopped");
        }

        // 处理数据包（零拷贝处理）
        private async Task ProcessLoop() {
            while (_isRunning) {
                UdpInfo udpInfo = await channel.Reader.ReadAsync().ConfigureAwait(false);
                OnReceived?.Invoke(this, udpInfo);
                arrayPool.Return(udpInfo.Data);
            }
        }

        // 异步发送（使用原始内存块）
        public ValueTask<int> SendAsync(Memory<byte> data, EndPoint remoteEP) {
            return _socket.SendToAsync(data, SocketFlags.None, remoteEP);
        }

        public void Dispose() {
            _isRunning = false;
            _socket?.Dispose();
        }
    }
}
