using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour {

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip death;
    [SerializeField] AudioClip success;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem successParticles;

    [SerializeField] float levelLoadDelay = 2f;

    Rigidbody rigidBody;
    AudioSource audioSource;

    bool isTransitioning = false;


    bool collisionDisabled = false;

    // Start is called before the first frame update
    void Start() {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {

        if (!isTransitioning) {
            RespondToRotateInput();
            RespondToThrustInput();
            if (Debug.isDebugBuild) RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys() {
        if (Input.GetKeyDown(KeyCode.L)) {
            LoadNextLevel();
        }

        if (Input.GetKeyUp(KeyCode.C)) {
            collisionDisabled = !collisionDisabled;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (isTransitioning || collisionDisabled) return;
        switch (collision.gameObject.tag) {
            case "Friendly":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence() {
        isTransitioning = true;
        audioSource.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextLevel", levelLoadDelay); //va appeler la méthode LoadNextScene après une seconde
    }


    private void StartDeathSequence() {
        isTransitioning = true;
        if (audioSource.isPlaying) {
            audioSource.Stop();
        }
        audioSource.PlayOneShot(death);
        deathParticles.Play();
        Invoke("LoadFirstLevel", levelLoadDelay);
    }


    private void LoadFirstLevel() {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel() {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int numberOfScenes = SceneManager.sceneCountInBuildSettings;
        int nextSceneIndex;
        if (currentSceneIndex < (numberOfScenes - 1)) {
            nextSceneIndex = currentSceneIndex + 1;
        } else {
            nextSceneIndex = 0;
        }
        SceneManager.LoadScene(nextSceneIndex);
    }



    private void RespondToRotateInput() {
        rigidBody.angularVelocity = Vector3.zero;
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q)) {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        } else if (Input.GetKey(KeyCode.D)) {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

    }

    private void RespondToThrustInput() {

        if (Input.GetKey(KeyCode.Space)) {
            ApplyThrust();
        } else {
            StopApplyingThrust();
        }
    }

    private void StopApplyingThrust() {
        audioSource.Stop();
        mainEngineParticles.Stop();
    }

    private void ApplyThrust() {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);

        if (!audioSource.isPlaying) {
            audioSource.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }
}
