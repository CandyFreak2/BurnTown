using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial -", menuName = "WaterDept/Tutorial")]
public class TutorialEvents : ScriptableObject {
    public List<Tutorials> tutorials;
}

[Serializable]
public class Tutorials {
    [TextArea(2, 5)]
    public string title, description;
}
