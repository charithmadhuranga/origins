using Godot;

namespace OriginsGame.Characters.Archer
{
    public class ArcherData : Resource
    {
        [Export(PropertyHint.Layers2dPhysics)] private uint _groundLayer = 1;
        [Export] private float _gravityForce = 9.8f * 2;
        [Export] private float _maxFallSpeed = 300f;

        [Export] private float _horizontalMoveSpeed = 300f;
        [Export] private float _horizontalMoveAcceleration = 150f;
        [Export] private float _airHorizontalSpeed = 250f;
        [Export] private float _airHorizontalAcceleration = 100f;

        [Export] private float _jumpDuration = 0.3f;
        [Export] private float _jumpVerticalSpeed = 300f;
        [Export(PropertyHint.ExpEasing)] private float _jumpSpeedCurve = 0.1f;
        [Export] private float _jumpStopVerticalSpeed = 30f;
        [Export] private float _doubleJumpDuration = 0.3f;
        [Export] private float _doubleJumpVerticalSpeed = 250f;
        [Export(PropertyHint.ExpEasing)] private float _doubleJumpSpeedCurve = -0.05f;

        [Export] private float _rollSpeed = 250f;
        [Export] private Curve _rollSpeedCurve = null;
        [Export] private ulong _rollCooldownInMS = 3000L;

        [Export] private float _wallslideSpeed = 130f;
        [Export] private Vector2 _walljumpForce = new Vector2(200f, -250f);
        [Export] private float _walljumpDuration = 0.2f;

        [Export] private PackedScene _shootScene = null;
        [Export] private PackedScene _doubleJumpGfxScene = null;

        public uint GroundLayer => _groundLayer;
        public float GravityForce => _gravityForce;
        public float MaxFallSpeed => _maxFallSpeed;
        public float HorizontalMoveSpeed => _horizontalMoveSpeed;
        public float HorizontalMoveAcceleration => _horizontalMoveAcceleration;
        public float AirHorizontalSpeed => _airHorizontalSpeed;
        public float AirHorizontalAcceleration => _airHorizontalAcceleration;
        public float JumpDuration => _jumpDuration;
        public float JumpVerticalSpeed => _jumpVerticalSpeed;
        public float JumpStopVerticalSpeed => _jumpStopVerticalSpeed;
        public float JumpSpeedCurve => _jumpSpeedCurve;
        public float DoubleJumpDuration => _doubleJumpDuration;
        public float DoubleJumpVerticalSpeed => _doubleJumpVerticalSpeed;
        public float DoubleJumpSpeedCurve => _doubleJumpSpeedCurve;
        public float RollSpeed => _rollSpeed;
        public Curve RollSpeedCurve => _rollSpeedCurve;
        public ulong RollCooldownInMS => _rollCooldownInMS;
        public float WallslideSpeed => _wallslideSpeed;
        public Vector2 WalljumpForce => _walljumpForce;
        public float WalljumpDuration => _walljumpDuration;
        public PackedScene ShootScene => _shootScene;
        public PackedScene DoubleJumpGfxScene => _doubleJumpGfxScene;
    }
}