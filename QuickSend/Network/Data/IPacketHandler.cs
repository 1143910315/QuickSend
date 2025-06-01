using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace QuickSend.Network.Data {
    internal interface IPacketHandler {
        public delegate void ProcessEventHandler(IPacket packet, object? obj);
        Type PackgeType(); // 包类型
        void Decode(Span<byte> rawData, object? obj); // 解码方法
        event ProcessEventHandler? Process;
    }
}
