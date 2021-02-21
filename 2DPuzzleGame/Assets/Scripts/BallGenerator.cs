using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BallGenerator : MonoBehaviour
{
    public GameObject Ball = null;

    public List<GameObject> Balls = new List<GameObject>();

    public void BallsGenerate()
    {
        for (int i = 0; i < 30; i++)
        {
            var ball = Instantiate(Ball, this.transform);
            Balls.Add(ball);
        }
    }

}
