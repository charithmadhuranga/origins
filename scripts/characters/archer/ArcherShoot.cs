using Godot;
using GrayGame.Entities;

namespace GrayGame.Characters.Archer
{
    public class ArcherShoot : Area2D
    {
        public enum State { Traveling, Stagnant }

        [Export] private int _damage = 6;
        [Export] private Vector2 _initialVelocity = new Vector2(500f, 0f);
        [Export] private float _baseStrength = 90;
        [Export] private float _mass = 0.1f;

        [Export] private float _maxTravelLifeTime = 5f;
        [Export] private float _stagnantLifeTime = 1.2f;

        private State CurrentState { get; set; }

        private CharacterHitbox Hitbox { get; set; }

        private Vector2 _motion;

        private float _elapsedTime;
        private float _stagnantElapsedTime;
        private int _direction;

        public void Setup(ArcherCharacter archer, Vector2 position, int direction)
        {
            Position = position;
            _direction = direction;
            _motion = new Vector2(_initialVelocity.x * direction, _initialVelocity.y);

            Hitbox = GetNode<CharacterHitbox>("HitboxArea");
            Hitbox.Character = archer;
            Hitbox.GetHit = () => new CharacterHit(_damage, _motion.Normalized().Abs(), _baseStrength * _motion.Length() / _initialVelocity.Length(), _direction);
        }

        public override void _PhysicsProcess(float delta)
        {
            // TODO: Fix direction hit
            switch (CurrentState)
            {
                case State.Traveling:
                    _motion += GravityVec * Gravity * _mass;
                    Position += _motion * delta;
                    Rotation = _motion.Angle();
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

        public void SwitchToStagnant()
        {
            if (CurrentState == State.Stagnant)
                return;

            CurrentState = State.Stagnant;
            Hitbox.QueueFree();
        }

        private void OnAreaEntered(Area2D area) => SwitchToStagnant();

        private void OnBodyEntered(Node body) => SwitchToStagnant();

        private void OnHitboxAreaHitEntity(EntityHurtbox hurtbox) => QueueFree();
    }
}