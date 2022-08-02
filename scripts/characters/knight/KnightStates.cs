using Godot;

namespace GrayGame.Characters.Knight
{
    public abstract class KnightStateBase
    {
        public KnightStateMachine Machine { get; }
        public KnightCharacter Character => Machine.Character;

        public KnightStateBase(KnightStateMachine machine)
        {
            this.Machine = machine;
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
            Character.SetCollisionShape(KnightCollisionShape.Stand);
            Character.SwitchAnimation(KnightCharacter.IdleAnimationKey);
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

            if (Character.InputReader.HorizontalMove != 0)
                Machine.EnterState(Machine.RunState);
        }

        public override void OnPhysicsProcess(float delta)
        {
            Character.ApplyHorizontalFriction(Character.Data.HorizontalMoveAcceleration);
        }
    }

    public class KnightRunState : KnightStateBase
    {
        public KnightRunState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Character.InputReader.HorizontalMove != 0 && Character.IsGrounded;

        public override void OnEnter(KnightStateBase previousState)
        {
            Character.SetCollisionShape(KnightCollisionShape.Stand);
            Character.SwitchAnimation(KnightCharacter.RunAnimationKey);
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

            if (Character.InputReader.HorizontalMove == 0)
                Machine.EnterState(Machine.IdleState);
        }

        public override void OnPhysicsProcess(float delta)
        {
            Character.ApplyHorizontalMovement(Character.Data.HorizontalMoveAcceleration * Character.InputReader.HorizontalMove, Character.Data.HorizontalMoveSpeed);
            Character.FlipByVelocity();
        }
    }

    public class KnightJumpState : KnightStateBase
    {
        private float _elapsedTime;

        public KnightJumpState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Character.IsGrounded && Character.CanStand && Character.InputReader.Jump.Pressed;

        public override void OnEnter(KnightStateBase previousState)
        {
            _elapsedTime = 0;

            Character.SetCollisionShape(KnightCollisionShape.Stand);
            Character.SwitchAnimation(KnightCharacter.JumpAnimationKey);
            Character.GravityScale = 0.0f;
            // TODO: Play jump particle
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime > Character.Data.JumpDuration || Character.IsOnCeiling())
            {
                Machine.EnterDefaultState();
            }
            else if (!Character.InputReader.Jump.Pressed)
            {
                Character.BodyVelocity.y = -Character.Data.JumpStopVerticalSpeed;
                Machine.EnterDefaultState();
            }
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            float progress = _elapsedTime / Character.Data.JumpDuration;
            float curveMultiplier = Mathf.Ease(progress, Character.Data.JumpSpeedCurve);
            float verticalSpeed = Character.Data.JumpVerticalSpeed * curveMultiplier;

            Character.ApplyHorizontalAirMovement(Character.Data.AirHorizontalAcceleration * Character.InputReader.HorizontalMove, Character.Data.AirHorizontalSpeed, Character.Data.AirHorizontalAcceleration);
            Character.BodyVelocity.y = -verticalSpeed;

            Character.FlipByVelocity();
        }

