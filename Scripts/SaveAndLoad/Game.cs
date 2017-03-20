using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Game {

    /// <summary>
    /// Last level saved
    /// </summary>
    public string m_sLevelName = string.Empty;

    /// <summary>
    /// Last position Saved
    /// </summary>
    public string m_LastCheckPoint = string.Empty;

    /// <summary>
    /// All light vein in the level that was activated
    /// </summary>
    public List<string> m_LightVeinActivated = new List<string>();

    /// <summary>
    /// If the player completes the game, I start from the begining.
    /// </summary>
    public bool m_IsGameComplete = false;


}
