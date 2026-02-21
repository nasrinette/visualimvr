using UnityEngine;
using System.Collections.Generic;

public class ClassroomDialogueManager : MonoBehaviour
{
    [Header("Guide Character")]
    [SerializeField] private ClassroomGuideCharacter guideCharacter;

    [Header("Scene References")]
    [SerializeField] private CurtainInteraction[] curtains;
    [SerializeField] private LightSwitch[] lightSwitches;
    [SerializeField] private Transform whiteboardTransform;
    [SerializeField] private Transform playerHead; // XR Camera

    [Header("Thresholds")]
    [SerializeField] private float curtainClosedThreshold = 0.85f;
    [SerializeField] private float curtainOpenThreshold = 0.3f;
    [SerializeField] private float closeToWhiteboardDistance = 1.5f;
    [SerializeField] private float farFromWhiteboardDistance = 3.0f;
    [SerializeField] private float darkRoomThreshold = 0.6f;

    [Header("Phase 0 - Welcome")]
    [SerializeField] private AudioClip welcomeClip;
    [SerializeField] private float welcomeDelay = 2f;

    [Header("Phase 1 - Instructions")]
    [SerializeField] private AudioClip instructionsClip;

    [Header("Phase 2A - Dark Room (curtains closed, lights off)")]
    [SerializeField] private AudioClip darkRoomClip;

    [Header("Phase 2B - Curtains Closed, Lights On")]
    [SerializeField] private AudioClip curtainsClosedLightsOnClip;

    [Header("Phase 2C - Bright Glare (curtains open, lights on)")]
    [SerializeField] private AudioClip brightGlareClip;

    [Header("Phase 2D - Lights Turned Off")]
    [SerializeField] private AudioClip lightsOffClip;

    [Header("Phase 2E - Close to Whiteboard")]
    [SerializeField] private AudioClip closeToWhiteboardClip;

    [Header("Phase 2F - Far from Whiteboard in Dim Light")]
    [SerializeField] private AudioClip farDimWhiteboardClip;

    [Header("Phase 3 - Summary")]
    [SerializeField] private AudioClip summaryClip;
    [SerializeField] private int phase2TriggersForSummary = 3;

    // One-shot flags
    private bool welcomePlayed;
    private bool instructionsPlayed;
    private bool darkRoomPlayed;
    private bool curtainsClosedLightsOnPlayed;
    private bool brightGlarePlayed;
    private bool lightsOffPlayed;
    private bool closeWhiteboardPlayed;
    private bool farDimWhiteboardPlayed;
    private bool summaryPlayed;

    private int phase2Count;
    private bool readyForTriggers;

    // Light state tracking (to detect toggle moment)
    private bool[] previousLightStates;

    // Dialogue queue to prevent overlap
    private Queue<AudioClip> dialogueQueue = new Queue<AudioClip>();

