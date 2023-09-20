using Spaccanavi.Audio;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Spaccanavi.ObjectPooling;
using Spaccanavi.Gameplay.UI;
using System.Collections.Generic;

namespace Spaccanavi.Gameplay
{
    /// <summary>
    /// A singleton class that contains fields about the level.
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        // Singleton
        public static LevelManager Instance { get; private set; }

        // Components
        [Header("Components")]

        [SerializeField] private Camera contentCamera;
        [SerializeField] private Camera backgroundCamera;
        [SerializeField] private Transform playerContainer;
        [SerializeField] private Transform playerBulletContainer;
        [SerializeField] private Transform enemyContainer;
        [SerializeField] private Transform enemyBulletContainer;
        [SerializeField] private Transform pickupContainer;
        [SerializeField] private ItemBoxUIController itemBoxUIController;

        private AudioManager audioManager;
        private ObjectPoolingManager objectPoolingManager;

        public Transform PlayerBulletContainer => playerBulletContainer;
        public Transform EnemyBulletContainer => enemyBulletContainer;

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

        // UI
        [Header("UI")]

        [SerializeField] private Text waveText;
        [SerializeField] private Image waveFill;
        [SerializeField] private Image healthFill;
        [SerializeField] private Canvas pauseCanvas;
        [SerializeField] private Canvas respawnCanvas;
        [SerializeField] private Canvas panel;
        private int waveLevel = 1;
        public bool pause = false;
        public bool gameover = false;

        private int waveExperience = 0;
        private int maxWaveExperience = 100;

        // Player instance
        public Player Player { get; private set; }

        private readonly List<string> enemyTagList = new List<string>
        {
            "Spinner",
            "Turret"
        };

        private int currentEnemyCount;
        private int currentMaxEnemyCount;



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

