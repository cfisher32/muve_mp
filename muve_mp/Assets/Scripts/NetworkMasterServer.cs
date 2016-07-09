using UnityEngine;
using System.Collections;

public class NetworkMasterServer : MonoBehaviour {

    public enum menustate
    {
        networklobby,
        ingame,
    }

    public menustate gamemenustate = menustate.networklobby;
    public string gameType = "SambpleUnityNetworkingDemo v1.0";
    private string gameName = "LetsPlay";
    public int serverPort = 25002;
    private float lastHostListRequest = -1000.0f;
    private float hostListRefreshTimeout = 10.0f;
    private ConnectionTesterStatus natCapable = ConnectionTesterStatus.Undetermined;
    private bool filterNATHosts = false;
    private bool probingPublicIP = false;
    private bool doneTesting = false;
    private float timer = 0.0f;
    private string testMessage = "Testing NAT capablities";
    public GUIStyle format = new GUIStyle();
    private bool useNat = false;

    //MasterServer.dedicatedServer = true; //enable if not running client

    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Debug.Log(info);
    }

    void OnGUI()
    {
        ShowGUI();
    }
    
    void Awake()
    {
        DontDestroyOnLoad(this);

        natCapable = Network.TestConnection();

        if (Network.HavePublicAddress())
            Debug.Log("This machine has a PUBLIC IP Address");
        else
            Debug.Log("This machine has a PRIVATE IP address");

        gamemenustate = menustate.networklobby;
    }

    void Update()
    {
        if(!doneTesting)
        {
            TestConnection();
        }

        if(Time.realtimeSinceStartup > lastHostListRequest + hostListRefreshTimeout)
        {
            MasterServer.ClearHostList();

            MasterServer.RequestHostList(gameType);

            lastHostListRequest = Time.realtimeSinceStartup;

            Debug.Log("Refresh available GS List");
        }
    }

    void TestConnection()
    {
        natCapable = Network.TestConnection();

        switch(natCapable)
        {
            case ConnectionTesterStatus.Error:
                testMessage = "Problem determining NAT capabilities";
                doneTesting = true;
                break;
            case ConnectionTesterStatus.Undetermined:
                testMessage = "Testing NAT capabilities";
                doneTesting = false;
                break;
            case ConnectionTesterStatus.PublicIPIsConnectable:
                testMessage = "Directly connectable public IP address";
                useNat = false;
                doneTesting = true;
                break;
            case ConnectionTesterStatus.PublicIPPortBlocked:
                testMessage = "Non-connectable public IP address (port " + serverPort + " blocked) running a server is impossible.";
                useNat = false;

                if(!probingPublicIP)
                {
                    Debug.Log("Testing if firewall can be circumvented");

                    natCapable = Network.TestConnectionNAT();
                    probingPublicIP = true;
                    timer = Time.time + 10;
                }
                else if(Time.time > timer)
                {
                    probingPublicIP = false;
                    useNat = true;
                    doneTesting = true;
                }
                break;
            case ConnectionTesterStatus.PublicIPNoServerStarted:
                testMessage = "Public IP address but server not inititalized, it must be started to check server accessiblity. Restart connect test when ready.";
                doneTesting = true;
                break;
			case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
				testMessage = "Limited NAT punchthrough capabilities. Cannot " +
					"connect to all types of NAT servers. Running a server " +
					"is ill advised as not everyone can connect.";
				useNat = true;
				doneTesting = true;
				break;
			default:
                testMessage = "Error in test routine, got " + natCapable;

                if(string.Compare("Limited", 0, natCapable.ToString(), 0, 7) == 0)
                {
                    useNat = true;
                    doneTesting = true;
                    break;
                }
                break;
        }
        Debug.Log(testMessage);
    }

    void ShowGUI()
    {
        if(gamemenustate == menustate.ingame)
        {
            if(GUI.Button(new Rect(10,10,90,30), "Disconnect"))
            {
                Network.Disconnect();
                MasterServer.UnregisterHost();
                gamemenustate = menustate.networklobby;

                Application.LoadLevel("MasterGameServerLobby");
            }
        }
        else
        {
            if(gamemenustate == menustate.networklobby)
            {
                if(Network.peerType == NetworkPeerType.Disconnected)
                {
                    format.fontSize = 28;

                    GUI.Label(new Rect(((Screen.width / 2) - 80) * 0.2f, (Screen.height / 2) - 200, 400, 50), "Your game title here", format);
                    gameName = GUI.TextField(new Rect((((Screen.width / 2)) * 0.2f)+200, (Screen.height / 2) - 100, 200, 30), gameName);

                    if(GUI.Button(new Rect(((Screen.width/2)-100)*0.2f, (Screen.height/2)-100, 200,30),"Start Server"))
                    {
                        if(doneTesting)
                        {
                            Network.InitializeServer(32, serverPort, useNat);
                        }
                        else
                        {
                            Network.InitializeServer(32, serverPort, !Network.HavePublicAddress());
                            MasterServer.updateRate = 3;
                            MasterServer.RegisterHost(gameType, gameName, "This is early network testing for my game");
                        }

                        HostData[] data = MasterServer.PollHostList();
                        int _cnt = 0;

                        foreach(HostData gs in data)
                        {
                            if(!(filterNATHosts && gs.useNat))
                            {
                                string name = gs.gameName + " " + gs.comment + "(" + gs.connectedPlayers + "/" + gs.playerLimit + ")";

                                if(GUI.Button(new Rect(((Screen.width / 2) - 100) * 0.2f, (Screen.height/2)+(50 * _cnt),600, 30), name ))
                                {
                                    useNat = gs.useNat;

                                    if(useNat)
                                    {
                                        print("Using NAT punchthrough to connect");
                                        Network.Connect(gs.guid);
                                    }
                                    else
                                    {
                                        print("Connecting directly to host");
                                        Network.Connect(gs.ip, gs.port);
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
    }
}
