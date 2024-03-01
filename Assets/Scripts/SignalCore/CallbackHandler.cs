using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DanceGraph;
using DanceGraph.sig;
using UnityEngine;
using UnityEngine.Assertions;

namespace dg
{
    namespace sig
    {
        public static class CallbackHandler
        {
            public enum SignalType : int
            {
                Control = 0,
                Environment,
                Client
            }

            static ISignalHandler _ExtractHandler(GameObject go, int sigIdx)
            {
                return go.GetComponent<SignalContainer>().GetSignalHandler(sigIdx);
            }

            // Callback for all signals
            public static void ProcessSignalData(ReadOnlySpan<byte> data, in SignalMetadata sigMeta)
            {
                var sigType = (SignalType)sigMeta.sigType;
                ISignalHandler handler = null;
                switch (sigType)
                {
                    case SignalType.Control:
                        Debug.Assert(false); // This should be handled in the native plugin
                        break;
                    case SignalType.Environment:
                        var goScene = World.instance.scene;
                        World.instance.envSignalCountMap.Add(sigMeta);
                        handler = _ExtractHandler(goScene, sigMeta.sigIdx);
                        break;
                    case SignalType.Client:
                        var goClient = World.instance.RequestClient(sigMeta.userIdx);
                        handler = _ExtractHandler(goClient, sigMeta.sigIdx);
                        World.instance.userSignalCountMap.Add(sigMeta);
                        break;
                    default:
                        break;
                }
                if (handler != null)
                    handler.HandleSignalData(data, in sigMeta);
                else
                    DefaultSignalHandler(data, in sigMeta);
            }

            // The default signal handler prints the byte sequence for this signal packet
            static void DefaultSignalHandler(ReadOnlySpan<byte> data, in SignalMetadata sigMeta)
            {
                Debug.Log($"Handler not found. Signal data content: packetId={sigMeta.packetId}, sigIdx={sigMeta.sigIdx}, sigType={sigMeta.sigType}, userIdx={sigMeta.userIdx}");
#if false
                const int MAX_DISPLAY_LEN = 10;
                var text = BitConverter.ToString(mem, 0, Math.Min(size, MAX_DISPLAY_LEN));
                string more = size > MAX_DISPLAY_LEN ? "..." : "";
                Debug.Log($"{sigMeta}, size: {size}, contents: {text}{more}");
#endif
            }
        }

        public static class SpanConsumer
        {
            public static T ReadValue<T>(ref ReadOnlySpan<byte> span) where T : unmanaged
            {
                int numBytes;
                unsafe
                {
                    numBytes = sizeof(T);
                }
                Debug.Assert(numBytes <= span.Length, $"Reading span value {typeof(T).FullName}, expecting {numBytes} got {span.Length}");
                var value = MemoryMarshal.Read<T>(span.Slice(0, numBytes));
                span = span.Slice(numBytes);
                return value;
            }
        }
    }
}
