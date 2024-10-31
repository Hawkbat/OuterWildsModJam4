using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class FakeEyeSequenceDirector : MonoBehaviour
    {
        const float ENDING_DURATION = 17f;
        const float ENDING_SOUND_DELAY = 3f;
        const float ENDING_WRAITHS_DELAY = 2f;

        GameObject _dimensionBody;
        Transform _fakeEyeSequence;
        DarkZone _darkZone;
        Campfire _campfire;
        OWAudioSource _endingMusicSource;
        EndlessReturnToPointVolume _endlessVolume;
        DreamWraith[] _dreamWraiths;
        Transform[] _wraithTargets;
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
            _endingMusicSource = _fakeEyeSequence.Find("MusicSource").GetComponent<OWAudioSource>();
            _endlessVolume = _fakeEyeSequence.GetComponentInChildren<EndlessReturnToPointVolume>();
            _endlessVolume.SetActivation(false);

            _dreamWraiths = _fakeEyeSequence.GetComponentsInChildren<DreamWraith>();
            _wraithTargets = new Transform[_dreamWraiths.Length];
            for (int i = 0; i < _wraithTargets.Length; i++)
            {
                _wraithTargets[i] = new GameObject("Wraith Target").transform;
                _wraithTargets[i].parent = _fakeEyeSequence.transform;
            }

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
                _playingEnding = true;
                PlaceWraith(0, new Vector3(0f, 2f, 5f));
                Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.PostCredit_RuinReveal);
            }
        }

        void PlaceWraith(int index, Vector3 localPosition)
        {
            _wraithTargets[index].localPosition = localPosition;
            _dreamWraiths[index].Warp(_wraithTargets[index], true, true);
        }

        void Update()
        {
            if (_playingEnding)
            {
                _endingTimer += Time.deltaTime;
                for (int i = 1; i < _dreamWraiths.Length; i++)
                {
                    if (PassedThreshold(_endingTimer, ENDING_WRAITHS_DELAY + 0.1f * i))
                    {
                        PlaceWraith(i, Random.onUnitSphere * Random.Range(5f, 20f));
                    }
                }
                if (PassedThreshold(_endingTimer, ENDING_SOUND_DELAY))
                {
                    Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.PostCredit_RuinReveal);
                    _endingMusicSource.Play();
                }
                if (PassedThreshold(_endingTimer, ENDING_DURATION))
                {
                    _endCreditsVolume.SetActive(true);
                    _endCreditsVolume.transform.position = Locator.GetPlayerTransform().position;
                }
            }
        }

        bool PassedThreshold(float timer, float threshold)
        {
            return timer - Time.deltaTime < threshold && timer >= threshold;
        }
    }
}
