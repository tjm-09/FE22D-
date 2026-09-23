using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseMenu.activeSelf)
            {
                Time.timeScale = 0f;
                pauseMenu.SetActive(true);
                Cursor.visible = true;
            }
            else
            {
                               Time.timeScale = 1f;
                pauseMenu.SetActive(false);
                Cursor.visible = false;
                
            }
        }
    }

    public void ResumeGame()
    {
        
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}