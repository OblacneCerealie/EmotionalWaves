using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Objective Settings")]
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private float objectiveFadeInDuration = 0.5f;
    [SerializeField] private float objectiveFadeOutDuration = 0.5f;

    [Header("Dialogue Settings")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float dialogueDisplayDuration = 2f;

    private List<string> activeObjectives = new List<string>();
    private Coroutine currentDialogueCoroutine;
    private CanvasGroup objectiveCanvasGroup;
    private CanvasGroup dialogueCanvasGroup;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup canvas groups for fading
        if (objectivePanel != null)
        {
            objectiveCanvasGroup = objectivePanel.GetComponent<CanvasGroup>();
            if (objectiveCanvasGroup == null)
            {
                objectiveCanvasGroup = objectivePanel.AddComponent<CanvasGroup>();
            }
            objectivePanel.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialogueCanvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (dialogueCanvasGroup == null)
            {
                dialogueCanvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
            }
            dialoguePanel.SetActive(false);
        }
    }

    // ==================== OBJECTIVE METHODS ====================

    /// <summary>
    /// Adds a new objective to the list and displays it
    /// </summary>
    public void SetObjective(string objectiveDescription)
    {
        if (!activeObjectives.Contains(objectiveDescription))
        {
            activeObjectives.Add(objectiveDescription);
            UpdateObjectiveDisplay();
            StartCoroutine(FadeInObjective());
        }
    }

    /// <summary>
    /// Marks an objective as done and removes it from the list
    /// </summary>
    public void ObjectiveDone(string objectiveDescription)
    {
        if (activeObjectives.Contains(objectiveDescription))
        {
            activeObjectives.Remove(objectiveDescription);
            
            if (activeObjectives.Count == 0)
            {
                StartCoroutine(FadeOutObjective());
            }
            else
            {
                UpdateObjectiveDisplay();
            }
        }
    }

    /// <summary>
    /// Clears all active objectives
    /// </summary>
    public void ClearAllObjectives()
    {
        activeObjectives.Clear();
        StartCoroutine(FadeOutObjective());
    }

    private void UpdateObjectiveDisplay()
    {
        if (objectiveText != null)
        {
            string displayText = "Objectives:\n";
            foreach (string objective in activeObjectives)
            {
                displayText += "• " + objective + "\n";
            }
            objectiveText.text = displayText;
        }
    }

    private IEnumerator FadeInObjective()
    {
        if (objectivePanel != null && objectiveCanvasGroup != null)
        {
            objectivePanel.SetActive(true);
            float elapsed = 0f;
            objectiveCanvasGroup.alpha = 0f;

            while (elapsed < objectiveFadeInDuration)
            {
                elapsed += Time.deltaTime;
                objectiveCanvasGroup.alpha = Mathf.Clamp01(elapsed / objectiveFadeInDuration);
                yield return null;
            }

            objectiveCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator FadeOutObjective()
    {
        if (objectivePanel != null && objectiveCanvasGroup != null)
        {
            float elapsed = 0f;
            objectiveCanvasGroup.alpha = 1f;

            while (elapsed < objectiveFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                objectiveCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / objectiveFadeOutDuration);
                yield return null;
            }

            objectiveCanvasGroup.alpha = 0f;
            objectivePanel.SetActive(false);
        }
    }

    // ==================== DIALOGUE METHODS ====================

    /// <summary>
    /// Displays dialogue text with typewriter effect
    /// </summary>
    public void ShowDialogue(string text)
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }
        currentDialogueCoroutine = StartCoroutine(DisplayDialogueCoroutine(text));
    }

    private IEnumerator DisplayDialogueCoroutine(string text)
    {
        if (dialoguePanel != null && dialogueText != null && dialogueCanvasGroup != null)
        {
            // Show panel
            dialoguePanel.SetActive(true);
            dialogueCanvasGroup.alpha = 1f;
            dialogueText.text = "";

            // Typewriter effect
            foreach (char letter in text)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typewriterSpeed);
            }

            // Wait before removing
            yield return new WaitForSeconds(dialogueDisplayDuration);

            // Remove text instantly
            dialogueText.text = "";
            dialogueCanvasGroup.alpha = 0f;
            dialoguePanel.SetActive(false);
        }

        currentDialogueCoroutine = null;
    }

    /// <summary>
    /// Hides dialogue immediately
    /// </summary>
    public void HideDialogue()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        if (dialoguePanel != null)
        {
            dialogueText.text = "";
            dialogueCanvasGroup.alpha = 0f;
            dialoguePanel.SetActive(false);
        }
    }
}

