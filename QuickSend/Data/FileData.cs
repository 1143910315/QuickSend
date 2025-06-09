using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Data {
    internal class FileData(string fileName, long fileSize) {
        private readonly ConcurrentDictionary<int, FileChunk> data = new();

        public ConcurrentDictionary<int, FileChunk> Data => data;

        public string FileName => fileName;
        public long FileSize => fileSize;
    }
}
