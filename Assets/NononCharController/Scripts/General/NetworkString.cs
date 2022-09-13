using Unity.Netcode;
using Unity.Collections;

namespace zone.nonon
{

    public struct NetworkString : INetworkSerializable
    {
        
        private ForceNetworkSerializeByMemcpy<FixedString64Bytes> info;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref info);
        }

        public override string ToString()
        {
            return info.Value.ToString();
        }

        public static implicit operator string(NetworkString s) => s.ToString();
        public static implicit operator NetworkString(string s) => new NetworkString() { info = new FixedString64Bytes(s) };
    }
}
