using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    [SerializeField] private GamePlay dataGameplay;

    private void Start()
    {
        dataGameplay.endTime = DateTime.Now;
        GamePlay();
    }
    public void GamePlay()
    {
        GameManager.Instance.LogGamePlay(dataGameplay.starTime, dataGameplay.endTime);
    }
}