using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Data {
    internal class ClientInfo(IPEndPoint ipEndPoint) {
        private bool trusted = false;
        private string name = "未命名设备";
        private readonly ConcurrentDictionary<int, Action<bool>> confirmActions = new();
        private int _nextId = 0; // ID 计数器

        public IPEndPoint IpEndPoint {
            get => ipEndPoint;
        }
        public bool Trusted {
            get => trusted;
            set => trusted = value;
        }
        public string Name {
            get => name;
            set => name = value;
        }
        public int AddConfirmAction(Action<bool> action) {
            while (confirmActions.TryAdd(++_nextId, action)) {
            }
            return _nextId;
        }
        public void RemoveConfirmAction(int id, bool confirmed) {
            if (confirmActions.TryRemove(id, out Action<bool>? action)) {
                action.Invoke(confirmed);
            }
        }
    }
}
