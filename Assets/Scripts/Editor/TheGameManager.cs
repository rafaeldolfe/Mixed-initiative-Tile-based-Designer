using MissionGrammar;
using ShapeGrammar;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TheGameManager : OdinMenuEditorWindow
{
    [OnValueChanged("StateChange")]
    [LabelText("Manager View")]
    [LabelWidth(100f)]
    [EnumToggleButtons]
    [ShowInInspector]
    private ManagerState managerState;

    private int enumIndex = 0;
    private bool treeRebuild = false;

    private DrawSelected<MissionRuleScriptableObject> drawMissionRules = new DrawSelected<MissionRuleScriptableObject>();
    private DrawSelected<SpaceRuleScriptableObject> drawSpaceRules = new DrawSelected<SpaceRuleScriptableObject>();

    private string missionRulesPath = Constants.MISSION_RULES_PATH;
    private string spaceRulesPath = Constants.SPACE_RULES_PATH;

    [MenuItem("Tools/Manager")]
    public static void OpenWindow()
    {
        GetWindow<TheGameManager>().Show();
    }

    private void StateChange()
    {
        treeRebuild = true;
    }

    protected override void Initialize()
    {
        drawMissionRules.SetPath(missionRulesPath);
        drawSpaceRules.SetPath(spaceRulesPath);

        //drawCheatManager.FindMyObject();
        //drawDBM.FindMyObject();
    }

    protected override void OnGUI()
    {
        if (treeRebuild && Event.current.type == EventType.Layout)
        {
            ForceMenuTreeRebuild();
            treeRebuild = false;
        }
        SirenixEditorGUI.Title("The Manager", "Editor tool to aid creation of missions", TextAlignment.Center, true);
        EditorGUILayout.Space();
        switch (managerState)
        {
            case ManagerState.missionRules:
            case ManagerState.spaceRules:
                // First run has the DrawEditor give an
                // index out of bounds error
                try
                {
                    DrawEditor(enumIndex);
                }
                catch
                {
                }
                break;

            default:
                break;
        }
        EditorGUILayout.Space();
        base.OnGUI();
    }

    protected override void DrawEditors()
    {
        switch (managerState)
        {
            case ManagerState.missionRules:
                //DrawEditor(enumIndex);
                drawMissionRules.SetSelected(this.MenuTree.Selection.SelectedValue);
                break;

            case ManagerState.spaceRules:
                drawSpaceRules.SetSelected(this.MenuTree.Selection.SelectedValue);
                break;

            default:
                break;
        }

        DrawEditor((int)managerState);
    }

    protected override IEnumerable<object> GetTargets()
    {
        List<object> targets = new List<object>();
        targets.Add(drawMissionRules);
        targets.Add(drawSpaceRules);
        targets.Add(base.GetTarget());

        enumIndex = targets.Count - 1;

        return targets;
    }

    protected override void DrawMenu()
    {
        switch (managerState)
        {
            case ManagerState.missionRules:
            case ManagerState.spaceRules:
                base.DrawMenu();
                break;

            default:
                break;
        }
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new OdinMenuTree();

        switch (managerState)
        {
            case ManagerState.missionRules:
                tree.AddAllAssetsAtPath("Rules", missionRulesPath, typeof(MissionRuleScriptableObject));
                tree.SortMenuItemsByName();
                break;

            case ManagerState.spaceRules:
                tree.AddAllAssetsAtPath("Rules", spaceRulesPath, typeof(SpaceRuleScriptableObject));
                tree.SortMenuItemsByName();
                break;

            default:
                break;
        }
        return tree;
    }

    private void GetMenusAtPath(OdinMenuTree tree, string path)
    {
        string[] directories = Directory.GetDirectories(Application.persistentDataPath);

        foreach (string directory in directories)
        {
            OdinMenuTree subtree = new OdinMenuTree();
            //tree.Add(subtree);
        }

        //tree.Add
    }

    public enum ManagerState
    {
        missionRules,
        spaceRules
    }
}

