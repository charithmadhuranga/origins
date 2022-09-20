using Godot;

namespace OriginsGame.Characters.Knight
{
    public abstract class KnightStateBase
    {
        public KnightStateMachine Machine { get; }
        public KnightCharacter Knight => Machine.Knight;

        public KnightStateBase(KnightStateMachine machine)
        {
            Machine = machine;
        }

        public abstract bool CanEnter();

        public virtual void OnEnter(KnightStateBase previousState) { }

        public virtual void DoTransitionLogic() { }

        public virtual void OnProcess(float delta) { }

        public virtual void OnPhysicsProcess(float delta) { }

        public virtual void OnExit() { }

        public bool TryEnter()
        {
            if (CanEnter())
            {
                Machine.EnterState(this);
                return true;
            }
            return false;
        }
    }

    public interface IKnightCrouchState { }
    public interface IKnightInvincibleState { }

    public class KnightIdleState : KnightStateBase
    {
        public KnightIdleState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;

        public override void OnEnter(KnightStateBase previousState)
        {
            Knight.SetCollisionShape(KnightCollisionShape.Stand);
            Knight.SwitchAnimation(KnightCharacter.IdleAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.CrouchIdleState.TryEnter() ||
                Machine.CrouchWalkState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.RollState.TryEnter() ||
                Machine.TryEnterAttackState())
                return;

            if (Knight.InputReader.HorizontalMove != 0)
                Machine.EnterState(Machine.RunState);
        }

        public override void OnPhysicsProcess(float delta)
        {
            Knight.ApplyHorizontalFriction(Knight.Data.HorizontalMoveAcceleration);
        }
    }

    public class KnightRunState : KnightStateBase
    {
        public KnightRunState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Knight.InputReader.HorizontalMove != 0 && Knight.IsGrounded;

        public override void OnEnter(KnightStateBase previousState)
        {
            Knight.SetCollisionShape(KnightCollisionShape.Stand);
            Knight.SwitchAnimation(KnightCharacter.RunAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.CrouchIdleState.TryEnter() ||
                Machine.CrouchWalkState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.RollState.TryEnter() ||
                Machine.TryEnterAttackState())
                return;

            if (Knight.InputReader.HorizontalMove == 0)
                Machine.EnterState(Machine.IdleState);
        }

        public override void OnPhysicsProcess(float delta)
        {
            Knight.ApplyHorizontalMovement(Knight.Data.HorizontalMoveAcceleration * Knight.InputReader.HorizontalMove, Knight.Data.HorizontalMoveSpeed);
            Knight.FlipByVelocity();
        }
    }

    public class KnightJumpState : KnightStateBase
    {
        private float _elapsedTime;

        public KnightJumpState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Knight.IsGrounded && Knight.CanStand && Knight.InputReader.Jump.Performed;

        public override void OnEnter(KnightStateBase previousState)
        {
            _elapsedTime = 0.0f;

            Knight.SetCollisionShape(KnightCollisionShape.Stand);
            Knight.SwitchAnimation(KnightCharacter.JumpAnimationKey);
            Knight.GravityScale = 0.0f;
            // TODO: Play jump particle
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime > Knight.Data.JumpDuration || Knight.IsOnCeiling() || !Knight.InputReader.Jump.Pressed)
            {
                Knight.BodyVelocity.y = -Knight.Data.JumpStopVerticalSpeed;
                Machine.EnterDefaultState();
            }
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            float progress = _elapsedTime / Knight.Data.JumpDuration;
            float curveMultiplier = Mathf.Ease(1f - progress, Knight.Data.JumpSpeedCurve);
            float verticalSpeed = Knight.Data.JumpVerticalSpeed * curveMultiplier;

            Knight.ApplyHorizontalAirMovement(Knight.Data.AirHorizontalAcceleration * Knight.InputReader.HorizontalMove, Knight.Data.AirHorizontalSpeed, Knight.Data.AirHorizontalAcceleration);
            Knight.BodyVelocity.y = -verticalSpeed;

