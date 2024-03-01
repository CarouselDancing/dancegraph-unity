using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Timers;
using dg.sig;
using UnityEngine;
using DanceGraph.SignalBehaviours;

namespace DanceGraph
{
    namespace SignalSerialization
    {
        public class EnvSignal : MonoBehaviour, sig.ISignalHandler
        {
            public static void _SendStruct<T>(T s) where T: struct {
                int size = System.Runtime.InteropServices.Marshal.SizeOf(s);
                byte []arr = new byte[size];
                // We should be able to do this without the alloc and copy
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(s, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);
                const int sigIdx = 0; // always a single one, e.g. env/v1.0
                DanceGraphMinimalCpp.SendEnvSignal(arr, size, sigIdx);
            }
            
            public void HandleSignalData(ReadOnlySpan<byte> data, in SignalMetadata sigMeta)
            {

                var envSigId = MemoryMarshal.Cast<byte, SignalGeneric>(data)[0].signalID;

                Debug.Log($"Received Env Signal from user {sigMeta.userIdx}, type {envSigId}");         
                switch (envSigId)
                {
                    
                    case SignalID.EnvMusicStateID:
                        var newMusicState = Global.ReadStruct<EnvMusicState>(data.ToArray()); 
                        _OnNewMusicState(newMusicState);
                        break;
                    case SignalID.EnvUserStateID:
                        var newUserState = Global.ReadStruct<EnvUserState>(data.ToArray());
                        
                        _OnNewUserState(newUserState);
                        break;
                    default:
                        Debug.LogWarning($"Unhandled env subsig: {envSigId}");
                        break;
                }
            }
            
            void _OnNewMusicState( in EnvMusicState newState)
            {
                var m = World.instance.musicPlayer.GetComponent<MusicController>();
                
                musicState._isPlaying = newState.isPlaying;
                musicState._musicTime = newState.musicTime;
                musicState._trackName = newState.trackName;

                m.OnMusicEnvironmentUpdate(musicState);
                // m.OnSetPlay(newState._isPlaying, newState._musicTime, false);

            }
            
            void _OnNewUserState( in EnvUserState newState)
            {
                if (newState._userID < 0 || newState._userID > 1000)
                {
                    Debug.LogWarning("_OnNewUserState: Invalid UserID " + newState._userID);
                    Debug.LogWarning("Invalid user has name " + newState._userName);
                    return;
                }
                Debug.Log("Called _OnNewUserState");

                if (userStates == null)
                    userStates = new EnvUserState[]{};

                
                if (newState._userID >= userStates.Length) {
                    Array.Resize(ref userStates, newState._userID + 1);
                }
                userStates[newState._userID] = newState;
                userStates[newState._userID]._userName = newState._userName;
                userStates[newState._userID]._userID = newState._userID;

                MatchStateWithClient(newState._userID);

                if (newState._userID < World.instance.clients.Count) {
                    var go = World.instance.clients[newState._userID];
                    var newIsActive = newState._isActive[0] > 0;
                    if(go.activeSelf != newIsActive)
                        go.SetActive(newIsActive);
                }
                if (newState._isActive[0] == 0) {
                    Debug.Log($"Destroying avatar {newState._userID}");
                    World.instance.DestroyAvatar(newState._userID);
                }
                Debug.Log($"Getting Env User State for user {newState._userID}, name {newState._userName}");
            }

            public void EnvMusicState_SetIsPlaying(bool value)
            {
                musicState._isPlaying = value;
                musicState.signalID = SignalID.EnvMusicStateID;
                _SendStruct(musicState);         
            }

            public void EnvMusicState_SetTrack(String name, double musicTime = 0.0)
            {
                musicState._trackName = name;
                musicState._musicTime = musicTime;
                musicState.signalID = SignalID.EnvMusicStateID;
                _SendStruct(musicState);
            }


            
            public void EnvUserState_SetName(int idx, string name)
            {
                var user = userStates[idx]; 
                user._avatarDesc = "ybot avatar";
                userStates[idx] = user;
                user._userName = name;

                _SendStruct(user);
            }

            public void EnvUserState_SendUserReq(int idx)
            {
                userDataRequest.userID = idx;
                //_SendStruct(userDataRequest);
            }

            public void MatchStateWithClient(int userIdx) {
                // Called when a client is created or a userstate env signal comes in
                
                // The client data hasn't shown up yet
                if (userIdx >= World.instance.clients.Count) {
                    Debug.Log($"Can't match state with uninstanced client {userIdx}");
                    return;
                }
                
                // The env state data hasn't shown up yet
                if (userIdx >= userStates.Length) {
                    Debug.Log($"Can't match client {userIdx} with uncreated env state");                    
                    return;
                }
                
                World.instance.clients[userIdx].name = userStates[userIdx]._userName;
                var zs = World.instance.clients[userIdx].GetComponent<ZedSignal>();
                zs.skeletonHandler.ChangeNameTag(userStates[userIdx]._userName);
                Debug.Log($"Matching avatar {userIdx} state and client, name {userStates[userIdx]._userName}");                
            }
            
            public EnvMusicState musicState = new EnvMusicState();
            public EnvUserState [] userStates = new EnvUserState[0]{};
            public EnvUserDataRequest userDataRequest = new EnvUserDataRequest();
            
        }


        
    }

    
    
}
