using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PeriscopeManager : MonoBehaviour {

    public GameObject[] periscopes;
    public float minimumTime, maximumTime;
    private float currentTime;

    private List<GameObject> random = new List<GameObject>();

    void Start() {
        this.SetRandomTime();
    }

    void Update() {
        if (random.Count == 0) {
            random = periscopes.OrderBy(x => Random.Range(0.0F, 1.0F)).ToList();
        }

        currentTime -= Time.deltaTime;
        if (currentTime <= 0) {
            random[0].GetComponent<Animator>().SetTrigger("Action");
            this.SetRandomTime();
            random.RemoveAt(0);

        }
    }


    private void SetRandomTime() {
        currentTime = Random.Range(minimumTime, maximumTime);
    }

}
