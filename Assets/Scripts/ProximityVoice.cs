// ProximityVoice.cs
using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity; // Recorder, Speaker
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class ProximityVoice : MonoBehaviour
{
    [Header("Proximity Voice Settings")]
    [Tooltip("Max audible distance for voice (meters)")]
    public float maxDistance = 12f;
    [Tooltip("Min distance for full volume (meters)")]
    public float minDistance = 1f;
    [Tooltip("Use push-to-talk instead of voice detection")]
    public bool usePushToTalk = true;
    [Tooltip("Key for push-to-talk")]
    public KeyCode pushToTalkKey = KeyCode.T;

    private PhotonView pv;
    private Recorder recorder;
    private Speaker[] speakersCache;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        // The Recorder component should be attached to the prefab too (or child). Try to find it.
        recorder = GetComponentInChildren<Recorder>(true);
    }

    void Start()
    {
        // If this is the local player, enable Recorder and configure it
        if (pv.IsMine)
        {
            if (recorder == null)
            {
                Debug.LogWarning("ProximityVoice: No Recorder found on player prefab. Add one to the prefab or child.");
                return;
            }

            // Make sure this Recorder uses the scene's VoiceConnection (if not set)
            if (recorder.VoiceClient == null && VoiceConnection.Instance != null)
            {
                recorder.VoiceClient = VoiceConnection.Instance.Client;
            }

            // For local player, enable recording (we will toggle transmit)
            recorder.TransmitEnabled = !usePushToTalk && recorder.VoiceDetection;
            recorder.DebugEchoMode = false;

            // hide remote speakers on local client? no, we want them audible.
        }
        else
        {
            // For remote players ensure their Speaker's audio source uses 3D spatial settings.
            // We'll configure when a Speaker component appears.
            // Optional immediate pass to existing speakers:
            ConfigureExistingSpeakers();
        }
    }

    void Update()
    {
        if (!pv.IsMine || recorder == null) return;

        if (usePushToTalk)
        {
            // Push-to-talk: enable transmit only while key is held
            if (Input.GetKeyDown(pushToTalkKey))
            {
                recorder.TransmitEnabled = true;
            }
            else if (Input.GetKeyUp(pushToTalkKey))
            {
                recorder.TransmitEnabled = false;
            }
        }
        else
        {
            // If not push-to-talk and voice detection is disabled, keep transmit on
            if (!recorder.VoiceDetection)
            {
                recorder.TransmitEnabled = true;
            }
        }
    }

    // Call this via other code if you want to toggle voice detection
    public void SetVoiceDetect(bool enabled)
    {
        if (recorder != null)
            recorder.VoiceDetection = enabled;
    }

    // When remote speakers get created, their Speaker component will be added; we configure spatial settings
    private void ConfigureExistingSpeakers()
    {
        speakersCache = GetComponentsInChildren<Speaker>(true);
        foreach (var s in speakersCache)
            ConfigureSpeakerAudioSource(s);
    }

    // called when a speaker component is added: Photon Voice adds Speaker at runtime.
    void OnTransformChildrenChanged()
    {
        // find speakers and apply config
        var speakers = GetComponentsInChildren<Speaker>(true);
        foreach (var s in speakers)
        {
            ConfigureSpeakerAudioSource(s);
        }
    }

    private void ConfigureSpeakerAudioSource(Speaker speaker)
    {
        if (speaker == null) return;
        AudioSource a = speaker.GetComponent<AudioSource>();
        if (a == null)
        {
            // Photon Voice usually adds an AudioSource automatically, but if not, add one.
            a = speaker.gameObject.AddComponent<AudioSource>();
        }

        a.spatialBlend = 1f; // fully 3D
        a.rolloffMode = AudioRolloffMode.Linear;
        a.minDistance = Mathf.Max(0.01f, minDistance);
        a.maxDistance = Mathf.Max(a.minDistance + 0.1f, maxDistance);
        a.spatialize = false; // set true if you have spatializer plugin
        a.loop = false;
        a.playOnAwake = false;
        // optionally set volume, pan, etc.
    }
}
