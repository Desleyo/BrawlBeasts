using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ("Reusable"), menuName = "Item/New Knuppel")]
public class Knuppel : ItemReusable {

    private Animator animator;
    private Sound swingSound;

    protected override void OnUseItem(GameObject user) {
        if (animator == null) {
            animator = user.transform.root.GetComponentInChildren<Animator>();
        }
        animator.SetTrigger("Swing Bat");

        if (swingSound == null) {
            swingSound = SoundManager.Instance.GetSoundByName("Knuppel Swing");
        }
        swingSound.Play();

    }

}
