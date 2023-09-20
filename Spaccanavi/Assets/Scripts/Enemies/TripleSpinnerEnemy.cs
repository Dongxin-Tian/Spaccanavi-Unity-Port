using Spaccanavi.ObjectPooling;
using System.Collections;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public sealed class TripleSpinnerEnemy : SpinnerEnemy
    {
        [SerializeField] private Transform bulletFirePos3;

        private readonly Color tripleSpinnerColor = new Color(0.972549f, 0.4078431f, 0.2431373f);

        protected override IEnumerator FireRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(fireInterval);

                GameObject b1 = objectPoolingManager.Spawn("Enemy Bullet Orange", levelManager.EnemyBulletContainer);
                b1.transform.SetPositionAndRotation(bulletFirePos1.position, Quaternion.Euler(bulletFirePos1.transform.rotation.eulerAngles));
                b1.GetComponent<EnemyBullet>().Shooter = this.gameObject;

                yield return new WaitForSeconds(fireInterval);

                GameObject b2 = objectPoolingManager.Spawn("Enemy Bullet Orange", levelManager.EnemyBulletContainer);
                b2.transform.SetPositionAndRotation(bulletFirePos2.position, Quaternion.Euler(bulletFirePos2.transform.rotation.eulerAngles));
                b2.GetComponent<EnemyBullet>().Shooter = this.gameObject;

                yield return new WaitForSeconds(fireInterval);

                GameObject b3 = objectPoolingManager.Spawn("Enemy Bullet Orange", levelManager.EnemyBulletContainer);
                b3.transform.SetPositionAndRotation(bulletFirePos3.position, Quaternion.Euler(bulletFirePos3.transform.rotation.eulerAngles));
                b3.GetComponent<EnemyBullet>().Shooter = this.gameObject;
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
            effect.SetColor(tripleSpinnerColor);
            effect.Play();
        }
    }
}
