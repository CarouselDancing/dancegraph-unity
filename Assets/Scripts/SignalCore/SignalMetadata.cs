using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace dg
{
    namespace sig
    {
        // This struct should be 16 bytes, aligned at 8 bytes (bc long)
        [StructLayout(LayoutKind.Sequential)]
        public struct SignalMetadata
        {
            public long acquisitionTime;
            public uint packetId;
            public short userIdx;
            public byte sigIdx;
            public byte sigType;

            public override string ToString()
            {
                return $"id:{packetId}, user:{userIdx}, sig:{sigType}-{sigIdx}, acquisitionTime:{acquisitionTime}";
            }
        }
    }
}