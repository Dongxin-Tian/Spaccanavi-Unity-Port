using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spaccanavi.Gameplay.UI
{
    public class ItemBoxUIController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image frameImage;

        private readonly Dictionary<AbilityType, Sprite> spriteDict = new Dictionary<AbilityType, Sprite>();

        private void Awake()
        {
            spriteDict.Add(AbilityType.Medkit, Resources.Load<Sprite>("Sprites/Pickups/AB_medkit"));
            spriteDict.Add(AbilityType.Multigun, Resources.Load<Sprite>("Sprites/Pickups/AB_crosshair"));
            spriteDict.Add(AbilityType.Shield, Resources.Load<Sprite>("Sprites/Pickups/AB_shield"));
            spriteDict.Add(AbilityType.SlowMotion, Resources.Load<Sprite>("Sprites/Pickups/AB_clock"));
            spriteDict.Add(AbilityType.Bullet, Resources.Load<Sprite>("Sprites/Pickups/AB_bullet"));
            spriteDict.Add(AbilityType.Laser, Resources.Load<Sprite>("Sprites/Pickups/AB_laser"));

            ClearItem();
        }

        public void UpdateItem(AbilityType ability)
        {
            canvasGroup.alpha = 1f;
            iconImage.sprite = spriteDict[ability];
        }

        public void ClearItem()
        {
            canvasGroup.alpha = 0f;
            iconImage.sprite = null;
        }

        public void UseItem(float duration)
        {
            StartCoroutine(UseItemRoutine(duration));
        }

        private IEnumerator UseItemRoutine(float duration)
        {
            float temp = duration;
            while (temp > 0)
            {
                temp -= Time.deltaTime;
                frameImage.fillAmount = temp / duration;
                yield return null;
            }

            frameImage.fillAmount = 1f;
        }
    }
}