            Knight.FlipByVelocity();
        }

        public override void OnExit()
        {
            Knight.GravityScale = 1.0f;
        }
    }

    public class KnightFallState : KnightStateBase
    {
        private float _fallStartYPosition;

        public KnightFallState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => !Knight.IsGrounded;

        public override void OnEnter(KnightStateBase previousState)
        {
            _fallStartYPosition = Knight.Position.y;
            Knight.SwitchAnimation(KnightCharacter.FallAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Knight.IsGrounded)
                Machine.EnterDefaultState();
            else
                Machine.WallslideState.TryEnter();
        }

        public override void OnPhysicsProcess(float delta)
        {
            Knight.ApplyHorizontalAirMovement(Knight.Data.AirHorizontalAcceleration * Knight.InputReader.HorizontalMove, Knight.Data.AirHorizontalSpeed, Knight.Data.AirHorizontalAcceleration);
            Knight.FlipByVelocity();
        }
    }

    public class KnightCrouchTransitionState : KnightStateBase, IKnightCrouchState
    {
        public KnightStateBase TransitToState { get; set; }

        public KnightCrouchTransitionState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;

        public override void OnEnter(KnightStateBase previousState)
        {
            Knight.SwitchAnimationAndWaitFinish(KnightCharacter.CrouchTransitionAnimationKey, OnTransitionFinished);
        }

        public void Enter(KnightStateBase transitToState)
        {
            TransitToState = transitToState;
            Machine.EnterState(this);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.SlideState.TryEnter() ||
                Machine.RollState.TryEnter() ||
                Machine.TryEnterAttackState())
                return;

            if (!Knight.InputReader.Crouch.Pressed && Knight.CanStand)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            Knight.ApplyHorizontalMovement(Knight.Data.CrouchWalkAcceleration * Knight.InputReader.HorizontalMove, Knight.Data.CrouchWalkSpeed);
            Knight.FlipByVelocity();
        }

        private void OnTransitionFinished()
        {
            Machine.EnterState(TransitToState);
        }
    }

    public class KnightCrouchIdleState : KnightStateBase, IKnightCrouchState
    {
        public KnightCrouchIdleState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Knight.IsGrounded && Knight.InputReader.HorizontalMove == 0 && (Knight.InputReader.Crouch.Pressed || !Knight.CanStand);

        public override void OnEnter(KnightStateBase previousState)
        {
            bool shouldMakeTransition = !(previousState is IKnightCrouchState);

            Knight.BodyVelocity.x = 0;
            Knight.SetCollisionShape(KnightCollisionShape.Crouch);

            if (shouldMakeTransition)
                Machine.CrouchTransitionState.Enter(this);
            else
                Knight.SwitchAnimation(KnightCharacter.CrouchIdleAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.SlideState.TryEnter() ||
                Machine.TryEnterAttackState())
                return;

            if (Knight.InputReader.HorizontalMove != 0)
                Machine.EnterState(Machine.CrouchWalkState);
            else if (!Knight.InputReader.Crouch.Pressed && Knight.CanStand)
                Machine.CrouchTransitionState.Enter(Machine.IdleState);
        }
    }

    public class KnightCrouchWalkState : KnightStateBase, IKnightCrouchState
    {
        public override bool CanEnter() => Knight.IsGrounded && Knight.InputReader.HorizontalMove != 0 && (Knight.InputReader.Crouch.Pressed || !Knight.CanStand);

        public KnightCrouchWalkState(KnightStateMachine machine) : base(machine)
        {
        }

        public override void OnEnter(KnightStateBase previousState)
        {
            bool shouldMakeTransition = !(previousState is IKnightCrouchState);

            Knight.BodyVelocity.x = 0;
            Knight.SetCollisionShape(KnightCollisionShape.Crouch);

            if (shouldMakeTransition)
                Machine.CrouchTransitionState.Enter(this);
            else
                Knight.SwitchAnimation(KnightCharacter.CrouchWalkAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.SlideState.TryEnter() ||
                Machine.TryEnterAttackState())
                return;

            if (Knight.InputReader.HorizontalMove == 0)
                Machine.EnterState(Machine.CrouchIdleState);
            else if (!Knight.InputReader.Crouch.Pressed && Knight.CanStand)
                Machine.CrouchTransitionState.Enter(Machine.IdleState);
        }

        public override void OnPhysicsProcess(float delta)
        {
            Knight.ApplyHorizontalMovement(Knight.Data.CrouchWalkAcceleration * Knight.InputReader.HorizontalMove, Knight.Data.CrouchWalkSpeed);
            Knight.FlipByVelocity();
        }
    }

    public class KnightRollState : KnightStateBase, IKnightInvincibleState
    {
        private float _animDuration;
        private float _elapsedTime;
        private ulong _lastExitTime = ulong.MinValue;
        public bool IsInCooldown => OS.GetTicksMsec() - _lastExitTime < Knight.Data.RollCooldownInMS;

        public KnightRollState(KnightStateMachine machine) : base(machine)
        {
            _animDuration = machine.Knight.PlayerAnimator.GetAnimation(KnightCharacter.RollAnimationKey).Length;
        }

        public override bool CanEnter() => Knight.IsGrounded && !IsInCooldown && Knight.CanStand && Knight.InputReader.Dash.Pressed && !Knight.InputReader.Crouch.Pressed;

        public override void OnEnter(KnightStateBase previousState)
        {
            _elapsedTime = 0;

            Knight.SetCollisionShape(KnightCollisionShape.Stand);
            Knight.SwitchAnimation(KnightCharacter.RollAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= _animDuration)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            float curveMultiplier = Knight.Data.RollSpeedCurve.Interpolate(_elapsedTime / _animDuration);
            Knight.BodyVelocity.x = Knight.Data.RollSpeed * curveMultiplier * Knight.FacingDirection;
        }

        public override void OnExit()
        {
            _lastExitTime = OS.GetTicksMsec();
        }
    }

    public class KnightSlideState : KnightStateBase, IKnightCrouchState
    {
        private bool _inQuittingAnim;
        private float _elapsedTime;
        private ulong _lastExitTime = ulong.MinValue;
        public bool IsInCooldown => OS.GetTicksMsec() - _lastExitTime < Knight.Data.SlideCooldownInMS;

        public override bool CanEnter() => Knight.IsGrounded && !IsInCooldown && (!Knight.CanStand || Knight.InputReader.Crouch.Pressed) && Knight.InputReader.Dash.Pressed;

        public KnightSlideState(KnightStateMachine machine) : base(machine)
        {
        }

        public override void OnEnter(KnightStateBase previousState)
        {
            _inQuittingAnim = false;
            _elapsedTime = 0;
            Knight.SetCollisionShape(KnightCollisionShape.Crouch);
            Knight.SwitchAnimation(KnightCharacter.SlideAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= Knight.Data.SlideDuration)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            if (!_inQuittingAnim && _elapsedTime > Knight.Data.SlideDuration - Knight.Data.SlideTransitionTime)
            {
                Knight.SwitchAnimation(KnightCharacter.SlideTransitionAnimationKey);
                _inQuittingAnim = true;
            }

            float progress = _elapsedTime / Knight.Data.SlideDuration;
            float curveMultiplier = Knight.Data.SlideSpeedCurve.Interpolate(progress);
            Knight.BodyVelocity.x = Knight.Data.SlideSpeed * curveMultiplier * Knight.FacingDirection;
        }

        public override void OnExit()
        {
            _lastExitTime = OS.GetTicksMsec();
        }
    }

    public class KnightWallslideState : KnightStateBase
    {
        public KnightWallslideState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => !Knight.IsGrounded && Knight.IsTouchingWall && Knight.InputReader.HorizontalMove == Knight.FacingDirection;

        public override void OnEnter(KnightStateBase previousState)
        {
            Knight.GravityScale = 0f;

            Knight.SetCollisionShape(KnightCollisionShape.Stand);
            Knight.SwitchAnimation(KnightCharacter.WallslideAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Knight.InputReader.Jump.Performed)
                Machine.EnterState(Machine.WallJumpState);
            else if (Knight.IsGrounded || !Knight.IsTouchingWall || Knight.InputReader.HorizontalMove != Knight.FacingDirection)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            Knight.BodyVelocity = new Vector2(0, Knight.Data.WallslideSpeed);
        }

        public override void OnExit()
        {
            Knight.GravityScale = 1f;
        }
    }

    public class KnightWalljumpState : KnightStateBase
    {
        private float _elapsedTime;

        public KnightWalljumpState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Machine.CurrentState == Machine.WallslideState && Knight.InputReader.Jump.Pressed;

        public override void OnEnter(KnightStateBase previousState)
        {
            _elapsedTime = 0;
            Knight.GravityScale = 0f;

            Knight.SetCollisionShape(KnightCollisionShape.Stand);
            Knight.SwitchAnimation(KnightCharacter.JumpAnimationKey);
            Knight.FlipFacingDirection();
            Knight.BodyVelocity = new Vector2(Knight.Data.WalljumpSpeed.x * Knight.FacingDirection, Knight.Data.WalljumpSpeed.y);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime > Knight.Data.WalljumpDuration || Knight.IsGrounded || Knight.IsOnCeiling())
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            Knight.ApplyHorizontalFriction(Knight.Data.AirHorizontalAcceleration);
        }

        public override void OnExit()
        {
            Knight.GravityScale = 1f;
        }
    }

    public class KnightAttackState : KnightStateBase
    {
        protected enum ExitAttackCommand { None, Roll, Slide }

        public static int LastStandAttack { get; set; }
        public static ulong LastAttackTime { get; set; }

        private bool _attackIsFinished;

        public KnightAttackState NextAttackState { get; set; }

        protected string AnimationKey { get; set; }
        protected KnightCollisionShape CollisionShape { get; set; }

        protected bool Triggered { get; set; }
        protected ExitAttackCommand ExitCommand { get; set; }

        public KnightAttackState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter()
        {
            switch (CollisionShape)
            {
                case KnightCollisionShape.Stand:
                    return Knight.CanStand && Knight.IsGrounded && Knight.InputReader.Attack.Pressed;
                case KnightCollisionShape.Crouch:
                    return Knight.IsGrounded && (!Knight.CanStand || Knight.InputReader.Crouch.Pressed) && Knight.InputReader.Attack.Pressed;
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }

        public override void OnEnter(KnightStateBase previousState)
        {
            _attackIsFinished = false;
            Triggered = false;
            ExitCommand = ExitAttackCommand.None;

            Knight.BodyVelocity = Vector2.Zero;
            Knight.SetCollisionShape(CollisionShape);
            Knight.SwitchAnimationAndWaitFinish(AnimationKey, OnAnimationFinished);
        }

        public override void DoTransitionLogic()
        {
            if (!_attackIsFinished)
                return;

            switch (ExitCommand)
            {
                case ExitAttackCommand.None:
                    if (!Knight.InputReader.Attack.Pressed)
                        break;

                    bool nextAttackIsCrouch = NextAttackState is IKnightCrouchState;
                    bool crouchPressed = Knight.InputReader.Crouch.Pressed;

                    if (nextAttackIsCrouch && (!crouchPressed && Knight.CanStand))
                        Machine.EnterState(Machine.FirstAttackState);
                    else if (!nextAttackIsCrouch && crouchPressed)
                        Machine.EnterState(Machine.CrouchAttackState);
                    else
                        Machine.EnterState(NextAttackState);
                    return;
                case ExitAttackCommand.Roll:
                    if (Machine.RollState.TryEnter())
                        return;
                    break;
                case ExitAttackCommand.Slide:
                    if (Machine.SlideState.TryEnter())
                        return;
                    break;
            }

            Machine.EnterDefaultState();
        }

        public override void OnProcess(float delta)
        {
            if (Knight.InputReader.Dash.Pressed)
            {
                if (Knight.InputReader.Crouch.Pressed || !Knight.CanStand)
                    ExitCommand = ExitAttackCommand.Slide;
                else
                    ExitCommand = ExitAttackCommand.Roll;
            }
        }

        public override void OnExit()
        {
            if (Knight.InputReader.HorizontalMove != 0)
                Knight.FlipFacingDirectionTo((int)(Mathf.Sign(Knight.InputReader.HorizontalMove)));
        }

        public void PerformAttack(int damage, float moveOffset, Vector2 angle, float strength)
        {
            Knight.AttackHitbox.Scale = new Vector2(Mathf.Abs(Knight.AttackHitbox.Scale.x) * Knight.FacingDirection, Knight.AttackHitbox.Scale.y);
            Knight.AttackHitbox.GetHit = () => new CharacterHit(damage, angle, strength, Knight.FacingDirection);
            Knight.MoveAndCollide(new Vector2(moveOffset * Knight.FacingDirection, 0), false);
        }

        public void SetupData(string animationKey, KnightCollisionShape collisionShape)
        {
            AnimationKey = animationKey;
            CollisionShape = collisionShape;
        }

        private void OnAnimationFinished() => _attackIsFinished = true;

        public static int StepAttack(ulong attackComboMaxDelayInMs)
        {
            LastStandAttack++;

            if (LastStandAttack > 2 || OS.GetTicksMsec() - LastAttackTime >= attackComboMaxDelayInMs)
                LastStandAttack = 1;

            LastAttackTime = OS.GetTicksMsec();

            return LastStandAttack;
        }
    }

    public class KnightCrouchAttackState : KnightAttackState, IKnightCrouchState
    {
        public KnightCrouchAttackState(KnightStateMachine machine) : base(machine)
        {
            SetupData(KnightCharacter.CrouchAttackAnimationKey, KnightCollisionShape.Crouch);
            NextAttackState = this;
        }
    }

    public class KnightHurtState : KnightStateBase
    {
        public Entities.EntityHitbox CurrentHitbox { get; set; }
        public Entities.EntityHit CurrentHit { get; set; }

        public KnightHurtState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;

        public override void OnEnter(KnightStateBase previousState)
        {
            Knight.SwitchAnimationAndWaitFinish(KnightCharacter.HurtAnimationKey, Machine.EnterDefaultState);
            var knockback = CurrentHit.angle * CurrentHit.strength;
            knockback.x *= (Knight.Position.x - CurrentHitbox.Position.x) >= 0f ? 1f : -1f;
            Knight.BodyVelocity = knockback;
        }

        public override void OnPhysicsProcess(float delta)
        {
            float frictionAmount = Knight.Data.HurtAirFriction;

            if (Knight.BodyVelocity.Length() > frictionAmount)
                Knight.BodyVelocity -= frictionAmount * Knight.BodyVelocity.Normalized();
            else
                Knight.BodyVelocity = Vector2.Zero;
        }

        public void Enter(Entities.EntityHitbox hitbox)
        {
            CurrentHitbox = hitbox;
            CurrentHit = hitbox.CurrentHit;
            Machine.EnterState(this);
        }
    }

    public class KnightDieState : KnightStateBase
    {
        public KnightDieState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;

        public override void OnEnter(KnightStateBase previousState)
        {
            Knight.SwitchAnimation(KnightCharacter.DieAnimationKey);
        }
    }

    public class KnightAutoMoveState : KnightStateBase
    {
        public int WalkDirection { get; set; }
        public float CurrentWalkDuration { get; set; }

        public KnightAutoMoveState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;
    }

    public class KnightValidationState : KnightStateBase
    {
        public KnightValidationState(KnightStateMachine machine) : base(machine)
        {

        }
        public override bool CanEnter() => true;
    }
}
