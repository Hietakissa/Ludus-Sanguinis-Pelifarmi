using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UIElements;
using HietakissaUtils;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class HKAlignerTool : HKTool
{
    public override string toolName => "Object Aligner";
    public override string iconName => "Aligner_Icon";

    Transform[] selectionTransforms = new Transform[0];
    bool aligning;

    bool hasScene;

    Dictionary<Transform, Vector3> offsets = new Dictionary<Transform, Vector3>();
    Dictionary<Transform, Transform> originClonePairs = new Dictionary<Transform, Transform>();
    Dictionary<Transform, Transform> cloneOriginPairs = new Dictionary<Transform, Transform>();
    List<Transform> cloneTransforms = new List<Transform>();

    Transform rootTransform;

    //Vector3 oldPos;
    //Vector3 oldRot;

    public override void Enter()
    {
        aligning = false;
        ClosePhysicsSceneIfOpen();

        Selection.selectionChanged += SelectionChange;
        SelectionChange();
    }

    public override void Exit()
    {
        Selection.selectionChanged -= SelectionChange;
        
        ClosePhysicsSceneIfOpen();
    }

    public override void Update()
    {
        if (!aligning) return;

        //if (Vector3.Distance(rootTransform.position, oldPos) > 0.001f || Vector3.Distance(rootTransform.rotation.eulerAngles, oldRot) > 0.001f) AlignToSurface();
        AlignToSurface();
        //oldPos = rootTransform.position;
        //oldRot = rootTransform.rotation.eulerAngles;
    }

    void SelectionChange()
    {
        if (aligning) return;
        selectionTransforms = Selection.transforms;
        Draw();
    }

    void Draw()
    {
        page.Clear();
        DrawTitle();
        DrawScrollView();

        if (selectionTransforms.Length == 0) return;

        if (!aligning) DrawButton(page, AlignSelectedToSurface, "Quick Align", 16f);

        if (EditorApplication.isPlaying)
        {
            page.Add(new Label("Unavailable in Edit mode."));
            return;
        }

        VisualElement cancelApplyHolder = new VisualElement();
        cancelApplyHolder.style.flexDirection = FlexDirection.Row;

        DrawButton(cancelApplyHolder, StartAlign, aligning ? "Cancel" : "Start Dynamic Align", 16f, 1f);
        if (aligning) DrawButton(cancelApplyHolder, Apply, "Apply", 16f, 1f);

        page.Add(cancelApplyHolder);



        void DrawScrollView()
        {
            Label selectedObjectLabel = new Label($"({selectionTransforms.Length}) Selected Objects");
            selectedObjectLabel.style.paddingLeft = 10;
            selectedObjectLabel.style.paddingBottom = 5;
            page.Add(selectedObjectLabel);

            ScrollView selectedObjectsPreviewScroller = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            selectedObjectsPreviewScroller.style.minHeight = 0f;
            selectedObjectsPreviewScroller.style.maxHeight = GetStyleLengthForPercentage(20f);

            selectedObjectsPreviewScroller.style.backgroundColor = new Color(0.19f, 0.19f, 0.19f);
            selectedObjectsPreviewScroller.style.paddingLeft = 10;
            page.Add(selectedObjectsPreviewScroller);

            foreach (Transform t in selectionTransforms)
            {
                Label selectionInfoLabel = new Label(t.name);

                selectedObjectsPreviewScroller.Add(selectionInfoLabel);
            }
        }
    }

    void StartAlign()
    {
        if (!hasScene)
        {
            CreateAndInitializeScene();
            aligning = true;

            AlignToSurface();
        }
        else
        {
            ClosePhysicsSceneIfOpen();
            aligning = false;
        }
        Draw();
    }

    void AlignToSurface()
    {
        foreach (Transform t in cloneTransforms)
        {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in cloneTransforms)
        {
            t.localPosition = offsets[t];
            t.rotation = cloneOriginPairs[t].rotation;

            RaycastHit[] hits = Physics.RaycastAll(t.position + Vector3.up * 0.01f, Vector3.down, 100f);
            hits = hits.OrderBy(hit => hit.distance).ToArray();

            foreach (RaycastHit hit in hits)
            {
                if (!selectionTransforms.Contains(hit.collider.transform))
                {
                    Debug.DrawLine(t.position, hit.point, Color.green, 0.2f);

                    t.position = hit.point;
                    t.rotation = Quaternion.FromToRotation(t.up, hit.normal) * t.rotation;
                    break;
                }
            }
        }

        foreach (Transform t in cloneTransforms)
        {
            t.gameObject.SetActive(true);
        }
    }

    void AlignSelectedToSurface()
    {
        Undo.RecordObjects(selectionTransforms, "Align Objects With HK Aligner Tool");

        foreach (Transform t in selectionTransforms)
        {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in selectionTransforms)
        {
            if (Physics.Raycast(t.position + Vector3.up * 0.01f, Vector3.down, out RaycastHit hit, 100f))
            {
                t.position = hit.point;
                t.rotation = Quaternion.FromToRotation(t.up, hit.normal) * t.rotation;
            }
        }

        foreach (Transform t in selectionTransforms)
        {
            t.gameObject.SetActive(true);
        }
    }

    void Apply()
    {
        //aligning = false;

        //if (Selection.activeTransform == rootTransform) Selection.activeTransform = null;

        Undo.RecordObjects(selectionTransforms, "Align Objects With HK Aligner Tool");

        foreach (Transform t in selectionTransforms)
        {
            t.position = originClonePairs[t].position;
            t.rotation = originClonePairs[t].rotation;
        }

        //ClosePhysicsSceneIfOpen();
        Draw();
    }

    void CreateAndInitializeScene()
    {
        SelectionChange();

        Scene beforeActiveScene = EditorSceneManager.GetActiveScene();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene.name = "HK_ObjectAligner_Temp_Scene";
        hasScene = true;


        List<Vector3> positions = new List<Vector3>();
        foreach (Transform t in selectionTransforms) positions.Add(t.position);
        Vector3 selectionAveragePos = Maf.Vector3Average(positions.ToArray());

        rootTransform = new GameObject("Aligner Root").transform;
        rootTransform.position = selectionAveragePos;
        Selection.activeGameObject = rootTransform.gameObject;


        offsets.Clear();
        originClonePairs.Clear();
        cloneOriginPairs.Clear();
        cloneTransforms.Clear();

        foreach (Transform t in selectionTransforms)
        {
            Transform clone = GameObject.Instantiate(t, rootTransform, true);
            clone.name = $"{t.name} (Aligner Clone)";

            offsets[clone] = t.position - rootTransform.position;
            originClonePairs[t] = clone;
            cloneOriginPairs[clone] = t;
            cloneTransforms.Add(clone);
        }

        EditorSceneManager.SetActiveScene(beforeActiveScene);
    }

    void ClosePhysicsSceneIfOpen()
    {
        Scene foundScene = EditorSceneManager.GetSceneByName("HK_ObjectAligner_Temp_Scene");

        if (hasScene || foundScene.IsValid())
        {
            EditorSceneManager.CloseScene(foundScene, true);
            hasScene = false;
        }
    }
}