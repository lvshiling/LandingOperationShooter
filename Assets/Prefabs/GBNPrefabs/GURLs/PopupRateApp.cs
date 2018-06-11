using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupRateApp : MonoBehaviour {

    [SerializeField]
    private bool startHidden = true;

    [SerializeField]
    private GameObject[] starsOn;
    [SerializeField]
    private GameObject[] starsOff;

    private string btnAssignInfo = "";

    private bool rated = false;

    private bool isShowing = false;

    public void Show(string url, string email = "")
    {
        if (!gameObject.activeSelf)
        {
            isShowing = true;

            ResetStars();

            AssignButtonsAction(url, email);

            gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        //защита от показа без инициализации
        if (!isShowing)
        {
            Hide();
            Debug.LogError("PopupRateApp Error: Use Show() to open popup!");
        }
    }

    private void AssignButtonsAction(string url, string email)
    {
        if (!btnAssignInfo.Equals(url + email))
        {
            btnAssignInfo = url + email;

            if (starsOff != null)
            {
                for (int i = 0; i < starsOff.Length; i++)
                {
                    if (starsOff[i] != null)
                    {
                        Button starBtn = starsOff[i].GetComponent<Button>();
                        int index = i;
                        if (starBtn != null)
                        {
                            starBtn.onClick.RemoveAllListeners();
                            starBtn.onClick.AddListener(() =>
                            {
                                OnStarButtonClick(index, url, email);
                            });
                            starBtn.enabled = true;
                        }
                    }
                }
            }
        }
    }

    private void OnStarButtonClick(int index, string url, string email)
    {
        if (rated)
        {
            return;
        }

        rated = true;

        index++; //numering from 1;

        for (int i = 0; i < Mathf.Min(starsOn.Length, index); i++)
        {
            starsOn[i].SetActive(true);
        }

        for (int i = 0; i < Mathf.Max(starsOff.Length, index); i++)
        {
            if (i < index)
            {
                starsOff[i].SetActive(false);
            }
            else
            {
                Button btn = starsOff[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.enabled = false;
                }
            }
        }

        if (index < 4 && !string.IsNullOrEmpty(email))
        {
            // Send Message
            StartCoroutine(OpenUrlOnNextFrame("mailto:" + email + "?subject=" + Application.productName + "&body=I rate this game at " + index.ToString()));
        }
        else
        {
            // Review App Normal
            StartCoroutine(OpenUrlOnNextFrame(url));
        }

        StartCoroutine(InvokeRealTime(Hide, 0.5f));
    }

    private IEnumerator OpenUrlOnNextFrame(string url)
    {
        yield return null;
        Application.OpenURL(url);
    }

    private IEnumerator InvokeRealTime(System.Action action, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (action != null)
        {
            action.Invoke();
        }
    }

    private void ResetStars()
    {
        rated = false;

        if (starsOn != null)
        {
            for (int i = 0; i < starsOn.Length; i++)
            {
                if (starsOn[i] != null)
                {
                    starsOn[i].SetActive(false);
                }
            }
        }
        if (starsOff != null)
        {
            for (int i = 0; i < starsOff.Length; i++)
            {
                if (starsOff[i] != null)
                {
                    starsOff[i].SetActive(true);
                    Button starBtn = starsOff[i].GetComponent<Button>();
                    if (starBtn != null)
                    {
                        starBtn.enabled = true;
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        isShowing = false;
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

	void Awake () {
        if (startHidden && !isShowing)
            Hide();
    }
}
