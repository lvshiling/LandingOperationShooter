using UnityEngine;

public class PlayTrigger : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayTriggerMusic()
    {
        SndManager.Instance.Play("Trigger");
    }

    public void PlayMusic(string name)
    {
        SndManager.Instance.Play(name);
    }

    private void OnShootAnimation()
    {
        //if (animator.GetBool("TakeAim") == false)
        //{
        //    GamePlayController.Instance.player.isControl = false;
        //    DOTween.defaultUpdateType = UpdateType.Late;
        //    Sequence sequence = DOTween.Sequence();
        //    sequence.Append(Camera.main.transform.DOLocalRotate(new Vector3(Camera.main.transform.localEulerAngles.x - 10, 0, 0), 0.2f))
        //        .Append(Camera.main.transform.DOLocalRotate(new Vector3(Camera.main.transform.localEulerAngles.x + 10, 0, 0), 0.2f))
        //        .OnComplete(() => GamePlayController.Instance.player.isControl = true);
        //}
    }
}