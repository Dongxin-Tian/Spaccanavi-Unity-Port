using Spaccanavi.Audio;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using UnityEngine.SceneManagement;

namespace Spaccanavi.Gameplay
{
    /// <summary>
    /// A singleton class that contains fields about the level.
    /// </summary>
    public sealed class MenuManager : MonoBehaviour
    {
        // Singleton
        public static MenuManager Instance { get; private set; }

        // Components
        [Header("Components")]

        [SerializeField] private Camera contentCamera;
        [SerializeField] private Camera backgroundCamera;
        [SerializeField] private Transform playerContainer;

        private AudioManager audioManager;

        // Background
        [Header("Background")]

        [SerializeField] private float parallax1MoveSpeed = 1;
        [SerializeField] private float parallax2MoveSpeed = 0.5f;
        [SerializeField] private float parallax3MoveSpeed = 0.1f;

        private Material parallax1Material;
        private Material parallax2Material;
        private Material parallax3Material;

        private Vector2 parallax1Offset;
        private Vector2 parallax2Offset;
        private Vector2 parallax3Offset;

        [Header("UI")]

        [SerializeField] private Button playButton;
        private Vector2 input;
        private const float diagonalMovementMultiplier = 0.7071068f; // sqrt(2)/2
        public float moveSpeed = .05f;


        private void Awake()
        {
            Instance = this;

            // Initialize parallax backgrounds
            parallax1Material = Resources.Load<Material>("Materials/Background/Parallax 1");
            parallax2Material = Resources.Load<Material>("Materials/Background/Parallax 2");
            parallax3Material = Resources.Load<Material>("Materials/Background/Parallax 3");

            parallax1Material.SetTextureOffset("_MainTex", Vector2.zero);
            parallax2Material.SetTextureOffset("_MainTex", Vector2.zero);
            parallax3Material.SetTextureOffset("_MainTex", Vector2.zero);

            parallax1Offset = Vector2.zero;
            parallax2Offset = Vector2.zero;
            parallax3Offset = Vector2.zero;
        }

        private void Start()
        {
            // Get audio manager reference
            audioManager = AudioManager.Instance;

            // Start BGM
            audioManager.Play("BGM");

            input = Vector2.zero;
        }

        private void Update()
        {
            if(UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("exiting");
                Application.Quit();
            }

            input = new Vector2(10, 0) + Vector2.MoveTowards(this.transform.position, contentCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition), 5f);
            if (input.x != 0f && input.y != 0f)
                input *= diagonalMovementMultiplier;
            MoveBackground(input * moveSpeed);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void MoveBackground(Vector2 movement)
        {
            parallax1Offset += parallax1MoveSpeed * Time.deltaTime * movement;
            parallax2Offset += parallax2MoveSpeed * Time.deltaTime * movement;
            parallax3Offset += parallax3MoveSpeed * Time.deltaTime * movement;

            parallax1Material.SetTextureOffset("_MainTex", parallax1Offset);
            parallax2Material.SetTextureOffset("_MainTex", parallax2Offset);
            parallax3Material.SetTextureOffset("_MainTex", parallax3Offset);
        }

        public void Play()
        {
            Debug.Log("play");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
