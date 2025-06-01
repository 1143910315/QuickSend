using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Data {
    internal class ClientInfo {
        private IPEndPoint? _ipEndPoint;
        private bool trusted = false;
        private string name = "未命名设备";

        public IPEndPoint? IpEndPoint {
            get => _ipEndPoint;
            set => _ipEndPoint = value;
        }
        public bool Trusted {
            get => trusted;
            set => trusted = value;
        }
        public string Name {
            get => name;
            set => name = value;
        }
    }
}
