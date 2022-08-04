using Godot;

namespace OriginsGame.Entities
{
    public class EntityHitboxSetup : Node2D
    {
        [Export] private int _damage = 0;
        [Export] private float _strength = 150f;
        [Export] private Vector2 _angle = new Vector2(2f, -1.25f);

        public override void _Ready()
        {
            EntityHitbox hitbox = GetNodeOrNull<EntityHitbox>("Hitbox");
            if (hitbox != null)
                hitbox.CurrentHit = new EntityHit(_damage, _strength, _angle);
        }
    }
}