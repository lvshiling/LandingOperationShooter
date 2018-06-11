using EazyTools.SoundManager;
using UnityEngine;

public enum sndType
{
    back,
    front,
    fx,
    btn
}

[System.Serializable]
public class sndSetup
{
    public string name;
    public AudioClip clip;
    public float volume = 1;
    public Audio clipSetup;
    public sndType type;
}

public class SndManager : MonoBehaviour
{
    private static SndManager _instance;

    public static SndManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SndManager>();
            }
            return _instance;
        }
    }

    public sndSetup[] sounds;

    private void Start()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            switch (sounds[i].type)
            {
                case sndType.back:
                    if (sounds[i].clipSetup != null && sounds[i].clipSetup.paused)
                    {
                        sounds[i].clipSetup.Resume();
                    }
                    else
                    {
                        sounds[i].clipSetup = SoundManager.GetAudio(SoundManager.PlayMusic(sounds[i].clip, 0, true, false));
                    }
                    break;

                case sndType.fx:
                    sounds[i].clipSetup = SoundManager.GetAudio(SoundManager.PlaySound(sounds[i].clip, 0));
                    break;
            }
        }

        SetOldVAlue();
    }

    private void SetOldVAlue()
    {
        oldValume = new float[sounds.Length];
        for (int i = 0; i < sounds.Length; i++)
        {
            oldValume[i] = sounds[i].volume;
        }
    }

    public void Play(string name)
    {
        sndSetup currentSnd = GetBuyName(name);
        int audioID = 0;
        if (currentSnd == null)
        {
            return;
        }
        switch (currentSnd.type)
        {
            case sndType.back:

                SoundManager.PauseAllMusic();
                audioID = SoundManager.PlayMusic(currentSnd.clip, currentSnd.volume, true, false);
                SoundManager.GetAudio(audioID).SetVolume(currentSnd.volume);

                currentSnd.clipSetup = SoundManager.GetAudio(audioID);
                SoundManager.GetAudio(audioID).Play(currentSnd.volume);
                currentSnd.clipSetup.Resume();
                //SoundManager.GetAudio(audioID).Play(currentSnd.volume);

                break;

            case sndType.fx:
                audioID = SoundManager.PlaySound(currentSnd.clip, currentSnd.volume);
                SoundManager.GetAudio(audioID).Play(currentSnd.volume);
                currentSnd.clipSetup = SoundManager.GetAudio(audioID);
                break;

            case sndType.front:

                audioID = SoundManager.PlaySound(currentSnd.clip, currentSnd.volume);
                if (!SoundManager.GetAudio(audioID).playing)
                {
                    SoundManager.GetAudio(audioID).Play(currentSnd.volume);
                    currentSnd.clipSetup = SoundManager.GetAudio(audioID);
                }

                break;
        }
    }

    public void Stop(string name)
    {
        sndSetup currentSnd = GetBuyName(name);
        int audioID = 0;
        if (currentSnd == null)
        {
            return;
        }

        audioID = SoundManager.PlaySound(currentSnd.clip, currentSnd.volume);
        if (SoundManager.GetAudio(audioID).playing)
        {
            SoundManager.GetAudio(audioID).Pause();
            currentSnd.clipSetup = SoundManager.GetAudio(audioID);
        }
    }

    public sndSetup GetBuyName(string name)
    {
        foreach (sndSetup snd in sounds)
        {
            if (snd.name == name)
            {
                return snd;
            }
        }
        TestDebugLog.Instance.DebugLog(name + " Такого звука нет");
        return null;
    }

    private float[] oldValume;

    public void SoundValueOff(string nameSoundException)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (!nameSoundException.Equals(sounds[i].name))
            {
                sounds[i].volume = 0;
            }
        }
    }

    public void SoundValueOff()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            sounds[i].volume = 0;
        }
    }

    public void SoundFxValueOff()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].type == sndType.fx)
            {
                sounds[i].volume = 0;
            }
        }
    }

    public void SoundValueOn()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            sounds[i].volume = oldValume[i];
        }

    }
}