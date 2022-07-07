using UnityEngine;

public class Periscope : MonoBehaviour
{
    public float minimumTime, maximumTime;

    private Animator animator;

    private float currentTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        this.SetRandomTime();
    }

    void Update()
    {

        currentTime -= Time.deltaTime;
        if (currentTime <= 0) {
            animator.SetTrigger("Action");
            this.SetRandomTime();
        }
    }


    private void SetRandomTime() {
        currentTime = Random.Range(minimumTime, maximumTime);
    }

}
