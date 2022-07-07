using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public float transitionSpeed = 2;

    [Space]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject roomsMenu;
    [SerializeField] GameObject createMenu;

    CanvasGroup mainCanvasGroup;
    CanvasGroup settingsCanvasGroup;
    CanvasGroup roomsCanvasGroup;
    CanvasGroup createCanvasGroup;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        mainCanvasGroup = mainMenu.GetComponent<CanvasGroup>();
        settingsCanvasGroup = settingsMenu.GetComponent<CanvasGroup>();
        roomsCanvasGroup = roomsMenu.GetComponent<CanvasGroup>();
        createCanvasGroup = createMenu.GetComponent<CanvasGroup>();

        SoundManager.Instance.SetCurrentMusic(SoundManager.Instance.GetSoundByName("Main Menu Music"));
        Debug.Log("Play it");

        StartCoroutine(InitialTransition());
    }

    IEnumerator InitialTransition()
    {
        yield return new WaitForSeconds(1);

        float alpha = mainCanvasGroup.alpha;
        while(mainCanvasGroup.alpha < 1)
        {
            alpha += Time.deltaTime * transitionSpeed;
            mainCanvasGroup.alpha = alpha;
            yield return null; 
        }
    }

    public void OpenRoomList(bool opening)
    {
        StopAllCoroutines();
        StartCoroutine(Transition(opening, mainCanvasGroup, roomsCanvasGroup));
    }
    public void OpenCreateMenu(bool opening)
    {
        StopAllCoroutines();
        StartCoroutine(Transition(opening, roomsCanvasGroup, createCanvasGroup));
    }
    public void OpenSettings(bool opening)
    {
        StopAllCoroutines();
        StartCoroutine(Transition(opening, mainCanvasGroup, settingsCanvasGroup));
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator Transition(bool opening, CanvasGroup option1, CanvasGroup option2)
    {
        CanvasGroup group1 = opening ? option1 : option2;
        CanvasGroup group2 = opening ? option2 : option1;
        float alpha;

        alpha = group1.alpha;
        while (group1.alpha > 0)
        {
            alpha -= Time.deltaTime * transitionSpeed;
            group1.alpha = alpha;
            yield return null;
        }
        group1.gameObject.SetActive(false);

        alpha = group2.alpha;
        group2.gameObject.SetActive(true);
        while (group2.alpha < 1)
        {
            alpha += Time.deltaTime * transitionSpeed;
            group2.alpha = alpha;
            yield return null;
        }
    }
}
