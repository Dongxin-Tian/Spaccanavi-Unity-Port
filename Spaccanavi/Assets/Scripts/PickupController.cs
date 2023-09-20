using Spaccanavi.ObjectPooling;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public sealed class PickupController : MonoBehaviour, IPooledGameObject
    {
        /* Components */
        [Header("Components")]

        [SerializeField] private Transform outlineTransform;
        
        private LevelManager levelManager;



        /* Attributes */
        [Header("Attributes")]

        [SerializeField] private AbilityType ability;
        [SerializeField] private float duration;

        public AbilityType Ability => ability;
        public float Duration => duration;

        private const float playerFollowingRange = 3f;
        private const float playerFollowingSpeed = 5f;
        private const float rotatingSpeed = 100f;

        private Vector2 direction;

        private const float minimumDistanceBetweenPlayer = 30f;



        private void Start()
        {
            levelManager = LevelManager.Instance;
        }

        public void OnSpawn()
        {
            direction = Vector2.zero;
        }

        private void Update()
        {
            // Deactivate if it's too far from the player
            if (Vector2.Distance(transform.position, levelManager.Player.transform.position) > minimumDistanceBetweenPlayer)
            {
                gameObject.SetActive(false);
                return;
            }

            // Animation
            outlineTransform.Rotate(rotatingSpeed * Time.deltaTime * -Vector3.forward);

            // Movement
            if (levelManager.Player != null)
            {
                if (Vector2.Distance(transform.position, levelManager.Player.transform.position) <= playerFollowingRange)
                {
                    // Follow the player
                    Vector2 dir = (levelManager.Player.transform.position - transform.position).normalized;
                    direction = Vector2.MoveTowards(direction, dir, Time.deltaTime);
                }
                else
                {
                    // Slowly change velocity back to zero
                    direction = Vector2.MoveTowards(direction, Vector2.zero, Time.deltaTime);
                }
            }

            transform.position += playerFollowingSpeed * Time.deltaTime * (Vector3)direction; // Move
        }
    }
}
