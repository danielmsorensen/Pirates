using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

    public void Play() {
        NetworkManager.JoinGame();
    }

    public void Options() {

    }

    public void Quit() {
        SceneSwitcher.Quit();
    }

    void OnJoinedGame() {
        SceneSwitcher.Play();
    }

    void OnEnable() {
        NetworkManager.OnJoinedGame += OnJoinedGame;
    }
    void OnDisable() {
        NetworkManager.OnJoinedGame -= OnJoinedGame;
    }
}
