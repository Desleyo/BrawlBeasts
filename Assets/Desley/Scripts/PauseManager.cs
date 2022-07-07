using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class PauseManager : MonoBehaviour
{
    public static PauseManager manager;
    public bool pauseMenuOpen;

    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject areUSurePanel;

    private void Awake() {
        manager = this;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (settingsPanel.activeSelf)
                Settings(!settingsPanel.activeSelf);
            else if (areUSurePanel.activeSelf)
                MainMenu(false);
            else
                Pause(!pausePanel.activeSelf);
        }
    }

    public void Pause(bool opening)
    {
        pausePanel.SetActive(opening);
        pauseMenuOpen = opening;

        if (opening)
            Cursor.lockState = CursorLockMode.None;
        else if (GameModeManager.manager.currentRound != -1)
            Cursor.lockState = CursorLockMode.Locked;
    }

    public void Settings(bool opening)
    {
        settingsPanel.SetActive(opening);
        pausePanel.SetActive(!opening);
    }

    public void MainMenu(bool confirmed)
    {
        if (!confirmed)
        {
            pausePanel.SetActive(areUSurePanel.activeSelf);
            areUSurePanel.SetActive(!pausePanel.activeSelf);
        }
        else
        {
            Invoke(nameof(WaitForTransition), 1f);
            areUSurePanel.SetActive(false);
        }
    }

    void WaitForTransition()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }
}
