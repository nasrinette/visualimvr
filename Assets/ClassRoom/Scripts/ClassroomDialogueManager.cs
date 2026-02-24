using UnityEngine;
using System.Collections.Generic;

public class ClassroomDialogueManager : MonoBehaviour
{
    private enum FlowState
    {
        WaitingToStart,
        PlayingWelcome,
        WaitingForCurtains,
        PlayingTurnOnLights,
        WaitingForLights,
        PlayingApproachBoard,
        WaitingForBoard,
        PlayingConclusion,
        Done
    }

    [Header("Guide Character")]
    [SerializeField] private ClassroomGuideCharacter guideCharacter;

    [Header("Scene References")]
    [SerializeField] private CurtainInteraction[] curtains;
    [SerializeField] private LightSwitch[] lightSwitches;
    [SerializeField] private Transform whiteboardTransform;
    [SerializeField] private Transform playerHead;

    [Header("Thresholds")]
    [SerializeField] private int curtainsClosedForTrigger = 2;
    [SerializeField] private float curtainClosedThreshold = 0.8f;
    [SerializeField] private float approachBoardDistance = 1.94f;

    [Header("Audio - Phase 1: Welcome")]
    [SerializeField] private AudioClip welcomeClip;
    [SerializeField] private float welcomeDelay = 2f;

    [Header("Audio - Phase 2: Turn On Lights")]
    [SerializeField] private AudioClip turnOnLightsClip;

    [Header("Audio - Phase 3: Approach Board")]
    [SerializeField] private AudioClip approachBoardClip;

    [Header("Audio - Phase 4: Conclusion")]
    [SerializeField] private AudioClip conclusionClip;

    private FlowState state = FlowState.WaitingToStart;
    private Queue<AudioClip> dialogueQueue = new Queue<AudioClip>();

    // reset on entry
    void OnEnable()
    {
        state = FlowState.WaitingToStart;
        dialogueQueue.Clear();
        Invoke(nameof(StartWelcome), welcomeDelay);
    }

    void OnDisable()
    {
        CancelInvoke();
        dialogueQueue.Clear();
    }

    void Start()
    {
        if (curtains == null || curtains.Length == 0)
            curtains = FindObjectsOfType<CurtainInteraction>();

        if (lightSwitches == null || lightSwitches.Length == 0)
            lightSwitches = FindObjectsOfType<LightSwitch>();

        if (playerHead == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                playerHead = mainCam.transform;
        }

        if (guideCharacter != null)
            guideCharacter.OnDialogueFinished += OnDialogueFinished;
    }

    void OnDestroy()
    {
        if (guideCharacter != null)
            guideCharacter.OnDialogueFinished -= OnDialogueFinished;
    }

    void Update()
    {
        if (guideCharacter != null && !guideCharacter.IsPlaying && dialogueQueue.Count > 0)
        {
            guideCharacter.PlayDialogue(dialogueQueue.Dequeue());
        }

        switch (state)
        {
            case FlowState.WaitingForCurtains:
                CheckCurtainsClosed();
                break;
            case FlowState.WaitingForLights:
                CheckLightsOn();
                break;
            case FlowState.WaitingForBoard:
                CheckApproachBoard();
                break;
        }
    }

    private void StartWelcome()
    {
        state = FlowState.PlayingWelcome;
        QueueDialogue(welcomeClip);
    }

    // advance state machine
    private void OnDialogueFinished()
    {
        switch (state)
        {
            case FlowState.PlayingWelcome:
                state = FlowState.WaitingForCurtains;
                break;
            case FlowState.PlayingTurnOnLights:
                state = FlowState.WaitingForLights;
                break;
            case FlowState.PlayingApproachBoard:
                state = FlowState.WaitingForBoard;
                break;
            case FlowState.PlayingConclusion:
                state = FlowState.Done;
                break;
        }
    }

    // curtain phase check
    private void CheckCurtainsClosed()
    {
        int closedCount = GetClosedCurtainCount();
        if (closedCount >= curtainsClosedForTrigger)
        {
            if (AnyLightOn())
            {
                state = FlowState.PlayingApproachBoard;
                QueueDialogue(approachBoardClip);
            }
            else
            {
                state = FlowState.PlayingTurnOnLights;
                QueueDialogue(turnOnLightsClip);
            }
        }
    }

    private void CheckLightsOn()
    {
        if (AnyLightOn())
        {
            state = FlowState.PlayingApproachBoard;
            QueueDialogue(approachBoardClip);
        }
    }

    // proximity check
    private void CheckApproachBoard()
    {
        if (playerHead == null || whiteboardTransform == null)
        {
            Debug.Log($"[Classroom] CheckApproachBoard skipped - playerHead:{playerHead != null}, whiteboard:{whiteboardTransform != null}");
            return;
        }

        float distance = Vector3.Distance(playerHead.position, whiteboardTransform.position);
        Debug.Log($"[Classroom] Distance to whiteboard: {distance:F2}m (need < {approachBoardDistance}m)");
        if (distance < approachBoardDistance)
        {
            Debug.Log("[Classroom] Player reached whiteboard - playing conclusion");
            state = FlowState.PlayingConclusion;
            QueueDialogue(conclusionClip);
        }
    }

    private void QueueDialogue(AudioClip clip)
    {
        if (clip == null) return;

        if (guideCharacter != null && !guideCharacter.IsPlaying && dialogueQueue.Count == 0)
            guideCharacter.PlayDialogue(clip);
        else
            dialogueQueue.Enqueue(clip);
    }

    private int GetClosedCurtainCount()
    {
        int count = 0;
        foreach (var c in curtains)
        {
            if (c != null && c.GetClosedAmount() > curtainClosedThreshold)
                count++;
        }
        return count;
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
}
