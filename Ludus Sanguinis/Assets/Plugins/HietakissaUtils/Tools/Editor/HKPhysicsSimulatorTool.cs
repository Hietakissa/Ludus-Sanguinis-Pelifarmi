using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class HKPhysicsSimulatorTool : HKTool
{
    public override string toolName => "Physics Simulator";
    public override string iconName => "Physics_Icon";

    Dictionary<Transform, int> objectAvailability = new Dictionary<Transform, int>();
    Dictionary<Transform, KeyFrameInfo> objectKeyFrames = new Dictionary<Transform, KeyFrameInfo>();

    List<Transform> simulatedObjects = new List<Transform>();
    List<Transform> simulatedObjectClones = new List<Transform>();
    
    Transform[] selectionTransforms = new Transform[0];

    List<GameObject> openSceneObjects = new List<GameObject>();

    bool selectionLocked = false;

    float simulationTime = 1;
    float timelineProgress;

    bool useSetSeed;
    int seed;

    #region Rigidbody Settings
    bool showSettings;
    bool showVelocities;
    bool showConstraints;

    bool relativeVelocity;
    Vector3 minVelocity;
    Vector3 maxVelocity;
    Vector3 minAngularVelocity;
    Vector3 maxAngularVelocity;

    bool useGravity = true;
    float mass = 1;
    float drag = 0f;
    float angularDrag = 0.05f;

    bool freezeXPos;
    bool freezeYPos;
    bool freezeZPos;
    bool freezeXRot;
    bool freezeYRot;
    bool freezeZRot;
    #endregion

    System.Type proBuilderType;

    Scene scene;
    PhysicsScene physicsScene;
    bool hasScene = false;

    public override void Enter()
    {
        proBuilderType = System.Type.GetType("UnityEngine.ProBuilder.ProBuilderMesh, Unity.ProBuilder");

        Selection.selectionChanged += EditorSelectionChanged;
        EditorApplication.playModeStateChanged += PlayModeChange;

        simulatedObjects.Clear();

        showSettings = false;
        showVelocities = false;
        showConstraints = false;
        ClosePhysicsSceneIfOpen();
        EditorSelectionChanged();
    }
    public override void Exit()
    {
        Selection.selectionChanged -= EditorSelectionChanged;
        EditorApplication.playModeStateChanged -= PlayModeChange;

        ClosePhysicsSceneIfOpen();
        selectionLocked = false;
    }

    void PlayModeChange(PlayModeStateChange stateChange)
    {
        if (stateChange == PlayModeStateChange.EnteredPlayMode)
        {
            ClosePhysicsSceneIfOpen();
            Draw();
        }
        else if (stateChange == PlayModeStateChange.EnteredEditMode) Draw();
    }

    void Draw()
    {
        page.Clear();
        DrawTitle();

        if (EditorApplication.isPlaying)
        {
            page.Add(new Label("Unavailable in Edit mode."));
            return;
        }
        else if (selectionTransforms.Length == 0) page.Add(new Label("No objects selected. Select some to begin."));
        else
        {
            DrawScrollView();
            DrawSelectionLockHolder();
        }

        if (hasScene) DrawSimulateHolder();

        if (hasScene && simulatedObjects.Count != 0)
        {
            DrawTimeline();
        }
        
        DrawCreatePhysicsSceneHolder();
        DrawSettingsFoldout();


        #region UI Drawing

        void DrawScrollView()
        {
            Label selectedObjectLabel = new Label($"({selectionTransforms.Length}) Selected Objects");
            selectedObjectLabel.style.paddingLeft = 10;
            selectedObjectLabel.style.paddingBottom = 5;
            page.Add(selectedObjectLabel);

            ScrollView selectedObjectsPreviewScroller = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            selectedObjectsPreviewScroller.style.minHeight = 0f;
            selectedObjectsPreviewScroller.style.maxHeight = GetStyleLengthForPercentage(25f);

            selectedObjectsPreviewScroller.style.backgroundColor = new Color(0.19f, 0.19f, 0.19f);
            selectedObjectsPreviewScroller.style.paddingLeft = 10;
            page.Add(selectedObjectsPreviewScroller);

            foreach (Transform t in selectionTransforms)
            {
                Label selectionInfoLabel = new Label(t.name);

                int availability = objectAvailability[t];
                Color color;

                if (availability == 0) color = Color.green;
                else if (availability == 1) color = Color.yellow;
                else color = Color.red;

                selectionInfoLabel.style.color = color;

                selectedObjectsPreviewScroller.Add(selectionInfoLabel);
            }
        }

        void DrawSelectionLockHolder()
        {
            VisualElement selectionLockHolder = new VisualElement();
            selectionLockHolder.style.height = GetStyleLengthForPercentage(7f);
            selectionLockHolder.style.flexDirection = FlexDirection.Row;
            selectionLockHolder.style.paddingTop = 10;

            Toggle useSetSeedtoggle = new Toggle("Set Seed");
            useSetSeedtoggle.SetValueWithoutNotify(useSetSeed);
            useSetSeedtoggle.RegisterValueChangedCallback((callback) =>
            {
                useSetSeed = callback.newValue;
                Draw();
            });
            selectionLockHolder.Add(useSetSeedtoggle);

            if (useSetSeed)
            {
                IntegerField seedField = new IntegerField("Seed");
                seedField.SetValueWithoutNotify(seed);
                seedField.RegisterValueChangedCallback((callback) => seed = callback.newValue);
                selectionLockHolder.Add(seedField);
            }

            DrawButton(selectionLockHolder, ToggleLockSelection, selectionLocked ? "Unlock Selection" : "Lock Selection", 16f, 1f);
            page.Add(selectionLockHolder);
        }
        void DrawCreatePhysicsSceneHolder()
        {
            VisualElement createSceneHolder = new VisualElement();
            createSceneHolder.style.height = GetStyleLengthForPercentage(9f);
            createSceneHolder.style.flexDirection = FlexDirection.Row;
            createSceneHolder.style.paddingTop = 20;

            DrawButton(createSceneHolder, CreateAndInitializeScene, hasScene ? "Update Physics Scene" : "Create Physics Scene", 16f, 1f);
            if (hasScene) DrawButton(createSceneHolder, CloseScene, "Close Physics Scene", 16f, 1f);
            page.Add(createSceneHolder);


            void CloseScene()
            {
                ClosePhysicsSceneIfOpen();
                Draw();
            }
        }

        void DrawSimulateHolder()
        {
            VisualElement simulateHolder = new VisualElement();
            simulateHolder.style.height = GetStyleLengthForPercentage(7f);
            simulateHolder.style.flexDirection = FlexDirection.Row;
            simulateHolder.style.paddingTop = 5;

            FloatField timeField = new FloatField();
            timeField.RegisterValueChangedCallback(SetSimulationTime);
            timeField.style.flexGrow = 2;
            timeField.style.fontSize = 16;
            timeField.value = simulationTime;

            simulateHolder.Add(timeField);
            DrawButton(simulateHolder, BeginSimulation, "Simulate", 16f, 9f);
            if (hasScene && simulatedObjects.Count != 0) DrawButton(simulateHolder, ApplySimulation, "Apply Simulation", 16f, 9f);
            page.Add(simulateHolder);
        }
        void DrawTimeline()
        {
            Label timelineLabel = new Label("Timeline");
            timelineLabel.style.fontSize = 16;
            timelineLabel.style.paddingTop = 5;

            Slider timelineSlider = new Slider(0f, 1f);
            timelineSlider.style.paddingLeft = 15;
            timelineSlider.style.paddingRight = 15;
            timelineSlider.showInputField = true;
            timelineSlider.SetValueWithoutNotify(timelineProgress);
            timelineSlider.RegisterValueChangedCallback(SetPreviewTime);

            page.Add(timelineLabel);
            page.Add(timelineSlider);
        }

        void DrawSettingsFoldout()
        {
            Foldout settingsFoldout = new Foldout();
            settingsFoldout.text = "Rigidbody Settings";
            settingsFoldout.value = showSettings;
            settingsFoldout.RegisterValueChangedCallback((callback) => showSettings = callback.newValue);


            Foldout velocityFoldout = new Foldout();
            velocityFoldout.text = "Initial Velocity";
            velocityFoldout.value = showVelocities;
            velocityFoldout.RegisterValueChangedCallback((callback) => showVelocities = callback.newValue);


            Toggle relativeToggle = new Toggle("Relative Velocity");
            relativeToggle.SetValueWithoutNotify(relativeVelocity);
            relativeToggle.RegisterValueChangedCallback((callback) => relativeVelocity = callback.newValue);

            Vector3Field minVelocityField = new Vector3Field("Min Velocity");
            minVelocityField.SetValueWithoutNotify(minVelocity);
            minVelocityField.RegisterValueChangedCallback((callback) => minVelocity = callback.newValue);

            Vector3Field maxVelocityField = new Vector3Field("Max Velocity");
            maxVelocityField.SetValueWithoutNotify(maxVelocity);
            maxVelocityField.RegisterValueChangedCallback((callback) => maxVelocity = callback.newValue);

            Vector3Field minAngularVelocityField = new Vector3Field("Min Angular Velocity");
            minAngularVelocityField.SetValueWithoutNotify(minAngularVelocity);
            minAngularVelocityField.RegisterValueChangedCallback((callback) => minAngularVelocity = callback.newValue);

            Vector3Field maxAngularVelocityField = new Vector3Field("Max Angular Velocity");
            maxAngularVelocityField.SetValueWithoutNotify(maxAngularVelocity);
            maxAngularVelocityField.RegisterValueChangedCallback((callback) => maxAngularVelocity = callback.newValue);


            Foldout constraintsFoldout = new Foldout();
            constraintsFoldout.text = "Constraints";
            constraintsFoldout.value = showConstraints;
            constraintsFoldout.RegisterValueChangedCallback((callback) => showConstraints = callback.newValue);

            VisualElement freezeHolder = new VisualElement();
            freezeHolder.style.flexDirection = FlexDirection.Row;
            VisualElement freezeLabelHolder = new VisualElement();
            freezeLabelHolder.style.paddingRight = 80f;
            VisualElement freezePosRotHolder = new VisualElement();
            VisualElement freezePosRotTop = new VisualElement();
            freezePosRotTop.style.flexDirection = FlexDirection.Row;
            VisualElement freezePosRotBottom = new VisualElement();
            freezePosRotBottom.style.flexDirection = FlexDirection.Row;

            Label freezePosLabel = new Label("Position");
            Label freezeRotLabel = new Label("Rotation");

            Toggle freezeXToggle = new Toggle("X");
            freezeXToggle.SetValueWithoutNotify(freezeXPos);
            freezeXToggle.RegisterValueChangedCallback((callback) => freezeXPos = callback.newValue);

            Toggle freezeYToggle = new Toggle("Y");
            freezeYToggle.SetValueWithoutNotify(freezeYPos);
            freezeYToggle.RegisterValueChangedCallback((callback) => freezeYPos = callback.newValue);

            Toggle freezeZToggle = new Toggle("Z");
            freezeZToggle.SetValueWithoutNotify(freezeZPos);
            freezeZToggle.RegisterValueChangedCallback((callback) => freezeZPos = callback.newValue);

            Toggle freezeXRotToggle = new Toggle("X");
            freezeXRotToggle.SetValueWithoutNotify(freezeXRot);
            freezeXRotToggle.RegisterValueChangedCallback((callback) => freezeXRot = callback.newValue);

            Toggle freezeYRotToggle = new Toggle("Y");
            freezeYRotToggle.SetValueWithoutNotify(freezeYRot);
            freezeYRotToggle.RegisterValueChangedCallback((callback) => freezeYRot = callback.newValue);

            Toggle freezeZRotToggle = new Toggle("Z");
            freezeZRotToggle.SetValueWithoutNotify(freezeZRot);
            freezeZRotToggle.RegisterValueChangedCallback((callback) => freezeZRot = callback.newValue);


            Toggle useGravityToggle = new Toggle("Use Gravity");
            useGravityToggle.SetValueWithoutNotify(useGravity);
            useGravityToggle.RegisterValueChangedCallback((callback) => useGravity = callback.newValue);

            FloatField massField = new FloatField("Mass");
            massField.SetValueWithoutNotify(mass);
            massField.RegisterValueChangedCallback((callback) => mass = callback.newValue);

            FloatField dragField = new FloatField("Drag");
            dragField.SetValueWithoutNotify(drag);
            dragField.RegisterValueChangedCallback((callback) => drag = callback.newValue);

            FloatField angularDragField = new FloatField("Angular Drag");
            angularDragField.SetValueWithoutNotify(angularDrag);
            angularDragField.RegisterValueChangedCallback((callback) => angularDrag = callback.newValue);


            velocityFoldout.Add(relativeToggle);
            velocityFoldout.Add(minVelocityField);
            velocityFoldout.Add(maxVelocityField);
            velocityFoldout.Add(minAngularVelocityField);
            velocityFoldout.Add(maxAngularVelocityField);

            freezeLabelHolder.Add(freezePosLabel);
            freezeLabelHolder.Add(freezeRotLabel);
            freezePosRotHolder.Add(freezePosRotTop);
            freezePosRotHolder.Add(freezePosRotBottom);

            freezePosRotTop.Add(freezeXToggle);
            freezePosRotTop.Add(freezeYToggle);
            freezePosRotTop.Add(freezeZToggle);
            freezePosRotBottom.Add(freezeXRotToggle);
            freezePosRotBottom.Add(freezeYRotToggle);
            freezePosRotBottom.Add(freezeZRotToggle);

            freezeHolder.Add(freezeLabelHolder);
            freezeHolder.Add(freezePosRotHolder);
            constraintsFoldout.Add(freezeHolder);

            settingsFoldout.Add(velocityFoldout);
            settingsFoldout.Add(constraintsFoldout);
            settingsFoldout.Add(useGravityToggle);
            settingsFoldout.Add(massField);
            settingsFoldout.Add(dragField);
            settingsFoldout.Add(angularDragField);
            page.Add(settingsFoldout);
        }

        #endregion
    }

    Dictionary<Transform, List<Vector3>> positionKeyFrames = new Dictionary<Transform, List<Vector3>>();
    Dictionary<Transform, List<Quaternion>> rotationKeyFrames = new Dictionary<Transform, List<Quaternion>>();
    void BeginSimulation()
    {
        if (!selectionLocked) ToggleLockSelection();

        simulatedObjects.Clear();

        foreach (Transform t in selectionTransforms)
        {
            if (objectAvailability[t] < 2) simulatedObjects.Add(t);
        }

        Simulate();
        Draw();

        Debug.Log($"Simulation finished");

        void Simulate()
        {
            UpdateSelectedObjectsToScene();

            foreach (GameObject sceneObject in openSceneObjects)
            {
                if (sceneObject != null) sceneObject.SetActive(false);
            }

            SimulationMode beforeMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            
            positionKeyFrames.Clear();
            rotationKeyFrames.Clear();
            foreach (Transform t in simulatedObjectClones)
            {
                positionKeyFrames[t] = new List<Vector3>();
                rotationKeyFrames[t] = new List<Quaternion>();
            }

            int ticks = (int)(simulationTime / Time.fixedDeltaTime);
            const int ticksPerKeyFrame = 5;
            int ticksSinceLastKeyFrame = 0;

            Debug.Log($"Starting simulation lasting {simulationTime} seconds or {ticks} ticks");

            AddKeyFrames();

            for (int tick = 0; tick < ticks; tick++)
            {
                physicsScene.Simulate(Time.fixedDeltaTime);

                if (ticksSinceLastKeyFrame >= ticksPerKeyFrame)
                {
                    AddKeyFrames();
                    ticksSinceLastKeyFrame = 0;
                }
                else ticksSinceLastKeyFrame++;
            }

            if (ticksSinceLastKeyFrame != 0) AddKeyFrames();

            foreach (Transform t in simulatedObjectClones)
            {
                objectKeyFrames[t] = new KeyFrameInfo(positionKeyFrames[t].ToArray(), rotationKeyFrames[t].ToArray());
            }

            Physics.simulationMode = beforeMode;

            foreach (GameObject sceneObject in openSceneObjects)
            {
                if (sceneObject != null) sceneObject.SetActive(true);
            }

            SetPreviewTime(timelineProgress);


            void UpdateSelectedObjectsToScene()
            {
                foreach (Transform t in simulatedObjectClones)
                {
                    if (t != null) GameObject.DestroyImmediate(t.gameObject);
                }

                simulatedObjectClones.Clear();

                if (useSetSeed) Random.InitState(seed);

                foreach (Transform t in simulatedObjects)
                {
                    GameObject go = GameObject.Instantiate(t.gameObject, t.position, t.rotation);
                    if (!go.TryGetComponent(out Rigidbody rb))
                    {
                        rb = go.AddComponent<Rigidbody>();

                        rb.useGravity = useGravity;
                        rb.mass = mass;
                        rb.drag = drag;
                        rb.angularDrag = angularDrag;

                        RigidbodyConstraints constraints = RigidbodyConstraints.None;
                        if (freezeXPos) constraints |= RigidbodyConstraints.FreezePositionX;
                        if (freezeYPos) constraints |= RigidbodyConstraints.FreezePositionY;
                        if (freezeZPos) constraints |= RigidbodyConstraints.FreezePositionZ;
                        if (freezeXRot) constraints |= RigidbodyConstraints.FreezeRotationX;
                        if (freezeYRot) constraints |= RigidbodyConstraints.FreezeRotationY;
                        if (freezeZRot) constraints |= RigidbodyConstraints.FreezeRotationZ;
                        rb.constraints = constraints;
                    }

                    if (relativeVelocity)
                    {
                        rb.velocity = t.forward * Mathf.Lerp(minVelocity.z, maxVelocity.z, Random.Range(0f, 1f)) + t.right * Mathf.Lerp(minVelocity.x, maxVelocity.x, Random.Range(0f, 1f)) + t.up * Mathf.Lerp(minVelocity.y, maxVelocity.y, Random.Range(0f, 1f));
                        rb.velocity = t.forward * Mathf.Lerp(minAngularVelocity.z, maxAngularVelocity.z, Random.Range(0f, 1f)) + t.right * Mathf.Lerp(minAngularVelocity.x, maxAngularVelocity.x, Random.Range(0f, 1f)) + t.up * Mathf.Lerp(minAngularVelocity.y, maxAngularVelocity.y, Random.Range(0f, 1f));
                    }
                    else
                    {
                        rb.velocity = new Vector3(Random.Range(minVelocity.x, maxVelocity.x), Random.Range(minVelocity.y, maxVelocity.y), Random.Range(minVelocity.z, maxVelocity.z));
                        rb.angularVelocity = new Vector3(Random.Range(minAngularVelocity.x, maxAngularVelocity.x), Random.Range(minAngularVelocity.y, maxAngularVelocity.y), Random.Range(minAngularVelocity.z, maxAngularVelocity.z));
                    }

                    simulatedObjectClones.Add(go.transform);
                    EditorSceneManager.MoveGameObjectToScene(go, scene);
                }
            }
            void AddKeyFrames()
            {
                foreach (Transform t in simulatedObjectClones)
                {
                    positionKeyFrames[t].Add(t.position);
                    rotationKeyFrames[t].Add(t.rotation);
                }
            }
        }
    }
    void ApplySimulation()
    {
        Undo.RecordObjects(simulatedObjects.ToArray(), "Apply HK Physics Tool Simulation");

        for (int i = 0; i < simulatedObjects.Count; i++)
        {
            simulatedObjects[i].position = simulatedObjectClones[i].position;
            simulatedObjects[i].rotation = simulatedObjectClones[i].rotation;
        }
        simulatedObjects.Clear();

        foreach (Transform t in simulatedObjectClones)
        {
            GameObject.DestroyImmediate(t.gameObject);
        }

        Draw();
    }

    void CreateAndInitializeScene()
    {
        ClosePhysicsSceneIfOpen();

        Scene beforeActiveScene = EditorSceneManager.GetActiveScene();
        int sceneCount = EditorSceneManager.sceneCount;

        List<GameObject> sceneObjects = new List<GameObject>();

        for (int i = 0; i < sceneCount; i++)
        {
            Scene sourceScene = EditorSceneManager.GetSceneAt(i);
            GameObject[] rootObjects = sourceScene.GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects)
            {
                if (rootObject.isStatic && rootObject.activeSelf && rootObject.TryGetComponent(out Collider coll)) sceneObjects.Add(rootObject);
                AddAllChildrenToList(rootObject);
            }

            void AddAllChildrenToList(GameObject obj)
            {
                if (obj.activeSelf) openSceneObjects.Add(obj);

                foreach (Transform t in obj.transform)
                {
                    AddAllChildrenToList(t.gameObject);

                    if (t.gameObject.isStatic && t.gameObject.activeSelf && t.TryGetComponent(out Collider coll)) sceneObjects.Add(t.gameObject);
                }
            }
        }

        scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene.name = "HK_Physics_Temp_Simulation_Scene";

        foreach (GameObject obj in sceneObjects)
        {
            if (proBuilderType != null && obj.TryGetComponent(proBuilderType, out Component comp)) continue;

            GameObject go = GameObject.Instantiate(obj, obj.transform.position, obj.transform.rotation);
            if (go.TryGetComponent(out MeshRenderer renderer)) GameObject.DestroyImmediate(renderer);
            EditorSceneManager.MoveGameObjectToScene(go, scene);
        }

        physicsScene = scene.GetPhysicsScene();

        EditorSceneManager.SetActiveScene(beforeActiveScene);

        hasScene = true;
        Draw();
    }
    void ClosePhysicsSceneIfOpen()
    {
        Scene foundScene = EditorSceneManager.GetSceneByName("HK_Physics_Temp_Simulation_Scene");

        if (hasScene || foundScene.IsValid())
        {
            EditorSceneManager.CloseScene(foundScene, true);
            hasScene = false;
        }
    }

    void EditorSelectionChanged()
    {
        if (!selectionLocked)
        {
            selectionTransforms = Selection.transforms;

            UpdateSelectionData();
            Draw();
        }
    }
    void UpdateSelectionData()
    {
        objectAvailability.Clear();

        foreach (Transform transform in selectionTransforms)
        {
            objectAvailability[transform] = GetAvailabilityStatusForTransform(transform);
        }

        selectionTransforms = selectionTransforms.ToList().OrderBy(t => objectAvailability[t])
            .ThenBy(t => t.name)
            .ToArray();

        int GetAvailabilityStatusForTransform(Transform t)
        {
            if (t.gameObject.isStatic || !t.gameObject.activeSelf) return 2;
            else if (proBuilderType != null && t.TryGetComponent(proBuilderType, out Component comp)) return 2;

            Rigidbody rb = t.GetComponent<Rigidbody>();
            Collider collider = t.GetComponent<Collider>();

            if (!collider) return 2;
            else if (rb) return 0;
            else return 1;
        }
    }


    void ToggleLockSelection()
    {
        selectionLocked = !selectionLocked;

        Draw();
    }
    void SetSimulationTime(ChangeEvent<float> changeEvent) => simulationTime = changeEvent.newValue;
    void SetPreviewTime(ChangeEvent<float> changeEvent) => HiddenSetPreviewTime(changeEvent.newValue);
    void SetPreviewTime(float manualTime) => HiddenSetPreviewTime(manualTime);

    void HiddenSetPreviewTime(float time)
    {
        if (simulatedObjectClones.Count == 0) return;

        timelineProgress = time;

        int totalKeyFrames = objectKeyFrames[simulatedObjectClones[0]].objectPositionKeyframes.Length;
        float progressPerKeyFrame = 1f / (totalKeyFrames - 1);
        int keyFrameStartIndex = (int)(timelineProgress / progressPerKeyFrame);
        int keyFrameEndIndex = keyFrameStartIndex == totalKeyFrames - 1 ? keyFrameStartIndex : keyFrameStartIndex + 1;

        foreach (Transform t in simulatedObjectClones)
        {
            t.position = GetPositionForTransform(t);
            t.rotation = GetRotationForTransform(t);
        }

        Vector3 GetPositionForTransform(Transform t)
        {
            Vector3 startPos = objectKeyFrames[t].objectPositionKeyframes[keyFrameStartIndex];
            Vector3 endPos = objectKeyFrames[t].objectPositionKeyframes[keyFrameEndIndex];

            return Vector3.Lerp(startPos, endPos, timelineProgress % progressPerKeyFrame / progressPerKeyFrame);
        }

        Quaternion GetRotationForTransform(Transform t)
        {
            Quaternion startPos = objectKeyFrames[t].objectRotationKeyframes[keyFrameStartIndex];
            Quaternion endPos = objectKeyFrames[t].objectRotationKeyframes[keyFrameEndIndex];

            return Quaternion.Lerp(startPos, endPos, timelineProgress % progressPerKeyFrame / progressPerKeyFrame);
        }
    }
}

struct KeyFrameInfo
{
    public Vector3[] objectPositionKeyframes { get; private set; }
    public Quaternion[] objectRotationKeyframes { get; private set; }

    public KeyFrameInfo(Vector3[] positionKeyFrames, Quaternion[] rotationKeyframes)
    {
        objectPositionKeyframes = positionKeyFrames;
        objectRotationKeyframes = rotationKeyframes;
    }
}
