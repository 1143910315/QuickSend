using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Network.Data {
    public interface IPacket {
        int NeededBufferSize(); // 需要的缓冲区大小
        void Encode(Span<byte> buffer); // 编码方法
    }
}
