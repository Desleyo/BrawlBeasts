using Photon.Pun;
using UnityEngine;

public class KingOfTheHill : GameMode {

    public int circleDiameter = 3;

    public float timeBetweenCircleChanges = 5.0F;

    public Transform[] circlePlaces;
    public Transform currentPlace;

    public Item knuppel;

    private float circleChangeTimeLeft;

    private Sound hillSound;

    void OnEnable() {
        Initiate();
        hillSound = SoundManager.Instance.GetSoundByName("New Hill Location");
        circlePlaces = GameModeManager.manager.currentMap.GetComponent<MapInfo>().circlePositions;
        circleChangeTimeLeft = 3.0F;

        foreach (GameObject player in players) {
            if (player.GetComponent<PhotonView>().IsMine) {
                player.GetComponent<Inventory>().SetCurrentItem(knuppel);
                return;
            }
        }
    }

    public override void OnGameUpdate() {
        if (currentPlace)
        {
            foreach (GameObject player in players)
            {
                float distX = player.transform.position.x - currentPlace.position.x;
                float distZ = player.transform.position.z - currentPlace.position.z;
                float dist = Mathf.Sqrt(distX * distX + distZ * distZ);

                if (dist <= circleDiameter)
                {
                    if (!leaderBoard.ContainsKey(player))
                        leaderBoard.Add(player, 0);

                    leaderBoard[player] += Time.deltaTime;
                    UpdateScoreBoard(player);
                }
            }

        }

        circleChangeTimeLeft -= Time.deltaTime;
        if (circleChangeTimeLeft <= 0 && PhotonNetwork.IsMasterClient) {
            photonView.RPC("SetNewRandomCircle", RpcTarget.All, Random.Range(0, circlePlaces.Length));
        }
    }

    [PunRPC]
    private void SetNewRandomCircle(int randomPos) {
        if (players.Length == 1)
            players = GameObject.FindGameObjectsWithTag("Player");

        foreach (Transform place in circlePlaces)
            place.gameObject.SetActive(false);

        currentPlace = circlePlaces[randomPos];
        circlePlaces[randomPos].gameObject.SetActive(true);
        circleChangeTimeLeft = timeBetweenCircleChanges;

        hillSound.Play();
    }

    [PunRPC]
    void AddTime(float addedTime)
    {
        if (gameDuration == Mathf.Infinity)
        {
            gameDuration = addedTime;
            startTime = Time.time;
        }
        else
            gameDuration += addedTime;

        timeBarSlider.maxValue = gameDuration;

        timeBarSlider.enabled = gameDuration == Mathf.Infinity ? false : true;
    }
}
