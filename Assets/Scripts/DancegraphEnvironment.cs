using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;

public static class SignalConstants {
    // Just so all the size values are in one place
    public const int MUSICTRACK_MAX_SIZE = 256;
    public const int USERNAME_MAX_SIZE = 256;
    public const int USERAVATAR_MAX_SIZE = 256;
    public const int SCENENAME_MAX_SIZE = 256;
    public const int ENVSIGNAL_MAX_SIZE = 1024;
};


public static class SignalID {
    // Sigh
    public const byte EnvMessageGenericID = 255;
    public const byte EnvSceneStateID = 4;
    public const byte EnvSceneRequestID = 5;
    public const byte EnvUserStateID = 8;
    public const byte EnvUserRequestID = 9;
    public const byte EnvMusicStateID = 12;
    public const byte EnvMusicRequestID = 13;
    public const byte EnvTestStateID = 16;
    public const byte EnvTestRequestID = 17;
};

public static class Global {

    // SendStruct also needs to send the metadata
    public static void SendStruct<T>(string name, T s) where T: struct {
        int size = System.Runtime.InteropServices.Marshal.SizeOf(s);

        byte []arr = new byte[size];


        // We should be able to do this without the alloc and copy
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(s, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        Debug.Log($"Sending struct size {size} via SendStruct");
        DanceGraphMinimalCpp.SendSignal(arr, size);
        //Debug.Log(String.Format("Sending Signal of size {0}", size));
        /*
        // This doesn't work due to reference types
        // Even when the types are inlined in the struct
       
        Span<T> bsp = MemoryMarshal.CreateSpan<T>(ref s, 1);
        var bytespan = MemoryMarshal.Cast<T, byte>(bsp);
       
        DanceNetCpp.SendSignal(bytespan, size);
        */
    }
    
    public static T ReadStruct<T>(byte [] bytes)
    {

        // Read in a byte array
        //byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

        // Pin the managed memory while, copy it out the data, then unpin it
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T structY = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();

        return structY;
    }

    public static ulong TimeStamp() {
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        ulong uStamp = (ulong)t.TotalMilliseconds;
        return uStamp;
    }


#if false
    public static EnvSceneState sceneState;
    public static EnvMusicState musicState;
    public static EnvTestState testState;

    public static Dictionary<int, EnvUserState> userStates;
    public static EnvUserState selfUserState;
    
    // These have to be changed without triggering the setters
    public static void ChangeSceneState(EnvSceneState ess) {
        sceneState._sceneName = ess.sceneName;
    }

    public static void ChangeMusicState(EnvMusicState ems) {
        musicState._trackName = ems.trackName;
        musicState._musicTime = ems.musicTime;
        musicState._isPlaying = ems.isPlaying;
        
    }

    public static void ChangeUserState(int uIdx, EnvUserState eus) {
        Debug.Log("State for user {0} to be updated");

        if (userStates.ContainsKey(uIdx)) {
            userStates[uIdx] = eus;

        }
        else {
            userStates.TryAdd(uIdx, eus);
        }
        
    }


    public static void ChangeTestState(EnvTestState ets) {
        testState._payload = ets.payload;       
    }
    

    public static void DumpSceneState() {
        Debug.Log(String.Format("Scene is {0}", sceneState.sceneName));
    }

    public static void DumpMusicState() {
        string isp = musicState.isPlaying? "playing": "paused";
        Debug.Log(String.Format("Music is {0} @{1} {2}",
                                musicState.trackName,
                                musicState.musicTime,
                                isp
                  ));
    }

    public static void DumpTestState() {
        Debug.Log(String.Format("Test payload is {0}", testState.payload));
    }

    public static void DumpUserState() {
        

    }
#endif   
}

// Shared Environment State Data

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnvSceneState {
    private T SendValue<T>(T v) {
        signalID = SignalID.EnvSceneStateID;
        //timeStamp = Global.TimeStamp();
        Global.SendStruct<EnvSceneState>("EnvSceneState", this);
        return v;
    }
    /*
      [MarshalAs(UnmanagedType.U4)]
      public int senderID;
      [MarshalAs(UnmanagedType.U8)]
      public ulong timeStamp;
    */

    
    [MarshalAs(UnmanagedType.U1)]    
    public byte signalID;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SignalConstants.SCENENAME_MAX_SIZE)]    
    public string _sceneName;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]    
    public byte [] padding;
    
    public string sceneName {
        get => _sceneName;
        set => _sceneName = SendValue(value);
    }
};

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SceneDataRequest {
    private T SendValue<T>(T v) {
        signalID = SignalID.EnvSceneRequestID;
        //timeStamp = Global.TimeStamp();
        Global.SendStruct<SceneDataRequest>("SceneDataRequest", this);  
        return v;
    }
   
    [MarshalAs(UnmanagedType.U4)]    
    public byte signalID;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]    
    public byte [] padding;

    
};



