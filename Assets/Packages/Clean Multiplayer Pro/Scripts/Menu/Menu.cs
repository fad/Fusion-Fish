#if CMPSETUP_COMPLETE
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AvocadoShark
{
    public class Menu : MonoBehaviour
    {
        [Header("Game Stats")]
        public TextMeshProUGUI Players_In_Rooms;
        public TextMeshProUGUI RoomsMade;
        [HideInInspector]
        public TextMeshProUGUI optionchosentext;

        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private Toggle askToggle;
        public Action<CharacterType> newCharacterEvent;

        public enum CharacterType { Clownfish, Goldfish }
        private void Start()
        {
            ChooseNumberCharacter(PlayerPrefs.GetInt("ChosenCharacter"));
        }

        void Update()
        {
            RoomsMade.text = "Total Rooms: " + FusionConnection.Instance.nRooms;
            Players_In_Rooms.text = "Players In Rooms: " + FusionConnection.Instance.nPPLOnline;
        }

        public void ExitButton()
        {
            if(PlayerPrefs.GetString("Feedback")!="Don't ask" && UnityEngine.Random.Range(1,3) == 1)
            {
                feedbackPanel.SetActive(true);
            }
            else
            {
                ExitGame();
            }
        }

        public void DontAskAgain()
        {
            if(askToggle.isOn){
                PlayerPrefs.SetString("Feedback","Don't ask");
                Debug.Log("a");
            }
            else
                PlayerPrefs.DeleteKey("Feedback");
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
        
        private void ChooseNumberCharacter(int characterNumber)
        {
            switch (characterNumber)
            {
                case (int)CharacterType.Clownfish:
                    ChooseCharacter(CharacterType.Clownfish);
                break;                
                
                case (int)CharacterType.Goldfish:
                    ChooseCharacter(CharacterType.Goldfish);
                break;
            }
        }

        public void ChooseCharacter(CharacterType characterType)
        {
            if (characterType == CharacterType.Clownfish)
            {
                PlayerPrefs.SetInt("ChosenCharacter", (int)CharacterType.Clownfish);
                optionchosentext.text = "Choose your character: \n <b>Clownfish Chosen</b>";
                newCharacterEvent?.Invoke(CharacterType.Clownfish);
            }

            if (characterType == CharacterType.Goldfish)
            {
                PlayerPrefs.SetInt("ChosenCharacter", (int)CharacterType.Goldfish);
                optionchosentext.text = "Choose your character: \n <b>Goldfish Chosen</b>";
                newCharacterEvent?.Invoke(CharacterType.Goldfish);
            }
        }
    }
}
#endif