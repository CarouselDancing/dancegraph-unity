using System;
using System.Runtime.InteropServices;

namespace net
{
    // Below should be 16 bytes
    [StructLayout(LayoutKind.Sequential, Pack=0)]
    public struct SignalMetadata
    {
        public ulong acquisitionTime;
        public uint packetId;
        // index of client or listener
        public short userIdx;
        // index of signal in scene : up to 256 signals per scene
        public byte sigIdx;
        // type of signal: control, client or environment
        public byte sigType;
    }

    public static class SignalUtil
    {
        public static T OnPacket<T>(byte[] packet)
        {
            // TODO: preallocate pinned packet memory for each type
            // TODO: creating/returning msg on the fly could be expensive for big messages
            // TODO: maybe use Span<byte>
            GCHandle pinnedPacket = GCHandle.Alloc(packet, GCHandleType.Pinned);
            T msg = (T)Marshal.PtrToStructure(
                pinnedPacket.AddrOfPinnedObject(),
                typeof(T));        
            pinnedPacket.Free();
            return msg;
        }
    }
}