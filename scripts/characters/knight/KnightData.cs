using Godot;

namespace OriginsGame.Characters.Knight
{
    public class KnightData : Resource
    {
        [Export] private float _gravityForce = 100f;
        [Export] private float _maxFallSpeed = 300f;

        [Export] private float _horizontalMoveAcceleration = 150f;
        [Export] private float _horizontalMoveSpeed = 600f;
        [Export] private float _airHorizontalSpeed = 500f;
        [Export] private float _airHorizontalAcceleration = 100f;

        [Export] private float _crouchWalkSpeed = 300f;
        [Export] private float _crouchWalkAcceleration = 75f;

        [Export] private float _jumpDuration = 0.35f;
        [Export] private float _jumpVerticalSpeed = 300f;
        [Export] private float _jumpStopVerticalSpeed = 30f;
        [Export(PropertyHint.ExpEasing)] private float _jumpSpeedCurve = 0.1f;

        [Export] private float _slideDuration = 0.6f;
        [Export] private float _slideSpeed = 1200f;
        [Export] private float _slideTransitionTime = 0.1f;
        [Export] private ulong _slideCooldownInMS = 4000L;
        [Export] private Curve _slideSpeedCurve = null;

        [Export] private float _rollSpeed = 700f;
        [Export] private Curve _rollSpeedCurve = null;
        [Export] private ulong _rollCooldownInMS = 3500L;

        [Export] private ulong _attackComboMaxDelayInMs = 3000L;

        [Export] private float _wallslideSpeed = 150f;
        [Export] private Vector2 _walljumpSpeed = new Vector2(500f, 800f);
        [Export] private float _walljumpDuration = 0.2f;

        [Export] private float _hurtAirFriction = 300f;

        public float GravityForce => _gravityForce;
        public float MaxFallSpeed => _maxFallSpeed;
        public float HorizontalMoveAcceleration => _horizontalMoveAcceleration;
        public float HorizontalMoveSpeed => _horizontalMoveSpeed;
        public float AirHorizontalSpeed => _airHorizontalSpeed;
        public float AirHorizontalAcceleration => _airHorizontalAcceleration;
        public float CrouchWalkSpeed => _crouchWalkSpeed;
        public float CrouchWalkAcceleration => _crouchWalkAcceleration;
        public float JumpDuration => _jumpDuration;
        public float JumpVerticalSpeed => _jumpVerticalSpeed;
        public float JumpStopVerticalSpeed => _jumpStopVerticalSpeed;
        public float JumpSpeedCurve => _jumpSpeedCurve;
        public float SlideDuration => _slideDuration;
        public float SlideSpeed => _slideSpeed;
        public float SlideTransitionTime => _slideTransitionTime;
        public ulong SlideCooldownInMS => _slideCooldownInMS;
        public Curve SlideSpeedCurve => _slideSpeedCurve;
        public float RollSpeed => _rollSpeed;
        public Curve RollSpeedCurve => _rollSpeedCurve;
        public ulong RollCooldownInMS => _rollCooldownInMS;
        public ulong AttackComboMaxDelayInMs => _attackComboMaxDelayInMs;
        public float WallslideSpeed => _wallslideSpeed;
        public Vector2 WalljumpSpeed => _walljumpSpeed;
        public float WalljumpDuration => _walljumpDuration;
        public float HurtAirFriction => _hurtAirFriction;
    }
}