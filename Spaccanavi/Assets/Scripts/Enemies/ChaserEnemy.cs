using Spaccanavi.ObjectPooling;
using System.Collections;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public class ChaserEnemy : EnemyBase
    {
        [SerializeField] private bool shoot = false;
        [SerializeField] private ChaserEnemyGroup group = null;
        [SerializeField] private Transform firePos = null;

        private Vector2 chasingDir = Vector2.zero;

        private Coroutine fireCoroutine = null;

        private const float fireIntervalBetweenEachShot = 0.25f;
        private const float minFireIntervalBetweenEachBurst = 3f;
        private const float maxFireIntervalBetweenEachBurst = 5f;
        private const int burstShotCount = 3;
        private const float chasingSpeed = 10f;

        private readonly Color chaserColor = new Color(1f, 0.8196079f, 0.4f);



        public override void OnSpawn()
        {
            base.OnSpawn();

            chasingDir = Vector2.zero;

            if (shoot)
            {
                if (fireCoroutine != null)
                    StopCoroutine(fireCoroutine);
                fireCoroutine = StartCoroutine(FireRoutine());
            }
        }

        protected override void Update()
        {
            //base.Update(); Don't call base Update since it will spawn enemies at a high rate

            if (Vector2.Distance(transform.position, levelManager.Player.transform.position) > maximumDistanceBetweenPlayer)
            {
                // Despawn
                gameObject.SetActive(false);
                if (group != null)
                    group.OnChildDespawned();
            }

            // Move
            transform.position += (Vector3)(force * Time.deltaTime);
            force = Vector2.MoveTowards(force, Vector2.zero, Time.deltaTime);

            Vector2 dir = (levelManager.Player.transform.position - transform.position).normalized;
            chasingDir = Vector2.MoveTowards(chasingDir, dir, Time.deltaTime);
            transform.position += (Vector3)(chasingSpeed * Time.deltaTime * chasingDir);

            // Look at the player
            float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector3 rot = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(rot.x, rot.y, z);
        }

        protected override void OnCollisionStay2D(Collision2D collision)
        {
            // Collision with the player
            if (collision.gameObject.CompareTag("Player"))
            {
                levelManager.Player.TakeDamage(damage, -collision.GetContact(0).normal);
                levelManager.Player.ResetMovingSpeed();

                GetRekt();
            }
            // Collision with other enemies
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
                enemy.ApplyForce(-collision.GetContact(0).normal * (damage / 10f));
            }
        }

        public override void OnGotRekt()
        {
            base.OnGotRekt();

            // Play destroyed effect
            if (ObjectPoolingManager.Instance == null)
                return;

            if (!ObjectPoolingManager.Instance.Spawn("Death Effect").TryGetComponent(out DeathEffect effect))
                return;
            effect.transform.position = transform.position;
            effect.SetColor(chaserColor);
            effect.Play();
        }

        public override void GetRekt()
        {
            base.GetRekt();

            if (group != null)
                group.OnChildDespawned();
        }

        private IEnumerator FireRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minFireIntervalBetweenEachBurst, maxFireIntervalBetweenEachBurst));

                for (int i = 0; i < burstShotCount; i++)
                {
                    yield return new WaitForSeconds(fireIntervalBetweenEachShot);

                    // Fire at the player
                    GameObject b = objectPoolingManager.Spawn("Enemy Bullet Plasma", levelManager.EnemyBulletContainer);
                    b.transform.SetPositionAndRotation(firePos.position, Quaternion.Euler(firePos.transform.rotation.eulerAngles));
                    b.GetComponent<EnemyBullet>().Shooter = this.gameObject;
                }
            }
        }

        private void OnDisable()
        {
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
            }
        }
    }
}
