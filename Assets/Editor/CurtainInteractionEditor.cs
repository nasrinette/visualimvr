using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurtainInteraction))]
public class CurtainInteractionEditor : Editor
{
    private bool isAnimating;
    private double lastTime;

    void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnEditorUpdate()
    {
        if (!isAnimating || Application.isPlaying) return;

        CurtainInteraction curtain = (CurtainInteraction)target;
        if (curtain == null) { isAnimating = false; return; }

        double now = EditorApplication.timeSinceStartup;
        float dt = (float)(now - lastTime);
        lastTime = now;

        // Clamp dt to avoid huge jumps
        dt = Mathf.Min(dt, 0.05f);

        bool done = curtain.AnimateStep(dt);

        if (done)
            isAnimating = false;

        // Mark dirty so the scene view updates and changes can be saved
        EditorUtility.SetDirty(curtain);
        if (curtain.leftCurtain != null) EditorUtility.SetDirty(curtain.leftCurtain);
        if (curtain.rightCurtain != null) EditorUtility.SetDirty(curtain.rightCurtain);
        SceneView.RepaintAll();
    }

    void StartAnimation()
    {
        isAnimating = true;
        lastTime = EditorApplication.timeSinceStartup;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CurtainInteraction curtain = (CurtainInteraction)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Open", GUILayout.Height(30)))
        {
            Undo.RecordObject(curtain.leftCurtain, "Curtain Open");
            Undo.RecordObject(curtain.rightCurtain, "Curtain Open");
            curtain.DebugSetOpen();
            if (Application.isPlaying) return;
            StartAnimation();
        }

        if (GUILayout.Button("Half Open", GUILayout.Height(30)))
        {
            Undo.RecordObject(curtain.leftCurtain, "Curtain Half Open");
            Undo.RecordObject(curtain.rightCurtain, "Curtain Half Open");
            curtain.DebugSetHalfOpen();
            if (Application.isPlaying) return;
            StartAnimation();
        }

        if (GUILayout.Button("Closed", GUILayout.Height(30)))
        {
            Undo.RecordObject(curtain.leftCurtain, "Curtain Closed");
            Undo.RecordObject(curtain.rightCurtain, "Curtain Closed");
            curtain.DebugSetClosed();
            if (Application.isPlaying) return;
            StartAnimation();
        }

        EditorGUILayout.EndHorizontal();

        if (isAnimating)
        {
            EditorGUILayout.HelpBox("Animating...", MessageType.Info);
            Repaint();
        }
    }
}
