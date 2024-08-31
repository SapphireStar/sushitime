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
    private float fullBar;
    public float FullBar
    {
        get => fullBar;
        set
        {
            ChangePropertyAndNotify<float>(ref fullBar, value);
        }
    }
    private float maxFullBar;
    public float MaxFullBar
    {
        get => maxFullBar;
        set
        {
            ChangePropertyAndNotify<float>(ref maxFullBar, value);
        }
    }
    private int score;
    public int Score
    {
        get => score;
        set
        {
            ChangePropertyAndNotify<int>(ref score, value);
        }
    }
    private int totalSlices;
    public int TotalSlices
    {
        get => totalSlices;
        set
        {
            ChangePropertyAndNotify<int>(ref totalSlices, value);
        }
    }
    public void Reset()
    {
        PlayerDead = false;
        IsPaused = false;
        IsClimbing = false;
        IsPickedUp = false;
    }
    public void RestartGame()
    {
        PlayerDead = false;
        IsPaused = false;
        IsClimbing = false;
        IsPickedUp = false;
        PatienceBar = 100;
        MaxPatienceBar = 100;
        Score = 0;
        FullBar = 0;
        maxFullBar = 100;
        TotalSlices = 9;
    }
}
