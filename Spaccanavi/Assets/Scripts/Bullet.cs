using Spaccanavi.ObjectPooling;
using System.Collections;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public class Bullet : MonoBehaviour, IPooledGameObject
    {
        [SerializeField] private float maxDistance = 10f;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private int damage = 5;
        [SerializeField] private float lifeTime = 3f;

        private Coroutine spawnCoroutine = null;

        public void OnSpawn()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(lifeTime);

            gameObject.SetActive(false);
        }

        private void Update()
        {
            // Check for slowmo here
            if (Time.timeScale == 0.5f)
            {
                transform.position += moveSpeed * 2f * Time.deltaTime * this.transform.right;
            }
            else
            {
                transform.position += moveSpeed * Time.deltaTime * this.transform.right;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
                enemy.TakeDamage(damage, Vector2.zero);
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
}
