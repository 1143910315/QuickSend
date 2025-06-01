using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace QuickSend.Network.Data {
    internal class HelloPacket(string name) : IPacket {
        private readonly string _name = name;

        public string Name => _name;

        public void Encode(Span<byte> buffer) {
            Encoding.UTF8.GetBytes(_name, buffer);
        }

        public int NeededBufferSize() {
           return Encoding.UTF8.GetByteCount(_name);
        }
    }
}
