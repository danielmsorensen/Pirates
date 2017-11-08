using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class NetworkManager : PunBehaviour {

    void Start() {
        PhotonNetwork.ConnectUsingSettings(Application.version);
    }

    public override void OnJoinedLobby() {
        print("Joined Lobby");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
        print("No Room Found");
        print("Creating Room");
        PhotonNetwork.CreateRoom("");
    }

    public override void OnJoinedRoom() {
        print("Joined Room");
    }
}
