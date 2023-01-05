using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class NetworkServer
{
    public static bool masterClient = false;

    private static SpaceRepository _repository;
    private static ISpace _serverSpace;
    private static ISpace _ownSpace;

    private static Dictionary<string,ISpace> _playerSpaces = new Dictionary<string,ISpace>();
    public static string playerId;
    private static List<string> _playerIds = new List<string>();
    private static int _currentId;
    private static Dictionary<int,NetworkTransform> networkObjects = new();
    private static Dictionary<int, string> networkObjectOwners = new();
    private static Dictionary<string, GameObject> prefabs = new();

    public static void StartServer(ServerInfo info)
    {
        masterClient = true;
        _repository = new SpaceRepository();
        _repository.AddGate(string.Format("{0}://{1}:{2}?{3}", info.protocol, info.ip, info.port, info.connectionType));
        _serverSpace = new SequentialSpace();
        _repository.AddSpace(info.space, _serverSpace);

        Debug.Log("Server Started: " + info);

        playerId = RandomString(16);

        _ownSpace = new SequentialSpace();

        _repository.AddSpace(playerId, _ownSpace);
        _playerSpaces.Add(playerId, _ownSpace);

        _playerIds.Add(playerId);

        Thread serverThread = new Thread(new ThreadStart(() => HandleServerUpdates()));

        serverThread.Start();

        LoadResources();

        GBHelper.Start(HandleUpdates());

        
    }

    public static void JoinServer(ServerInfo info)
    {
        masterClient = false;

        _serverSpace = new RemoteSpace(string.Format("{0}://{1}:{2}/{3}?{4}", info.protocol, info.ip, info.port, info.space, info.connectionType));

        playerId = RandomString(16);

        _playerIds.Add(playerId);

        _serverSpace.Put("Server", "Join", playerId);

        Debug.Log("Connected to server: " + info);

        _serverSpace.Get(playerId, "Join");

        Debug.Log("Connected to private space");

        _ownSpace = new RemoteSpace(string.Format("{0}://{1}:{2}/{3}?{4}", info.protocol, info.ip, info.port, playerId, info.connectionType));

        LoadResources();

        GBHelper.Start(HandleUpdates());
    }

    private static void LoadResources()
    {
        var objs = Resources.LoadAll("NetworkPrefabs",typeof(GameObject));

        foreach(var v in objs)
        {
            prefabs.Add(v.name, (GameObject)v);
        }
    }

    public static void Instantiate(string objName)
    {
        if (!prefabs.ContainsKey(objName)) throw new ArgumentException("Object does not exist in prefabs");

        _serverSpace.Put("Server","Instantiate","Player",playerId);
    }

    public static void MovementUpdate(Packet packet)
    {
        (int, Vector3) data = ((int, Vector3))packet.data;

        _serverSpace.Put(packet.target, packet.type.ToString(), data.Item1, data.Item2.x, data.Item2.y, data.Item2.z);
    }

    private static void BroadcastMovementUpdate(Packet packet)
    {
        (int, float, float, float) data = ((int, float, float, float))packet.data;
        foreach (string id in _playerIds)
        {
            if (id == networkObjectOwners[data.Item1]) { Debug.Log("Stopping packet to owner");return; }

            _playerSpaces[id].Put(id, packet.type.ToString(), data.Item1, data.Item2, data.Item3, data.Item4);
        }
    }

    private static void BroadcastInstantiateUpdate(Packet packet)
    {
        foreach (string id in _playerIds)
        {
            (string, string, int) data = ((string, string, int))packet.data;
            _playerSpaces[id].Put(id, packet.type.ToString(), data.Item1, data.Item2, data.Item3);
        }
    }

    private static IEnumerator HandleUpdates()
    {
        while(true)
        {
            ITuple tuple = _ownSpace.GetP(playerId, typeof(string), typeof(int), typeof(float), typeof(float), typeof(float));
            if (tuple != null && (string)tuple[1] == "Movement")
            {
                Debug.Log("Got movement update");
                networkObjects[(int)tuple[2]].UpdatePosition(new Vector3((float)tuple[3], (float)tuple[4], (float)tuple[5]));
            }

            tuple = _ownSpace.GetP(playerId, typeof(string), typeof(string), typeof(string), typeof(int));

            if (tuple != null && (string)tuple[1] == "Instantiate")
            {
                Debug.Log("Got Inst update");
                GameObject gb = GBHelper.Instantiate(prefabs[(string)tuple[2]]);

                gb.GetComponent<NetworkTransform>().id = (int)tuple[4];

                networkObjects.Add((int)tuple[4], gb.GetComponent<NetworkTransform>());

                if((string)tuple[3] == playerId)
                {
                    gb.GetComponent<NetworkTransform>().isOwner = true;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private static void HandleServerUpdates()
    {
        while(true)
        {
            ITuple tuple = _serverSpace.GetP("Server", typeof(string), typeof(string));

            if (tuple != null && (string)tuple[1] == "Join")
            {
                Debug.Log("Player " + tuple[2] + " Joined");

                _playerIds.Add((string)tuple[2]);

                ISpace newPlayerSpace = new SequentialSpace();

                _repository.AddSpace((string)tuple[2],newPlayerSpace);
                _playerSpaces.Add((string)tuple[2],newPlayerSpace);
                _serverSpace.Put((string)tuple[2],"Join");
            }

            tuple = _serverSpace.GetP("Server", typeof(string), typeof(string), typeof(string));

            if (tuple != null && (string)tuple[1] == "Instantiate")
            {
                Debug.Log("Got server inst update");
                networkObjectOwners.Add(_currentId, (string)tuple[3]);
                BroadcastInstantiateUpdate(new Packet(PacketType.Instantiate, "Server", "Player", ((string)tuple[2], (string)tuple[3], _currentId++)));
            }

            tuple = _serverSpace.GetP("Server", typeof(string), typeof(int), typeof(float), typeof(float), typeof(float));

            if (tuple != null && (string)tuple[1] == "Movement")
            {
                Debug.Log("Got server movement update");
                BroadcastMovementUpdate(new Packet(PacketType.Movement, "Server", "Player", ((int)tuple[2], (float)tuple[3], (float)tuple[4], (float)tuple[5])));
            }
        }
    }

    private static System.Random random = new System.Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

public struct ServerInfo
{
    public readonly string protocol;
    public readonly string ip;
    public readonly int port;
    public readonly string space;
    public readonly string connectionType;

    public ServerInfo(string protocol, string ip, int port, string space, string connectionType)
    {
        this.protocol = protocol;
        this.ip = ip;
        this.port = port;
        this.space = space;
        this.connectionType = connectionType;
    }

    public override string ToString()
    {
        return string.Format("Started: {0}://{1}:{2}?{3} | Space :{4}", protocol, ip, port, connectionType,space);
    }
}

public struct Packet
{
    public readonly PacketType type;
    public readonly string source;
    public readonly string target;
    public object data;

    public Packet(PacketType type, string source, string target, object data)
    {
        this.type = type;
        this.source = source;
        this.target = target;
        this.data = data;
    }
}

public enum PacketType
{
    Movement,Instantiate
}