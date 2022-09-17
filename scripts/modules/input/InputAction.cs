using Godot;

namespace OriginsGame.Modules.Input
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
            Performed = Godot.Input.IsActionJustPressed(action);
            Pressed = Godot.Input.IsActionPressed(action);
            Released = Godot.Input.IsActionJustReleased(action);
        }
    }
}