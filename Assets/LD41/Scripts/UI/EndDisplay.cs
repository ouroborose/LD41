using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDisplay : BaseUIElement {
    public void Restart()
    {
        Main.Instance.RestartGame();
    }
}
