using UnityEngine;
using System.Collections;

public class InstantiatePlayer : MonoBehaviour {

    public Transform PlayerAvatar;
    public Transform[] spawn;

    void InstantiatePlayerOnNetworkLoadedLevel()
    {
        Transform spawnarea;
        int spawnpoint = (int)Mathf.Round(Random.Range(0.0f, 1.0f));

        Debug.Log("Spawning at: " + spawnpoint.ToString());

        spawnarea = spawn[spawnpoint];

        GameObject go = GameObject.Find("Cube");

        Debug.Log("Instantiate a new player");

        spawnarea.LookAt(new Vector3(go.transform.position.x, spawnarea.position.y, go.transform.position.z));

        Network.Instantiate(PlayerAvatar, spawnarea.position, spawnarea.rotation, 0);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Server destroying player");

        Network.RemoveRPCs(player, 0);
        Network.DestroyPlayerObjects(player);


    }
}
