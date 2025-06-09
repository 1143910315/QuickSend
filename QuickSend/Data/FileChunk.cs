using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Data {
    internal class FileChunk {
        private byte[]? data = new byte[1024];

        public byte[]? Data {
            get => data;
        }

        public void Finish() {
            data = null;
        }
        public bool IsFinish() {
            return data == null;
        }
    }
}
