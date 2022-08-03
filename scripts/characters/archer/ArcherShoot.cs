using Godot;
using GrayGame.Entities;

namespace GrayGame.Characters.Archer
{
    public class ArcherShoot : KinematicBody2D
    {
        public enum State { Traveling, Stagnant }

        [Export] private int _damage = 6;
        [Export] private Vector2 _hitAngle = new Vector2(1, 0);
        [Export] private float _hitStrength = 90;

        [Export] private float _speed = 500f;
        [Export] private float _maxTravelLifeTime = 5f;
        [Export] private float _stagnantLifeTime = 1.2f;

        private State CurrentState { get; set; }

        private CharacterHitbox Hitbox { get; set; }

        private Vector2 _motion;

        private float _elapsedTime;
        private float _stagnantElapsedTime;

        public void Setup(ArcherCharacter archer, Vector2 position, int direction)
        {
            Position = position;
            RotationDegrees = -180f + (direction + 1) * 90f;
            _motion.x = _speed * direction;

            Hitbox = GetNode<CharacterHitbox>("HitboxArea");
            Hitbox.CurrentHit = new CharacterHit(_damage, _hitAngle, _hitStrength, direction);
            Hitbox.Character = archer;
        }

        public override void _PhysicsProcess(float delta)
        {
            switch (CurrentState)
            {
                case State.Traveling:
                    _elapsedTime += delta;
                    _motion = MoveAndSlide(_motion);
                    if (_motion.LengthSquared() <= 0)
                        CurrentState = State.Stagnant;
                    if (_elapsedTime >= _maxTravelLifeTime)
                        QueueFree();
                    break;

                case State.Stagnant:
                    _stagnantElapsedTime += delta;
                    Modulate = new Color(Modulate, 1 - (_stagnantElapsedTime / _stagnantLifeTime));
                    if (_stagnantElapsedTime >= _stagnantLifeTime)
                        QueueFree();
                    break;
            }
        }

        private void OnHitboxAreaHitEntity(EntityHurtbox hurtbox) => QueueFree();
    }
}