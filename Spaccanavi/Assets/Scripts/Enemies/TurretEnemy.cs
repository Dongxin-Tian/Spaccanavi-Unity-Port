using System.Collections;
using Spaccanavi.ObjectPooling;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public sealed class TurretEnemy : EnemyBase
    {
        [SerializeField] private Transform firePos;
        [SerializeField] private float fireInterval = 0.1f;
        [SerializeField] private float lifeTime = 30f;

        private Coroutine fireCoroutine = null;
        private Coroutine lifeTimeCoroutine = null;

        private readonly Color turretColor = new Color(0.02352941f, 0.8392157f, 0.627451f);

        public override void OnSpawn()
        {
            base.OnSpawn();

            // Stop previous fire coroutine and start a new one
            if (fireCoroutine != null)
                StopCoroutine(fireCoroutine);
            fireCoroutine = StartCoroutine(FireRoutine());

            // Start life time counting
            if (lifeTimeCoroutine != null)
                StopCoroutine(lifeTimeCoroutine);
            lifeTimeCoroutine = StartCoroutine(LifeTimeRoutine());
        }

        protected override void Update()
        {
            base.Update();

            // Look at the player smoothly
            Vector2 dir = levelManager.Player.transform.position - transform.position;
            float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, z);
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
            effect.SetColor(turretColor);
            effect.Play();
        }

        private IEnumerator FireRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(fireInterval);

                GameObject b = objectPoolingManager.Spawn("Enemy Bullet Lime", levelManager.EnemyBulletContainer);
                b.transform.SetPositionAndRotation(firePos.position, Quaternion.Euler(firePos.transform.rotation.eulerAngles));
                b.GetComponent<EnemyBullet>().Shooter = gameObject;
            }
        }

        private IEnumerator LifeTimeRoutine()
        {
            yield return new WaitForSeconds(lifeTime);

            this.OnGotRekt();
            gameObject.SetActive(false);
        }
    }
}
