using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace QuickSend.Network.Data {
    internal class PreparePacket(int confirmId, long fileSize, string fileName) : IPacket {
        public int ConfirmId => confirmId;
        public long FileSize => fileSize;
        public string FileName => fileName;

        public void Encode(Span<byte> buffer) {
            if (BitConverter.TryWriteBytes(buffer, confirmId)) {
                if (BitConverter.TryWriteBytes(buffer[sizeof(int)..], fileSize)) {
                    Encoding.UTF8.GetBytes(fileName, buffer[(sizeof(int) + sizeof(long))..]);
                    return;
                }
            }
            throw new ArgumentException("Could not write to buffer");
        }

        public int NeededBufferSize() {
            return sizeof(int) + sizeof(long) + Encoding.UTF8.GetByteCount(fileName);
        }
    }
}
