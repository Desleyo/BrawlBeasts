using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ("Reusable"), menuName = "Item/New Hammer")]
public class Hammer : ItemReusable {

    private Movement movement;
    private Animator animator;

    protected override void OnUseItem(GameObject user) {
        if (!animator) {
            animator = user.transform.root.GetComponentInChildren<Animator>();
        }

        if (!movement) {
            movement = user.transform.root.GetComponent<Movement>();
        }
        movement.ApplySpeedBoost(0, 0.75f);
        animator.SetTrigger("Hammer ARMS");
        animator.SetTrigger("Hammer Down");
    }
}
