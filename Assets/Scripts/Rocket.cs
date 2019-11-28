using UnityEngine;
using UnityEditor.SceneManagement;
using System;

public class Rocket : MonoBehaviour
{
    Rigidbody rigidBody;
    AudioSource audioSource;

    public float enginePower = 5f;
    public float deltaWings = 100f;
    [SerializeField] float levelLoadDelay = .7f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip deathAudio;
    [SerializeField] AudioClip levelCompleteAudio;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem levelCompleteParticles;

    enum State { Alive, Dying, Transcending }
    State state = State.Alive;
    bool collisionsEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            RespondRotateInput();
            RespondThrustInput();
        }
        if (Debug.isDebugBuild)
        {
            RespondToDebutKeys();
        }
    }

    private void RespondToDebutKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }
        else if (Input.GetKeyDown(KeyCode.C)) {
            collisionsEnabled = !collisionsEnabled;
            Debug.Log(collisionsEnabled);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || !collisionsEnabled) { return; }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                print("OK");
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audioSource.Stop();
        audioSource.PlayOneShot(levelCompleteAudio);
        levelCompleteParticles.Play();
        Invoke("LoadNextLevel", levelLoadDelay);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        audioSource.PlayOneShot(deathAudio);
        deathParticles.Play();
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = EditorSceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex == EditorSceneManager.sceneCountInBuildSettings)
        {
            EditorSceneManager.LoadScene(0);
        } 
        else
        {
            EditorSceneManager.LoadScene(currentSceneIndex + 1);
        }
    }

    private void LoadFirstLevel()
    {
        EditorSceneManager.LoadScene(0);
    }

    private void RespondRotateInput()
    {
        float rotationSpeed = deltaWings * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            RotateInput(rotationSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            RotateInput(-rotationSpeed);
        }
    }

    private void RotateInput(float rotationSpeed)
    {
        rigidBody.freezeRotation = true;
        transform.Rotate(Vector3.forward * rotationSpeed);
        rigidBody.freezeRotation = false;
    }

    private void RespondThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * enginePower * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }
}
