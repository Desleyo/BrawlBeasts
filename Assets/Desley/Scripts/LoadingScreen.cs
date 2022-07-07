using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] GameObject[] mapImages;
    [SerializeField] string[] gameModeTitles;
    [TextArea(6, 5), SerializeField] string[] descriptions;

    List<string> usefulTips;
    #region tips
    //General Tips
    [Header("General Tips")]
    [TextArea(1, 3), SerializeField] string[] generalTips;

    //GameMode Tips
    [Header("GameMode tips")]
    [TextArea(1, 3), SerializeField] string[] tailGrabberTips;
    [TextArea(1, 3), SerializeField] string[] wackAMoleTips;
    [TextArea(1, 3), SerializeField] string[] kingOfTheHillTips;
    [TextArea(1, 3), SerializeField] string[] brawlTips;

    //Map tips
    [Header("Map tips")]
    [TextArea(1, 3), SerializeField] string[] antarcticaTips;
    [TextArea(1, 3), SerializeField] string[] zooTips;
    [TextArea(1, 3), SerializeField] string[] japantownTips;
    #endregion

    [Space]
    [SerializeField] TextMeshProUGUI gameModeText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI tipsText;
    [SerializeField] TextMeshProUGUI roundStatusText;
    [SerializeField] GameObject devToolsText;

    GameObject currentMap;

    public void SetupLoadingScreen(int modeIndex, int mapIndex, int currentRound, int maxRounds)
    {
        if (currentMap)
            currentMap.SetActive(false);

        SoundManager.Instance.SetCurrentMusic(SoundManager.Instance.GetSoundByName("Loading Screen Music"));
        currentMap = mapImages[mapIndex];
        currentMap.SetActive(true);
        gameModeText.text = gameModeTitles[modeIndex];
        descriptionText.text = descriptions[modeIndex];
        roundStatusText.text = "Round " + (currentRound + 1) + "/" + maxRounds;

        if (GameModeManager.manager.devTools && PhotonNetwork.IsMasterClient)
            devToolsText.SetActive(true);

        SetupTipList(modeIndex, mapIndex);

        StartCoroutine(ShowTips());
    }

    void SetupTipList(int modeIndex, int mapIndex)
    {
        usefulTips = new List<string>();

        foreach (string tip in generalTips)
            usefulTips.Add(tip);
        foreach (string tip in ModeTips(modeIndex))
            usefulTips.Add(tip);
        foreach (string tip in MapTips(mapIndex))
            usefulTips.Add(tip);
    }

    IEnumerator ShowTips()
    {
        int firstTipIndex = Random.Range(0, usefulTips.Count);
        tipsText.text = usefulTips[firstTipIndex];

        yield return new WaitForSeconds(GameModeManager.manager.transitionTime / 2);
        usefulTips.RemoveAt(firstTipIndex);

        int secondTipIndex = Random.Range(0, usefulTips.Count);
        tipsText.text = usefulTips[secondTipIndex];
    }

    string[] ModeTips(int modeIndex)
    {
        return 
            modeIndex == 0 ? tailGrabberTips :
            modeIndex == 1 ? wackAMoleTips :
            modeIndex == 2 ? kingOfTheHillTips :
            modeIndex == 3 ? brawlTips :
            null;
    }

    string[] MapTips(int mapIndex)
    {
        return
            mapIndex == 0 ? antarcticaTips :
            mapIndex == 1 ? zooTips :
            mapIndex == 2 ? japantownTips :
            null;
    }
}
