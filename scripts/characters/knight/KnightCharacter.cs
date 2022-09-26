using Godot;
using OriginsGame.Entities;
using System;
using System.Collections.Generic;

namespace OriginsGame.Characters.Knight
{
    public enum KnightCollisionShape { Stand, Crouch }

    public class KnightCharacter : CharacterBase
    {
        public const string IdleAnimationKey = "idle";
        public const string RunAnimationKey = "run";
        public const string JumpAnimationKey = "jump";
        public const string FallAnimationKey = "fall";
        public const string CrouchIdleAnimationKey = "crouch-idle";
        public const string CrouchTransitionAnimationKey = "crouch-transition";
        public const string CrouchWalkAnimationKey = "crouch-walk";
        public const string RollAnimationKey = "roll";
        public const string SlideAnimationKey = "slide";
        public const string SlideTransitionAnimationKey = "slide-transition";
        public const string WallslideAnimationKey = "wallslide";
        public const string FirstAttackAnimationKey = "first-attack";
        public const string SecondAttackAnimationKey = "second-attack";
        public const string CrouchAttackAnimationKey = "crouch-attack";
        public const string HurtAnimationKey = "hurt";
        public const string DieAnimationKey = "die";

        [Export] private KnightData _data = null;
        public KnightData Data => _data;

        public KnightStateMachine StateMachine { get; private set; }
        public KnightInputReader InputReader { get; private set; }

        public Vector2 BodyVelocity;
        public float GravityScale { get; set; }

        public bool CanStand { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsTouchingWall { get; private set; }

        public Sprite PlayerSprite { get; private set; }
        public AnimationPlayer PlayerAnimator { get; private set; }
        public RayCast2D GroundRayCast { get; private set; }
        public RayCast2D WallRayCast { get; private set; }
        public CollisionShape2D StandCollision { get; private set; }
        public CollisionShape2D CrouchCollision { get; private set; }
        public Area2D HeadArea { get; private set; }
        public CharacterHitbox AttackHitbox { get; private set; }
        public CharacterHurtbox Hurtbox { get; private set; }

        private Queue<Action> _animationFinishQueues;

        public override void _Ready()
        {
            PlayerSprite = GetNode<Sprite>("Sprite");
            PlayerAnimator = GetNode<AnimationPlayer>("Animator");
            StandCollision = GetNode<CollisionShape2D>("StandCollision");
            CrouchCollision = GetNode<CollisionShape2D>("CrouchCollision");
            GroundRayCast = GetNode<RayCast2D>("RayCastContainer/GroundRayCast");
            WallRayCast = GetNode<RayCast2D>("RayCastContainer/WallRayCast");
            HeadArea = GetNode<Area2D>("HeadArea");
            AttackHitbox = GetNode<CharacterHitbox>("AttackArea");
            Hurtbox = GetNode<CharacterHurtbox>("Hurtbox");

            GravityScale = 1f;
            FacingDirection = -1;
            FlipFacingDirection();

            AttackHitbox.Character = this;

            _animationFinishQueues = new Queue<Action>();

            InputReader = new KnightInputReader(this);
            StateMachine = new KnightStateMachine(this);
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

            CanStand = HeadArea.GetOverlappingBodies().Count == 0;
            IsGrounded = GroundRayCast.IsColliding();
            IsTouchingWall = WallRayCast.IsColliding();

            if (Hurtbox.ActiveHitbox != null && !IsInvincible())
                StateMachine.HurtState.Enter(Hurtbox.ActiveHitbox);

            StateMachine.PhysicsProcess(delta);

            BodyVelocity = MoveAndSlide(BodyVelocity, upDirection: Vector2.Up, stopOnSlope: true);
        }

        public void SwitchAnimation(string animationKey)
        {
            PlayerAnimator.Play(animationKey);
        }

        public void WaitForAnimationFinish(Action finishAction)
        {
            _animationFinishQueues.Enqueue(finishAction);
        }

        public void SwitchAnimationAndWaitFinish(string animationKey, Action finishAction)
        {
            SwitchAnimation(animationKey);
            WaitForAnimationFinish(finishAction);
        }

        public void SetCollisionShape(KnightCollisionShape shape)
        {
            switch (shape)
            {
                case KnightCollisionShape.Stand:
                    StandCollision.Disabled = false;
                    CrouchCollision.Disabled = true;
                    break;
                case KnightCollisionShape.Crouch:
                    CrouchCollision.Disabled = false;
                    StandCollision.Disabled = true;
                    break;
            }
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

        public bool IsInvincible()
        {
            return StateMachine.CurrentState is IKnightInvincibleState;
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

        public void PerformAttack(int damage, float moveOffset, Vector2 angle, float strength)
        {
            (StateMachine.CurrentState as KnightAttackState)?.PerformAttack(damage, moveOffset, angle, strength);
        }

        public void OnAnimatorAnimationFinish(string animName)
        {
            while (_animationFinishQueues.Count != 0)
                _animationFinishQueues.Dequeue().Invoke();
        }
    }
}
