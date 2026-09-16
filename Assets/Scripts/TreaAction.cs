using UnityEngine;

[CreateAssetMenu(fileName = "TreaAction", menuName = "Scriptable Objects/TreaAction")]
public class TreaAction : ScriptableObject
{
    public ActionType actionType;
    public string actionName;
    public bool hasAnimation;
    public string animationName;
}
public enum ActionType
{
    Inverter,
    Reapter,
    Selector,
    Sequence
}