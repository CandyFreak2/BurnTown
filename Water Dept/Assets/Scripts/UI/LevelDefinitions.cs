using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "WaterDept/LevelTypes")]
public class LevelDefinitions : ScriptableObject {
    public string levelName;
    public int buildIndex;
    public Sprite levelIcon;
}
