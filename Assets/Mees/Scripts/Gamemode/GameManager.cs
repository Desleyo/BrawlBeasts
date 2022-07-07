using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int gameCount;
   /* public List<Sthng> pickFromThisList;
    public List<Sthng> gameModes;*/

    //idee van scores is dat als je 1e bent je een x aantal punten krijgt, calculation daarvoor: playerCount - index. Index is de plaats die ze hebben in de scoreboard dict van de GameMode
    public Dictionary<GameMode, float> scores;

    //gameModes[0] voor current gm

    /*private List<Sthng> GenerateGameList() {
        int count = 0;
        List<Sthng> copy = pickFromThisList;
        while (count < gameCount) {
            int rnd = Random.Range(0, copy.Count);
            gameModes.Add(copy[rnd]);
            copy.RemoveAt(rnd);
            if (copy.Count == 0) {
                copy = pickFromThisList;
            }
            count++;
        }
    }*/

}