        public override void OnExit()
        {
            Character.GravityScale = 1.0f;
        }
    }

    public class KnightFallState : KnightStateBase
    {
        private float _fallStartYPosition;

        public KnightFallState(KnightStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => !Character.IsGrounded;

        public override void OnEnter(KnightStateBase previousState)
        {
            _fallStartYPosition = Character.Position.y;
            Character.SwitchAnimation(KnightCharacter.FallAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Character.IsGrounded)
                Machine.EnterDefaultState();
            else
                Machine.WallslideState.TryEnter();
        }

        public override void OnPhysicsProcess(float delta)
        {
            Character.ApplyHorizontalAirMovement(Character.Data.AirHorizontalAcceleration * Character.InputReader.HorizontalMove, Character.Data.AirHorizontalSpeed, Character.Data.AirHorizontalAcceleration);
            Character.FlipByVelocity();
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
            Character.SwitchAnimationAndWaitFinish(KnightCharacter.CrouchTransitionAnimationKey, OnTransitionFinished);
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

            if (!Character.InputReader.Crouch.Pressed && Character.CanStand)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            Character.ApplyHorizontalMovement(Character.Data.CrouchWalkAcceleration * Character.InputReader.HorizontalMove, Character.Data.CrouchWalkSpeed);
            Character.FlipByVelocity();
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

        public override bool CanEnter() => Character.IsGrounded && Character.InputReader.HorizontalMove == 0 && (Character.InputReader.Crouch.Pressed || !Character.CanStand);

        public override void OnEnter(KnightStateBase previousState)
        {
            bool shouldMakeTransition = !(previousState is IKnightCrouchState);

            Character.BodyVelocity.x = 0;
            Character.SetCollisionShape(KnightCollisionShape.Crouch);

            if (shouldMakeTransition)
                Machine.CrouchTransitionState.Enter(this);
            else
                Character.SwitchAnimation(KnightCharacter.CrouchIdleAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.SlideState.TryEnter() ||
                Machine.TryEnterAttackState())
                return;

            if (Character.InputReader.HorizontalMove != 0)
                Machine.EnterState(Machine.CrouchWalkState);
            else if (!Character.InputReader.Crouch.Pressed && Character.CanStand)
                Machine.CrouchTransitionState.Enter(Machine.IdleState);
        }
    }

    public class KnightCrouchWalkState : KnightStateBase, IKnightCrouchState
    {
        public override bool CanEnter() => Character.IsGrounded && Character.InputReader.HorizontalMove != 0 && (Character.InputReader.Crouch.Pressed || !Character.CanStand);

        public KnightCrouchWalkState(KnightStateMachine machine) : base(machine)
        {
        }

        public override void OnEnter(KnightStateBase previousState)
        {
            bool shouldMakeTransition = !(previousState is IKnightCrouchState);

            Character.BodyVelocity.x = 0;
            Character.SetCollisionShape(KnightCollisionShape.Crouch);

            if (shouldMakeTransition)
                Machine.CrouchTransitionState.Enter(this);
            else
                Character.SwitchAnimation(KnightCharacter.CrouchWalkAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.SlideState.TryEnter() ||
                Machine.TryEnterAttackState())
                return;

            if (Character.InputReader.HorizontalMove == 0)
                Machine.EnterState(Machine.CrouchIdleState);
            else if (!Character.InputReader.Crouch.Pressed && Character.CanStand)
                Machine.CrouchTransitionState.Enter(Machine.IdleState);
        }

        public override void OnPhysicsProcess(float delta)
        {
            Character.ApplyHorizontalMovement(Character.Data.CrouchWalkAcceleration * Character.InputReader.HorizontalMove, Character.Data.CrouchWalkSpeed);
            Character.FlipByVelocity();
        }
    }

    public class KnightRollState : KnightStateBase, IKnightInvincibleState
    {
        private float _animDuration;
        private float _elapsedTime;
        private ulong _lastExitTime = ulong.MinValue;
        public bool IsInCooldown => OS.GetTicksMsec() - _lastExitTime < Character.Data.RollCooldownInMS;

        public override bool CanEnter() => Character.IsGrounded && !IsInCooldown && Character.CanStand && Character.InputReader.Dash.Pressed && !Character.InputReader.Crouch.Pressed;

        public KnightRollState(KnightStateMachine machine) : base(machine)
        {
            _animDuration = machine.Character.PlayerAnimator.GetAnimation(KnightCharacter.RollAnimationKey).Length;
        }

        public override void OnEnter(KnightStateBase previousState)
        {
            _elapsedTime = 0;

            Character.SetCollisionShape(KnightCollisionShape.Stand);
            Character.SwitchAnimation(KnightCharacter.RollAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= _animDuration)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            float curveMultiplier = Character.Data.RollSpeedCurve.Interpolate(_elapsedTime / _animDuration);
            Character.BodyVelocity.x = Character.Data.RollSpeed * curveMultiplier * Character.FacingDirection;
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
        public bool IsInCooldown => OS.GetTicksMsec() - _lastExitTime < Character.Data.SlideCooldownInMS;

        public override bool CanEnter() => Character.IsGrounded && !IsInCooldown && (!Character.CanStand || Character.InputReader.Crouch.Pressed) && Character.InputReader.Dash.Pressed;

        public KnightSlideState(KnightStateMachine machine) : base(machine)
        {
        }

        public override void OnEnter(KnightStateBase previousState)
        {
            _inQuittingAnim = false;
            _elapsedTime = 0;
            Character.SetCollisionShape(KnightCollisionShape.Crouch);
            Character.SwitchAnimation(KnightCharacter.SlideAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= Character.Data.SlideDuration)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            if (!_inQuittingAnim && _elapsedTime > Character.Data.SlideDuration - Character.Data.SlideTransitionTime)
            {
                Character.SwitchAnimation(KnightCharacter.SlideTransitionAnimationKey);
                _inQuittingAnim = true;
            }

            float progress = _elapsedTime / Character.Data.SlideDuration;
            float curveMultiplier = Character.Data.SlideSpeedCurve.Interpolate(progress);
            Character.BodyVelocity.x = Character.Data.SlideSpeed * curveMultiplier * Character.FacingDirection;
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

        public override bool CanEnter() => !Character.IsGrounded && Character.IsTouchingWall && Character.InputReader.HorizontalMove == Character.FacingDirection;

        public override void OnEnter(KnightStateBase previousState)
        {
            Character.GravityScale = 0f;

            Character.SetCollisionShape(KnightCollisionShape.Stand);
            Character.SwitchAnimation(KnightCharacter.WallslideAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Character.InputReader.Jump.Performed)
                Machine.EnterState(Machine.WallJumpState);
            else if (Character.IsGrounded || !Character.IsTouchingWall || Character.InputReader.HorizontalMove != Character.FacingDirection)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            Character.BodyVelocity = new Vector2(0, Character.Data.WallslideSpeed);
        }

        public override void OnExit()
        {
            Character.GravityScale = 1f;
        }
    }

    public class KnightWalljumpState : KnightStateBase
    {
        private float _elapsedTime;

        public override bool CanEnter() => Machine.CurrentState == Machine.WallslideState && Character.InputReader.Jump.Pressed;

        public KnightWalljumpState(KnightStateMachine machine) : base(machine)
        {
        }

        public override void OnEnter(KnightStateBase previousState)
        {
            _elapsedTime = 0;
            Character.GravityScale = 0f;

            Character.SetCollisionShape(KnightCollisionShape.Stand);
            Character.SwitchAnimation(KnightCharacter.JumpAnimationKey);
            Character.FlipFacingDirection();
            Character.BodyVelocity = new Vector2(Character.Data.WalljumpSpeed.x * Character.FacingDirection, Character.Data.WalljumpSpeed.y);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime > Character.Data.WalljumpDuration || Character.IsGrounded || Character.IsOnCeiling())
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            Character.ApplyHorizontalFriction(Character.Data.AirHorizontalAcceleration);
        }

        public override void OnExit()
        {
            Character.GravityScale = 1f;
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
                    return Character.CanStand && Character.IsGrounded && Character.InputReader.Attack.Pressed;
                case KnightCollisionShape.Crouch:
                    return Character.IsGrounded && (!Character.CanStand || Character.InputReader.Crouch.Pressed) && Character.InputReader.Attack.Pressed;
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }

        public override void OnEnter(KnightStateBase previousState)
        {
            _attackIsFinished = false;
            Triggered = false;
            ExitCommand = ExitAttackCommand.None;

            Character.BodyVelocity = Vector2.Zero;
            Character.SetCollisionShape(CollisionShape);
            Character.SwitchAnimationAndWaitFinish(AnimationKey, OnAnimationFinished);
        }

        public override void DoTransitionLogic()
        {
            if (!_attackIsFinished)
                return;

            switch (ExitCommand)
            {
                case ExitAttackCommand.None:
                    if (!Character.InputReader.Attack.Pressed)
                        break;

                    bool nextAttackIsCrouch = NextAttackState is IKnightCrouchState;
                    bool crouchPressed = Character.InputReader.Crouch.Pressed;

                    if (nextAttackIsCrouch && (!crouchPressed && Character.CanStand))
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
            if (Character.InputReader.Dash.Pressed)
            {
                if (Character.InputReader.Crouch.Pressed || !Character.CanStand)
                    ExitCommand = ExitAttackCommand.Slide;
                else
                    ExitCommand = ExitAttackCommand.Roll;
            }
        }

        public override void OnExit()
        {
            if (Character.InputReader.HorizontalMove != 0)
                Character.FlipFacingDirectionTo((int)(Mathf.Sign(Character.InputReader.HorizontalMove)));
        }

        public void PerformAttack(int damage, float moveOffset, Vector2 angle, float strength)
        {
            Character.AttackHitbox.Scale = new Vector2(Mathf.Abs(Character.AttackHitbox.Scale.x) * Character.FacingDirection, Character.AttackHitbox.Scale.y);
            Character.AttackHitbox.CurrentHit = new CharacterHit(damage, angle, strength, Character.FacingDirection);
            Character.MoveAndCollide(new Vector2(moveOffset * Character.FacingDirection, 0), false);
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
            Character.SwitchAnimationAndWaitFinish(KnightCharacter.HurtAnimationKey, Machine.EnterDefaultState);
            var knockback = CurrentHit.angle * CurrentHit.strength;
            knockback.x *= (Character.Position.x - CurrentHitbox.Position.x) >= 0f ? 1f : -1f;
            Character.BodyVelocity = knockback;
        }

        public override void OnPhysicsProcess(float delta)
        {
            float frictionAmount = Character.Data.HurtAirFriction;

            if (Character.BodyVelocity.Length() > frictionAmount)
                Character.BodyVelocity -= frictionAmount * Character.BodyVelocity.Normalized();
            else
                Character.BodyVelocity = Vector2.Zero;
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
            Character.SwitchAnimation(KnightCharacter.DieAnimationKey);
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
