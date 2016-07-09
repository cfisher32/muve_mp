using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class NetworkLoadLevel : MonoBehaviour {

    private int lastLevelPrefix = 0;
    private NetworkMasterServer mgs;

    void Awake()
    {
        DontDestroyOnLoad(this);
        GetComponent<NetworkView>().group = 1;

        mgs = GameObject.Find("MasterServerMenu").GetComponent<NetworkMasterServer>() as NetworkMasterServer;
    }

    void Update()
    {
        if(Network.peerType != NetworkPeerType.Disconnected && mgs.gamemenustate == NetworkMasterServer.menustate.networklobby && Network.isServer)
        {
            Network.RemoveRPCsInGroup(0);
            Network.RemoveRPCsInGroup(1);

            GetComponent<NetworkView>().RPC("LoadLevel", RPCMode.AllBuffered, "TestGameLevel", lastLevelPrefix + 1);
        }
    }

    [RPC]
    IEnumerator LoadLevel(string level, int levelPrefix)
    {
        if(mgs.gamemenustate != NetworkMasterServer.menustate.ingame)
        {
            mgs.gamemenustate = NetworkMasterServer.menustate.ingame;
            Debug.Log("Loading level " + level + "with prefix " + levelPrefix);
            lastLevelPrefix = levelPrefix;

            Network.SetSendingEnabled(0, false);
            Network.isMessageQueueRunning = false;
            Network.SetLevelPrefix(levelPrefix);

            Application.LoadLevel(level);

            yield return 0; //new WaitForSeconds(1);

            Network.isMessageQueueRunning = true;
            Network.SetSendingEnabled(0, true);

            GameObject go = GameObject.Find("PlayerSpawn");
            InstantiatePlayer io = (InstantiatePlayer)go.GetComponent(typeof(InstantiatePlayer));

            io.SendMessage("InstantiatePlayerOnNetworkLoadedLevel", SendMessageOptions.RequireReceiver);
        }
    }

    void OnDisconnectedFromServer()
    {
        Application.LoadLevel("MasterGameServerLobby");
    }
}
