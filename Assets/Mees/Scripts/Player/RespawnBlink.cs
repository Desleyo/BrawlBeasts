using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RespawnBlink : MonoBehaviourPunCallbacks {
    public float totalDuration = 2.0F;
    public float blinkDuration = 1.0F;

    private SkinnedMeshRenderer bodyRenderer, tailRenderer, wingRenderer;
    public Animator animator;

    private Color blinkColor;
    private float index;

    public PlayerInfo playerInfo;

    private Color previousBody, previousTail, previousWing;

    private void OnEnable() {
        index = 0;

        playerInfo = gameObject.GetComponent<PlayerInfo>();
        bodyRenderer = playerInfo.bodyRenderer;
        previousBody = bodyRenderer.material.color;

        tailRenderer = playerInfo.tailRenderer;
        previousTail = tailRenderer.material.color;

        if (playerInfo.wingRenderer) {
            wingRenderer = playerInfo.wingRenderer;
            previousWing = wingRenderer.material.color;
        }
    }

    private void OnDisable() {
        bodyRenderer.material.color = previousBody;
        tailRenderer.material.color = previousTail;
        if (wingRenderer) {
            wingRenderer.material.color = previousWing;
        }
    }

    void Update() {
        index = Mathf.Min(index + Time.deltaTime, totalDuration);

        float modulo = (index / blinkDuration) % 1;
        float val = modulo <= 0.5F ? modulo * 2.0F : 1.0F - modulo;

        bodyRenderer.material.color = ColorUtils.combineColors(previousBody, blinkColor, val);
        tailRenderer.material.color = ColorUtils.combineColors(previousTail, blinkColor, val);
        if (wingRenderer) {
            wingRenderer.material.color = ColorUtils.combineColors(previousWing, blinkColor, val);
        }


        if (index >= totalDuration) {
            this.enabled = false;
            return;
        }
    }

    [PunRPC]
    public void StartAnimation(bool black, float totalDuration, float blinkDuration, bool stunned) {
        this.blinkColor = black ? Color.black : Color.red;
        this.totalDuration = totalDuration;
        this.blinkDuration = blinkDuration;
        //this.stunned = stunned;
        this.enabled = true;
    }

}
