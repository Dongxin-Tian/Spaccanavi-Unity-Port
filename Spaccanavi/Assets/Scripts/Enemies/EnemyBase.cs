using UnityEngine;
using Spaccanavi.ObjectPooling;
using Spaccanavi.Audio;

namespace Spaccanavi.Gameplay
{
    public abstract class EnemyBase : MonoBehaviour, IPooledGameObject, ITakeDamage
    {
        [SerializeField] protected int maxHealth = 100;

        [Tooltip("The damage dealt to the player when they collided.")]
        [SerializeField] protected int damage = 10;

        [SerializeField] protected int experience = 10;

        [SerializeField] protected float maximumDistanceBetweenPlayer = 20f;

        protected int health = 100;

        protected Vector2 force = Vector2.zero;

        protected LevelManager levelManager;
        protected ObjectPoolingManager objectPoolingManager;
        protected AudioManager audioManager;

        public int Experience => experience;



        protected virtual void Start()
        {
            levelManager = LevelManager.Instance;
            objectPoolingManager = ObjectPoolingManager.Instance;
            audioManager = AudioManager.Instance;
        }

        protected virtual void Update()
        {
            // Despawn if it's too far from the player
            if (Vector2.Distance(transform.position, levelManager.Player.transform.position) > maximumDistanceBetweenPlayer)
            {
                // Despawn
                gameObject.SetActive(false);
                levelManager.SpawnEnemy();
            }

            // Move
            transform.position += (Vector3)(force * Time.deltaTime);

            force = Vector2.MoveTowards(force, Vector2.zero, Time.deltaTime);
        }

        public virtual void OnSpawn()
        {
            // Reset attributes
            health = maxHealth;
            force = Vector2.zero;
        }

        public virtual void TakeDamage(int damage, Vector2 direction)
        {
            health -= damage;

            // Got rekt
            if (health <= 0)
            {
                levelManager.GetWaveExperience(experience);

                OnGotRekt();

                gameObject.SetActive(false);
            }
        }

        public virtual void OnGotRekt()
        {
            audioManager.Play("Death");
        }

        public virtual void GetRekt()
        {
            OnGotRekt();
            gameObject.SetActive(false);
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            // Collision with the player
            if (collision.gameObject.CompareTag("Player"))
            {
                levelManager.Player.TakeDamage(damage, -collision.GetContact(0).normal);
                levelManager.Player.ResetMovingSpeed();
            }
        }

        public void ApplyForce(Vector2 force)
            => this.force += force;
    }
}