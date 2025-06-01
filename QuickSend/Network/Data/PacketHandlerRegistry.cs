using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuickSend.Network.Data {
    public static class PacketHandlerRegistry {
        private static readonly List<IPacketHandler> _handlers = [];

        // 初始化注册
        public static void Initialize() {
            // 扫描所有实现IPacketHandler的类
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                          typeof(IPacketHandler).IsAssignableFrom(t));
            foreach (var type in handlerTypes) {
                if (Activator.CreateInstance(type) is IPacketHandler handler) {
                    _handlers.Add(handler);
                }
            }
        }

        // 解码数据包
        public static void Decode(Span<byte> data, object? obj) {
            if (data.Length == 0) {
                return;
            }

            byte packetType = data[0];
            int packetLength = BitConverter.ToInt32(data[1..]);
            const int headerLength = 1 + sizeof(int);
            if (packetType < _handlers.Count && packetLength >= 0 && packetLength == data.Length - headerLength) {
                _handlers[packetType].Decode(data[headerLength..], obj);
            }
        }

        // 编码数据包
        public static byte[] Encode(IPacket packet) {
            int neededSize = packet.NeededBufferSize();
            const int headerLength = 1 + sizeof(int);
            byte[] result = new byte[headerLength + neededSize]; // 预分配缓冲区
            if (BitConverter.TryWriteBytes(result.AsSpan(1, sizeof(int)), neededSize)) {
                packet.Encode(result.AsSpan(headerLength));
                int index = _handlers.Index().First(h => packet.GetType() == h.Item.PackgeType()).Index;
                result[0] = (byte)index;
                return result;
            } else {
                throw new InvalidOperationException("Failed to write packet length.");
            }
        }

        internal static IPacketHandler GetHandlerForType(Type packetType) {
            return _handlers.First(h => h.PackgeType() == packetType);
        }

        // 订阅特定包类型事件
        public static void Subscribe<T, U>(Action<T, U?> handler) where T : IPacket where U : class {
            var packetType = typeof(T);
            // 获取对应的处理器并订阅
            IPacketHandler packetHandler = GetHandlerForType(packetType);
            packetHandler.Process += (packet, obj) => {
                if (packet is T t) {
                    handler(t, obj as U);
                }
            };
        }
    }
}
