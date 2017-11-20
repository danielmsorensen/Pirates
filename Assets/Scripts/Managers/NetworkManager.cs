using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

[RequireComponent(typeof(PhotonView))]
public class NetworkManager : PunBehaviour {

    public static int seed { get; private set; }

    public static System.Action OnJoinedGame;

    public static Player localPlayer;

    static NetworkManager main;

    public static bool inRoom {
        get {
            return PhotonNetwork.inRoom;
        }
    }

    void Awake() {
        if(main != null) {
            Destroy(gameObject);
        }
        main = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        PhotonNetwork.ConnectUsingSettings(Application.version);
    }
    
    public static void JoinGame() {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
        PhotonNetwork.CreateRoom("",
            new RoomOptions() {
                CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {
                    { "seed", Random.Range(int.MinValue, int.MaxValue) }
                }
            },
            new TypedLobby() {
                Name = null,
                Type = LobbyType.Default
            }
        );
    }

    public override void OnJoinedRoom() {
        seed = (int)PhotonNetwork.room.CustomProperties["seed"];
        if (OnJoinedGame != null) {
            OnJoinedGame.Invoke();
        }
    }

    public static Player SpawnPlayer() {
        localPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0).GetComponent<Player>();
        return localPlayer;
    }
}
