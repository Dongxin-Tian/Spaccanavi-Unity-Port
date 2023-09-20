using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public class PlayerBulletAndLaser : MonoBehaviour
    {
        private LevelManager levelManager;

        public Transform PlayerTransform { get; set; }

        private void Start()
            => levelManager = LevelManager.Instance;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
                enemy.GetRekt();
                levelManager.GetWaveExperience(enemy.Experience);
            }
        }
    }
}
