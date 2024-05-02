using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System;

public class HKToolsEditorWindow : EditorWindow
{
    VisualElement[] toolElements;

    static HKTool[] tools = new HKTool[] { new HKPhysicsSimulatorTool(), new HKAlignerTool() };
    static Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();

    static HKTool currentTool;

    [MenuItem("Tools/HK Tools")]
    public static void OpenWindow()
    {
        InitializeWindow();

        void InitializeWindow()
        {
            HKToolsEditorWindow wnd = GetWindow<HKToolsEditorWindow>();

            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/3rd Party/HietakissaUtils/Editor/Icons/Tools_Icon.png");
            wnd.titleContent = new GUIContent("HK Tools", icon);

            wnd.minSize = new Vector2(480, 240);
            wnd.maxSize = new Vector2(1280, 720);
        }
    }

    public void CreateGUI()
    {
        LoadIcons();
        CreateToolButtonList();

        VisualElement root = rootVisualElement;

        TwoPaneSplitView splitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
        root.Add(splitView);

        VisualElement leftPage = new VisualElement();
        VisualElement rightPage = new VisualElement();
        splitView.Add(leftPage);
        splitView.Add(rightPage);

        foreach (VisualElement tool in toolElements) leftPage.Add(tool);

        foreach (HKTool tool in tools) tool.SetPageElement(rightPage, this);
    }

    void OnDestroy()
    {
        if (currentTool != null) currentTool.Exit();
        currentTool = null;
    }

    void Update()
    {
        if (currentTool == null) return;
        currentTool.Update();
    }

    void CreateToolButtonList()
    {
        List<VisualElement> toolElements = new List<VisualElement>();

        int holderHeight = 35;

        for (int i = 0; i < tools.Length; i++)
        {
            int ID = i;
            HKTool tool = tools[ID];
            CreateHolder(tool.toolName, tool.iconName, () => SelectTool(ID));
        }

        this.toolElements = toolElements.ToArray();

        void CreateHolder(string buttonText, string iconName, System.Action onClickEvent)
        {
            VisualElement holder = new VisualElement();

            holder.style.height = holderHeight;
            holder.style.flexDirection = FlexDirection.Row;

            if (icons.TryGetValue(iconName, out Sprite spriteIcon))
            {
                Image icon = new Image();
                icon.sprite = spriteIcon;
                icon.style.width = holderHeight;
                icon.style.height = holderHeight;
                holder.Add(icon);
            }

            Button button = new Button(onClickEvent);
            button.text = buttonText;
            button.style.flexGrow = 1;

            holder.Add(button);
            toolElements.Add(holder);
        }
    }

    void SelectTool(int toolID)
    {
        HKTool tool = tools[toolID];
        
        if (currentTool != null)
        {
            if (tool == currentTool) return;
            currentTool.Exit();
        }

        currentTool = tool;
        tool.Enter();
    }

    void LoadIcons()
    {
        icons.Clear();

        string[] iconGUIDs = AssetDatabase.FindAssets("*_Icon", new string[] { "Assets/3rd Party/HietakissaUtils/Editor/Icons/" });
        foreach (string GUID in iconGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(GUID);
            icons[AssetPathToAssetName(path)] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        string AssetPathToAssetName(string path)
        {
            string[] split = path.Split("/");
            string[] asset = split[^1].Split(".");
            return asset[0];
        }
    }
}

public abstract class HKTool
{
    public abstract string toolName { get; }
    public abstract string iconName { get; }

    protected VisualElement page;
    protected HKToolsEditorWindow window;

    public virtual void Enter()
    {
        Debug.Log($"No Enter method for tool {toolName}. Override to draw your own GUI.");
    }

    public virtual void Exit()
    {

    }

    public virtual void Update()
    {

    }

    protected void DrawTitle()
    {
        Label title = new Label(toolName);
        title.style.alignSelf = Align.Center;
        title.style.fontSize = 16f;
        title.style.paddingTop = 5;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;

        page.Add(title);
    }

    protected VisualElement DrawButton(VisualElement element, Action onClickEvent, string buttonText = "", float fontSize = 12, float flexGrow = 0)
    {
        Button button = new Button(onClickEvent);
        button.text = buttonText;
        button.style.fontSize = fontSize;
        button.style.flexGrow = flexGrow;

        element.Add(button);
        return element;
    }

    protected VisualElement DrawToggle(VisualElement element, EventCallback<ChangeEvent<bool>> setValue, string name = "", bool defaultValue = false)
    {
        Toggle toggle = new Toggle();
        toggle.text = name;
        toggle.SetValueWithoutNotify(defaultValue);
        toggle.RegisterValueChangedCallback(setValue);

        element.Add(toggle);
        return element;
    }

    protected StyleLength GetStyleLengthForPercentage(float percentage)
    {
        return new StyleLength(new Length(percentage, LengthUnit.Percent));
    }

    public void SetPageElement(VisualElement pageElement, HKToolsEditorWindow editorWindow)
    {
        page = pageElement;
        window = editorWindow;
    }
}