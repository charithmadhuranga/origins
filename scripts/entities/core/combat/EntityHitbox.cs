using Godot;

namespace OriginsGame.Entities
{
    public class EntityHitbox : Area2D
    {
        public EntityCore Entity { get; set; }

        public EntityHit CurrentHit { get; set; }
    }
}