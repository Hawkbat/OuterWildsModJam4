using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class FakeEyeSequenceDirector : MonoBehaviour
    {
        const float ENDING_DURATION = 16f;

        GameObject _dimensionBody;
        Transform _fakeEyeSequence;
        DarkZone _darkZone;
        Campfire _campfire;
        OWAudioSource _musicSource;
        EndlessReturnToPointVolume _endlessVolume;
        GameObject _endCreditsVolume;
        bool _playingEnding;
        float _endingTimer;

        void Awake()
        {
            _dimensionBody = GreenFlameBlade.Instance.NewHorizons.GetPlanet("Ringed Planet");
            
            _fakeEyeSequence = _dimensionBody.transform.Find("Sector/FakeEyeSequence");

            var darkZoneObj = new GameObject("Dark Zone");
            darkZoneObj.SetActive(false);
            darkZoneObj.AddComponent<OWTriggerVolume>();
            _darkZone = darkZoneObj.AddComponent<DarkZone>();
            _darkZone._ambientLight = _dimensionBody.transform.Find("Sector/Atmosphere/AmbientLight_DB_Interior").GetComponent<Light>();
            _darkZone._planetaryFog = _dimensionBody.transform.Find("Sector/Atmosphere/FogSphere_Hub").GetComponent<PlanetaryFogController>();
            _darkZone.transform.parent = _fakeEyeSequence.transform;
            darkZoneObj.SetActive(true);

            _campfire = _fakeEyeSequence.GetComponentInChildren<Campfire>();
            _campfire.OnCampfireStateChange += OnCampfireStateChange;
            _musicSource = _fakeEyeSequence.Find("MusicSource").GetComponent<OWAudioSource>();
            _endlessVolume = _fakeEyeSequence.GetComponentInChildren<EndlessReturnToPointVolume>();
            _endlessVolume.SetActivation(false);


            _endCreditsVolume = _dimensionBody.transform.Find("Sector/EndCreditsVolume").gameObject;
            _endCreditsVolume.SetActive(false);

            GlobalMessenger.AddListener(GlobalMessengerEvents.EnterFakeEyeSequence, OnEnterFakeEyeSequence);
        }

        void OnDestroy()
        {
            _campfire.OnCampfireStateChange -= OnCampfireStateChange;
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.EnterFakeEyeSequence, OnEnterFakeEyeSequence);
        }

        void OnEnterFakeEyeSequence()
        {
            DialogueConditionManager.SharedInstance.SetConditionState("GFB_FAKE_EYE_SEQUENCE", true);
            _darkZone.AddPlayerToZone(true);
            _endlessVolume.SetActivation(true);
            _endlessVolume.WarpBody(Locator.GetPlayerBody());
        }

        void OnCampfireStateChange(Campfire fire)
        {
            if (fire.GetState() == Campfire.State.LIT)
            {
                _musicSource.Play();
                _playingEnding = true;
            }
        }

        void Update()
        {
            if (_playingEnding)
            {
                _endingTimer += Time.deltaTime;
                if (_endingTimer - Time.deltaTime < ENDING_DURATION && _endingTimer >= ENDING_DURATION)
                {
                    _endCreditsVolume.SetActive(true);
                    _endCreditsVolume.transform.position = Locator.GetPlayerTransform().position;
                }
            }
        }
    }
}
