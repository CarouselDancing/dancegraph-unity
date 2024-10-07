using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace cfg
{

[Serializable]
public class YBotConfig
{
	public float [] colour1;
	public float [] colour2;
}

[Serializable]
public class LouiseConfig
{
    public string placeholder;
}

[Serializable]
public class MagipeepConfig
{
    public string model;
    public int materialIdx;
}


public class MainAvatarConfig {
    public string race;
    public string gender;
    public int model;
    public float shape;
}



[Serializable]
public class AvatarConfig
{
    public string type;
    public JObject options;
}


[Serializable]
public class UserRole_UserSignal
{
    public string name;
    public string transformer;
    public bool isReflexive;
}

[Serializable]
public class UserRole
{
    public UserRole_UserSignal[] user_signals;
}

[Serializable]
public class Scene
{
    public string[] env_signals;
    public Dictionary<string, UserRole> user_roles;
}

[Serializable]
public class Signal
{
    public string dll;
}

[Serializable]
public class SignalModule
{
    public Signal config;
    public Dictionary<string, Signal> producers;
    public Dictionary<string, Signal> consumers;
    public Dictionary<string, Signal> transformers;
}

[Serializable]
public class DanceGraphConfig
{
    public string dll_folder;
    public Dictionary<string, Scene> scenes;
    public Dictionary<string, Dictionary<string, Signal>> generic_producers;
    public Dictionary<string, Dictionary<string, Signal>> generic_consumers;
    public Dictionary<string, Dictionary<string, SignalModule>> user_signals;
    public Dictionary<string, Dictionary<string, SignalModule>> env_signals;
}

namespace rt
{
    // Runtime preset config
    [Serializable]
    public class Address
    {
        public string ip;
        public int port;
    }

[Serializable]
public class RuntimeSignal
{
    public string name;
    public JObject opts;
}

[Serializable]
public class RuntimeTransformer
{
    public string name;
    public JObject opts;
}
        
        [Serializable]
        public class ClientListenerCommon
        {
            public string username;
            public Address address;
            public Address server_address;
            public string scene;
            public Dictionary<string, List<RuntimeSignal>> env_signal_consumers;
            public Dictionary<string, List<RuntimeSignal>> user_signal_consumers;
            public bool include_ipc_consumers;
            public bool single_port_operation;
            public bool ignore_ntp;

        };
		
		[Serializable]
		public class Client : ClientListenerCommon
		{
			public string role;
			public Dictionary<string, RuntimeSignal> producer_overrides;
			public List<RuntimeTransformer> transformers;
			
			
		};
		
		[Serializable]
		public class Listener : ClientListenerCommon
		{
			public List<string> clients;
			public List<string> signals;
		};
		
		[Serializable]
		public class Server : ClientListenerCommon
		{
		};
		
		[Serializable]
		public class PresetDb
		{
			public Dictionary<string, Server> server;
			public Dictionary<string, Client> client;
			public Dictionary<string, Listener> listener;
			public string username;
		}
}

}

