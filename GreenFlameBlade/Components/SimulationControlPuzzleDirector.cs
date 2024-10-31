using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class SimulationControlPuzzleDirector : MonoBehaviour
    {
        const float CONTROL_ROOM_MOVE_SPEED = 1f;
        const float CONTROL_ROOM_ROTATE_SPEED = 9f;
        static readonly bool[] CANDLE_COMBINATION = [
            // Clockwise
            // Hut
            true,
            true,
            // Tower
            true,
            false,
            // Cliff
            false,
            true,
            // Diving Bell
            false,
            false,
        ];
        const float ESCAPE_SPEED = 2f;

        Transform _controlRoom;
        bool _controlRoomOpen;
        DreamCandle[] _candles;
        InteractReceiver _interactReceiver;
        bool _escaping;
        float _escapeTimer;
        Wraith[] _escapeWraiths;
        GameObject _cablePlugged;
        GameObject _cableUnplugged;
        OWAudioSource _stingerSource;

        void Awake()
        {
            // Set up candles for control room combination lock
            _controlRoom = Locator.GetAstroObject(AstroObject.Name.RingWorld).transform.Find("Sector_RingWorld/SimulationControlRoom/SimulationControl");
            var candleRoot = Locator.GetAstroObject(AstroObject.Name.DreamWorld).transform.Find("Sector_DreamWorld/SimulationControlCandles");
            _candles = candleRoot.GetComponentsInChildren<DreamCandle>();
            foreach (var candle in _candles)
            {
                candle.OnLitStateChanged += CheckCombination;
            }

            // Set up projector and pedestal in the house above the well
            var projector = Locator.GetAstroObject(AstroObject.Name.DreamWorld).transform.Find("Sector_DreamWorld/Prefab_IP_SlideProjector_Dream (1)").GetComponent<DreamSlideProjector>();
            projector._light._light.spotAngle = 40f;
            projector._light.SetIntensity(1.5f);
            var projectionPivot = projector.transform.Find("ProjectionPivot");
            projectionPivot.localEulerAngles = Vector3.zero;
            var screenPivot = projectionPivot.Find("BuildingKitPivot");
            screenPivot.localPosition = Vector3.zero;
            screenPivot.localEulerAngles = Vector3.zero;
            var screen = screenPivot.Find("Structure_IP_TheaterScreen");
            screen.localPosition = new Vector3(0f, -3.7f, 9f);
            screen.localEulerAngles = new Vector3(0, 180f, 0f);
            screen.localScale = Vector3.one;
            var lockOnTarget = projector._lockOnTransform;
            lockOnTarget.transform.localPosition = new Vector3(0f, -1f, 7f);
            var pedestal = Locator.GetAstroObject(AstroObject.Name.DreamWorld).transform.Find("Sector_DreamWorld/Prefab_IP_DreamLibraryPedestal (1)").GetComponent<DreamLibraryPedestal>();
            pedestal._doorsToOpen = new AbstractDoor[0];
            pedestal._projector = projector;

            // Set up wraith escape sequence
            DialogueConditionManager.SharedInstance.SetConditionState("GFB_CONTROL_ROOM_DISABLED", false);
            _interactReceiver = _controlRoom.GetComponentInChildren<InteractReceiver>();
            _interactReceiver.ChangePrompt(GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("GFB_InteractControlRoom"));
            _interactReceiver.SetInteractionEnabled(true);
            _interactReceiver.OnPressInteract += StartEscapeSequence;
            _escapeWraiths = _controlRoom.GetComponentsInChildren<Wraith>();

            _cablePlugged = _controlRoom.Find("Cable_Plugged").gameObject;
            _cableUnplugged = _controlRoom.Find("Cable_Unpugged").gameObject;
            _cableUnplugged.SetActive(false);
            _cablePlugged.SetActive(true);

            _stingerSource = _controlRoom.Find("StingerSource").GetComponent<OWAudioSource>();
        }

        void OnDestroy()
        {
            foreach (var candle in _candles)
            {
                candle.OnLitStateChanged -= CheckCombination;
            }
            _interactReceiver.OnPressInteract -= StartEscapeSequence;
        }

        void Update()
        {
            var targetPoint = _controlRoomOpen ? Vector3.up * 8f : Vector3.down * 12f;
            _controlRoom.transform.localPosition = Vector3.MoveTowards(_controlRoom.transform.localPosition, targetPoint, Time.deltaTime * CONTROL_ROOM_MOVE_SPEED);
            var targetRot = _controlRoomOpen ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 359.99f, 0f);
            _controlRoom.transform.localRotation = Quaternion.RotateTowards(_controlRoom.transform.localRotation, targetRot, Time.deltaTime * CONTROL_ROOM_ROTATE_SPEED);

            if (_escaping)
            {
                var dt = Time.deltaTime * ESCAPE_SPEED;
                _escapeTimer += dt;
                for (int i = 0; i < _escapeWraiths.Length; i++)
                {
                    if (_escapeTimer > i && _escapeTimer - dt <= i)
                    {
                        _escapeWraiths[i].Warp(WraithShip.Instance.transform, false, true);
                    }
                }
            }
        }

        void CheckCombination()
        {
            for (var i = 0; i < CANDLE_COMBINATION.Length; i++)
            {
                if (_candles[i].IsLit() != CANDLE_COMBINATION[i])
                {
                    if (_controlRoomOpen)
                    {
                        _controlRoomOpen = false;
                    }
                    return;
                }
            }
            if (!_controlRoomOpen)
            {
                _controlRoomOpen = true;
            }
        }

        void StartEscapeSequence()
        {
            _interactReceiver.SetInteractionEnabled(false);
            _escaping = true;
            _escapeTimer = 0f;
            Locator.GetShipLogManager().RevealFact("GFB_CONTROL_ROOM_DISABLED");
            DialogueConditionManager.SharedInstance.SetConditionState("GFB_CONTROL_ROOM_DISABLED", true);
            foreach (var dreamFire in FindObjectsOfType<DreamCampfire>())
            {
                dreamFire.SetState(Campfire.State.SMOLDERING);
            }
            _cableUnplugged.SetActive(true);
            _cablePlugged.SetActive(false);
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.ElectricShock);
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.NomaiPowerOff);
            _stingerSource.Play();
        }
    }
}