using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using MazeChase.Race;

public class FinishLineTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject winLosePanel;
    public TextMeshProUGUI winLoseText;
    public TextMeshProUGUI finalTimeText;

    private bool gameEnded = false;
    private RaceGameManager raceGameManager;
    private Button restartButton;
    private BoxCollider finishArea;

    void Start()
    {
        Debug.Log("FinishLineTrigger is ACTIVE on: " + gameObject.name);
        ConfigureEndScreen();

        if (winLosePanel != null)
            winLosePanel.SetActive(false);

        raceGameManager = FindAnyObjectByType<RaceGameManager>();
        finishArea = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (gameEnded)
        {
            if (Input.GetKeyDown(KeyCode.R))
                RestartGame();

            return;
        }

        if (finishArea == null) return;

        Bounds bounds = finishArea.bounds;
        foreach (RaceParticipant participant in
                 FindObjectsByType<RaceParticipant>())
        {
            if (participant.ParticipantKind == RaceParticipantKind.None)
                continue;

            if (!bounds.Contains(participant.ParticipantTransform.position))
                continue;

            EndGame(
                participant.ParticipantKind == RaceParticipantKind.Player,
                participant);
            return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameEnded) return;

        Debug.Log("FinishLine hit by: " + other.name);

        RaceParticipant participant =
            other.GetComponentInParent<RaceParticipant>();

        if (participant != null)
        {
            if (participant.ParticipantKind == RaceParticipantKind.Player)
            {
                EndGame(true, participant);
                return;
            }

            if (participant.ParticipantKind == RaceParticipantKind.AI)
            {
                EndGame(false, participant);
                return;
            }
        }

        if (other.CompareTag("Player"))
            EndGame(true, null);
    }

    void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    public void AIWinsDirectCall()
    {
        if (gameEnded) return;
        RaceParticipant ai = FindAIParticipant();
        EndGame(false, ai);
    }

    private void EndGame(bool playerWon, RaceParticipant participant)
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log(playerWon ? "=== PLAYER WINS ===" : "=== AI WINS ===");

        if (RaceTimer.Instance != null)
            RaceTimer.Instance.StopTimer();

        if (participant != null)
            raceGameManager?.RegisterFinish(participant);

        if (winLosePanel != null)
        {
            winLosePanel.SetActive(true);
            winLosePanel.transform.SetAsLastSibling();

            if (winLoseText != null)
            {
                winLoseText.text = playerWon ? "YOU WIN!" : "YOU LOSE!";
                winLoseText.color = playerWon
                    ? new Color(0.18f, 1f, 0.35f)
                    : new Color(1f, 0.24f, 0.22f);
            }

            if (finalTimeText != null && RaceTimer.Instance != null)
                finalTimeText.text = "Time: " +
                    RaceTimer.Instance.GetTimeString();

            if (restartButton != null)
                restartButton.Select();
        }

        Time.timeScale = 0f;
    }

    private void ConfigureEndScreen()
    {
        if (winLosePanel == null) return;

        RectTransform panelRect =
            winLosePanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.localScale = Vector3.one;
        }

        Image overlay = winLosePanel.GetComponent<Image>();
        if (overlay != null)
            overlay.color = new Color(0.03f, 0.035f, 0.045f, 0.88f);

        Transform card = winLosePanel.transform.Find("EndScreenCard");
        if (card == null)
        {
            GameObject cardObject = new GameObject(
                "EndScreenCard",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            cardObject.transform.SetParent(winLosePanel.transform, false);
            card = cardObject.transform;
        }

        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(430f, 285f);
        cardRect.localScale = Vector3.one;

        Image cardImage = card.GetComponent<Image>();
        cardImage.color = new Color(0.075f, 0.085f, 0.105f, 0.96f);

        ConfigureText(winLoseText, card, new Vector2(0f, 72f),
            new Vector2(380f, 82f), 56f, FontStyles.Bold,
            TextAlignmentOptions.Center);
        ConfigureText(finalTimeText, card, new Vector2(0f, 14f),
            new Vector2(380f, 42f), 28f, FontStyles.Normal,
            TextAlignmentOptions.Center);

        Transform restart =
            winLosePanel.transform.Find("RestartButton") ??
            card.Find("RestartButton");
        if (restart != null)
        {
            restart.SetParent(card, false);
            RectTransform restartRect =
                restart.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0.5f, 0.5f);
            restartRect.anchorMax = new Vector2(0.5f, 0.5f);
            restartRect.pivot = new Vector2(0.5f, 0.5f);
            restartRect.anchoredPosition = new Vector2(0f, -82f);
            restartRect.sizeDelta = new Vector2(240f, 54f);
            restartRect.localScale = Vector3.one;

            Image restartImage = restart.GetComponent<Image>();
            if (restartImage != null)
                restartImage.color = new Color(0.86f, 0.92f, 1f, 1f);

            restartButton = restart.GetComponent<Button>();
            ConfigureButtonLabel(restart, "Restart(R)", winLoseText);
        }

        Transform mainMenu =
            winLosePanel.transform.Find("MainMenuButton") ??
            card.Find("MainMenuButton");
        if (mainMenu != null)
            mainMenu.gameObject.SetActive(false);
    }

    private static void ConfigureText(
        TextMeshProUGUI text,
        Transform parent,
        Vector2 position,
        Vector2 size,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment)
    {
        if (text == null) return;

        text.transform.SetParent(parent, false);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.color = Color.white;
    }

    private static void ConfigureButtonLabel(
        Transform button,
        string label,
        TextMeshProUGUI fontSource)
    {
        TextMeshProUGUI text =
            button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (text == null)
        {
            GameObject labelObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(button, false);
            text = labelObject.GetComponent<TextMeshProUGUI>();
        }

        text.gameObject.SetActive(true);
        text.transform.SetAsLastSibling();

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        text.text = label;
        if (fontSource != null && fontSource.font != null)
            text.font = fontSource.font;

        text.fontSize = 24f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.03f, 0.04f, 0.055f, 1f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
    }

    private RaceParticipant FindAIParticipant()
    {
        foreach (RaceParticipant participant in
                 FindObjectsByType<RaceParticipant>())
        {
            if (participant.ParticipantKind == RaceParticipantKind.AI)
                return participant;
        }

        return null;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}   
