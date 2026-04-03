using UnityEngine;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "SlotApp/Symbol")]
public class Symbol : ScriptableObject
{
    public int symbolId;
    public string symbolName;
    public Sprite sprite;
}
