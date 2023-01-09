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

    private static bool verbose = false;

    public static void StartServer(ServerInfo info)
    {
        masterClient = true;
        _repository = new SpaceRepository();
        _repository.AddGate(string.Format("{0}://{1}:{2}?{3}", info.protocol, info.ip, info.port, info.connectionType));
        _serverSpace = new SequentialSpace();
        _repository.AddSpace(info.space, _serverSpace);

        if(verbose) Debug.Log("Server Started: " + info);

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

        if(verbose) Debug.Log("Connected to server: " + info);

        _serverSpace.Get(playerId, "Join");

        _ownSpace = new RemoteSpace(string.Format("{0}://{1}:{2}/{3}?{4}", info.protocol, info.ip, info.port, playerId, info.connectionType));

        if(verbose) Debug.Log("Connected to private space " + string.Format("{0}://{1}:{2}/{3}?{4}", info.protocol, info.ip, info.port, playerId, info.connectionType));

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

    public static void Instantiate(string objName,Vector3 position, Quaternion rotation)
    {
        if (!prefabs.ContainsKey(objName)) throw new ArgumentException("Object does not exist in prefabs");

        _serverSpace.Put(playerId,"Instantiate",objName+"|"+NetworkPackager.Package(position)+"|"+NetworkPackager.Package(rotation));
    }

    public static void MovementUpdate(Packet packet)
    {
        if(verbose) Debug.Log("Client: Sending Movement Packet: " + packet.source + "," + packet.type + "," + packet.data);
        _serverSpace.Put(packet.source,packet.type.ToString(),packet.data);
    }

    public static void DamagePlayer(Packet packet)
    {
        if (verbose) Debug.Log("Client: Sending Movement Packet: " + packet.source + "," + packet.type + "," + packet.data);

        _serverSpace.Put(packet.source, packet.type.ToString(), packet.data);
    }

    //internal static void RotationUpdate(Packet packet)
    //{
    //    (int, Quaternion) data = ((int, Quaternion))packet.data;

    //    _serverSpace.Put(packet.target, packet.type.ToString(), data.Item1, data.Item2.x, data.Item2.y, data.Item2.z, data.Item2.w);
    //}

    private static void BroadcastPacket(Packet packet)
    {
        foreach(var id in _playerIds)
        {
            if (id == packet.source) continue;

            if(verbose) Debug.Log("Server: Sending packet: " + packet);

            _playerSpaces[id].Put(packet.type.ToString(), packet.data);
        }
    }


    //private static void BroadcastMovementUpdate(Packet packet)
    //{
    //    (int, float, float, float) data = ((int, float, float, float))packet.data;
    //    foreach (string id in _playerIds)
    //    {
    //        if (id == networkObjectOwners[data.Item1]) { if(verbose) Debug.Log("Stopping packet to owner " + networkObjectOwners[data.Item1]);continue; }

    //        if(verbose) Debug.Log(id);

    //        _playerSpaces[id].Put(id, packet.type.ToString(), data.Item1, data.Item2, data.Item3, data.Item4);
    //    }
    //}

    private static void BroadcastRotationUpdate(Packet packet)
    {
        //(int, float, float, float, float) data = ((int, float, float, float, float))packet.data;
        //foreach (string id in _playerIds)
        //{
        //    if (id == networkObjectOwners[data.Item1]) { if(verbose) Debug.Log("Stopping packet to owner " + networkObjectOwners[data.Item1]); continue; }

        //    _playerSpaces[id].Put(id, packet.type.ToString(), data.Item1, data.Item2, data.Item3, data.Item4, data.Item5);
        //}
    }

    private static void BroadcastInstantiateUpdate(Packet packet)
    {
        //foreach (string id in _playerIds)
        //{
        //    (string, string, int) data = ((string, string, int))packet.data;
        //    _playerSpaces[id].Put(id, packet.type.ToString(), data.Item1, data.Item2, data.Item3);
        //}
    }

    private static IEnumerator HandleUpdates()
    {
        while(true)
        {
            yield return new WaitForEndOfFrame();
            ITuple tuple = _ownSpace.GetP(typeof(string), typeof(string));
            if (tuple == null) continue;

            string type = (string)tuple[0];
            string data = (string)tuple[1];

            if(verbose) Debug.Log("Client: Received Packet of Type " + (string)tuple[0]);

            if (type == "Movement")
            {
                string[] splitData = data.Split("|");

                int id = int.Parse(splitData[0]);
                Vector3 position = NetworkPackager.UnpackageVector3(splitData[1]);
                Quaternion rotation = NetworkPackager.UnpackgeQuaternion(splitData[2]);
                networkObjects[id].UpdatePosition(position);
                networkObjects[id].UpdateRotation(rotation);

            }
            if (type == "Instantiate")
            {
                string[] splitData = data.Split("|");

                int objId = int.Parse(splitData[0]);
                string id = splitData[1];
                string prefabName = splitData[2];
                string prefabPos = splitData[3];
                string prefabRot = splitData[4];

                GameObject gb = GBHelper.Instantiate(prefabs[prefabName]);

                gb.GetComponent<NetworkTransform>().id = objId;
                gb.GetComponent<NetworkTransform>().owner = id;

                gb.transform.position = NetworkPackager.UnpackageVector3(prefabPos);
                gb.transform.rotation = NetworkPackager.UnpackgeQuaternion(prefabRot);

                networkObjects.Add(objId, gb.GetComponent<NetworkTransform>());

                if (id == playerId)
                {
                    gb.GetComponent<NetworkTransform>().isOwner = true;
                }
            }
            if(type == "Health")
            {
                string[] splitData = data.Split("|");

                int id = int.Parse(splitData[0]);
                int health = int.Parse(splitData[1]);
                networkObjects[id].GetComponent<PlayerController>().UpdateHealth(health);
            }
        }
    }


    private static void HandleServerUpdates()
    {
        if(verbose) Debug.Log("Server: Handler started");

        while (true)
        {
            ITuple tuple = _serverSpace.GetP(typeof(string), typeof(string), typeof(string));

            if (tuple == null) continue;

            if(verbose) Debug.Log("Server: Received Packet of Type " + (string)tuple[1]);

            if (tuple != null && (string)tuple[1] == "Join")
            {
                Debug.Log("Player " + tuple[2] + " Joined");

                _playerIds.Add((string)tuple[2]);

                ISpace newPlayerSpace = new SequentialSpace();

                _repository.AddSpace((string)tuple[2],newPlayerSpace);
                _playerSpaces.Add((string)tuple[2],newPlayerSpace);
                _serverSpace.Put((string)tuple[2],"Join");
                continue;
            }

            if (tuple != null && (string)tuple[1] == "Instantiate")
            {
                networkObjectOwners.Add(_currentId, (string)tuple[0]);
                BroadcastPacket(new Packet(PacketType.Instantiate, "All", "Server", _currentId++ + "|" + (string)tuple[0] + "|" + (string)tuple[2]));
            }

            if (tuple != null && (string)tuple[1] == "Movement")
            {
                BroadcastPacket(new Packet(PacketType.Movement, (string)tuple[0], "Server", (string)tuple[2]));
            }

            if (tuple != null && (string)tuple[1] == "Health")
            {
                BroadcastPacket(new Packet(PacketType.Health, "All", "Server", (string)tuple[2]));
            }

            //tuple = _serverSpace.GetP("Server", typeof(string), typeof(int), typeof(float), typeof(float), typeof(float), typeof(float));


            //if (tuple != null && (string)tuple[1] == "Rotation")
            //{
            //    if(verbose) Debug.Log("Got server rotaion update");
            //    BroadcastRotationUpdate(new Packet(PacketType.Rotation, "Server", "Player", ((int)tuple[2], (float)tuple[3], (float)tuple[4], (float)tuple[5], (float)tuple[6])));
            //}


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
    public readonly string data;

    public Packet(PacketType type, string source, string target, string data)
    {
        this.type = type;
        this.source = source;
        this.target = target;
        this.data = data;
    }

    public override string ToString()
    {
        return string.Format("Type: {0} - Source: {1} - Target: {2} - Data: {3}", type, source, target, data);
    }
}

public enum PacketType
{
    Movement,Instantiate,Health
}