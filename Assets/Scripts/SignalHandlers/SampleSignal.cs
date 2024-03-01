using System;
using System.Collections.Generic;
using System.Timers;
using dg.sig;
using UnityEngine;

namespace DanceGraph
{
    namespace SignalSerialization
    {
        public class SampleSignal : MonoBehaviour, sig.ISignalHandler
        {
            public int counter;

            public void HandleSignalData(ReadOnlySpan<byte> data, in SignalMetadata sigMeta)
            {
                counter = SpanConsumer.ReadValue<int>(ref data);
            }
        }
    }
}