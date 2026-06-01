using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MazeChase.Race;

namespace MazeChase.AI
{
    public class AlgorithmSelectorUI : MonoBehaviour
    {
        [Header("UI Panel")]
        public GameObject selectorPanel;

        [Header("Reference")]
        public AIAgentController aiController; // Changed to match your previous script

        [Header("Start Screen")]
        public GameObject[] gameplayHudToHideDuringSelection;

        private readonly List<GameObject> hiddenHudObjects = new();

        private void Start()
        {
            if (selectorPanel != null)
            {
                selectorPanel.SetActive(true);
                ConfigureStartScreen();
            }

            SetGameplayHudVisible(false);

            if (aiController == null)
                aiController = FindAnyObjectByType<AIAgentController>();

            Debug.Log("Press 1 for UCS | Press 2 for BFS | Press 3 for A*");
        }

        private void Update()
        {
            // Press 1 for UCS (Index 0)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectAlgorithm(0, "UCS");
            }

            // Press 2 for BFS (Index 1)
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectAlgorithm(1, "BFS");
            }

            // --- ADDED THIS FOR A* ---
            // Press 3 for A* (Index 2)
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectAlgorithm(2, "A*");
            }
        }

        private void SelectAlgorithm(int index, string label)
        {
            Debug.Log($"{label} selected!");

            if (aiController != null)
                aiController.SetAlgorithm(index);

            if (selectorPanel != null)
                selectorPanel.SetActive(false);

            SetGameplayHudVisible(true);
        }

        private void SetGameplayHudVisible(bool visible)
        {
            if (visible)
            {
                foreach (GameObject hudObject in hiddenHudObjects)
                {
                    if (hudObject != null)
                        hudObject.SetActive(true);
                }

                hiddenHudObjects.Clear();
                return;
            }

            hiddenHudObjects.Clear();

            if (gameplayHudToHideDuringSelection != null)
            {
                foreach (GameObject hudObject in gameplayHudToHideDuringSelection)
                    HideHudObject(hudObject);
            }

            HideHudObject(GameObject.Find("DashboardPanel"));
            HideHudObject(GameObject.Find("PausePanel"));
            HideHudObject(GameObject.Find("MinimapUI"));
            HideHudObject(GameObject.Find("RaceTimerText"));
            HideHudObject(GameObject.Find("BarrierCountText"));
            HideHudObject(GameObject.Find("BarrierPromptUI"));
            HideHudObject(GameObject.Find("RecalculatingText"));
            HideHudObject(GameObject.Find("CountdownText"));
        }

        private void HideHudObject(GameObject hudObject)
        {
            if (hudObject == null || hudObject == selectorPanel || !hudObject.activeSelf)
                return;

            hudObject.SetActive(false);
            hiddenHudObjects.Add(hudObject);
        }

        private void ConfigureStartScreen()
        {
            Image overlay = selectorPanel.GetComponent<Image>();
            if (overlay != null)
            {
                overlay.color = new Color(0.02f, 0.025f, 0.035f, 0.86f);
                overlay.raycastTarget = false;
            }

            RectTransform panelRect = selectorPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
            }

            RectTransform card = EnsurePanel("StartCard", new Vector2(620f, 430f), Vector2.zero);
            Image cardImage = card.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.color = new Color(0.03f, 0.035f, 0.045f, 0.98f);
                cardImage.raycastTarget = false;
            }

            TextMeshProUGUI title = selectorPanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.transform.SetParent(card, false);
                RectTransform titleRect = title.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 1f);
                titleRect.anchorMax = new Vector2(0.5f, 1f);
                titleRect.pivot = new Vector2(0.5f, 1f);
                titleRect.anchoredPosition = new Vector2(0f, -42f);
                titleRect.sizeDelta = new Vector2(560f, 76f);

                title.text = "MAZE CHASE";
                title.fontSize = 50f;
                title.fontStyle = FontStyles.Bold;
                title.alignment = TextAlignmentOptions.Center;
                title.color = new Color(0.92f, 0.96f, 1f, 1f);
                title.enableWordWrapping = false;
                title.margin = Vector4.zero;
            }

            TextMeshProUGUI subtitle = EnsureText("StartSubtitle", card, "SELECT AI PATHFINDING ALGORITHM", 22f);
            RectTransform subtitleRect = subtitle.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 1f);
            subtitleRect.anchorMax = new Vector2(0.5f, 1f);
            subtitleRect.pivot = new Vector2(0.5f, 1f);
            subtitleRect.anchoredPosition = new Vector2(0f, -112f);
            subtitleRect.sizeDelta = new Vector2(560f, 40f);
            subtitle.color = new Color(0.47f, 0.98f, 0.76f, 1f);

            StyleButton("UCSButton", card, new Vector2(0f, -185f), "(1) UCS - Optimal weighted path");
            StyleButton("BFSButton", card, new Vector2(0f, -248f), "(2) BFS - Shortest number of hops");
            StyleButton("AStarButton", card, new Vector2(0f, -311f), "(3) A* - Fast intelligent search");

            TextMeshProUGUI hint = EnsureText("StartHint", card, "Press 1, 2, or 3 to start", 20f);
            RectTransform hintRect = hint.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0f);
            hintRect.anchorMax = new Vector2(0.5f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.anchoredPosition = new Vector2(0f, 28f);
            hintRect.sizeDelta = new Vector2(520f, 38f);
            hint.color = new Color(0.78f, 0.82f, 0.9f, 1f);
        }

        private RectTransform EnsurePanel(string objectName, Vector2 size, Vector2 position)
        {
            Transform existing = selectorPanel.transform.Find(objectName);
            GameObject panelObject = existing != null ? existing.gameObject : new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(selectorPanel.transform, false);
            panelObject.transform.SetAsFirstSibling();

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            return rect;
        }

        private TextMeshProUGUI EnsureText(string objectName, Transform parent, string value, float fontSize)
        {
            Transform existing = parent.Find(objectName);
            GameObject textObject = existing != null ? existing.gameObject : new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.margin = Vector4.zero;

            return text;
        }

        private void StyleButton(string buttonName, Transform parent, Vector2 position, string label)
        {
            Transform buttonTransform = selectorPanel.transform.Find(buttonName) ?? parent.Find(buttonName);
            if (buttonTransform == null)
                return;

            buttonTransform.SetParent(parent, false);

            RectTransform rect = buttonTransform.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(500f, 52f);
            rect.localRotation = Quaternion.identity;

            Image image = buttonTransform.GetComponent<Image>();
            if (image != null)
                image.color = new Color(0.08f, 0.095f, 0.12f, 0.98f);

            Button button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(0.08f, 0.095f, 0.12f, 0.98f);
                colors.highlightedColor = new Color(0.16f, 0.34f, 0.28f, 1f);
                colors.pressedColor = new Color(0.07f, 0.22f, 0.17f, 1f);
                colors.selectedColor = colors.highlightedColor;
                button.colors = colors;
            }

            TextMeshProUGUI buttonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            if (buttonText != null)
            {
                buttonText.text = label;
                buttonText.fontSize = 22f;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.color = new Color(0.94f, 0.97f, 1f, 1f);
                buttonText.enableWordWrapping = false;
                buttonText.margin = Vector4.zero;
            }
        }
    }
}
