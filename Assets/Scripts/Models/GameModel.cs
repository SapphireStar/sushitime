using Isekai.UI.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : Model
{
    private bool playerDead;
    public bool PlayerDead
    {
        get => playerDead;
        set
        {
            ChangePropertyAndNotify<bool>(ref playerDead, value);
        }
    }
    private bool isPaused;
    public bool IsPaused
    {
        get => isPaused;
        set
        {
            ChangePropertyAndNotify<bool>(ref isPaused, value);
        }
    }
    public void Reset()
    {
        playerDead = false;
        isPaused = false;
    }
}
