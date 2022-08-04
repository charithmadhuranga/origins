using System.Collections.Generic;
using Godot;
using OriginsGame.Entities;

namespace OriginsGame.Characters
{
    public class CharacterHurtbox : Area2D
    {
        [Signal] public delegate void HitEntered(EntityHitbox hitbox);
        [Signal] public delegate void HitExited(EntityHitbox hitbox);

        public List<EntityHitbox> Hitboxes { get; private set; }
        public EntityHitbox ActiveHitbox { get; private set; }

        public override void _Ready()
        {
            Hitboxes = new List<EntityHitbox>();
        }

        public void OnHurtboxAreaEntered(Area2D area)
        {
            if (area is EntityHitbox hitbox)
            {
                EmitSignal(nameof(HitEntered), hitbox);
                Hitboxes.Add(hitbox);
                ActiveHitbox = hitbox;
            }
        }

        public void OnHurtboxAreaExited(Area2D area)
        {
            if (area is EntityHitbox hitbox)
            {
                EmitSignal(nameof(HitExited), hitbox);
                Hitboxes.Remove(hitbox);
                if (ActiveHitbox == hitbox)
                    ActiveHitbox = Hitboxes.Count > 0 ? Hitboxes[Hitboxes.Count - 1] : null;
            }
        }
    }
}