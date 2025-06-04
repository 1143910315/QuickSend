using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Network.Data {
    internal class ConfirmPacketHandler : IPacketHandler {

        public event IPacketHandler.ProcessEventHandler? Process;

        public void Decode(Span<byte> rawData, object? obj) {
            Process?.Invoke(new ConfirmPacket(BitConverter.ToInt32(rawData), BitConverter.ToBoolean(rawData[sizeof(int)..])), obj);
        }

        public Type PackgeType() {
            return typeof(ConfirmPacket);
        }
    }
}