            // Create player
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
            Player = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity).GetComponent<Player>();
            Player.transform.SetParent(playerContainer);
            Player.gameObject.name = playerPrefab.name;
            Player.Initialize(
                contentCamera: contentCamera, 
                healthFill: healthFill
            );

            // Load UI
            UpdateWaveUI();

            currentEnemyCount = 0;
            currentMaxEnemyCount = 5;
        }

        private void Start()
        {
            audioManager = AudioManager.Instance;
            objectPoolingManager = ObjectPoolingManager.Instance;

            // Start BGM
            audioManager.Play("BGM");

            StartCoroutine(SpawnEnemyRoutine());
            StartCoroutine(SpawnItemRoutine());
        }

        private void Update()
        {
            if (!gameover)
            {
                if (pause)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        pause = false;
                        audioManager.Unpause("BGM");
                        Time.timeScale = 1;
                        pauseCanvas.gameObject.SetActive(false);
                        panel.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        pause = true;
                        audioManager.Pause("BGM");
                        Time.timeScale = 0;
                        pauseCanvas.gameObject.SetActive(true);
                        panel.gameObject.SetActive(true);
                    }
                }
            }
        }

        public void LoadMenu()
        {
            Time.timeScale = 1;
            TransitionScene.TargetSceneName = "Menu";
            SceneManager.LoadScene("Transition");
        }

        public void Retry()
        {
            Time.timeScale = 1;
            TransitionScene.TargetSceneName = "Level";
            SceneManager.LoadScene("Transition");
        }

        private void OnDestroy()
            => Instance = null;



        public void MoveBackground(Vector2 movement)
        {
            parallax1Offset += parallax1MoveSpeed * Time.deltaTime * movement;
            parallax2Offset += parallax2MoveSpeed * Time.deltaTime * movement;
            parallax3Offset += parallax3MoveSpeed * Time.deltaTime * movement;

            Vector2 o1 = parallax1Offset;
            Vector2 o2 = parallax2Offset;
            Vector2 o3 = parallax3Offset;

            if (Player != null)
            {
                Vector2 offset = Player.CamOffset * 0.05f;

                o1 += offset;
                o2 += offset;
                o3 += offset;
            }

            parallax1Material.SetTextureOffset("_MainTex", o1);
            parallax2Material.SetTextureOffset("_MainTex", o2);
            parallax3Material.SetTextureOffset("_MainTex", o3);
        }

        public void GameOver()
        {
            audioManager.Stop("BGM");
            audioManager.Play("Death");
            gameover = true;
            respawnCanvas.gameObject.SetActive(true);
            panel.gameObject.SetActive(true);
        }

        public void GetWaveExperience(int ex)
        {
            waveExperience += ex;
            if (waveExperience >= maxWaveExperience)
            {
                waveExperience = 0;

                waveLevel++;
                if (waveLevel > 20)
                    waveLevel = 20;

                maxWaveExperience = (int)(maxWaveExperience * 1.25f);

                currentMaxEnemyCount = Mathf.CeilToInt(currentMaxEnemyCount * 1.5f);

                Player.UpgradeMaxHealth(450 + ((waveLevel - 1) * 20));
                Player.RefillHeath();

                TryToUnlockEnemies();
            }
            UpdateWaveUI();
        }

        private void UpdateWaveUI()
        {
            waveText.text = $"WAVE {waveLevel}";
            waveFill.fillAmount = waveExperience / (float)maxWaveExperience;
        }

        private IEnumerator SpawnEnemyRoutine()
        {
            while (!pause && !gameover)
            {
                float min = 5f / waveLevel;
                if (min < 1f)
                    min = 1f;
                float max = 10f / waveLevel;
                if (max < 1f)
                    max = 1f;

                yield return new WaitForSeconds(Random.Range(min, max));

                if (currentEnemyCount < currentMaxEnemyCount)
                    SpawnEnemy();
            }
        }

        private IEnumerator SpawnItemRoutine()
        {
            while (!pause && !gameover)
            {
                yield return new WaitForSeconds(Random.Range(3f, 7f));

                SpawnItem();
            }
        }

        public void SpawnEnemy()
        {
            string tag = enemyTagList[Random.Range(0, enemyTagList.Count)];

            if (objectPoolingManager.HasInactive(tag))
            {
                GameObject enemyGo = objectPoolingManager.Spawn(tag, enemyContainer);
                enemyGo.transform.position = GetRandomPositionOutsideTheScreen();
            }
        }

        private void SpawnItem()
        {
            string tag = ((AbilityType)Random.Range(1, 7)).ToString();
            if (objectPoolingManager.HasInactive(tag))
            {
                GameObject pickupGo = objectPoolingManager.Spawn(tag, pickupContainer);
                pickupGo.transform.position = GetRandomPositionOutsideTheScreen();
            }
        }

        private Vector2 GetRandomPositionOutsideTheScreen()
        {
            float x = Random.Range(-1.2f, 1.2f);
            float y;
            if (x >= -1.1f && x <= 1.1f)
            {
                y = Random.value > 0.5f ? Random.Range(1.1f, 1.2f) : Random.Range(-1.2f, -1.1f);
            }
            else
            {
                y = Random.Range(-1.2f, 1.2f);
            }

            return contentCamera.ViewportToWorldPoint(new Vector2(x, y));
        }

        public /*inline*/ void GetPickUp(AbilityType ability)
            => itemBoxUIController.UpdateItem(ability);

        public /*inline*/ void UsePickUp(float duration)
        {
            itemBoxUIController.ClearItem();
            itemBoxUIController.UseItem(duration);
        }

        private void TryToUnlockEnemies()
        {
            switch (waveLevel)
            {
                case 2:
                    enemyTagList.Add("Triple Spinner");
                    enemyTagList.Add("Chaser Group (Small)");
                    break;

                case 4:
                    enemyTagList.Add("Chaser (Large)");
                    enemyTagList.Add("Star");
                    break;

                case 5:
                    enemyTagList.Add("Sniper");
                    break;
            }
        }
    }
}