[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnvUserState {

    private T SendValue<T>(T v) {
        //timeStamp = Global.TimeStamp();
        signalID = SignalID.EnvUserStateID;
        Global.SendStruct<EnvUserState>("EnvUserState", this);
        return v;
    }
    /*
      [MarshalAs(UnmanagedType.U4)]
      public int senderID;
      [MarshalAs(UnmanagedType.U8)]
      public ulong timeStamp;
    */

        [MarshalAs(UnmanagedType.U1)]    
        public byte signalID;

    
    // Should be the SignalMetadata, rather than the signal data proper
    [MarshalAs(UnmanagedType.U4)]
    public int _userID;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SignalConstants.USERNAME_MAX_SIZE)]        
    public string _userName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SignalConstants.USERAVATAR_MAX_SIZE)]        
    public string _avatarDesc;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]    
    public float[] _position;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]    
    public short[] _orientation;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]    
    public byte [] _isActive;
    
    public int userID {
        get => _userID;
        set => _userID = SendValue(value);
    }
    public string userName {
        get => _userName;
        set => _userName = SendValue(value);
    }

    public string avatarDesc {
        get => _avatarDesc;
        set => _avatarDesc = SendValue(value);
    }

    // Three floats
    public float [] position {
        get => _position;
        set => _position = SendValue(value);
    }

    // Three 16-bit integers, being the (x, y, z) components of an (x, y, z, w) quaternion
    public short [] orientation {
        get => _orientation;
        set => _orientation = SendValue(value);
    }

    public byte [] isActive {
        get => isActive;
        set => _isActive = SendValue(value);
    }
};


[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnvUserDataRequest {
    private T SendValue<T>(T v) {
        //timeStamp = Global.TimeStamp();
        signalID = SignalID.EnvUserRequestID;
        Global.SendStruct<EnvUserDataRequest>("EnvUserDataRequest", this);      
        return v;
    }
    /*
      [MarshalAs(UnmanagedType.U4)]
      public int senderID;
      [MarshalAs(UnmanagedType.U8)]
      public ulong timeStamp;
    */
    [MarshalAs(UnmanagedType.U4)]    
    public byte signalID;

    [MarshalAs(UnmanagedType.U4)]    
    public int userID;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]    
    public byte [] padding;


    
};


[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnvMusicState {
    private T SendValue<T>(T v) {
        //timeStamp = Global.TimeStamp();
        signalID = SignalID.EnvMusicStateID;
        Global.SendStruct<EnvMusicState>("EnvMusicState", this);
        return v;
    }
    /*
      [MarshalAs(UnmanagedType.U4)]
      public int senderID;
      [MarshalAs(UnmanagedType.U8)]
      public ulong timeStamp;
    */
    [MarshalAs(UnmanagedType.U4)]    
    public byte signalID;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SignalConstants.SCENENAME_MAX_SIZE)]
    public string _trackName;

    // _musicTime should be when the track started playing, in microSeconds
    [MarshalAs(UnmanagedType.U8)]
    public double _musicTime;

    [MarshalAs(UnmanagedType.U1)]     
    public bool _isPlaying;

    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]    
    public byte [] padding;

    public string trackName {
        get => _trackName;
        set => _trackName = SendValue(value);
    }
    public double musicTime {
        get => _musicTime;
        set => _musicTime = SendValue(value);
    }
    public bool isPlaying {
        get => _isPlaying;
        set => _isPlaying = SendValue(value);
    }
};




[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MusicDataRequest {
    private T SendValue<T>(T v) {
        //timeStamp = Global.TimeStamp();
        signalID = SignalID.EnvMusicRequestID;
        Global.SendStruct<MusicDataRequest>("MusicDataRequest", this);  
        return v;
    }
    /*
      [MarshalAs(UnmanagedType.U4)]
      public int senderID;
      [MarshalAs(UnmanagedType.U8)]
      public ulong timeStamp;
    */
    [MarshalAs(UnmanagedType.U4)]    
    public byte signalID;
};


[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnvTestState {
    private T SendValue<T>(T v) {
        //timeStamp = Global.TimeStamp();
        //Debug.Log(String.Format("Setting the Test state to {0}", v));
        signalID = SignalID.EnvTestStateID;
        Global.SendStruct<EnvTestState>("EnvTestState", this);  
        return v;
    }
    /*
      [MarshalAs(UnmanagedType.U4)]
      public int senderID;
      [MarshalAs(UnmanagedType.U8)]
      public ulong timeStamp;
    */
    [MarshalAs(UnmanagedType.U4)]    
    public byte signalID;

    [MarshalAs(UnmanagedType.U4)]    
    public uint _payload;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]    
    public byte [] padding;

    public uint payload {
        get => _payload;
        set => _payload = SendValue(value);
    }
    
};


[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SignalGeneric {
    [MarshalAs(UnmanagedType.U4)]
    public byte signalID;
};
    
