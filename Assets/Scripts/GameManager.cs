using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController playerController;
    public EnemyAI enemyAI;

    private bool isPlayerTurn = true;
    private bool isTurnLocked = false; // Prevents simultaneous turn execution

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        if (isTurnLocked) return;

        isPlayerTurn = true;
        isTurnLocked = true; // Lock turn until the player completes their action
        Debug.Log("Player's Turn");
        // Enable player input here if needed
    }

    public void StartBotTurn()
    {
        if (isTurnLocked) return;

        isPlayerTurn = false;
        isTurnLocked = true; // Lock turn until the bot completes its action
        Debug.Log("Bot's Turn");
        enemyAI.StartTurn();
    }

    public void EndTurn()
    {
        isTurnLocked = false; // Unlock turn when action is completed

        if (isPlayerTurn)
        {
            StartBotTurn();
        }
        else
        {
            StartPlayerTurn();
        }
    }
}
