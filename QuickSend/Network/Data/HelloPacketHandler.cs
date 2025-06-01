using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Network.Data {
    internal class HelloPacketHandler : IPacketHandler {

        public event IPacketHandler.ProcessEventHandler? Process;

        public void Decode(Span<byte> rawData, object? obj) {
            Process?.Invoke(new HelloPacket(Encoding.UTF8.GetString(rawData)), obj);
        }

        public Type PackgeType() {
            return typeof(HelloPacket);
        }
    }
}
