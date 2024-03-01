using System;
using System.Collections.Generic;
using dg.sig;
using UnityEngine;

namespace DanceGraph
{
    namespace sig
    {
        public interface ISignalHandler
        {
            void HandleSignalData(ReadOnlySpan<byte> data, in SignalMetadata sigMeta);
        }
        
        public class SignalContainer : MonoBehaviour
        {
            private List<Component> signalIndexToComponent = new List<Component>();
            
            public void OnSignalAdded(Component signalComponent, int signalIdx)
            {
                Debug.Assert(signalIdx >= 0);
                while(signalIndexToComponent.Count <= signalIdx)
                    signalIndexToComponent.Add(null);
                signalIndexToComponent[signalIdx] = signalComponent;
            }

            public ISignalHandler GetSignalHandler(int signalIdx)
            {
                Debug.Assert(signalIdx >= 0 && signalIdx < signalIndexToComponent.Count);
                return (ISignalHandler)signalIndexToComponent[signalIdx];
            }
        }
    }
}