using UnityEngine;

namespace Spaccanavi.ObjectPooling
{
    public sealed class PooledParticleEffectController : MonoBehaviour, IPooledGameObject
    {
        [SerializeField] private new ParticleSystem particleSystem;



        public void OnSpawn()
            => particleSystem.Play();
    }
}
