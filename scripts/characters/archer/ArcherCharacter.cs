using Godot;

namespace OriginsGame.Characters.Archer
{
    public class ArcherCharacter : CharacterBase
    {
        public const string IdleAnimationKey = "idle";
        public const string RunAnimationKey = "run";
        public const string JumpAnimationKey = "jump";
        public const string DoubleJumpAnimationKey = "double-jump";
        public const string FallAnimationKey = "fall";
        public const string RollAnimationKey = "roll";
        public const string WallslideAnimationKey = "wallslide";
        public const string ShootAnimationKey = "shoot";
        public const string DieAnimationKey = "die";

        [Export] private ArcherData _data = null;
        public ArcherData Data => _data;

        public ArcherStateMachine StateMachine { get; private set; }
        public ArcherInputReader InputReader { get; private set; }

        public Vector2 BodyVelocity;
        public float GravityScale { get; set; }

        public bool IsGrounded { get; private set; }
        public bool IsTouchingWall { get; private set; }
        public bool IsShootObsolete { get; private set; }

        public Sprite PlayerSprite { get; private set; }
        public AnimationPlayer PlayerAnimator { get; private set; }
        public RayCast2D GroundRayCast { get; private set; }
        public RayCast2D WallRayCast { get; private set; }
        public CollisionShape2D CollisionShape { get; private set; }
        public Position2D ShootPosition { get; private set; }
        public Position2D DoubleJumpGfxPosition { get; private set; }
        public CharacterHurtbox Hurtbox { get; private set; }

        public bool LandedAfterDoubleJump { get; set; }

        public override void _Ready()
        {
            PlayerSprite = GetNode<Sprite>("Sprite");
            PlayerAnimator = GetNode<AnimationPlayer>("Animator");
            CollisionShape = GetNode<CollisionShape2D>("CollisionShape");
            GroundRayCast = GetNode<RayCast2D>("RayCastContainer/GroundRayCast");
            WallRayCast = GetNode<RayCast2D>("RayCastContainer/WallRayCast");
            ShootPosition = GetNode<Position2D>("ShootPosition");
            DoubleJumpGfxPosition = GetNode<Position2D>("DoubleJumpGfxPosition");
            Hurtbox = GetNode<CharacterHurtbox>("Hurtbox");

            GravityScale = 1f;
            FacingDirection = -1;
            LandedAfterDoubleJump = true;
            FlipFacingDirection();

            InputReader = new ArcherInputReader(this);
            StateMachine = new ArcherStateMachine(this);
        }

        public override void _Process(float delta)
        {
            InputReader.Process();
            StateMachine.Process(delta);
        }

        public override void _PhysicsProcess(float delta)
        {
            BodyVelocity.y += Data.GravityForce * GravityScale;
            if (BodyVelocity.y > Data.MaxFallSpeed)
                BodyVelocity.y = Data.MaxFallSpeed;

            IsGrounded = GroundRayCast.IsColliding();
            IsTouchingWall = WallRayCast.IsColliding();
            IsShootObsolete = GetWorld2d().DirectSpaceState.IntersectRay(GlobalPosition,
                GlobalPosition + new Vector2(ShootPosition.Position.x * FacingDirection, ShootPosition.Position.y),
                collisionLayer: Data.GroundLayer).Count != 0;

            if (!LandedAfterDoubleJump && IsGrounded || IsTouchingWall)
                LandedAfterDoubleJump = true;

            // TODO: Enter on hurt state

            StateMachine.PhysicsProcess(delta);

            BodyVelocity = MoveAndSlide(BodyVelocity, upDirection: Vector2.Up, stopOnSlope: true);
        }

        public void SwitchAnimation(string animationKey)
        {
            PlayerAnimator.Play(animationKey);
        }

        public void ApplyHorizontalMovement(float acceleration, float maxSpeed)
        {
            BodyVelocity.x += acceleration;
            BodyVelocity.x = Mathf.Clamp(BodyVelocity.x, -maxSpeed, maxSpeed);
        }

        public void ApplyHorizontalFriction(float amount)
        {
            if (Mathf.Abs(BodyVelocity.x) > amount)
                BodyVelocity.x -= Mathf.Sign(BodyVelocity.x) * amount;
            else BodyVelocity.x = 0;
        }

        public void ApplyHorizontalAirMovement(float acceleration, float maxSpeed, float friction)
        {
            if (Mathf.Abs(acceleration) - 0.1f > 0f)
                ApplyHorizontalMovement(acceleration, maxSpeed);
            else
                ApplyHorizontalFriction(friction);
        }

        public void FlipFacingDirection() => FlipFacingDirectionTo(FacingDirection * -1);

        public void FlipFacingDirectionTo(int direction)
        {
            FacingDirection = direction;
            PlayerSprite.FlipH = FacingDirection == -1;
            WallRayCast.Position = new Vector2(Mathf.Abs(WallRayCast.Position.x) * direction, WallRayCast.Position.y);
            WallRayCast.CastTo = new Vector2(Mathf.Abs(WallRayCast.CastTo.x) * direction, WallRayCast.CastTo.y);
        }

        public void FlipByVelocity()
        {
            if ((FacingDirection == -1 && BodyVelocity.x > 0) || (FacingDirection == 1 && BodyVelocity.x < 0))
                FlipFacingDirection();
        }

        public void CastShoot()
        {
            var shootPosition = Position + new Vector2(ShootPosition.Position.x * FacingDirection, ShootPosition.Position.y);
            var shootScene = Data.ShootScene.Instance<ArcherShoot>();
            shootScene.Setup(this, shootPosition, FacingDirection);
            GetTree().CurrentScene.AddChild(shootScene);
        }
    }
}