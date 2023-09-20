using Spaccanavi.Audio;
using Spaccanavi.ObjectPooling;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Spaccanavi.Gameplay
{
    public class Player : MonoBehaviour, ITakeDamage
    {
        // Components
        [Header("Components")]

        [SerializeField] private Animator animator;

        private Camera contentCamera;
        private LevelManager levelManager;
        private ObjectPoolingManager objectPoolingManager;
        private AudioManager audioManager;

        // Attributes
        [SerializeField] private float maxMoveSpeed = 3f;
        [SerializeField] private float shootCooldown = 0.07f;


        private float moveSpeed = 3f;

        // Status
        public Image healthBar;
        private float maxHealthWidth;
        public int Health;
        private int maxHealth;
        private bool isInvincible;

        private const float diagonalMovementMultiplier = 0.7071068f; // sqrt(2)/2

        private float timer;

        private const float movementChangingSpeed = 1f;

        private Vector2 force;
        private const float forceResettingSpeed = 5f;

        private Vector2 input;

        public Vector2 CamOffset { get; private set; } = Vector2.zero;

        // Shake Camera
        private float shakeTimer = 0f;
        private float decreaseFactor = 1f;
        private float shakeAmount = 0f;
        public bool isShaking = false;

        // Abilities
        [SerializeField] private AbilityType ability;
        [SerializeField] private AbilityType holdingAbility;
        public float abilityDuration;
        public float abilityTimer;
        public bool usingAbility;

        [SerializeField] private GameObject sprite;
        [SerializeField] private GameObject shield;
        [SerializeField] private GameObject bulletBill;
        [SerializeField] private GameObject laser;

        private Coroutine hurtCoroutine = null;



        /// <summary>
        /// Initialize the player game object instance.<br/>
        /// This method should only be called by the level manager.
        /// </summary>
        public void Initialize(Camera contentCamera, Image healthFill, int initialMaxHealth = 450)
        {
            // Get singleton references
            levelManager = LevelManager.Instance;
            objectPoolingManager = ObjectPoolingManager.Instance;
            audioManager = AudioManager.Instance;

            // Set fields from parameters
            this.contentCamera = contentCamera;
            this.healthBar = healthFill;

            // Initialize player attributes
            timer = shootCooldown;

            Health = maxHealth = initialMaxHealth;
            maxHealthWidth = healthBar.GetComponent<RectTransform>().rect.width;
            isInvincible = false;

            ability = AbilityType.None;
            holdingAbility = AbilityType.None;
            abilityDuration = 0f;
            abilityTimer = 0f;
            usingAbility = false;

            moveSpeed = 0f;

            input = Vector2.zero;
        }

        private void Update()
        {
            if (!levelManager.pause)
            {
                // Health test
                healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(((float)Health) / ((float)maxHealth) * maxHealthWidth, healthBar.GetComponent<RectTransform>().sizeDelta.y);
                if (Health > ((float)maxHealth) / 2f)
                {
                    healthBar.color = Color.green;
                }
                else if (Health < ((float)maxHealth) / 5f)
                {
                    healthBar.color = Color.red;
                }
                else
                {
                    healthBar.color = Color.yellow;
                }

                // Rotate the player to look at the cursor (https://answers.unity.com/questions/10615/rotate-objectweapon-towards-mouse-cursor-2d.html)
                var mousePosition = Input.mousePosition;
                var screenPoint = contentCamera.WorldToScreenPoint(transform.position);
                var offset = new Vector2(mousePosition.x - screenPoint.x, mousePosition.y - screenPoint.y);
                var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                this.transform.rotation = Quaternion.Euler(0, 0, angle);

                // Move the player based on arrow keys or WASD unless we are the bullet
                if (ability == AbilityType.Bullet && usingAbility)
                {
                    input = (Vector2)(contentCamera.ScreenToWorldPoint(Input.mousePosition) - this.transform.position);
                    if (input.x != 0f && input.y != 0f)
                        input *= diagonalMovementMultiplier;
                    levelManager.MoveBackground(8f * input);
                    transform.position += (Vector3)(8f * Time.deltaTime * input);
                }
                else
                {
                    Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                    if (rawInput.x != 0f && rawInput.y != 0f)
                        rawInput *= diagonalMovementMultiplier;

                    input = Vector2.MoveTowards(input, rawInput != Vector2.zero ? rawInput : Vector2.zero, 10f * Time.deltaTime);

                    moveSpeed = input != Vector2.zero ? maxMoveSpeed : 0f;//Mathf.MoveTowards(moveSpeed, input != Vector2.zero ? maxMoveSpeed : 0f, 500f * Time.deltaTime);

                    // Add check for slowmo here
                    if (ability == AbilityType.SlowMotion && usingAbility)
                    {
                        force += moveSpeed * 2f * Time.deltaTime * input;
                    }
                    else
                    {
                        force += moveSpeed * Time.deltaTime * input;
                    }

                    if (force.x >= maxMoveSpeed)
                    {
                        force.x = maxMoveSpeed;
                    }
                    else if (force.x <= -maxMoveSpeed)
                    {
                        force.x = -maxMoveSpeed;
                    }
                    if (force.y >= maxMoveSpeed)
                    {
                        force.y = maxMoveSpeed;
                    }
                    else if (force.y <= -maxMoveSpeed)
                    {
                        force.y = -maxMoveSpeed;
                    }

                    // Move background
                    levelManager.MoveBackground(force);

                    // Apply force to the player
                    transform.position += (Vector3)(force * Time.deltaTime);
                }

                // Update shoot cooldown
                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                }

                // Check if the player is trying to shoot except if they are the bullet or the laser
                if (timer <= 0 && Input.GetKey(KeyCode.Mouse0) && !(ability == AbilityType.Bullet && usingAbility) && !(ability == AbilityType.Laser && usingAbility))
                {
                    // Add check for the multigun ability here
                    if (ability == AbilityType.Multigun && usingAbility)
                    {
                        Multigun();
                    }
                    else
                    {
                        Shoot();
                    }
                    timer = shootCooldown;
                }

                // Check if the player is trying to use their ability
                if (Input.GetKeyDown(KeyCode.Mouse1) && holdingAbility != AbilityType.None && ability == AbilityType.None)
                {
                    levelManager.UsePickUp(abilityDuration);
                    ability = holdingAbility;
                    holdingAbility = AbilityType.None;

                    // Add medkit ability here, it will just heal the player and nothing else
                    if (ability == AbilityType.Medkit)
                    {
                        Health = maxHealth;
                        ability = AbilityType.None;
                    }
                    else
                    {
                        // Add shield ability here
                        if (ability == AbilityType.Shield)
                        {
                            shield.SetActive(true);
                            isInvincible = true;
                        }
                        // Add bullet ability here
                        else if (ability == AbilityType.Bullet)
                        {
                            bulletBill.SetActive(true);
                            isInvincible = true;
                        }
                        // Add laser ability here
                        else if (ability == AbilityType.Laser)
                        {
                            sprite.SetActive(false);
                            laser.SetActive(true);
                            isInvincible = true;
                            startShakeCamera(abilityDuration, .25f);
                        }
                        // Add slowmo abiliy here
                        else if (ability == AbilityType.SlowMotion)
                        {
                            Time.timeScale = 0.5f;
                            shootCooldown /= 2;
                        }
                        usingAbility = true;
                        abilityTimer = abilityDuration;
                    }
                }

                // Update for ability if they are using their ability
                if (usingAbility)
                {
                    // Check ability timer and update if > 0, otherwise end the ability
                    if (abilityTimer > 0f)
                    {
                        abilityTimer -= Time.deltaTime;
                    }
                    else
                    {
                        // Add shield ability here
                        if (ability == AbilityType.Shield)
                        {
                            shield.SetActive(false);
                            isInvincible = false;
                        }
                        // Add bullet ability here
                        else if (ability == AbilityType.Bullet)
                        {
                            bulletBill.SetActive(false);
                            isInvincible = false;
                        }
                        // Add laser ability here
                        else if (ability == AbilityType.Laser)
                        {
                            sprite.SetActive(true);
                            laser.SetActive(false);
                            isInvincible = false;
                            shakeTimer = 0f;
                            shakeAmount = 0f;
                            isShaking = false;
                        }
                        // Add slowmo abiliy here
                        else if (ability == AbilityType.SlowMotion)
                        {
                            Time.timeScale = 1f;
                            shootCooldown = shootCooldown * 2;
                        }
                        usingAbility = false;
                        ability = AbilityType.None;
                        abilityTimer = 0f;
                    }
                }

                // Adjust camera position so it follows the player unless it is shaking
                if (isShaking)
                {
                    shakeCamera();
                }
                else
                {
                    Vector2 mouseViewPortPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                    CamOffset = new Vector2(Mathf.Clamp(mouseViewPortPos.x - 0.5f, -0.25f, 0.25f),
                        Mathf.Clamp(mouseViewPortPos.y - 0.5f, -0.25f, 0.25f));

                    Vector3 pos = new Vector3(transform.position.x + CamOffset.x, transform.position.y + CamOffset.y, 
                        contentCamera.transform.position.z);
                    contentCamera.transform.position = pos;
                }

                if (input != Vector2.zero)
                    input = Vector2.MoveTowards(input, Vector2.zero, movementChangingSpeed * Time.deltaTime);

                // Resetting force applied on the player
                if (force != Vector2.zero)
                    force = Vector2.MoveTowards(force, Vector2.zero, forceResettingSpeed * Time.deltaTime);
            }
        }

        private void Shoot()
        {
            audioManager.Play("Shoot");

            // Spawn a new bullet and set the direction to be the rotation of the player
            GameObject newBullet = objectPoolingManager.Spawn("Player Bullet");
            newBullet.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z));
        }

        private void Multigun()
        {
            audioManager.Play("Shoot");

            // Spawn a five new bullets and set the direction to be the rotation of the player
            GameObject newBullet1 = objectPoolingManager.Spawn("Player Bullet");
            newBullet1.transform.SetPositionAndRotation(this.transform.position, Quaternion.Euler(0, 0, this.transform.rotation.eulerAngles.z - 21f));

            GameObject newBullet2 = objectPoolingManager.Spawn("Player Bullet");
            newBullet2.transform.SetPositionAndRotation(this.transform.position, Quaternion.Euler(0, 0, this.transform.rotation.eulerAngles.z - 7f));

            GameObject newBullet3 = objectPoolingManager.Spawn("Player Bullet");
            newBullet3.transform.SetPositionAndRotation(this.transform.position, Quaternion.Euler(0, 0, this.transform.rotation.eulerAngles.z));

            GameObject newBullet4 = objectPoolingManager.Spawn("Player Bullet");
            newBullet4.transform.SetPositionAndRotation(this.transform.position, Quaternion.Euler(0, 0, this.transform.rotation.eulerAngles.z + 7f));

            GameObject newBullet5 = objectPoolingManager.Spawn("Player Bullet");
            newBullet5.transform.SetPositionAndRotation(this.transform.position, Quaternion.Euler(0, 0, this.transform.rotation.eulerAngles.z + 21f));
        }

        public void PickUpAbility(AbilityType newAbility, float duration)
        {
            // If the player is already using an ability then do not update
            // Otherwise overwrite the current ability for the new one
            holdingAbility = newAbility;
            abilityDuration = duration;
            levelManager.GetPickUp(newAbility);
        }

        public void startShakeCamera(float duration, float amount)
        {
            shakeTimer = duration;
            shakeAmount = amount;
            isShaking = true;
        }

        private void shakeCamera()
        {
            if (shakeTimer > 0)
            {
                Vector3 camOffset = (Vector3)this.CamOffset;

                if (Time.timeScale == 0.5f)
                {
                    Vector3 shake = this.transform.position + camOffset + Random.insideUnitSphere * shakeAmount;
                    contentCamera.transform.localPosition = new Vector3(shake.x, shake.y, contentCamera.transform.localPosition.z);
                    //backgroundCamera.transform.localPosition = new Vector3(shake.x, shake.y, backgroundCamera.transform.localPosition.z);
                    shakeTimer -= Time.deltaTime * 2f * decreaseFactor;
                }
                else
                {
                    Vector3 shake = this.transform.position + camOffset + Random.insideUnitSphere * shakeAmount;
                    contentCamera.transform.localPosition = new Vector3(shake.x, shake.y, contentCamera.transform.localPosition.z);
                    //backgroundCamera.transform.localPosition = new Vector3(shake.x, shake.y, backgroundCamera.transform.localPosition.z);
                    shakeTimer -= Time.deltaTime * decreaseFactor;
                }
            }
            else
            {
                shakeAmount = 0f;
                isShaking = false;
            }
        }

        public void TakeDamage(int damage, Vector2 direction)
        {
            // Apply force to the player based on the damage
            ApplyForce(direction * (damage / 30f));

            // Ignore damage when invincible
            if (isInvincible)
                return;

            Health -= damage;

            // Shake camera
            startShakeCamera(0.5f, damage / 500f);

            // Check for game over
            if (Health <= 0)
            {
                // KIA ;(

                // Play death sound
                audioManager.Play("Death");

                // Spawn death effect
                DeathEffect effect = objectPoolingManager.Spawn("Death Effect").GetComponent<DeathEffect>();
                effect.transform.position = transform.position;
                effect.SetColor(Color.white);
                effect.Play();

                gameObject.SetActive(false);

                healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, healthBar.GetComponent<RectTransform>().sizeDelta.y);

                // Implement game over
                levelManager.GameOver();
                return;
            }

            // Sound effect
            audioManager.Play("Hurt");

            // Visual effects
            float z = Mathf.Atan2(Mathf.Cos(direction.x), Mathf.Sin(direction.y)) * Mathf.Rad2Deg;
            objectPoolingManager.Spawn("Player Get Hit Effect", transform.position, Quaternion.Euler(new Vector3(0f, 0f, z)));

            // Play hurt animation
            if (hurtCoroutine != null)
            {
                StopCoroutine(hurtCoroutine);
                hurtCoroutine = null;
            }
            hurtCoroutine = StartCoroutine(HurtRoutine());
        }

        public void ResetMovingSpeed()
            => moveSpeed = 0f;

        private IEnumerator HurtRoutine()
        {
            isInvincible = true;
            animator.SetBool("IsHurt", true);

            yield return new WaitForSeconds(1f);

            isInvincible = false;
            animator.SetBool("IsHurt", false);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            // Pick up pickups
            if (collider.CompareTag("Pickup"))
            {
                PickupController pickup = collider.GetComponent<PickupController>();
                PickUpAbility(pickup.Ability, pickup.Duration);
                collider.gameObject.SetActive(false);
            }
        }

        private /*inline*/ void ApplyForce(Vector2 force)
            => this.force += force;

        public /*inline*/ void RefillHeath()
            => Health = maxHealth;

        public /*inline*/ void UpgradeMaxHealth(int maxHealth)
            => this.maxHealth = maxHealth;
    }
}
