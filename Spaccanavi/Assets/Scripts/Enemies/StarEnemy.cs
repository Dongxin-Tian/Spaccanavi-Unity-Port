using Spaccanavi.ObjectPooling;
using System.Collections;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public sealed class StarEnemy : SpinnerEnemy
    {
        [SerializeField] private Transform bulletFirePos3;
        [SerializeField] private Transform bulletFirePos4;

        [SerializeField] private float burstInterval = 0.25f;

        private readonly Color starColor = new Color(0.8705882f, 0.2431373f, 0.9215686f);

        private const float followingSpeed = 2f;

        protected override void Update()
        {
            base.Update();

            // Stalk the player
            transform.position = (Vector3)Vector2.MoveTowards(transform.position, levelManager.Player.transform.position, followingSpeed * Time.deltaTime);
        }

        protected override IEnumerator FireRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(burstInterval);

                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForSeconds(fireInterval);

                    GameObject b1 = objectPoolingManager.Spawn("Enemy Bullet Purple", levelManager.EnemyBulletContainer);
                    b1.transform.SetPositionAndRotation(bulletFirePos1.position, Quaternion.Euler(bulletFirePos1.transform.rotation.eulerAngles));
                    b1.GetComponent<EnemyBullet>().Shooter = this.gameObject;

                    GameObject b2 = objectPoolingManager.Spawn("Enemy Bullet Purple", levelManager.EnemyBulletContainer);
                    b2.transform.SetPositionAndRotation(bulletFirePos2.position, Quaternion.Euler(bulletFirePos2.transform.rotation.eulerAngles));
                    b2.GetComponent<EnemyBullet>().Shooter = this.gameObject;

                    GameObject b3 = objectPoolingManager.Spawn("Enemy Bullet Purple", levelManager.EnemyBulletContainer);
                    b3.transform.SetPositionAndRotation(bulletFirePos3.position, Quaternion.Euler(bulletFirePos3.transform.rotation.eulerAngles));
                    b3.GetComponent<EnemyBullet>().Shooter = this.gameObject;

                    GameObject b4 = objectPoolingManager.Spawn("Enemy Bullet Purple", levelManager.EnemyBulletContainer);
                    b4.transform.SetPositionAndRotation(bulletFirePos4.position, Quaternion.Euler(bulletFirePos4.transform.rotation.eulerAngles));
                    b4.GetComponent<EnemyBullet>().Shooter = this.gameObject;
                }
            }
        }

        public override void OnGotRekt()
        {
            audioManager.Play("Death");

            // Play destroyed effect
            if (ObjectPoolingManager.Instance == null)
                return;

            if (!ObjectPoolingManager.Instance.Spawn("Death Effect").TryGetComponent(out DeathEffect effect))
                return;
            effect.transform.position = transform.position;
            effect.SetColor(starColor);
            effect.Play();
        }
    }
}
