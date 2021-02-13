using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BallGenerator : MonoBehaviour
{

    public GameObject Ball = null;

    private GridLayoutGroup GridLayoutGroup = null;

    private void Awake()
    {
        for (int i = 0; i < 30; i++)
        {
            Instantiate(Ball, this.transform);
        }
    }

}
