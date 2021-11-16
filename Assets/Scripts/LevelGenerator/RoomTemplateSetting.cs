using Sirenix.OdinInspector;
using System;

[Serializable]
public class RoomTemplateSetting
{
    [HideLabel]
    public RoomTemplateScriptableObject room;

    [HideLabel]
    [Title("Activated")]
    public bool activated;
}
