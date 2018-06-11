using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScene : MonoBehaviour {

    public GameController gameController;

    public GameObject player;
  
    private void OnEnable()
    {
        Time.timeScale = 1;

        SndManager.Instance.Play("disant");

        player.SetActive(false);

        GamePlayController.Instance.mainCamera.gameObject.SetActive(false);
    }

    public void OnEndAnimation()
    {
        gameController.StartGame();

        gameObject.SetActive(false);

        SndManager.Instance.Stop("wind");

        player.SetActive(true);
    }

    void PlaySndWind()
    {
        SndManager.Instance.Play("wind");
    }
}
