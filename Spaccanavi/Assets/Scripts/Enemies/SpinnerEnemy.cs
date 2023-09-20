using Spaccanavi.ObjectPooling;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spaccanavi.Gameplay
{
    public class SpinnerEnemy : EnemyBase
    {
        [SerializeField] protected Transform bulletFirePos1;
        [SerializeField] protected Transform bulletFirePos2;
        [SerializeField] protected float fireInterval = 0.1f;

        [SerializeField] protected float rotateSpeed = 100f;
        [SerializeField] protected RotateDirection rotateDirection = RotateDirection.Left;

        [SerializeField] protected float lifeTime = 30f;

        protected Vector2 staticForce = Vector2.zero;

        protected Coroutine fireCoroutine = null;
        protected Coroutine lifeTimeCoroutine = null;

        private readonly Color spinnerColor = new Color(0.9372549f, 0.2784314f, 0.4352941f);



        public override void OnSpawn()
        {
            base.OnSpawn();

            // Set random moving direction
            staticForce = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * Random.Range(0.1f, 0.5f);

            // Set random rotating direction
            rotateDirection = Random.value > 0.5f ? RotateDirection.Left : RotateDirection.Right;

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

            // Move
            transform.position += (Vector3)(staticForce * Time.deltaTime);

            // Rotate
            transform.Rotate(rotateSpeed * (rotateDirection == RotateDirection.Left ? 1f : -1f) * Time.deltaTime * Vector3.forward);
        }

        protected virtual IEnumerator FireRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(fireInterval);

                GameObject b1 = objectPoolingManager.Spawn("Enemy Bullet Red", levelManager.EnemyBulletContainer);
                b1.transform.SetPositionAndRotation(bulletFirePos1.position, Quaternion.Euler(bulletFirePos1.transform.rotation.eulerAngles));
                b1.GetComponent<EnemyBullet>().Shooter = this.gameObject;

                GameObject b2 = objectPoolingManager.Spawn("Enemy Bullet Red", levelManager.EnemyBulletContainer);
                b2.transform.SetPositionAndRotation(bulletFirePos2.position, Quaternion.Euler(bulletFirePos2.transform.rotation.eulerAngles));
                b2.GetComponent<EnemyBullet>().Shooter = this.gameObject;
            }
        }

        private IEnumerator LifeTimeRoutine()
        {
            yield return new WaitForSeconds(lifeTime);

            this.OnGotRekt();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
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
            effect.SetColor(spinnerColor);
            effect.Play();
        }
    }
}
