#if CMPSETUP_COMPLETE
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using StarterAssets;
using Fusion.Photon.Realtime;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Linq;

namespace AvocadoShark
{
    public class FusionConnection : MonoBehaviour, INetworkRunnerCallbacks
    {
        #region ThingsAddedByMe

        public List<PlayerRef> playersInSession = new List<PlayerRef>();

        #endregion

        public RoomEntry CurrentEntryBeingEdited;
        public Action OnDestroyRoomEntries;

        public static FusionConnection Instance;
        
        [Header("All player prefabs")]
        [SerializeField] private AllPlayerPrefabsSO allPlayerPrefabs;

        [SerializeField] private SpawnPointsData spawnPointsData;
        [SerializeField] private NetworkRunner runnerPrefab;
        public NetworkRunner Runner { get; private set; }

        public bool hasEnteredGameScene = false;

        [Header("Player 1")][SerializeField] public GameObject playerPrefabFirst;
        [Header("Player 2")][SerializeField] public GameObject playerPrefabSecond;

        [Header("Name Entry")] public GameObject mainObject;
        public Button submitButton;
        public TMP_InputField nameField;
        public GameObject characterselectionobject;
        private const string NamePlayerPrefs = "Name";
        private const string RoomNamePlayerPrefs = "RoomName";
        
        private const string DesiredRoomNamePrefs = "DesiredRoomName";

        [Header("Room List")] public RoomEntry roomEntryPrefab;
        public GameObject roomListObject;
        public Transform content;
        public Button createRoomButton;
        public TextMeshProUGUI NoRoomsText;
        public TMP_InputField room_search;

        [Header("Room List Refresh (s)")]
        [SerializeField]
        private float refreshInterval = 2f;

        private FusionVoiceClient _fvc;
        private Recorder _recorder;
        private VoiceManager _voiceManager;

        [Header("Loading Screen")] public LoadingScreen loadingScreenScript;

        [Header("Loading Screen")] public PopUp popup;

        [Header("UI")][SerializeField] private MenuCanvas menuCanvas;

        private bool initialRoomListPopulated = false;
        private List<SessionInfo> _sessionList = new List<SessionInfo>();
        private List<RoomEntry> _roomEntryList = new List<RoomEntry>();
        public int sceneNumber { get; private set; }
        [HideInInspector] public bool isConnected = false;
        [HideInInspector] public string _playerName = null;
        [HideInInspector] public int nRooms = 0;
        [HideInInspector] public int nPPLOnline = 0;

        public TMP_Dropdown region_select;

        [Header("Environment")]
        [SerializeField] private TMP_Dropdown environmentDropdown;
        [SerializeField] private Material oceanSky;
        [SerializeField] private Material riverSky;

        private GameObject _selectedPlayerPrefab;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            room_search.onValueChanged.AddListener(OnSearchTextValueChange);
#if UNITY_2022_3_OR_NEWER
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
#else
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
#endif

            int region_n = PlayerPrefs.GetInt("region");
            string region = null;
            if (region_n == 0)
            {
                region = "";
            }
            else if (region_n == 1)
            {
                region = "asia";
            }
            else if (region_n == 2)
            {
                region = "eu";
            }
            else if (region_n == 3)
            {
                region = "jp";
            }
            else if (region_n == 4)
            {
                region = "kr";
            }
            else if (region_n == 5)
            {
                region = "us";
            }
            region_select.value = region_n;
            PhotonAppSettings settings = Resources.Load<PhotonAppSettings>("PhotonAppSettings");
            settings.AppSettings.FixedRegion = region;
        }

        private void Start()
        {
            nameField.text = PlayerPrefs.GetString(NamePlayerPrefs);
            menuCanvas.nameInputField.text = PlayerPrefs.GetString(RoomNamePlayerPrefs);
        }

