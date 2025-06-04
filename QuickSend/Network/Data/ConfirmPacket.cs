using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace QuickSend.Network.Data {
    internal class ConfirmPacket(int id, bool isConfirmed) : IPacket {
        private readonly int _id = id;
        private readonly bool _isConfirmed = isConfirmed;

        public int Id => _id;
        public bool IsConfirmed => _isConfirmed;

        public void Encode(Span<byte> buffer) {
            if (BitConverter.TryWriteBytes(buffer, _id)) {
                if (BitConverter.TryWriteBytes(buffer[sizeof(int)..], _isConfirmed)) {
                    return;
                }
            }
            throw new ArgumentException("Could not write to buffer");
        }

        public int NeededBufferSize() {
            return sizeof(int) + sizeof(bool);
        }
    }
}