public class DrawSelected<T> where T : ScriptableObject
{
    [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
    public T selected;

    //[LabelWidth(100)]
    //[HorizontalGroup("CreateNew/Horizontal")]
    //public string nameForNew; s

    private string path;

    [PropertyOrder(-1)]
    [ColorFoldoutGroup("CreateNew", 0f, 1f, 0f)]
    [HorizontalGroup("CreateNew/Horizontal")]
    [GUIColor(0.7f, 0.7f, 1f)]
    [Button]
    public void CreateNew()
    {
        ScriptableObjectCreator.ShowDialog<T>(path, obj =>
        {
        });
    }

    [HorizontalGroup("CreateNew/Horizontal")]
    [GUIColor(1f, 0.7f, 0.7f)]
    [Button]
    public void DeleteSelected()
    {
        if (selected != null)
        {
            string _path = AssetDatabase.GetAssetPath(selected);
            AssetDatabase.DeleteAsset(_path);
            AssetDatabase.SaveAssets();
        }
    }

    public void SetSelected(object item)
    {
        var attempt = item as T;
        if (attempt != null)
            this.selected = attempt;
    }

    public void SetPath(string path)
    {
        this.path = path;
    }
}

public class DrawSceneObject<T> where T : MonoBehaviour
{
    [Title("Univese Creator")]
    [ShowIf("@myObject != null")]
    [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
    public T myObject;

    public void FindMyObject()
    {
        if (myObject == null)
        {
            myObject = GameObject.FindObjectOfType<T>();
        }
    }

    [ShowIf("@myObject != null")]
    [GUIColor(0.7f, 1f, 0.7f)]
    [ButtonGroup("Top Button", -1000)]
    public void SelectSceneObject()
    {
        if (myObject != null)
            Selection.activeGameObject = myObject.gameObject;
    }

    [ShowIf("@myObject != null")]
    [Button]
    private void CreateManagerObject()
    {
        GameObject newManager = new GameObject();
        newManager.name = "New " + typeof(T).ToString();
        myObject = newManager.AddComponent<T>();
    }
}

//public class DrawUnit : DrawSelected<UnitScriptableObject>
//{
//}
public class ColorFoldoutGroupAttribute : PropertyGroupAttribute
{
    public float R, G, B, A;

    public ColorFoldoutGroupAttribute(string path) : base(path)
    {
    }

    public ColorFoldoutGroupAttribute(string path, float r, float g, float b, float a = 1f) : base(path)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    protected override void CombineValuesWith(PropertyGroupAttribute other)
    {
        var otherAttr = (ColorFoldoutGroupAttribute)other;

        this.R = Math.Max(otherAttr.R, this.R);
        this.G = Math.Max(otherAttr.G, this.G);
        this.B = Math.Max(otherAttr.B, this.B);
        this.A = Math.Max(otherAttr.A, this.A);
    }
}

public class ColorFoldoutGroupAttributeDrawer : OdinGroupDrawer<ColorFoldoutGroupAttribute>
{
    private LocalPersistentContext<bool> isExpanded;

    protected override void Initialize()
    {
        this.isExpanded = this.GetPersistentValue<bool>("ColorFoldoutGroupAttributeDrawer.isExpanded",
            GeneralDrawerConfig.Instance.ExpandFoldoutByDefault);
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        GUIHelper.PushColor(new Color(this.Attribute.R, this.Attribute.G, this.Attribute.B, this.Attribute.A));
        SirenixEditorGUI.BeginBox();
        SirenixEditorGUI.BeginBoxHeader();
        GUIHelper.PopColor();

        this.isExpanded.Value = SirenixEditorGUI.Foldout(this.isExpanded.Value, label);
        SirenixEditorGUI.EndBoxHeader();

        if (SirenixEditorGUI.BeginFadeGroup(this, this.isExpanded.Value))
        {
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                this.Property.Children[i].Draw();
            }
        }
        SirenixEditorGUI.EndFadeGroup();
        SirenixEditorGUI.EndBox();
    }
}