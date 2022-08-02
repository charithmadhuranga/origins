using Godot;
using GrayGame.Characters;

namespace GrayGame.Tests
{
    public class EntityHitboxKnockback : KinematicBody2D
    {
        [Export] private float _gravity = 9.8f * 2f;
        [Export] private float _mass = 15f;

        private Vector2 _motion;

        public override void _PhysicsProcess(float delta)
        {
            if (_motion.Length() > _mass)
                _motion -= _mass * _motion.Normalized();
            else
                _motion = Vector2.Zero;

            _motion.y += _gravity;

            _motion = MoveAndSlide(_motion, Vector2.Up);
        }

        public void OnTakeHit(CharacterHitbox hitbox)
        {
            var knockback = hitbox.CurrentHit.angle * hitbox.CurrentHit.strength;
            knockback.x *= hitbox.CurrentHit.direction;
            _motion = knockback;
        }
    }
}