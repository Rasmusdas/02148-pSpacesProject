using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkServer
{

    #region Fields
    private static SpaceRepository _repository;
    private static ISpace _serverSpace;
    private static ISpace _ownSpace;
    private static Queue<Action> _updates = new Queue<Action>();

    private static Dictionary<string,ISpace> _playerSpaces = new Dictionary<string,ISpace>();
    public static Dictionary<int, NetworkTransform> networkObjects = new();
    private static Dictionary<int, string> _networkObjectOwners = new();
    private static Dictionary<int, string> _idToObjectType = new();
    private static Dictionary<string, GameObject> prefabs = new();
    private static Dictionary<int, Vector3> _startPos = new();
    private static List<string> _playerIds = new List<string>();

    private static int _currentId;
    private static int _playerSpawnCount;
    private static int _readyCount;
    private static int _playerJoinCount;
    private static int _maxPlayerCount = 4;
    private static bool _ready;

    public static bool running;
    public static string playerId;
    public static bool masterClient = false;

    const bool VERBOSE = true;
    private static System.Random random = new System.Random();
    private static int _restartCount;
    private static bool _restart;
    #endregion

    #region Server Setup

    /// <summary>
    /// Sets the Network Server state to the default state.
    /// </summary>
    private static void Init()
    {
        _readyCount = 0;
        _startPos = new();
        prefabs = new();
        _idToObjectType = new();
        _networkObjectOwners = new();
        networkObjects = new();
        _playerSpaces = new();
        _playerIds = new();
        _updates = new();
        _playerSpawnCount = 0;
        _currentId = 0;
        _ready = false;
        _restartCount = 0;
        LoadResources();
    }

    private static void Restart()
    {
        _readyCount = 0;
        _idToObjectType = new();
        _networkObjectOwners = new();
        networkObjects = new();
        _updates = new();
        _playerSpawnCount = 0;
        _currentId = 0;
        _ready = false;
        _restart = false;
        _restartCount = 0;
    }

    /// <summary>
    /// Starts a server with the provided arguments as settings.
    /// </summary>
    /// <param name="info"></param>
    public static void StartServer(ServerInfo info)
    {
        Init();
        masterClient = true;
        running = true;
        _repository = new SpaceRepository();
        _repository.AddGate(string.Format("{0}://{1}:{2}?{3}", info.protocol, info.ip, info.port, info.connectionType));
        _serverSpace = new SequentialSpace();
        _repository.AddSpace(info.space, _serverSpace);

        _startPos.Add(0, new Vector3(11, 1, 11));
        _startPos.Add(1, new Vector3(-11, 1, 11));
        _startPos.Add(2, new Vector3(11, 1, -11));
        _startPos.Add(3, new Vector3(-11, 1, -11));

        if (VERBOSE) Debug.Log("Server Started: " + info);

        playerId = RandomString(16);

        _playerJoinCount++;

        _ownSpace = new SequentialSpace();

        _repository.AddSpace(playerId, _ownSpace);
        _playerSpaces.Add(playerId, _ownSpace);

        _playerIds.Add(playerId);

        Thread serverThread = new Thread(new ThreadStart(() => HandleServerUpdates()));

        serverThread.Start();

        Thread clientThread = new Thread(new ThreadStart(() => HandleClientUpdates()));

        clientThread.Start();

        GBHelper.Start(HandleGameUpdates());

        
    }

    /// <summary>
    /// Joins a server with the provided arguments
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool JoinServer(ServerInfo info)
    {
        Init();
        masterClient = false;
        _serverSpace = new RemoteSpace(string.Format("{0}://{1}:{2}/{3}?{4}", info.protocol, info.ip, info.port, info.space, info.connectionType));

        playerId = RandomString(16);

        _playerIds.Add(playerId);

        _serverSpace.Put("Server", "Join", playerId);

        if (VERBOSE) Debug.Log("Connected to server: " + info);

        ITuple tuple = _serverSpace.Get(playerId, "Join", typeof(int));

        if ((int)tuple[2] == 0)
        {
            Debug.Log("Server was full");
            return false;
        }

        running = true;

        _ownSpace = new RemoteSpace(string.Format("{0}://{1}:{2}/{3}?{4}", info.protocol, info.ip, info.port, playerId, info.connectionType));

        if (VERBOSE) Debug.Log("Connected to private space " + string.Format("{0}://{1}:{2}/{3}?{4}", info.protocol, info.ip, info.port, playerId, info.connectionType));


        Thread clientThread = new Thread(new ThreadStart(() => HandleClientUpdates()));

        clientThread.Start();

        GBHelper.Start(HandleGameUpdates());

        return true;
    }

    /// <summary>
    /// Closes the open server.
    /// </summary>
    /// <param name="info"></param>
    public static void CloseServer(ServerInfo info)
    {
        if(masterClient)
        {
            _repository.CloseGate(string.Format("{0}://{1}:{2}?{3}", info.protocol, info.ip, info.port, info.connectionType));
        }
    }

    /// <summary>
    /// Loads in the prefabs that can be instantiated from the network. The load path is Resources/NetworkPrefabs.
    /// </summary>
    private static void LoadResources()
    {
        var objs = Resources.LoadAll("NetworkPrefabs",typeof(GameObject));

        foreach(var v in objs)
        {
            prefabs.Add(v.name, (GameObject)v);
        }
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Used to instantiate an object on all clients.
    /// </summary>
    /// <param name="objName"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void Instantiate(string objName,Vector3 position, Quaternion rotation)
    {
        if (!prefabs.ContainsKey(objName)) throw new ArgumentException("Object does not exist in prefabs");

        _serverSpace.Put(playerId,"Instantiate",objName+"|"+NetworkPackager.Package(position)+"|"+NetworkPackager.Package(rotation));
    }

    /// <summary>
    /// Destroys an object with the provided id on all clients.
    /// </summary>
    /// <param name="packet"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void Destroy(int id)
    {
        if (!networkObjects.ContainsKey(id)) throw new ArgumentException("Object does not exist on network");

        _serverSpace.Put(playerId,PacketType.Destroy.ToString(), id.ToString());
    }

    /// <summary>
    /// Sends a ready signal to the server to mark being ready to play.
    /// </summary>
    public static void MarkReady()
    {
        if (_ready) return;

        _ready = true;

        _serverSpace.Put(playerId, "Ready", "");
    }


    public static void RestartGame()
    {
        if (_restart) return;

        _restart = true;

        _serverSpace.Put(playerId, "Restart", "");
    }

    /// <summary>
    /// Updates the position of the object with the given id with the provided position and rotation on other clients.
    /// </summary>
    /// <param name="packet"></param>
    public static void MovementUpdate(int id, Vector3 position, Quaternion rotation)
    {
        //if(VERBOSE) Debug.Log("Client: Sending Movement Packet: " + playerId + "," + PacketType.Movement + "," + id + "|" + NetworkPackager.Package(position) + "|" + NetworkPackager.Package(rotation));
        _serverSpace.Put(playerId, PacketType.Movement.ToString(), id + "|" + NetworkPackager.Package(position) + "|" + NetworkPackager.Package(rotation));
    }

    /// <summary>
    /// Decreases the hp of the object with the given id with the provided amount.
    /// </summary>
    /// <param name="packet"></param>
    public static void DamagePlayer(int id, int amount)
    {
        if (VERBOSE) Debug.Log("Client: Sending Movement Packet: " + playerId + "," + PacketType.Health + "," + id+"|"+amount);

        _serverSpace.Put(playerId, PacketType.Health.ToString(), id + "|" + amount);
    }

    #endregion

    #region Private Methods

    #region Client Methods
    private static IEnumerator HandleGameUpdates()
    {
        while (running)
        {
            while (_updates.Count > 0)
            {
                _updates.Dequeue()();
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private static void HandleClientUpdates()
    {
        while(running)
        {
            IEnumerable<ITuple> tuples = _ownSpace.GetAll(typeof(string), typeof(string));

            foreach(ITuple tuple in tuples)
            {
                string type = (string)tuple[0];
                string data = (string)tuple[1];

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    data = data.Replace(",",".");
                }

                if (VERBOSE) Debug.Log("Client: Received Packet of Type " + (string)tuple[0]);

                switch (type)
                {
                    case "Movement":
                        HandleClientMovement(data);
                        break;
                    case "Instantiate":
                        HandleClientInstantiate(data);
                        break;
                    case "Health":
                        HandleClientHealth(data);
                        break;
                    case "Destroy":
                        HandleClientDestroy(data);
                        break;
                    case "Restart":
                        HandleClientRestart();
                        break;
                }
            }
        }
    }

    private static void HandleClientRestart()
    {
        _ownSpace.GetAll(typeof(string), typeof(string));
        _updates.Enqueue(() =>
        {
            Restart();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    private static void HandleClientDestroy(string data)
    {
        _updates.Enqueue(() =>
        {
            int id = int.Parse(data);
            UnityEngine.Object.Destroy(networkObjects[id].gameObject);
        });
    }

    private static void HandleClientHealth(string data)
    {
        string[] splitData = data.Split("|");

        _updates.Enqueue(() =>
        {
            int id = int.Parse(splitData[0]);
            int health = int.Parse(splitData[1]);
            networkObjects[id].GetComponent<PlayerController>().UpdateHealth(health);
        });
    }

    private static void HandleClientMovement(string data)
    {
        string[] splitData = data.Split("|");

        Vector3 position = NetworkPackager.UnpackageVector3(splitData[1]);
        Quaternion rotation = NetworkPackager.UnpackgeQuaternion(splitData[2]);

        _updates.Enqueue(() =>
        {
            int id = int.Parse(splitData[0]);
            networkObjects[id].UpdatePosition(position);
            networkObjects[id].UpdateRotation(rotation);
        });
    }

    private static void HandleClientInstantiate(string data)
    {
        string[] splitData = data.Split("|");

        _updates.Enqueue(() =>
        {
            int objId = int.Parse(splitData[0]);
            string id = splitData[1];
            string prefabName = splitData[2];
            string prefabPos = splitData[3];
            string prefabRot = splitData[4];
            GameObject gb = GBHelper.Instantiate(prefabs[prefabName], NetworkPackager.UnpackageVector3(prefabPos), NetworkPackager.UnpackgeQuaternion(prefabRot));

            gb.GetComponent<NetworkTransform>().id = objId;
            gb.GetComponent<NetworkTransform>().owner = id;

            if (prefabName != "Bullet")
            {
                networkObjects.Add(objId, gb.GetComponent<NetworkTransform>());
            }

            if (id == playerId)
            {
                gb.GetComponent<NetworkTransform>().isOwner = true;
            }
        });
    }

    #endregion

    #region Server Methods

    private static void HandleServerUpdates()
    {
        if(VERBOSE) Debug.Log("Server: Handler started");

        while(running)
        {
            IEnumerable<ITuple> tuples = _serverSpace.GetAll(typeof(string), typeof(string), typeof(string));

            foreach(ITuple tuple in tuples)
            {
                string type = (string)tuple[1];

                if (VERBOSE) Debug.Log("Server: Received Packet of Type " + (string)tuple[1]);

                switch (type)
                {
                    case "Join":
                        HandleServerJoin(tuple);
                        break;
                    case "Instantiate":
                        HandleServerInstantiate(tuple);
                        break;
                    case "Destroy":
                        BroadcastPacket(new Packet(PacketType.Destroy, "All", "Server", ((string)tuple[2]).Replace(".", ",")));
                        break;
                    case "Movement":
                        BroadcastPacket(new Packet(PacketType.Movement, (string)tuple[0], "Server", ((string)tuple[2]).Replace(".", ",")));
                        break;
                    case "Health":
                        BroadcastPacket(new Packet(PacketType.Health, "All", "Server", ((string)tuple[2]).Replace(".", ",")));
                        break;
                    case "Ready":
                        HandleServerReady();
                        break;
                    case "Restart":
                        HandleServerRestart();
                        break;
                }
            }
        }
    }

    private static void HandleServerRestart()
    {
        _restartCount++;
        Debug.Log(_restartCount);
        Debug.Log(_playerJoinCount);
        if (_restartCount >= _playerJoinCount)
        {
            Debug.Log("Restarting Server");
            foreach(var v in _playerSpaces)
            {
                v.Value.Put(PacketType.Restart.ToString(),"");
            }
            _serverSpace.GetAll(typeof(string), typeof(string), typeof(string));
        }
    }

    private static void BroadcastPacket(Packet packet)
    {
        foreach (var id in _playerIds)
        {
            if (id == packet.source) continue;

            if (VERBOSE) Debug.Log("Server: Sending packet: " + packet);

            _playerSpaces[id].Put(packet.type.ToString(), packet.data);
        }
    }

    private static void SendPacket(Packet packet, string target)
    {
        foreach (var id in _playerIds)
        {
            if (id != target) continue;

            if (VERBOSE) Debug.Log("Server: Sending packet: " + packet);

            _playerSpaces[id].Put(packet.type.ToString(), packet.data);
        }
    }

    private static void HandleServerReady()
    {
        _readyCount++;

        if (_readyCount >= _playerJoinCount)
        {
            foreach (var playerId in _playerIds)
            {
                _serverSpace.Put(playerId, "Instantiate", "Player");
            }
        }
    }

    private static void HandleServerInstantiate(ITuple tuple)
    {
        if (((string)tuple[2]).Contains("Player"))
        {
            _networkObjectOwners.Add(_currentId, (string)tuple[0]);
            _idToObjectType.Add(_currentId, "NewPlayer");
            BroadcastPacket(new Packet(PacketType.Instantiate, "All", "Server", _currentId++ + "|" + (string)tuple[0] + "|" + "NewPlayer" + "|" + NetworkPackager.Package(_startPos[_playerSpawnCount]) + "|" + NetworkPackager.Package(Quaternion.identity)));
            _playerSpawnCount = (_playerSpawnCount + 1) % 4;
        }
        else
        {
            if (!((string)tuple[2]).Contains("Bullet"))
            {
                _networkObjectOwners.Add(_currentId, (string)tuple[0]);
                _idToObjectType.Add(_currentId, (string)tuple[2]);
            }

            BroadcastPacket(new Packet(PacketType.Instantiate, "All", "Server", _currentId++ + "|" + (string)tuple[0] + "|" + ((string)tuple[2]).Replace(".", ",")));
        }
    }

    private static void HandleServerJoin(ITuple tuple)
    {
        Debug.Log("Player " + tuple[2] + " Joined");

        if (_playerJoinCount >= _maxPlayerCount)
        {
            _serverSpace.Put((string)tuple[2], "Join", 0);
            Debug.Log("Player " + tuple[2] + " was denied because server is too full");
            return;
        }

        _playerJoinCount++;

        _playerIds.Add((string)tuple[2]);

        ISpace newPlayerSpace = new SequentialSpace();

        _repository.AddSpace((string)tuple[2], newPlayerSpace);
        _playerSpaces.Add((string)tuple[2], newPlayerSpace);
        _serverSpace.Put((string)tuple[2], "Join", 1);


        foreach (var objs in _networkObjectOwners)
        {
            SendPacket(new Packet(PacketType.Instantiate, objs.Value, "Server", objs.Key + "|" + objs.Value + "|" + _idToObjectType[objs.Key] + "|" + NetworkPackager.Package(Vector3.zero) + "|" + NetworkPackager.Package(Quaternion.identity)), (string)tuple[2]);
        }
    }

    #endregion

    #region Shared/Utility Methods
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #endregion

    #endregion

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
    Movement,
    Instantiate,
    Health,
    Ready,
    Destroy,
    Restart
}