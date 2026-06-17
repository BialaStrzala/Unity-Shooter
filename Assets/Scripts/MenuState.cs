using UnityEngine;
using PurrNet.StateMachine;
using System.Collections.Generic;
using PurrNet;

using PurrNet.Transports; // For ConnectionState

public class MenuState : StateNode
{
    [SerializeField] public Camera menuCamera;

    // Tracks which players have clicked "Join Match"
    private readonly HashSet<PlayerID> _readyPlayers = new();

    private UnityEngine.UI.Button _hostButton;
    private UnityEngine.UI.Button _joinButton;

    // Runtime connection configuration variables
    private bool _isEntered = false;
    private int _selectedTransport = 0; // 0 = Cloud Relay, 1 = UDP Direct
    private string _roomName = "unity3d21236";
    private string _ipAddress = "127.0.0.1";
    private string _portStr = "5000";

    private void Start()
    {
        _isEntered = true;

        // Programmatically bind buttons immediately on startup while offline
        var hostGo = GameObject.Find("Canvas/MenuView/HostMatchButton") ?? GameObject.Find("HostMatchButton");
        if (hostGo != null && hostGo.TryGetComponent(out _hostButton))
        {
            _hostButton.onClick.RemoveAllListeners();
            _hostButton.onClick.AddListener(OnHostMatchClicked);
        }

        var joinGo = GameObject.Find("Canvas/MenuView/JoinMatchButton") ?? GameObject.Find("JoinMatchButton");
        if (joinGo != null && joinGo.TryGetComponent(out _joinButton))
        {
            _joinButton.onClick.RemoveAllListeners();
            _joinButton.onClick.AddListener(OnJoinMatchClicked);
        }
    }

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        _isEntered = true;
        menuCamera.gameObject.SetActive(true);
        if (InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
            gameViewManager.ShowView<MenuView>();

        if (asServer)
        {
            _readyPlayers.Clear();
            Debug.Log("Entered MenuState (server)");
        }
    }

    private void ApplyNetworkSettings()
    {
        if (NetworkManager.main == null) return;

        if (_selectedTransport == 0)
        {
            // Cloud Relay (PurrTransport)
            var purrTransport = NetworkManager.main.GetComponent<PurrTransport>();
            if (purrTransport != null)
            {
                purrTransport.roomName = _roomName;
                NetworkManager.main.transport = purrTransport;
                Debug.Log($"[NETWORK] Configured PurrTransport Relay with room: {_roomName}");
            }
        }
        else
        {
            // UDP Direct (UDPTransport)
            var udpTransport = NetworkManager.main.GetComponent<UDPTransport>();
            if (udpTransport != null)
            {
                udpTransport.address = _ipAddress;
                if (ushort.TryParse(_portStr, out ushort port))
                {
                    udpTransport.serverPort = port;
                }
                NetworkManager.main.transport = udpTransport;
                Debug.Log($"[NETWORK] Configured UDPTransport Direct with target: {_ipAddress}:{_portStr}");
            }
        }
    }

    private void OnGUI()
    {
        if (!_isEntered) return;

        // Draw a small configuration box in the top-right corner of the screen
        Rect rect = new Rect(Screen.width - 270, 10, 260, 190);
        GUILayout.BeginArea(rect, "Multiplayer Setup", GUI.skin.box);
        
        GUILayout.Space(5);
        GUILayout.Label("Connection Method:");
        _selectedTransport = GUILayout.SelectionGrid(_selectedTransport, new string[] { "Cloud Relay", "UDP Direct (LAN)" }, 2);

        GUILayout.Space(5);
        if (_selectedTransport == 0)
        {
            GUILayout.Label("Relay Room Name:");
            _roomName = GUILayout.TextField(_roomName);
            GUILayout.Label("<color=grey>Tip: Use a unique room name to avoid collisions</color>", new GUIStyle(GUI.skin.label) { richText = true });
        }
        else
        {
            GUILayout.Label("Host IP Address (Client only):");
            _ipAddress = GUILayout.TextField(_ipAddress);
            GUILayout.Label("Host Port:");
            _portStr = GUILayout.TextField(_portStr);
        }

        GUILayout.Space(5);
        string connStateStr = NetworkManager.main != null ? NetworkManager.main.clientState.ToString() : "Offline";
        GUILayout.Label($"Network Status: <b>{connStateStr}</b>", new GUIStyle(GUI.skin.label) { richText = true });

        GUILayout.EndArea();
    }

    public void OnHostMatchClicked()
    {
        if (InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
            gameViewManager.ShowView<WaitForPlayersView>();

        if (NetworkManager.main != null)
        {
            if (NetworkManager.main.isClient && NetworkManager.main.isServer)
            {
                NotifyServerPlayerReady();
                return;
            }

            NetworkManager.main.StopClient();
            NetworkManager.main.StopServer();

            ApplyNetworkSettings();

            NetworkManager.main.onClientConnectionState += OnHostConnected;
            NetworkManager.main.StartHost();
        }
    }

    private void OnHostConnected(ConnectionState state)
    {
        if (state == ConnectionState.Connected)
        {
            NetworkManager.main.onClientConnectionState -= OnHostConnected;
            NotifyServerPlayerReady();
        }
    }

    /// Called by the MainMenuView button — runs on the local client.
    /// Notifies the server.
    public void OnJoinMatchClicked()
    {
        if (InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
            gameViewManager.ShowView<WaitForPlayersView>();

        if (NetworkManager.main != null)
        {
            if (NetworkManager.main.isClient && NetworkManager.main.clientState == ConnectionState.Connected)
            {
                NotifyServerPlayerReady();
                return;
            }

            NetworkManager.main.StopClient();
            NetworkManager.main.StopServer();

            ApplyNetworkSettings();

            NetworkManager.main.onClientConnectionState += OnClientConnected;
            NetworkManager.main.StartClient();
        }
    }

    private void OnClientConnected(ConnectionState state)
    {
        if (state == ConnectionState.Connected)
        {
            NetworkManager.main.onClientConnectionState -= OnClientConnected;
            NotifyServerPlayerReady();
        }
    }

    public void EnterMatch()
    {
        OnJoinMatchClicked();
    }

    /// Client → Server RPC. Registers the calling player as ready.
    /// Only the server processes this; it advances the state when everyone is ready.
    [ServerRpc(requireOwnership: false)]
    private void NotifyServerPlayerReady(RPCInfo info = default)
    {
        PlayerID sender = info.sender;

        if (_readyPlayers.Contains(sender))
            return;

        _readyPlayers.Add(sender);
        Debug.Log($"[MenuState] Player ready: {sender} — {_readyPlayers.Count}/{networkManager.players.Count}");

        // Only advance once every connected player has clicked Join
        if (_readyPlayers.Count >= networkManager.players.Count)
        {
            Debug.Log("[MenuState] All players ready — moving to next state");

            if (InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
                gameViewManager.ShowView<MainGameView>(true);

            machine.Next();
        }
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
        _isEntered = false;
        if (NetworkManager.main != null)
        {
            NetworkManager.main.onClientConnectionState -= OnHostConnected;
            NetworkManager.main.onClientConnectionState -= OnClientConnected;
        }
    }
}