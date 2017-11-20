using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public MapGenerator map;

    void Awake() {
        map.autoInit = true;
        if (NetworkManager.inRoom) {
            map.seed = NetworkManager.seed;
            map.autoSpawn = true;
        }
        else {
            map.autoSpawn = false;
            NetworkManager.JoinGame();
        }
    }

    void OnJoinedGame() {
        map.seed = NetworkManager.seed;
        map.SpawnInitialTiles(map.GetTile(NetworkManager.localPlayer.transform.position));
    }

    void OnEnable() {
        NetworkManager.OnJoinedGame += OnJoinedGame;
    }
    void OnDisable() {
        NetworkManager.OnJoinedGame -= OnJoinedGame;
    }
}
