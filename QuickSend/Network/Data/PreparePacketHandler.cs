using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Network.Data {
    internal class PreparePacketHandler : IPacketHandler {

        public event IPacketHandler.ProcessEventHandler? Process;

        public void Decode(Span<byte> rawData, object? obj) {
            Process?.Invoke(new PreparePacket(BitConverter.ToInt64(rawData), Encoding.UTF8.GetString(rawData[sizeof(long)..])), obj);
        }

        public Type PackgeType() {
            return typeof(PreparePacket);
        }
    }
}
