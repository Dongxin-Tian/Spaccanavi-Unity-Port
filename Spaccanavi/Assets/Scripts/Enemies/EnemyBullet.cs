using Spaccanavi.ObjectPooling;
using System.Collections;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public sealed class EnemyBullet : MonoBehaviour, IPooledGameObject
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private bool hasTargetMoveSpeed = false;
        [SerializeField] private float targetMoveSpeed = 0f;
        [SerializeField] private float maxDelta = 1f;
        [SerializeField] private int damage = 5;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private bool applyForce = false;

        public GameObject Shooter { get; set; } = null;

        private float currentMoveSpeed;

        private Coroutine spawnCoroutine = null;

        private LevelManager levelManager;

        private void Start()
            => levelManager = LevelManager.Instance;

        public void OnSpawn()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnRoutine());

            currentMoveSpeed = moveSpeed;
        }

        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(lifeTime);

            gameObject.SetActive(false);
        }

        private void Update()
        {
            transform.position += currentMoveSpeed * Time.deltaTime * this.transform.right;

            if (hasTargetMoveSpeed)
                currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, maxDelta * Time.deltaTime);
        }

        private void OnDisable()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                levelManager.Player.TakeDamage(damage, applyForce ? -collision.GetContact(0).normal : Vector2.zero);
                gameObject.SetActive(false);
            }
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                // Ignore current shooter of this bullet
                if (collision.gameObject == Shooter)
                    return;

                // Apply force to enemy
                EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
                enemy.ApplyForce(-collision.GetContact(0).normal * damage / 30f);
                gameObject.SetActive(false);
            }
        }
    }
}
