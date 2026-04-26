using UnityEngine;
using UnityEngine.InputSystem;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource sfxSource;
    public AudioClip pickupSound;
    public AudioClip hitSound;

    void Awake()
    {
        instance = this;
    }

    public void PlayPickup()
    {
        sfxSource.PlayOneShot(pickupSound);
    }
    
    public void PlayHit()
    {
        sfxSource.PlayOneShot(hitSound);
    }
  
}