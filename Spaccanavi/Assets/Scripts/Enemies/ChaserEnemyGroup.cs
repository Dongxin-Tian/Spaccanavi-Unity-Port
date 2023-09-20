using Spaccanavi.ObjectPooling;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public sealed class ChaserEnemyGroup : MonoBehaviour, IPooledGameObject
    {
        [SerializeField] private ChaserEnemy[] enemies;

        private (ChaserEnemy, Vector2)[] entries;

        private void Awake()
        {
            entries = new (ChaserEnemy, Vector2)[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
                entries[i] = (enemies[i], enemies[i].transform.localPosition);

            enemies = null;
        }

        public void OnSpawn()
        {
            foreach ((ChaserEnemy enemy, Vector2 localPos) in entries)
            {
                enemy.gameObject.SetActive(true);
                enemy.transform.localPosition = localPos;
                enemy.OnSpawn();
            }
        }

        public void OnChildDespawned()
        {
            foreach ((ChaserEnemy enemy, _) in entries)
            {
                if (enemy.gameObject.activeSelf)
                    return;
            }

            gameObject.SetActive(false);
        }
    }
}
