using Godot;

namespace GrayGame.Characters.Archer
{
    public class ArcherInputReader
    {
        public ArcherCharacter Archer { get; }
        public float HorizontalMove { get; private set; }

        public InputAction Jump { get; }
        public InputAction Shoot { get; }
        public InputAction Roll { get; }

        public ArcherInputReader(ArcherCharacter character)
        {
            Archer = character;
            Jump = new InputAction(InputActions.JumpAction);
            Shoot = new InputAction(InputActions.AttackAction);
            Roll = new InputAction(InputActions.DashAction);
        }

        public void Process()
        {
            HorizontalMove = Input.GetAxis(InputActions.MoveLeftAction, InputActions.MoveRightAction);
            Jump.Update();
            Shoot.Update();
            Roll.Update();
        }
    }
}