using System.Collections;
using System.Collections.Generic;
using AvocadoShark;
using UnityEngine;
using CharacterType = AvocadoShark.Menu.CharacterType;

public class SelectFishButton : MonoBehaviour
{
    [SerializeField] private Menu menu;
    [SerializeField] private CharacterType fishType;

    public void ChooseCharacter()
    {
        menu.ChooseCharacter(fishType);
    }

}
