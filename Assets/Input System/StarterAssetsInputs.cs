#if CMPSETUP_COMPLETE
using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		public bool isPlayerWritingChat = false;

		public InputActionReference PushToTalkAction,moveAction,lookAction;
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool sprint;
		public bool attack;
		public bool suckIn;
		public bool jump;
        public bool setActiveStateMultiplayerUI;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Voice Input")]
		public bool pushToTalk;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

        public void OnSuckIn(InputValue value)
        {
            SuckInInput(value.isPressed);
        }

        public void OnJump(InputValue value)
        {
            OnJumpInput(value.isPressed);
        }
        public void OnSetActivationStateMultiplayerUI(InputValue value)
		{
			SetActivationStateMultiplayerUIInput(value.isPressed);
		}
		
		public void OnAttack(InputValue value)
		{
			AttackInput(value.isPressed);
		}

		public void EnablePushToTalk(InputAction.CallbackContext context) {
            if (isPlayerWritingChat)
                return;
            pushToTalk = true;
		}

		public void DisablePushToTalk(InputAction.CallbackContext context) {
            if (isPlayerWritingChat)
                return;
            pushToTalk = false;
        }
#endif


		public void MoveInput(Vector2 newMoveDirection) {
			if (isPlayerWritingChat)
				return;
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection) {
            if (isPlayerWritingChat)
                return;
            look = newLookDirection;
		}

		public void SprintInput(bool newSprintState) {
            if (isPlayerWritingChat)
                return;
            sprint = newSprintState;
		}
		
		public void AttackInput(bool newAttackState) {
			if (isPlayerWritingChat)
				return;
			attack = newAttackState;
		}
		
		public void SetActivationStateMultiplayerUIInput(bool newActivationState) {
			if (isPlayerWritingChat)
				return;
			setActiveStateMultiplayerUI = newActivationState;
		}		
		
		public void SuckInInput(bool newActivationState) {
			if (isPlayerWritingChat)
				return;
			suckIn = newActivationState;
		}
        public void OnJumpInput(bool newActivationState)
        {
            if (isPlayerWritingChat)
                return;
            jump = newActivationState;
        }
        
        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

		public void DisablePlayerInput()
		{
			moveAction.action.Disable();
			lookAction.action.Disable();
		}
		public void EnablePlayerInput()
		{
			moveAction.action.Enable();
			lookAction.action.Enable();
		}

		private void Awake() {
			PushToTalkAction.action.performed += EnablePushToTalk;
            PushToTalkAction.action.canceled += DisablePushToTalk;
        }
    }
	
}
#endif