        public void ChangeRegion(int region_n)
        {
            string region = null;
            PhotonAppSettings settings = Resources.Load<PhotonAppSettings>("PhotonAppSettings");
            if (region_n == 0)
            {
                region = "";
            }
            else if (region_n == 1)
            {
                region = "asia";
            }
            else if (region_n == 2)
            {
                region = "eu";
            }
            else if (region_n == 3)
            {
                region = "jp";
            }
            else if (region_n == 4)
            {
                region = "kr";
            }
            else if (region_n == 5)
            {
                region = "us";
            }
            settings.AppSettings.FixedRegion = region;
            PlayerPrefs.SetInt("region", region_n);
        }
        public void RefreshRoomList()
        {
            InitialRoomListSetup();
        }

        private IEnumerator AutoRefreshRoomList()
        {
            while (true)
            {
                RefreshRoomList();
                yield return new WaitForSeconds(refreshInterval);
            }
        }

        public void OnSearchTextValueChange(string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            {
                foreach (var i in _roomEntryList)
                {
                    i.gameObject.SetActive(true);
                }
            }

            foreach (var i in _roomEntryList)
            {
                if (value != null && i.roomName.text.Contains(value))
                    i.gameObject.SetActive(true);
                else
                {
                    i.gameObject.SetActive(false);
                }
            }
        }

        public void CreateRoom()
        {
            PlayerPrefs.SetInt("has_pass", 0);
            loadingScreenScript.gameObject.SetActive(true);
            Invoke(nameof(ContinueCreateRoom), loadingScreenScript.lerpSpeed);
        }
        
        public void CreateRoomSingleplayer()
        {
            PlayerPrefs.SetInt("has_pass", 0);
            loadingScreenScript.gameObject.SetActive(true);
            Invoke(nameof(ContinueCreateRoomSingleplayer), loadingScreenScript.lerpSpeed);
        }
        
        private void ContinueCreateRoom()
        {
            string sessionName = null;
            string sessionPassword = null;

            //it counts +2 for the scene index because there are the menu & game scene that need to be excluded
            // sceneNumber = environmentDropdown.value + 2;

            int maxPlayers = 10;
            if (IsRoomNameValid())
            {
                do
                {
                    // sessionName = menuCanvas.GetRoomName() + "-" + Random.Range(1000, 9999);
                    sessionName = PlayerPrefs.GetString(DesiredRoomNamePrefs, "Room") + "-" + Random.Range(1000, 9999);
                }
                while(_sessionList.FirstOrDefault(session => session.Name == sessionName));

                // PlayerPrefs.SetString(RoomNamePlayerPrefs, menuCanvas.nameInputField.text);
                PlayerPrefs.SetString(RoomNamePlayerPrefs, PlayerPrefs.GetString(DesiredRoomNamePrefs, "Room"));
                
                //sessionPassword = menuCanvas.GetPassword();
                //maxPlayers = menuCanvas.GetMaxPlayers();
            }
            else
            {
                int randomInt = Random.Range(1000, 9999);
                //sessionPassword = menuCanvas.GetPassword();
                //maxPlayers = menuCanvas.GetMaxPlayers();
                sessionName = "Room-" + randomInt;
            }

            Debug.Log($"Session name is {sessionName}");

            Debug.Log($"maxPlayers is {maxPlayers}");

            if (menuCanvas.isPasswordEnabled)
            {
                //PlayerPrefs.SetInt("has_pass", 1);
                //JoinRoom(sessionName, maxPlayers, sessionPassword);
            }
            else
            {
            }

            //If password is enabled, make this line of code be executed in the else statement above
            JoinRoom(sessionName, maxPlayers, string.Empty);

            StopCoroutine(AutoRefreshRoomList());
        }
        
        private void ContinueCreateRoomSingleplayer()
        {
            string sessionName = PlayerPrefs.GetString(NamePlayerPrefs);
            
            sessionName += "-" + Random.Range(1000, 9999);
            
            //it counts +2 for the scene index because there are the menu & game scene that need to be excluded
            // sceneNumber = environmentDropdown.value + 2;
            

            Debug.Log("Session name is Singleplayer");

            JoinRoomSingleplayer(sessionName);

            StopCoroutine(AutoRefreshRoomList());
        }

