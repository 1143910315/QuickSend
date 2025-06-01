using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Network.Udp {
    internal class UdpInfo(byte[] data, int dataLength, EndPoint remoteEndPoint) {
        private readonly byte[] data = data;
        private readonly int dataLength = dataLength;
        private readonly EndPoint remoteEndPoint = remoteEndPoint;

        public byte[] Data => data;

        public int DataLength => dataLength;

        public EndPoint RemoteEndPoint => remoteEndPoint;
    }
}
