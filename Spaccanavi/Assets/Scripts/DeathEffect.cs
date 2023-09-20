using Spaccanavi.ObjectPooling;
using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public class DeathEffect : MonoBehaviour, IPooledGameObject
    {
        [SerializeField] private new ParticleSystem particleSystem;

        public void OnSpawn()
            => particleSystem.Clear();

        public void SetColor(Color color)
        {
            var main = particleSystem.main;
            main.startColor = color;
        }

        public void Play()
            => particleSystem.Play();
    }
}
