using UnityEngine;

namespace APG
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private Platformer playerInputControls;

        private Vector2 inputMove;
        private Vector2 inputLook;
        private bool inputJump;

        // x direction maps to z direction in 3d space
        public float moveForward => inputMove.x;
        public float moveRight => inputMove.y;
        public float moveMagnitude => Vector2.SqrMagnitude(inputMove);
        public Vector3 moveDirection => new Vector3( inputMove.x, 0, inputMove.y);

        public float lookUp => inputLook.y;
        public float lookRight => inputLook.x;
        public Vector2 lookDir => inputLook;

        public bool hasJumpInput => inputJump;


        private void Awake()
        {
            playerInputControls = new Platformer();

            playerInputControls.Player.Move.performed += context => inputMove = context.ReadValue<Vector2>();
            playerInputControls.Player.Move.canceled += context => inputMove = Vector2.zero;

            playerInputControls.Player.Look.performed += context => inputLook = context.ReadValue<Vector2>();
            playerInputControls.Player.Look.canceled += context => inputLook = Vector2.zero;

            playerInputControls.Player.Jump.performed += context => inputJump = context.ReadValue<float>() > 0.5f;
            playerInputControls.Player.Jump.canceled += context => inputJump = false;

            playerInputControls.Player.Pause.performed += context => PauseApplication();
            playerInputControls.Player.Quit.performed += context => QuitApplication();
        }

        private void OnEnable()
        {
            playerInputControls.Player.Enable();
        }
        private void OnDisable()
        {
            playerInputControls.Player.Disable();
        }

        // Set game pause in the game manager
        private void PauseApplication()
        {
            //gameManager.TogglePaused();
        }

        private void QuitApplication() { Application.Quit(); }
    }
}