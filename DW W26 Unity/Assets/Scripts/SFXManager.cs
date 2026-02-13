using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip runnerSpawnClip;
    [SerializeField] private AudioClip runnerDeathClip;
    [SerializeField] private AudioClip sniperShotClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayRunnerSpawn()
    {
        Play(runnerSpawnClip);
    }

    public void PlayRunnerDeath()
    {
        Play(runnerDeathClip);
    }

    public void PlaySniperShot()
    {
        Play(sniperShotClip);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
