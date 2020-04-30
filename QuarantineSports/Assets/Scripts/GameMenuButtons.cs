using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuButtons : MonoBehaviour
{
    public void selectBackMenu()
    {
        SceneManager.LoadScene(0);
    }
}
