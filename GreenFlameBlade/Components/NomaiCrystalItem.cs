using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class NomaiCrystalItem : OWItem
    {
        [SerializeField] OWAudioSource _pickUpSource;

        public override string GetDisplayName() => GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("GFB_NomaiCrystalItem");

        public override void Awake()
        {
            _type = Enums.ItemType_NomaiCrystal;
            base.Awake();
        }

        public override void PickUpItem(Transform holdTranform)
        {
            base.PickUpItem(holdTranform);
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.ToolItemScrollPickUp);
            _pickUpSource.Play();
        }

        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            base.DropItem(position, normal, parent, sector, customDropTarget);
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.ToolItemScrollDrop);
        }
    }
}
