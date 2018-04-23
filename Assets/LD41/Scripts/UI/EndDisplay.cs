using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDisplay : BaseUIElement {
    public override void Show(bool instant = false)
    {
        base.Show(instant);
        AudioManager.Instance.PlayEndMusic();
    }

    public void Restart()
    {
        Main.Instance.RestartGame();
    }
}
