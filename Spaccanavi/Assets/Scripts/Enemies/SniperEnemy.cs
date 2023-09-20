using Spaccanavi.ObjectPooling;
using System.Collections;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public sealed class SniperEnemy : EnemyBase
    {
        [SerializeField] private Transform firePos;
        [SerializeField] private float fireInterval = 5f;

        [SerializeField] private LayerMask playerLayerMask;

        [SerializeField] private SpriteRenderer laserSpriteRenderer;

        [SerializeField] private int snipingDamage = 450;

        private readonly Color sniperColor = new Color(0.1960784f, 0.7411765f, 0.9411765f);

        private Coroutine fireCoroutine = null;

        public override void OnSpawn()
        {
            base.OnSpawn();

            if (fireCoroutine != null)
                StopCoroutine(fireCoroutine);
            fireCoroutine = StartCoroutine(FireRoutine());

            Color c = Color.yellow;
            c.a = 0.5f;
            laserSpriteRenderer.color = c;
        }

        protected override void Update()
        {
            base.Update();

            // Look at the player smoothly
            float lookAtSpeed = 300f / Vector2.Distance(transform.position, levelManager.Player.transform.position);
            Vector2 dir = levelManager.Player.transform.position - transform.position;
            Vector3 rot = transform.rotation.eulerAngles;
            float z = Mathf.MoveTowardsAngle(rot.z, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, lookAtSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(rot.x, rot.y, z);
        }

        private IEnumerator FireRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(fireInterval);

                Color c = Color.red;
                c.a = 0f;
                laserSpriteRenderer.color = Color.red;
                while (c.a < 1f)
                {
                    c.a += Time.deltaTime / 2f;
                    laserSpriteRenderer.color = c;
                    yield return null;
                }

                // Raycast
                RaycastHit2D hit = Physics2D.Raycast(
                    origin: firePos.transform.position,
                    direction: firePos.up,
                    distance: Mathf.Infinity,
                    layerMask: playerLayerMask
                );
                if (hit)
                    levelManager.Player.TakeDamage(snipingDamage, Vector2.zero);

                c = Color.yellow;
                c.a = 0.5f;
                laserSpriteRenderer.color = c;
            }
        }

        private void OnDisable()
        {
            if (fireCoroutine != null)
                StopCoroutine(fireCoroutine);
            fireCoroutine = null;
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
            effect.SetColor(sniperColor);
            effect.Play();
        }
    }
}
