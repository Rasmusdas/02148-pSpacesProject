using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NetworkServer
{
    public static bool masterClient = false;

    private static SpaceRepository _repository;
    private static ISpace _currentSpace;
    public static string playerId;
    private static List<string> _playerIds = new List<string>();

    private static int _currentId;
    private static Dictionary<int,NetworkTransform> networkObjects = new();

    public static void StartServer(ServerInfo info)
    {
        masterClient = true;
        _repository = new SpaceRepository();
        _repository.AddGate(string.Format("{0}://{1}:{2}?{3}", info.protocol, info.ip, info.port, info.connectionType));
        _currentSpace = new SequentialSpace();
        _repository.AddSpace(info.space, _currentSpace);
        Debug.Log("Server Started: " + info);

        playerId = Guid.NewGuid().ToString();

        _playerIds.Add(playerId);

        Thread serverThread = new Thread(new ThreadStart(() => HandleServerUpdates()));

        serverThread.Start();

        Thread clientThread = new Thread(new ThreadStart(() => HandleUpdates()));

        clientThread.Start();
    }

    public static void JoinServer(ServerInfo info)
    {
        masterClient = false;

        _currentSpace = new RemoteSpace(string.Format("{0}://{1}:{2}/{3}?{4}", info.protocol, info.ip, info.port,info.space, info.connectionType));

        Debug.Log("Connected to server: " + info);

        playerId = Guid.NewGuid().ToString();

        _playerIds.Add(playerId);

        _currentSpace.Put("Server",playerId);

        Thread thread = new Thread(new ThreadStart(() => HandleUpdates()));

        thread.Start();
    }

    public static void Instantiate(NetworkTransform obj)
    {
        networkObjects.Add(_currentId, obj);
        _currentId++;
    }

    public static void MovementUpdate(Packet packet)
    {
        _currentSpace.Put(packet.target, packet.type.ToString());
    }

    private static void BroadcastMovementUpdate(Packet packet)
    {
        foreach(string id in _playerIds)
        {
            if(id != playerId)
            {
                Debug.Log("Adding packet " + string.Format("{0},{1},{2}", id, packet.type.ToString(), packet.data));
                _currentSpace.Put(id,packet.type.ToString());
            }
        }
    }

    private static void HandleUpdates()
    {
        while(true)
        {
            ITuple tuple = _currentSpace.Get(playerId, typeof(string));

            if ((string)tuple[1] == "Movement")
            {
                Debug.Log("Got update packet");
                //var data = ((int, Vector3))tuple[2];
                //networkObjects[data.Item1].UpdatePosition(data.Item2);
            }
        }
    }

    private static void HandleServerUpdates()
    {
        while(true)
        {
            ITuple tuple = _currentSpace.Get("Server", typeof(string));
            Debug.Log("Got Server Packet");

            if ((string)tuple[1] == "Movement")
            {
                Debug.Log("Got movement packet");

                BroadcastMovementUpdate(new Packet(PacketType.Movement, playerId, "Player", null));
                //var data = ((int, Vector3))tuple[2];
                //networkObjects[data.Item1].UpdatePosition(data.Item2);
            }
            else
            {
                Debug.Log("Player " + tuple[1] + " Joined");

                _playerIds.Add((string)tuple[1]);
            }
        }
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
    Movement
}