using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VignetteTransition : MonoBehaviour
{
    public static VignetteTransition manager;

    [SerializeField] Animator animator;
    [SerializeField] GameObject mask;
    [SerializeField] GameObject placeHolder;

    private void Start()
    {
        manager = this;
        Invoke(nameof(WaitToTransition), .3f);
    }

    void WaitToTransition()
    {
        mask.SetActive(true);
        placeHolder.SetActive(false);
    }

    public void PlayTransition(bool fadeIn)
    {
        string trigger = fadeIn ? "FadeIn" : "FadeOut";
        animator.SetTrigger(trigger);
    }
}