        private bool IsRoomNameValid()
        {
            return menuCanvas.GetRoomName().Length != 0;
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if (initialRoomListPopulated == false)
            {
                //StartCoroutine(AutoRefreshRoomList());
                loadingScreenScript.FadeOutAndDisable();
            }

            _sessionList = sessionList;
            nRooms = sessionList.Count;

            nPPLOnline = 0;
            foreach (var session in sessionList)
            {
                nPPLOnline += session.PlayerCount;
            }

            RefreshRoomList();
        }

        private void InitialRoomListSetup()
        {
            if (roomListObject == null)
                return;
            initialRoomListPopulated = true;
            roomListObject.SetActive(true);

            OnDestroyRoomEntries?.Invoke();

            //foreach (Transform child in content)
            //{
            //    Destroy(child.gameObject);
            //}

            foreach (SessionInfo session in _sessionList)
            {
                if(session.Properties.TryGetValue("single", out SessionProperty isSingleplayer) && (bool)isSingleplayer.PropertyValue)
                {
                    continue;
                }
                
                if (CurrentEntryBeingEdited != null)
                {
                    if (session.Name == CurrentEntryBeingEdited.sessionInfo.Name)
                    {
                        UpdateCurrentEntryBeingEdited(session);
                        continue;
                    }
                }

                RoomEntry entryScript = Instantiate(roomEntryPrefab, content);
                entryScript.Init(session, this);
                _roomEntryList.Add(entryScript);
            }

            NoRoomsText.gameObject.SetActive(_sessionList.Count == 0);
        }

        //public void AddPlayerToBannedList(string name) {
        //runner.SessionInfo.UpdateCustomProperties();
        //}

        public void ConnectToRunner()
        {
            loadingScreenScript.gameObject.SetActive(true);
            Invoke(nameof(ContinueConnectToRunner), loadingScreenScript.lerpSpeed);
        }
        private void ContinueConnectToRunner()
        {
            // PlayerPrefs.SetString(NamePlayerPrefs, nameField.text);
            // _playerName = nameField.text;
            
            _playerName = PlayerPrefs.GetString(NamePlayerPrefs);
            mainObject.SetActive(false);
            characterselectionobject.SetActive(false);
            SetUpComponents();
            Runner.JoinSessionLobby(SessionLobby.Shared);
        }
        private void SetUpComponents()
        {
            Runner = Instantiate(runnerPrefab);
            _fvc = Runner.GetComponent<FusionVoiceClient>();
            _recorder = Runner.GetComponentInChildren<Recorder>();
            _voiceManager = Runner.GetComponentInChildren<VoiceManager>();
            Runner.AddCallbacks(this);
        }

        public async void JoinRoom(string sessionName, string password = null)
        {
            int buildIndex = -1;

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (sceneName == "Game")
                {
                    buildIndex = i;
                    break;
                }
            }

            StopCoroutine(AutoRefreshRoomList());
            if (Runner == null)
            {
                SetUpComponents();
            }