    void OnEnable()
    {
        if (!welcomePlayed)
        {
            Invoke(nameof(PlayWelcome), welcomeDelay);
        }
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Start()
    {
        // Auto-find references if not assigned
        if (curtains == null || curtains.Length == 0)
            curtains = FindObjectsOfType<CurtainInteraction>();

        if (lightSwitches == null || lightSwitches.Length == 0)
            lightSwitches = FindObjectsOfType<LightSwitch>();

        // Initialize light state tracking
        previousLightStates = new bool[lightSwitches.Length];
        for (int i = 0; i < lightSwitches.Length; i++)
        {
            if (lightSwitches[i] != null)
                previousLightStates[i] = lightSwitches[i].IsOn;
        }

        // Auto-find player camera if not assigned
        if (playerHead == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                playerHead = mainCam.transform;
        }

        // Subscribe to dialogue finished event for chaining
        if (guideCharacter != null)
        {
            guideCharacter.OnDialogueFinished += OnDialogueFinished;
        }
    }

    void OnDestroy()
    {
        if (guideCharacter != null)
        {
            guideCharacter.OnDialogueFinished -= OnDialogueFinished;
        }
    }

    void Update()
    {
        // Process dialogue queue
        if (guideCharacter != null && !guideCharacter.IsPlaying && dialogueQueue.Count > 0)
        {
            guideCharacter.PlayDialogue(dialogueQueue.Dequeue());
        }

        // Only check reactive triggers after instructions have played
        if (!readyForTriggers) return;
        if (guideCharacter != null && guideCharacter.IsPlaying) return;
        if (dialogueQueue.Count > 0) return;

        CheckDarkRoom();
        CheckCurtainsClosedLightsOn();
        CheckBrightGlare();
        CheckLightsOff();
        CheckCloseToWhiteboard();
        CheckFarDimWhiteboard();
        CheckSummary();

        // Update previous light states at end of frame
        UpdatePreviousLightStates();
    }

    // --- Phase 0 & 1 ---

    private void PlayWelcome()
    {
        welcomePlayed = true;
        QueueDialogue(welcomeClip);
    }

    private void OnDialogueFinished()
    {
        // Chain: welcome → instructions
        if (welcomePlayed && !instructionsPlayed)
        {
            instructionsPlayed = true;
            QueueDialogue(instructionsClip);
            return;
        }

        // After instructions finish, enable reactive triggers
        if (instructionsPlayed && !readyForTriggers)
        {
            readyForTriggers = true;
        }
    }

    // --- Phase 2 Checks ---

    private void CheckDarkRoom()
    {
        if (darkRoomPlayed) return;

        float curtainsClosed = GetAverageCurtainClosed();
        bool anyLightOn = AnyLightOn();

        if (curtainsClosed > curtainClosedThreshold && !anyLightOn)
        {
            darkRoomPlayed = true;
            phase2Count++;
            QueueDialogue(darkRoomClip);
        }
    }

    private void CheckCurtainsClosedLightsOn()
    {
        if (curtainsClosedLightsOnPlayed) return;

        float curtainsClosed = GetAverageCurtainClosed();
        bool anyLightOn = AnyLightOn();

        if (curtainsClosed > curtainClosedThreshold && anyLightOn)
        {
            curtainsClosedLightsOnPlayed = true;
            phase2Count++;
            QueueDialogue(curtainsClosedLightsOnClip);
        }
    }

    private void CheckBrightGlare()
    {
        if (brightGlarePlayed) return;

        float curtainsClosed = GetAverageCurtainClosed();
        bool anyLightOn = AnyLightOn();

        if (curtainsClosed < curtainOpenThreshold && anyLightOn)
        {
            brightGlarePlayed = true;
            phase2Count++;
            QueueDialogue(brightGlareClip);
        }
    }

    private void CheckLightsOff()
    {
        if (lightsOffPlayed) return;

        // Detect the moment a light is turned off
        for (int i = 0; i < lightSwitches.Length; i++)
        {
            if (lightSwitches[i] == null) continue;

            bool wasOn = previousLightStates[i];
            bool isOn = lightSwitches[i].IsOn;

            if (wasOn && !isOn)
            {
                lightsOffPlayed = true;
                phase2Count++;
                QueueDialogue(lightsOffClip);
                return;
            }
        }
    }

    private void CheckCloseToWhiteboard()
    {
        if (closeWhiteboardPlayed) return;
        if (playerHead == null || whiteboardTransform == null) return;

        float distance = Vector3.Distance(playerHead.position, whiteboardTransform.position);
        if (distance < closeToWhiteboardDistance)
        {
            closeWhiteboardPlayed = true;
            phase2Count++;
            QueueDialogue(closeToWhiteboardClip);
        }
    }

    private void CheckFarDimWhiteboard()
    {
        if (farDimWhiteboardPlayed) return;
        if (playerHead == null || whiteboardTransform == null) return;

        float distance = Vector3.Distance(playerHead.position, whiteboardTransform.position);
        float darkness = GetCurrentDarkness();

        if (distance > farFromWhiteboardDistance && darkness > darkRoomThreshold)
        {
            farDimWhiteboardPlayed = true;
            phase2Count++;
            QueueDialogue(farDimWhiteboardClip);
        }
    }

    // --- Phase 3 ---

    private void CheckSummary()
    {
        if (summaryPlayed) return;

        if (phase2Count >= phase2TriggersForSummary)
        {
            summaryPlayed = true;
            QueueDialogue(summaryClip);
        }
    }

    // --- Helpers ---

    private void QueueDialogue(AudioClip clip)
    {
        if (clip == null) return;

        if (guideCharacter != null && !guideCharacter.IsPlaying && dialogueQueue.Count == 0)
        {
            guideCharacter.PlayDialogue(clip);
        }
        else
        {
            dialogueQueue.Enqueue(clip);
        }
    }

    private float GetAverageCurtainClosed()
    {
        if (curtains == null || curtains.Length == 0) return 0f;

        float total = 0f;
        int count = 0;
        foreach (var c in curtains)
        {
            if (c != null)
            {
                total += c.GetClosedAmount();
                count++;
            }
        }
        return count > 0 ? total / count : 0f;
    }

    private bool AnyLightOn()
    {
        foreach (var ls in lightSwitches)
        {
            if (ls != null && ls.IsOn)
                return true;
        }
        return false;
    }

    private float GetCurrentDarkness()
    {
        float curtainsClosed = GetAverageCurtainClosed();
        bool anyLightOn = AnyLightOn();
        return curtainsClosed * (anyLightOn ? 0f : 1f);
    }

    private void UpdatePreviousLightStates()
    {
        for (int i = 0; i < lightSwitches.Length; i++)
        {
            if (lightSwitches[i] != null)
                previousLightStates[i] = lightSwitches[i].IsOn;
        }
    }
}
