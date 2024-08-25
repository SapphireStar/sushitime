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
    private bool isGameOver;
    public bool IsGameOver
    {
        get => isGameOver;
        set
        {
            ChangePropertyAndNotify<bool>(ref isGameOver, value);
        }
    }
    private bool isPickedUp;
    public bool IsPickedUp
    {
        get => isPickedUp;
        set
        {
            ChangePropertyAndNotify<bool>(ref isPickedUp, value);
        }
    }
    private bool isClimbing;
    public bool IsClimbing
    {
        get => isClimbing;
        set
        {
            ChangePropertyAndNotify<bool>(ref isClimbing, value);
        }
    }
    private float maxPatienceBar;
    public float MaxPatienceBar
    {
        get => maxPatienceBar;
        set
        {
            ChangePropertyAndNotify<float>(ref maxPatienceBar, value);
        }
    }
    private float patienceBar;
    public float PatienceBar
    {
        get => patienceBar;
        set
        {
            ChangePropertyAndNotify<float>(ref patienceBar, value);
        }
    }
    public void Reset()
    {
        playerDead = false;
        isPaused = false;
        isClimbing = false;
        isPickedUp = false;
    }
    public void RestartGame()
    {
        playerDead = false;
        isPaused = false;
        isClimbing = false;
        isPickedUp = false;
        PatienceBar = 100;
        MaxPatienceBar = 100;
    }
}
