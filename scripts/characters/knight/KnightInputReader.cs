using Godot;

namespace GrayGame.Characters.Knight
{
    public class KnightInputReader
    {
        public class InputAction
        {
            public string action;

            public bool Performed { get; private set; }
            public bool Pressed { get; private set; }
            public bool Released { get; private set; }

            public InputAction(string action)
            {
                this.action = action;
            }

            public void Update()
            {
                Performed = Input.IsActionJustPressed(action);
                Pressed = Input.IsActionPressed(action);
                Released = Input.IsActionJustReleased(action);
            }
        }

        public KnightCharacter Knight { get; }

        public float HorizontalMove { get; set; }

        public InputAction Jump { get; }
        public InputAction Dash { get; }
        public InputAction Crouch { get; }
        public InputAction Attack { get; }

        public KnightInputReader(KnightCharacter knight)
        {
            Knight = knight;

            Jump = new InputAction(InputActions.JumpAction);
            Dash = new InputAction(InputActions.DashAction);
            Crouch = new InputAction(InputActions.CrouchAction);
            Attack = new InputAction(InputActions.AttackAction);
        }

        public void Process()
        {
            HorizontalMove = Input.GetAxis(InputActions.MoveLeftAction, InputActions.MoveRightAction);
            Jump.Update();
            Dash.Update();
            Crouch.Update();
            Attack.Update();
        }
    }
}