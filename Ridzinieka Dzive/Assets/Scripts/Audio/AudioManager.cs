using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("UI Sounds")]
    public AudioClip buttonClick;
    public AudioClip notification;
    public AudioClip alarm;

    [Header("SFX")]
    public AudioClip door;
    public AudioClip fridge_open;
    public AudioClip eat;

    [Header("Fridge")]
    public AudioClip fridge_hum; // loop only when fridge panel is open

    [Header("Ambience")]
    public AudioClip outside_bg;   // you will add this later
    public AudioClip store_bg;     // shopping ambience
    public AudioClip store_work;   // cashier job ambience
    public AudioClip office_bg;    // office job ambience
    public AudioClip traffic_bg;   // taxi job ambience

    [Header("Audio Sources")]
    public AudioSource uiSource;
    public AudioSource sfxSource;
    public AudioSource ambienceSource;

    private GameManager gameManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
            gameManager.OnLocationChanged += HandleLocationChanged;
    }

    // ------------------------------
    // PLAY METHODS
    // ------------------------------

    public void PlayUI(AudioClip clip)
    {
        if (clip != null)
            uiSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayAmbience(AudioClip clip)
    {
        ambienceSource.Stop();

        if (clip == null)
        {
            ambienceSource.clip = null;
            return;
        }

        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }

    public void StopAmbience()
    {
        ambienceSource.Stop();
        ambienceSource.clip = null;
    }

    // ------------------------------
    // FRIDGE HUM CONTROL
    // ------------------------------

    public void PlayFridgeHum(bool on)
    {
        if (on)
        {
            PlayAmbience(fridge_hum);
        }
        else
        {
            StopAmbience();
            HandleLocationChanged(gameManager.CurrentLocation); 
        }
    }

    // ------------------------------
    // LOCATION HANDLING
    // ------------------------------

    private void HandleLocationChanged(GameManager.Location loc)
    {
        StopAmbience();

        switch (loc)
        {
            case GameManager.Location.Home:
                break;

            case GameManager.Location.Outside:
                PlayAmbience(outside_bg);
                break;

            case GameManager.Location.Shop:
                PlayAmbience(store_bg);
                break;

            case GameManager.Location.Work:
                HandleWorkAmbience();
                break;
        }
    }

    private void HandleWorkAmbience()
    {
        if (gameManager == null) return;

        switch (gameManager.SelectedJob)
        {
            case GameManager.JobType.Office:
                PlayAmbience(office_bg);
                break;

            case GameManager.JobType.Taxi:
                PlayAmbience(traffic_bg);
                break;

            case GameManager.JobType.Cashier:
                PlayAmbience(store_work);
                break;
        }
    }
}
