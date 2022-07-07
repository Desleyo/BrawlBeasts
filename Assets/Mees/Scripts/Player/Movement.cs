using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Movement : MonoBehaviourPunCallbacks {

    //movement
    [Header("Movement Settings")]
    public float speed = 10.0F;
    public float movementLerp = 5.0F;
    public float movementLerpOnIce = 1.0F;
    public float movmentMovingOnIce = 2.5F;

    [Header("Boost Settings")]
    public float boostSpeed;
    public float boostDurationLeft;

    [Header("Input Block Settings")]
    public float blockInputDuration = 1.25f;
    public float blockInputDurationLeft;
    private float stunTimeLeft;


    [Header("Dash Settings")]
    public bool dashing = false;
    public float dashSpeed = 10.0F;
    public float dashDuration = 0.3F;
    public float dashCooldown = 2.0F;
    public AnimationCurve dashAnimation;
    public float power = 5;
    public GameObject dashParticleObj;

    private float currentDashTimeLeft;
    private float dashCooldownLeft;
    private float dashAnimationCD = 0.5F;

    private bool shouldDash;
    private float dashAnimationDelay;

    [Header("Jump Settings")]
    public float jumpHeight = 5.0F;

    private bool onGround = true, previousOnGround;

    //jump -> animation
    private bool shouldJump;
    private float jumpAnimationDelay;
    private float jumpDelay;

    [Header("Rotation Settings")]
    public float rotationLerpSpeed = 10.0F;

    //animation
    private Animator animator;

    [Header("Misc Settings")]
    public GameObject particle;
    public GameObject flameParticle, stunParticle;

    private GameObject spawned;

    public LayerMask layer;
    private Rigidbody body;
    private CapsuleCollider capsuleCollider;
    private Vector3 previousVelocity;
    private float wtdAnim;

    [HideInInspector] public Sound walkingSound;
    private Sound dashSound, jumpSound, onhitSound, stunSound, fartSound;

    private bool shouldFart;
    private bool stunned;

    public readonly List<Collision> ice = new List<Collision>();
    public PlayerInfo playerInfo;
    [HideInInspector] public float knockbackMultiplier = 1.0F;

    void Start() {
        body = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerInfo = GetComponent<PlayerInfo>();
        animator = GetComponentInChildren<Animator>();
        walkingSound = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Walking"));
        dashSound = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Dash"));
        jumpSound = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Jump"));
        onhitSound = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Player On Hit"));
        stunSound = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Player Stunned"));
        fartSound = SoundManager.Instance.CreateCopy(SoundManager.Instance.GetSoundByName("Fart"));

    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownLeft <= 0 && photonView.IsMine && GameModeManager.manager.gameStarted) {
            this.DoDash(false, false);
        }

        //tijd afnemen van de cooldowns
        float deltaTime = Time.deltaTime;
        boostDurationLeft = Mathf.Max(boostDurationLeft - deltaTime, 0);
        dashCooldownLeft = Mathf.Max(dashCooldownLeft - deltaTime, 0);
        blockInputDurationLeft = Mathf.Max(blockInputDurationLeft - deltaTime, 0);
        currentDashTimeLeft = Mathf.Max(currentDashTimeLeft - deltaTime, 0);
        jumpAnimationDelay = Mathf.Max(jumpAnimationDelay - deltaTime, 0);
        dashAnimationDelay = Mathf.Max(dashAnimationDelay - deltaTime, 0);
        jumpDelay = Mathf.Max(jumpDelay - deltaTime, 0);
        stunTimeLeft = Mathf.Max(stunTimeLeft - deltaTime, 0);

        wtdAnim = dashAnimationDelay == 0 ? (1 / (dashCooldown - dashDuration) * dashCooldownLeft) : 1 - (1 / dashAnimationCD * dashAnimationDelay);
        if (dashAnimationDelay == 0 && shouldFart) {
            GetComponent<PlayerInfo>().fartParticle.SetActive(true);
            GetComponent<PlayerInfo>().fartParticle.GetComponent<ParticleSystem>().Play();
            shouldFart = false;
            fartSound.Play();
        }

        //Debug.Log(wtdAnim);
        animator.SetFloat("Walk to dash", 1.5F * wtdAnim);

        if (stunTimeLeft != 0) {
            if (stunned) {
                animator.SetBool("Stunned", stunTimeLeft != 0);
            } else {
                animator.SetBool("Stunned", false);
                animator.SetTrigger("Knockback");
            }
        } else {
            animator.SetBool("Stunned", false);
        }

        flameParticle.SetActive(boostDurationLeft != 0 && boostSpeed > 1.0F);
        stunParticle.SetActive(stunned && stunTimeLeft != 0);
        dashParticleObj.SetActive(wtdAnim != 0);

        previousVelocity.y = body.velocity.y;
        if (blockInputDurationLeft == 0 && (boostSpeed != 0.0F || boostDurationLeft == 0.0F)) {
            body.velocity = previousVelocity;
        }


        if (stunned) {
            if (stunTimeLeft != 0) {
                stunSound.Play();
            } else {
                stunSound.Stop();
            }
        }

    }

    void FixedUpdate() {
        if (!photonView.IsMine || !GameModeManager.manager.gameStarted)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 currentMovement = Vector3.zero;

        if (blockInputDurationLeft == 0) {

            if (boostSpeed != 0.0F || boostDurationLeft == 0.0F) {
                //Movement
                if (dashAnimationDelay == 0 && shouldDash && (!shouldJump)) {
                    this.currentDashTimeLeft = dashDuration;
                    shouldDash = false;
                }

                if (currentDashTimeLeft == 0) {
                    //Horizontal & Vertical Movement
                    currentMovement += vertical * Vector3.forward;
                    currentMovement += horizontal * Vector3.right;
                    currentMovement = currentMovement.normalized;
                    currentMovement *= boostDurationLeft > 0 && onGround ? boostSpeed : speed;

                    Vector3 divided = currentMovement * Time.fixedDeltaTime;
                    if (Physics.OverlapCapsule(capsuleCollider.bounds.center + divided, new Vector3(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y + capsuleCollider.radius + 0.1F, capsuleCollider.bounds.center.z) + divided, capsuleCollider.radius, layer).Length > 0) {
                        Vector3 down = Vector3.zero;
                        down.y = body.velocity.y;

                        body.velocity = down;
                        currentMovement = down;
                        return;
                    }

                    //Gravity & Jump
                    currentMovement.y = body.velocity.y;
                    onGround = Physics.OverlapCapsule(capsuleCollider.bounds.center, new Vector3(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y + capsuleCollider.radius - 0.1F, capsuleCollider.bounds.center.z), capsuleCollider.radius, layer).Length > 0;

                    if (onGround && !previousOnGround) {
                        spawned = PhotonNetwork.Instantiate(particle.name, transform.position, Quaternion.identity);
                        Invoke(nameof(DestroyEffect), 0.7f);
                    }

                    if (onGround && Input.GetButton("Jump") && jumpDelay == 0 && wtdAnim == 0.0F) {
                        shouldJump = true;
                        jumpAnimationDelay = 0.75F;
                        jumpDelay = 2.0F;

                        //fix
                        animator.SetTrigger("Jump");
                    }

                    if (shouldJump && jumpAnimationDelay == 0) {
                        Vector3 vCopy = body.velocity;
                        vCopy.y = jumpHeight;
                        body.velocity = vCopy;
                        shouldJump = false;

                        jumpSound.Play();
                    }
                    previousOnGround = onGround;

                } else {
                    currentMovement = transform.forward * dashAnimation.Evaluate(dashDuration - currentDashTimeLeft) * dashSpeed;
                }

                bool moving = (vertical != 0 || horizontal != 0);

                body.velocity = Vector3.Lerp(body.velocity, currentMovement, Time.fixedDeltaTime * (ice.Count == 0 || currentDashTimeLeft != 0 ? movementLerp : moving ? movmentMovingOnIce : movementLerpOnIce));
                dashing = currentDashTimeLeft != 0;

                //Rotation
                Vector3 copy = body.velocity;
                copy.y = 0;

                if (moving) {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(copy), Time.deltaTime * rotationLerpSpeed);
                    if (onGround) {
                        walkingSound.Play();
                    }
                }

                animator.SetBool("Input Walking", moving);
            } else {
                if (onGround) {
                    Vector3 velocity = body.velocity;
                    velocity.x = velocity.z = 0;
                    body.velocity = velocity;
                }
                animator.SetBool("Input Walking", false);
            }


        } else {
            animator.SetBool("Input Walking", false);
        }

        previousVelocity = body.velocity;
    }

    private void OnCollisionEnter(Collision collision) {
        if (!photonView.IsMine)
            return;

        if (collision.transform.CompareTag("Ice")) {
            ice.Add(collision);
        } else if (collision.transform.CompareTag("Player")) {
            int otherId = collision.gameObject.GetComponent<PhotonView>().ViewID;
            GameModeManager.manager.currentGameMode.GetComponent<GameMode>().OnPlayerCollide(photonView.ViewID, otherId);

            Brawl brawl = GameModeManager.manager.currentGameMode.GetComponentInChildren<Brawl>();
            if (dashing && brawl != null && brawl.gameObject.activeSelf) {
                collision.gameObject.GetComponent<Inventory>().photonView.RPC("DropItem", RpcTarget.All);
            }

        }
    }

    private void OnCollisionExit(Collision collision) {


        if (collision.transform.CompareTag("Ice")) {
            ice.Remove(collision);
        }
    }

    public void DoDash(bool isCalled, bool shouldFart) {
        if (onGround) {
            Invoke(nameof(ResetDash), dashDuration);
            this.shouldFart = shouldFart;
            this.dashCooldownLeft = dashCooldown;
            this.dashAnimationDelay = dashAnimationCD;
            dashing = shouldDash = true;
            dashSound.Play();

            if (!isCalled)
                photonView.RPC("CallDash", RpcTarget.Others, photonView.ViewID, shouldFart);
        }
    }

    [PunRPC]
    void CallDash(int playerId, bool shouldFart) {
        Movement playerMove = PhotonView.Find(playerId).GetComponent<Movement>();
        playerMove.DoDash(true, shouldFart);
        dashSound.Play();
    }

    void ResetDash() {
        dashing = false;
    }

    public void ResetMovement() {
        ice.Clear();
        body.velocity = Vector3.zero;
        onGround = previousOnGround = true;
    }

    [PunRPC]
    public void ApplyKnockback(int objectId, float power) {
        this.stunned = false;
        if (!photonView.IsMine)
            return;

        GameObject player = PhotonView.Find(objectId).gameObject;
        power *= knockbackMultiplier;

        //gameMode.OnPlayerCollide(player);
        photonView.RPC("OnHitSound", RpcTarget.All);

        if (blockInputDurationLeft == 0) {
            animator.SetTrigger("Knockback");
            Vector3 difference = (transform.position - player.transform.position).normalized * power;
            difference.y = body.velocity.y;
            body.velocity = difference;
            blockInputDurationLeft = blockInputDuration;

            RespawnBlink blink = GetComponent<RespawnBlink>();
            blink.photonView.RPC("StartAnimation", RpcTarget.All, false, blockInputDuration, blockInputDuration, true);


            Brawl brawl = GameModeManager.manager.currentGameMode.GetComponentInChildren<Brawl>();
            if (brawl != null && brawl.gameObject.activeSelf) {
                if (!player.CompareTag("Player")) {
                    foreach (GameObject playerr in GameObject.FindGameObjectsWithTag("Player")) {
                        if (playerr.GetComponent<PhotonView>().OwnerActorNr == player.GetComponent<PhotonView>().Owner.ActorNumber) {
                            brawl.photonView.RPC("OnCombatTag", RpcTarget.All, photonView.ViewID, playerr.GetComponent<PhotonView>().ViewID);
                            Debug.Log("b4 - " + playerr.GetComponent<PhotonView>().ViewID);
                            return;
                        }
                    }
                } else {
                    brawl.photonView.RPC("OnCombatTag", RpcTarget.All, photonView.ViewID, player.GetComponent<PhotonView>().ViewID);
                }

            }
        }
    }

    [PunRPC]
    public void ApplyStun(float duration) {
        this.stunTimeLeft = this.blockInputDurationLeft = duration;
        this.stunned = true;
    }

    [PunRPC]
    public void ApplySpeedBoost(float boostSpeed, float boostDurationLeft) {
        //TODO return bool
        if (boostDurationLeft > this.boostDurationLeft) {
            this.boostSpeed = boostSpeed;
            this.boostDurationLeft = boostDurationLeft;
        }
    }

    [PunRPC]
    public void OnHitSound() {
        onhitSound.Play();
    }

    void DestroyEffect() {
        PhotonNetwork.Destroy(spawned);
    }

}

