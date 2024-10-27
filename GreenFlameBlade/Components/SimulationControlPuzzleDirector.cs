using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class SimulationControlPuzzleDirector : MonoBehaviour
    {
        const float CONTROL_ROOM_MOVE_SPEED = 1f;
        const float CONTROL_ROOM_ROTATE_SPEED = 9f;
        static readonly bool[] CANDLE_COMBINATION = [
            true,
            false,
            true,
            false,
            true,
            false,
            true,
            false,
        ];

        Transform _controlRoom;
        bool _controlRoomOpen;
        DreamCandle[] _candles;

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
            lockOnTarget.transform.localPosition = new Vector3(0f, -2.5f, 7f);

            var pedestal = Locator.GetAstroObject(AstroObject.Name.DreamWorld).transform.Find("Sector_DreamWorld/Prefab_IP_DreamLibraryPedestal (1)").GetComponent<DreamLibraryPedestal>();
            pedestal._doorsToOpen = new AbstractDoor[0];
            pedestal._projector = projector;
        }

        void OnDestroy()
        {
            foreach (var candle in _candles)
            {
                candle.OnLitStateChanged -= CheckCombination;
            }
        }

        void Update()
        {
            var targetPoint = _controlRoomOpen ? Vector3.up * 8f : Vector3.down * 12f;
            _controlRoom.transform.localPosition = Vector3.MoveTowards(_controlRoom.transform.localPosition, targetPoint, Time.deltaTime * CONTROL_ROOM_MOVE_SPEED);
            var targetRot = _controlRoomOpen ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 359.99f, 0f);
            _controlRoom.transform.localRotation = Quaternion.RotateTowards(_controlRoom.transform.localRotation, targetRot, Time.deltaTime * CONTROL_ROOM_ROTATE_SPEED);
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
    }
}