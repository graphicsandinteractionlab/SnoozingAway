﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour {

    public void LoadLevel1()
    {
        SceneManager.LoadScene("level1");
    }

    public void LoadMainMenue()
    {
        SceneManager.LoadScene("Menu");
    }
}
