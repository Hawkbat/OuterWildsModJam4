using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class GenericRepairReceiver : RepairReceiver
    {
        public delegate void RepairEvent(GenericRepairReceiver target);

        public event RepairEvent OnRepaired;

        [SerializeField] float _repairFraction = 0f;
        [SerializeField] float _repairTime = 1f;
        bool _damaged = true;
        UITextType _uiTextType;

        public bool isDamaged => _damaged;
        public float repairFraction => _repairFraction;

        public new virtual bool IsRepairable() => _damaged;
        public new virtual bool IsDamaged() => _damaged;
        public new virtual float GetRepairFraction() => _repairFraction;

        new void Awake()
        {
            base.Awake();
            _type = Enums.RepairReceiverType_Generic;
            _damaged = _repairFraction <= 1f;
        }

        public new virtual void RepairTick()
        {
            if (!_damaged) return;
            _repairFraction = Mathf.Clamp01(_repairFraction + Time.deltaTime / _repairTime);
            if (_repairFraction >= 1f)
            {
                _damaged = false;
                if (OnRepaired != null)
                {
                    OnRepaired(this);
                }
            }
        }

        public new virtual UITextType GetRepairableName()
        {
            if (_uiTextType != UITextType.None) return _uiTextType;
            var value = GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI(name);
            var pair = TextTranslation.Get().m_table.theUITable.FirstOrDefault(p => p.Value == value);
            if (!pair.Equals(default(KeyValuePair<int, string>)))
            {
                _uiTextType = (UITextType)pair.Key;
            }
            else
            {
                var key = TextTranslation.Get().m_table.theUITable.Keys.Max() + 1;
                TextTranslation.Get().m_table.theUITable.Add(key, value);
                _uiTextType = (UITextType)key;
            }
            return _uiTextType;
        }
    }
}
