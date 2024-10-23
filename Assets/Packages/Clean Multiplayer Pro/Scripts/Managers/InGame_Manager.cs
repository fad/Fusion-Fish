#if CMPSETUP_COMPLETE
using Fusion;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AvocadoShark
{
    public class InGame_Manager : NetworkBehaviour
    {
        public TextMeshProUGUI roomName;
        public TextMeshProUGUI players;
        public Image lockImage;
        public Sprite lockedSprite;
        public TextMeshProUGUI lockStatusText;
        public TextMeshProUGUI infoText;
        [HideInInspector] public float deltaTime;
        private float fps;
        public int ping;
        public NetworkRunner runner;
        
        private void Start()
        {
            if (PlayerPrefs.GetInt("has_pass") == 1)
            {
                lockImage.sprite = lockedSprite;
                lockStatusText.text = "private";
            }
            fps = 1.0f / Time.smoothDeltaTime;

            runner = NetworkRunner.GetRunnerForGameObject(gameObject);
        }
        
        public void LeaveRoom()
        {
            var fusionManager = FindObjectOfType<FusionConnection>();
            if (fusionManager != null)
            {
                Destroy(fusionManager);
            }
            Runner.Shutdown();
            SceneManager.LoadScene("Menu");
        }
        
        private float smoothedRTT;
        private void LateUpdate()
        { 
            if(!Object)
                return;
            roomName.text = Runner.SessionInfo.Name;
            players.text = Runner.SessionInfo.PlayerCount + "/" + Runner.SessionInfo.MaxPlayers;
            var newFPS = 1.0f / Time.smoothDeltaTime;
            fps = Mathf.Lerp(fps, newFPS, 0.005f);

            var rttInSeconds = runner.GetPlayerRtt(PlayerRef.None);
            var rttInMilliseconds = (int)(rttInSeconds * 1000);
            smoothedRTT = Mathf.Lerp(smoothedRTT, rttInMilliseconds, 0.005f);
            var ping = (int)smoothedRTT / 2;
            infoText.text = "Ping: " + ping + "\n" + "FPS: " + (int)fps;
        }

        private void SwitchScene()
        {
            if (!HasStateAuthority)
            {
                SceneSwitchRpc();
                return;
            }
            if (!Runner.IsSceneAuthority) 
                return;
            
            //Assuming environment scene in additive mode is loaded at 1 index
            var environmentSceneIndex = 1;
            var environmentScene = SceneManager.GetSceneAt(environmentSceneIndex);
            print(environmentScene.name);
            
            Runner.LoadScene(SceneRef.FromIndex(environmentScene.buildIndex), LoadSceneMode.Additive);
            Runner.UnloadScene(SceneRef.FromIndex(environmentScene.buildIndex));
        }

        [Rpc(RpcSources.Proxies,RpcTargets.StateAuthority)]
        private void SceneSwitchRpc()
        {
            SwitchScene();
        }
    }
}
#endif
