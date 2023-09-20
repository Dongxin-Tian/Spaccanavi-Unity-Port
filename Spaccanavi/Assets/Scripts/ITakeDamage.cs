using UnityEngine;

namespace Spaccanavi.Gameplay
{
    public interface ITakeDamage
    {
        void TakeDamage(int damage, Vector2 direction);
    }
}
