using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChessMenu : MonoBehaviour
{
    public void OnPvAIClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void OnExit()
    {
        Application.Quit();
    }
}
