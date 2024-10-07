using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Timers;
using dg.sig;
using UnityEngine;
using DanceGraph.SignalBehaviours;

using Newtonsoft.Json.Linq;

namespace DanceGraph
{
    public enum ClientType {
        DEFAULT_CLIENT = 0,
        HUMAN_USER = 1,
        DEMO_BOT = 2
    };
    
    
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
                    case SignalID.EnvMusicRequestID:
                        Debug.Log("Responding to server music request");
                        musicState.SendStruct();
                        break;
                    default:
                        Debug.LogWarning($"Unhandled env subsig: {envSigId}");
                        break;
                }
            }
            
            void _OnNewMusicState( in EnvMusicState newState, bool nocontrol = false)
            {
                var m = World.instance.musicPlayer.GetComponent<MusicController>();
                
                musicState._isPlaying = newState.isPlaying;
                musicState._musicTime = newState.musicTime;
                musicState._trackName = newState.trackName;
				if (!nocontrol) {
					m.OnMusicEnvironmentUpdate(musicState);
				}
            }
            
            void _OnNewUserState( in EnvUserState newState)
            {
                if (newState._userID < 0 || newState._userID > 32000)
                {
                    Debug.LogWarning("_OnNewUserState: Invalid UserID " + newState._userID);
                    Debug.LogWarning("Invalid user has name " + newState._userName);
                    return;
                }
                Debug.Log($"({newState._userID}): New UserName: {newState._userName}");
                Debug.Log($"({newState._userID}): New UserAvatar Type: {newState._avatarType}");
                Debug.Log($"({newState._userID}): New UserAvatar Params: {newState._avatarParams}");
                Debug.Log($"({newState._userID}): New IsActive: {newState._isActive}");
                Debug.Log($"({newState._userID}): New ClientType: {newState._clientType}");
                if (userStates == null) {
                    userStates = new EnvUserState[]{};
                }
                
                bool addedState = false;

                if (newState._userID >= userStates.Length) {
                    Array.Resize(ref userStates, newState._userID + 1);
                    addedState = true;
                    userStates[newState._userID] = new EnvUserState();

                }


                EnvUserState_SetUserID(newState._userID, newState._userID, false);
                EnvUserState_SetName(newState._userID, newState._userName, false);
                EnvUserState_SetActive(newState._userID, newState._isActive != 0, false);
                EnvUserState_SetPosition(newState._userID, newState._position, false);
                EnvUserState_SetOrientation(newState._userID, newState._orientation, false);
                
                if ((newState.avatarType != userStates[newState._userID]._avatarType) || (newState.avatarParams != userStates[newState._userID]._avatarParams)) {
					userStates[newState._userID]._avatarType = newState._avatarType;
					userStates[newState._userID]._avatarParams = newState._avatarParams;

					// var zs = World.instance.clients[newState._userID].GetComponent<ZedSignal>();
					// zs.avatar.ReloadAvatar(newState._userID, userStates[newState._userID]);

				}
				if (addedState) {
					userStates[newState._userID]._clientType = newState.clientType;                    
					World.instance.UpdateClass(newState._userID, newState.clientType);
				}
				else {
					EnvUserState_SetClientType(newState._userID, (DanceGraph.ClientType)newState._clientType, false);
				}
				
				MatchStateWithClient(newState._userID);
				
				if (newState._userID < World.instance.clients.Count) {
					var go = World.instance.clients[newState._userID];
					var newIsActive = newState._isActive > 0;
					
					if(go.activeSelf != newIsActive)
						go.SetActive(newIsActive);
				}
            }

            // Call if we change the state locally so that all the bureaucracy gets done
            
            public void EnvMusicState_SetIsPlaying(bool value, bool send = false)
            {
                musicState._isPlaying = value;
                musicState.signalID = SignalID.EnvMusicStateID;
				if (send) 
					musicState.SendStruct();
                //_SendStruct(musicState);         
            }

            public void EnvMusicState_SetTrack(String name, double musicTime = 0.0, bool send = true)
            {
                musicState._trackName = name;
                musicState._musicTime = musicTime;
                musicState.signalID = SignalID.EnvMusicStateID;

                if (send)
                    //_SendStruct(musicState);
                    musicState.SendStruct();
            }

            public void EnvUserState_SetActive(int idx, bool isActive, bool send = true) {
                // Seriously, Microsoft? No byte literal in C#?
                userStates[idx]._isActive = isActive? (byte)1 : (byte)0;

                if (!isActive) {
                    Debug.Log($"Destroying avatar {idx}");
                    World.instance.DestroyAvatar(idx);
                }

                if (send) {
                    Debug.Log($"User State send for avatar {userStates[idx]._avatarType}, {userStates[idx]._avatarParams}");                    
                    userStates[idx].SendStruct();
                }
            }

            public void EnvUserState_SetUserID(int idx, int id, bool send = true) {
                userStates[idx]._userID = id;

                if (send) {
                    Debug.Log($"User State send for avatar {userStates[idx]._avatarType}, {userStates[idx]._avatarParams}");                    
                    userStates[idx].SendStruct();
                }
            }
            
            public void EnvUserState_SetClientType(int idx, DanceGraph.ClientType ctype, bool send = true) {

                if ((int)ctype != (int)userStates[idx]._clientType) {
                    Debug.Log($"User {idx} client type has changed from {userStates[idx]._clientType} to {ctype}, teleporting");
                    userStates[idx]._clientType = (byte) ctype;                    
                    World.instance.UpdateClass(idx, (int)ctype);
                }
                else {
                    userStates[idx]._clientType = (byte) ctype;
                }
                
                Debug.Log($"Client Type set for {idx}: {userStates[idx]._clientType}");



                
                if (send) {
                    Debug.Log($"User State send for avatar {userStates[idx]._avatarType}, {userStates[idx]._avatarParams}");
                    userStates[idx].SendStruct();
                }
            }
            
            public void EnvUserState_SetName(int idx, string name, bool send = true)
            {
                userStates[idx]._userName = name;
                if (send) {
                    Debug.Log($"User State send for avatar {userStates[idx]._avatarType}, {userStates[idx]._avatarParams}");
                    //_SendStruct(userStates[idx]);
                    userStates[idx].SendStruct();
                }
            }
            public void EnvUserState_SetAvatar(int idx, string type, JObject avatarparams, bool send = true) {
                userStates[idx]._avatarType = type;

                string param_output = Newtonsoft.Json.JsonConvert.SerializeObject(avatarparams);
                Debug.Log($"Attempting to set Avatar type {type}, params {param_output}");
                if (param_output.Length > SignalConstants.USERAVATARPARAMS_MAX_SIZE) {
                    Debug.LogWarning("Avatar Parameter size is too large!");
                    userStates[idx]._avatarParams = "";
                }
                else {
                    userStates[idx]._avatarParams = param_output;
                }

                if (send) {
                    Debug.Log($"User State send for avatar {userStates[idx]._avatarType}, {userStates[idx]._avatarParams}");
                    //_SendStruct(userStates[idx]);
                    userStates[idx].SendStruct();
                }
            }

            public void EnvUserState_SetPosition(int idx, float[] position, bool send = true) {
                userStates[idx]._position = position;

                if (send) {
                    Debug.Log($"User State send for position {userStates[idx]._position}");
                    userStates[idx].SendStruct();
                }
                
            }

            public void EnvUserState_SetOrientation(int idx, short [] orientation, bool send = true) {
                userStates[idx]._orientation = orientation;

                if (send) {
                    Debug.Log($"User State send for orientation {userStates[idx]._orientation}");
                    userStates[idx].SendStruct();
                }
                
            }


            public void EnvUserState_SendUserReq(int idx)
            {
                userDataRequest.userID = idx;
                userDataRequest.SendStruct();
                //_SendStruct(userDataRequest);
            }
            
            public void MatchStateWithClient(int userIdx) {
                // Called when a client is created or a userstate env signal comes in                
                // The client data hasn't shown up yet
                if (userIdx >= World.instance.clients.Count) {
                    Debug.Log($"Can't match state with uninstanced client {userIdx}/{World.instance.clients.Count}");
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

				zs.avatar.LoadAvatar(userIdx, userStates[userIdx]);

				Debug.Log($"Matching avatar {userIdx} state and client, name {userStates[userIdx]._userName}");      //          		 zs.avatar.ReloadAvatar(userIdx, userStates[userIdx]);
            }
            
            public EnvMusicState musicState = new EnvMusicState();
            public EnvUserState [] userStates = new EnvUserState[0]{};
            public EnvUserDataRequest userDataRequest = new EnvUserDataRequest();
            
        }
    }
    
}
