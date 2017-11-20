using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneSwitcher : MonoBehaviour {

    [SerializeField]
    GameObject loadingScreen;
    [SerializeField]
    public Slider progressBar;
    [SerializeField]
    public TMP_Text percentText;

    public static bool inGame { get; private set; }
    static bool _inGame;

    static SceneSwitcher main;

    void Awake() {
        if(main != null) {
            Destroy(gameObject);
        }
        main = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void Play() {
        main.StartCoroutine(main.LoadScene(1));
        _inGame = true;
    }
    public static void MainMenu() {
        main.StartCoroutine(main.LoadScene(0));
        _inGame = false;
    }
    public static void Quit() {
        main.StartCoroutine(main.LoadScene(-1));
    }

    IEnumerator LoadScene(int sceneIndex) {
        if (sceneIndex == -1) {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else {
            AsyncOperation opp = SceneManager.LoadSceneAsync(sceneIndex);
            loadingScreen.SetActive(true);

            while(!opp.isDone) {
                float p = opp.progress / 0.9f * 100f;

                if (progressBar != null) {
                    progressBar.value = p / 100f;
                }
                if(percentText != null) {
                    percentText.text = p.ToString() + "%";
                }

                yield return null;
            }

            loadingScreen.gameObject.SetActive(false);
        }

        yield return null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        inGame = _inGame;
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
