using Godot;

namespace GrayGame
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
}