using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Tokens")]
    public TokenMover playerToken;
    public TokenMover aiToken;

    [Header("Camera")]
    public Camera mainCamera;
    public Vector3 cameraOffset = new Vector3(0, 7, -9);

    [Header("UI")]
    public TMP_Text diceText;
    public TMP_Text turnText;
    public TMP_Text playerCardsText;
    public TMP_Text aiCardsText;
    public TMP_Text winText;

    public Button rollButton;
    public Button useCard1Button;
    public Button useCard2Button;
    public Button useCard3Button;

    [Header("Start Menu")]
    public GameObject startMenuPanel;

    private List<string> playerCards = new List<string>();
    private List<string> aiCards = new List<string>();

    private bool playerTurn = true;
    private bool gameOver = false;

    private string[] cardDeck =
    {
        "Extra Step",
        "Boost",
        "Attack",
        "Swap"
    };

    private int[] cardTiles =
    {
        5, 12, 20, 28, 36, 44, 52
    };

    void Start()
    {
        winText.text = "";

        playerToken.SetPosition(0);
        aiToken.SetPosition(0);

        for (int i = 0; i < 3; i++)
        {
            DrawCard(playerCards);
            DrawCard(aiCards);
        }

        rollButton.onClick.AddListener(PlayerRoll);
        useCard1Button.onClick.AddListener(() => UsePlayerCard(0));
        useCard2Button.onClick.AddListener(() => UsePlayerCard(1));
        useCard3Button.onClick.AddListener(() => UsePlayerCard(2));

        UpdateUI();
        FocusCameraOn(playerToken);

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void StartGame()
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        FocusCameraOn(playerToken);
    }

    void PlayerRoll()
    {
        if (!playerTurn || gameOver) return;
        StartCoroutine(PlayerTurnRoutine());
    }

    IEnumerator PlayerTurnRoutine()
    {
        rollButton.interactable = false;
        FocusCameraOn(playerToken);

        yield return StartCoroutine(RollAnimation("Player"));

        int roll = Random.Range(1, 7);
        diceText.text = "Player Roll: " + roll;

        if (roll == 6)
        {
            DrawCard(playerCards);
            diceText.text += "\nRolled 6! Card Drawn!";
        }

        playerToken.MoveSteps(roll);
        yield return new WaitForSeconds(1.3f);

        CheckSpecialTile(playerToken, playerCards, "Player");
        CheckWin();

        if (!gameOver)
        {
            playerTurn = false;
            UpdateUI();

            yield return new WaitForSeconds(0.8f);
            StartCoroutine(AITurnRoutine());
        }
    }

    IEnumerator AITurnRoutine()
    {
        turnText.text = "Turn: AI";
        rollButton.interactable = false;
        FocusCameraOn(aiToken);

        yield return new WaitForSeconds(0.8f);

        UseAICard();

        yield return StartCoroutine(RollAnimation("AI"));

        int roll = Random.Range(1, 7);
        diceText.text = "AI Roll: " + roll;

        if (roll == 6)
        {
            DrawCard(aiCards);
            diceText.text += "\nAI rolled 6! Card Drawn!";
        }

        aiToken.MoveSteps(roll);
        yield return new WaitForSeconds(1.3f);

        CheckSpecialTile(aiToken, aiCards, "AI");
        CheckWin();

        if (!gameOver)
        {
            playerTurn = true;
            UpdateUI();
            FocusCameraOn(playerToken);
        }
    }

    IEnumerator RollAnimation(string who)
    {
        for (int i = 0; i < 12; i++)
        {
            int tempRoll = Random.Range(1, 7);
            diceText.text = who + " Rolling: " + tempRoll;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void DrawCard(List<string> cards)
    {
        string card = cardDeck[Random.Range(0, cardDeck.Length)];
        cards.Add(card);
    }

    void UsePlayerCard(int index)
    {
        if (!playerTurn || gameOver) return;
        if (index >= playerCards.Count) return;

        string card = playerCards[index];

        if (card == "Extra Step")
        {
            playerToken.MoveSteps(2);
        }
        else if (card == "Boost")
        {
            playerToken.MoveSteps(3);
        }
        else if (card == "Attack")
        {
            aiToken.MoveSteps(-2);
        }
        else if (card == "Swap")
        {
            int temp = playerToken.currentIndex;
            playerToken.SetPosition(aiToken.currentIndex);
            aiToken.SetPosition(temp);
        }

        diceText.text = "Used Card: " + card;

        playerCards.RemoveAt(index);
        UpdateUI();
        CheckWin();
    }

    void UseAICard()
    {
        if (aiCards.Count == 0) return;

        string chosenCard = "";

        if (playerToken.currentIndex > aiToken.currentIndex + 6 && aiCards.Contains("Attack"))
        {
            chosenCard = "Attack";
        }
        else if (aiToken.currentIndex < playerToken.currentIndex && aiCards.Contains("Boost"))
        {
            chosenCard = "Boost";
        }
        else if (aiToken.currentIndex < playerToken.currentIndex && aiCards.Contains("Extra Step"))
        {
            chosenCard = "Extra Step";
        }
        else if (playerToken.currentIndex > aiToken.currentIndex + 10 && aiCards.Contains("Swap"))
        {
            chosenCard = "Swap";
        }

        if (chosenCard == "") return;

        if (chosenCard == "Extra Step")
        {
            aiToken.MoveSteps(2);
        }
        else if (chosenCard == "Boost")
        {
            aiToken.MoveSteps(3);
        }
        else if (chosenCard == "Attack")
        {
            playerToken.MoveSteps(-2);
        }
        else if (chosenCard == "Swap")
        {
            int temp = aiToken.currentIndex;
            aiToken.SetPosition(playerToken.currentIndex);
            playerToken.SetPosition(temp);
        }

        diceText.text = "AI Used Card: " + chosenCard;
        aiCards.Remove(chosenCard);
    }

    void CheckSpecialTile(TokenMover token, List<string> cards, string who)
    {
        for (int i = 0; i < cardTiles.Length; i++)
        {
            if (token.currentIndex == cardTiles[i])
            {
                DrawCard(cards);
                diceText.text = who + " landed on a Card Tile!\nCard Drawn!";
                break;
            }
        }
    }

    void CheckWin()
    {
        int finishIndex = playerToken.boardPoints.Length - 1;

        if (playerToken.currentIndex >= finishIndex)
        {
            gameOver = true;
            winText.text = "Player Wins!";
            rollButton.interactable = false;
        }
        else if (aiToken.currentIndex >= finishIndex)
        {
            gameOver = true;
            winText.text = "AI Wins!";
            rollButton.interactable = false;
        }
    }

    void UpdateUI()
    {
        turnText.text = playerTurn ? "Turn: Player" : "Turn: AI";
        rollButton.interactable = playerTurn && !gameOver;

        playerCardsText.text = "Player Cards:\n";

        for (int i = 0; i < playerCards.Count; i++)
        {
            if (playerCards[i] == "Extra Step")
            {
                playerCardsText.text += (i + 1) + ". Extra Step (+2 tiles)\n";
            }
            else if (playerCards[i] == "Boost")
            {
                playerCardsText.text += (i + 1) + ". Boost (+3 movement)\n";
            }
            else if (playerCards[i] == "Attack")
            {
                playerCardsText.text += (i + 1) + ". Attack (push enemy back)\n";
            }
            else if (playerCards[i] == "Swap")
            {
                playerCardsText.text += (i + 1) + ". Swap (switch positions)\n";
            }
        }

        aiCardsText.text = "AI Cards: " + aiCards.Count;

        useCard1Button.gameObject.SetActive(playerCards.Count > 0);
        useCard2Button.gameObject.SetActive(playerCards.Count > 1);
        useCard3Button.gameObject.SetActive(playerCards.Count > 2);
    }

    void FocusCameraOn(TokenMover token)
    {
        if (mainCamera == null) return;

        Vector3 targetPos = token.transform.position + cameraOffset;
        mainCamera.transform.position = targetPos;
        mainCamera.transform.LookAt(token.transform.position);
    }
}