            var result = await Runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                Scene = SceneRef.FromIndex(buildIndex),
                ObjectProvider = Runner.GetComponent<FusionPool>()
            });
            if (result.Ok)
                return;
            popup.ShowPopup(result.ShutdownReason.ToString());
        }

        public async void JoinRoom(string sessionName, int maxPlayers, string password = null)
        {
            int buildIndex = -1;

            var sessionProperties = new Dictionary<string, SessionProperty> { { "password", password }, { "sceneNumber", sceneNumber } };

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (sceneName == "Game")
                {
                    buildIndex = i;
                    break;
                }
            }

            StopCoroutine(AutoRefreshRoomList());

            if (Runner == null)
            {
                SetUpComponents();
            }

            var result = await Runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                Scene = SceneRef.FromIndex(buildIndex),
                SessionProperties = sessionProperties,
                PlayerCount = maxPlayers
            });
            if (result.Ok)
            {
                return;
            }
            popup.ShowPopup(result.ShutdownReason.ToString());
        }
        
        public async void JoinRoomSingleplayer(string sessionName)
        {
            int buildIndex = -1;

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (sceneName == "Game")
                {
                    buildIndex = i;
                    break;
                }
            }

            var sessionProperties = new Dictionary<string, SessionProperty>
            {
                {"single", true }, // Singleplayer session; short name to avoid too much traffic
                {"sceneNumber", sceneNumber}
            };


            StopCoroutine(AutoRefreshRoomList());
            if (Runner == null)
            {
                SetUpComponents();
            }

            var result = await Runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Single,
                SessionName = sessionName,
                Scene = SceneRef.FromIndex(buildIndex),
                ObjectProvider = Runner.GetComponent<FusionPool>(),
                SessionProperties = sessionProperties
            });
            
            if (result.Ok) return;
            popup.ShowPopup(result.ShutdownReason.ToString());
        }

        #region INetworkCallbacks

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("OnConnectedToServer");
            isConnected = true;
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            isConnected = false;
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            popup.ShowPopup(reason.ToString());
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            playersInSession.Add(player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }

            Debug.Log("Scene Load Done.");
            if (hasEnteredGameScene)
                return;
            hasEnteredGameScene = true;

            if (Runner.IsSceneAuthority)
            {
                Runner.LoadScene(SceneRef.FromIndex(sceneNumber), LoadSceneMode.Additive);

                RenderSettings.skybox = (sceneNumber) switch
                {
                    2 => oceanSky,
                    3 => riverSky,
                    _ => RenderSettings.skybox
                };
            }

            if (runner.GetPlayerObject(runner.LocalPlayer) != null)
                return;
            
            // var playerPrefab = PlayerPrefs.GetInt("ChosenCharacter") == 0 ? playerPrefabSecond : playerPrefabFirst;
            var playerPrefab = _selectedPlayerPrefab;

            if (runner.SessionInfo.Properties is not null)
            {
                sceneNumber = runner.SessionInfo.Properties["sceneNumber"];
            }
            
            if (!Runner.IsSceneAuthority)
            {
                RenderSettings.skybox = sceneNumber switch
                {
                    2 => oceanSky,
                    3 => riverSky,
                    _ => RenderSettings.skybox
                };
            }

            List<SpawnPoint> currentSceneSpawnPoints = spawnPointsData.GetSpawnPoints(sceneNumber);

            var spawnPoint = Random.Range(0, currentSceneSpawnPoints.Count);
            var location = currentSceneSpawnPoints[spawnPoint].location;
            var rotation = currentSceneSpawnPoints[spawnPoint].rotation;

            NetworkObject playerObject = runner.Spawn(playerPrefab, location, rotation);
            var playerTransform = playerObject.transform;
            playerTransform.position = location;
            playerTransform.rotation = rotation;

            _voiceManager.Init(playerObject.GetComponent<StarterAssetsInputs>(),
                playerObject.GetComponent<PlayerWorldUIManager>());


            runner.SetPlayerObject(runner.LocalPlayer, playerObject);

            //SessionPlayers.instance.AddPlayer(runner.LocalPlayer);
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }
        #endregion
        public void Checkforname()
        {
            if (nameField.text != "")
                submitButton.interactable = true;
            else
                submitButton.interactable = false;
        }

        public void UpdateCurrentEntryBeingEdited(SessionInfo session)
        {
            CurrentEntryBeingEdited.UpdateEntry(session, this);
        }
        public void SetCurrentEntryBeingEdited(RoomEntry roomEntry)
        {
            if (CurrentEntryBeingEdited != null)
            {
                // If we were previously editing an entry
                OnDestroyRoomEntries += CurrentEntryBeingEdited.DestroyEntry;
                CurrentEntryBeingEdited = roomEntry;
            }
            else
            {
                // If we were not editing any entry
                CurrentEntryBeingEdited = roomEntry;
            }
        }
        public void ResetCurrentEntryBeingEdited(RoomEntry roomEntry)
        {
            if (roomEntry == CurrentEntryBeingEdited)
            {
                OnDestroyRoomEntries += CurrentEntryBeingEdited.DestroyEntry;
                CurrentEntryBeingEdited = null;
            }
        }

        public void SetSceneNumber(int newSceneNumber)
        {
            sceneNumber = newSceneNumber;
        }

        public void SetPlayerPrefab(string prefabName)
        {
            _selectedPlayerPrefab = allPlayerPrefabs[prefabName].PlayableFish.FishPrefab;
        }
    }
}
#endif