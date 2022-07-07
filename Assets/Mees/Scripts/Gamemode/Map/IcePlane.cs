using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcePlane : MonoBehaviour
{
    public float distancePerPlayer = 1.0F;
    public float recoveryDistance = 1.0F;
    public GameObject player;

    private Animator animator;
    private GameObject particles;

    private bool isMainPlayerCollided;

    private Vector3 startingPosition, moveToPosition;

    private void Start() {
        animator = GetComponent<Animator>();
        startingPosition = moveToPosition = transform.position;
        particles = GetComponentInChildren<ParticleSystem>().transform.parent.gameObject;
    }

    private void Update() {
        if (isMainPlayerCollided) {
            if (Vector3.Distance(transform.position, player.transform.position) > 10) {
                isMainPlayerCollided = false;
            }
            moveToPosition.y -= distancePerPlayer * Time.deltaTime;
        } else {
            moveToPosition.y = Mathf.MoveTowards(transform.position.y, startingPosition.y, recoveryDistance * Time.deltaTime);
        }

        Vector3 difference = moveToPosition - transform.position;

        if (player != null) {
            player.transform.position += difference;
        }

        transform.position = moveToPosition;

        particles.SetActive(isMainPlayerCollided);
        animator.SetBool("OnContact", isMainPlayerCollided);

    }

    private void OnCollisionEnter(Collision other) {
        if (other.transform.tag == "Player") {
            isMainPlayerCollided = true;
            player = other.gameObject;
        }
    }

    private void OnCollisionExit(Collision other) {
        if (other.transform.tag == "Player") {
            isMainPlayerCollided = false;
            player = null;
        }
    }

}
