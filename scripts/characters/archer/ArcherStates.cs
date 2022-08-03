using Godot;

namespace GrayGame.Characters.Archer
{
    public abstract class ArcherStateBase
    {
        public ArcherStateMachine Machine { get; }
        public ArcherCharacter Archer => Machine.Archer;

        public ArcherStateBase(ArcherStateMachine machine)
        {
            Machine = machine;
        }

        public abstract bool CanEnter();

        public virtual void OnEnter(ArcherStateBase previousState) { }

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

    public class ArcherIdleState : ArcherStateBase
    {
        public ArcherIdleState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;

        public override void OnEnter(ArcherStateBase previousState)
        {
            Archer.SwitchAnimation(ArcherCharacter.IdleAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.RollState.TryEnter() ||
                Machine.ShootState.TryEnter() ||
                Machine.RunState.TryEnter())
                return;
        }

        public override void OnPhysicsProcess(float delta)
        {
            Archer.ApplyHorizontalFriction(Archer.Data.HorizontalMoveAcceleration);
        }
    }

    public class ArcherRunState : ArcherStateBase
    {
        public ArcherRunState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Archer.InputReader.HorizontalMove != 0.0f && Archer.IsGrounded;

        public override void OnEnter(ArcherStateBase previousState)
        {
            Archer.SwitchAnimation(ArcherCharacter.RunAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Machine.FallState.TryEnter() ||
                Machine.JumpState.TryEnter() ||
                Machine.RollState.TryEnter() ||
                Machine.ShootState.TryEnter())
                return;

            if (Archer.InputReader.HorizontalMove == 0.0f)
                Machine.EnterState(Machine.IdleState);
        }

        public override void OnPhysicsProcess(float delta)
        {
            Archer.ApplyHorizontalMovement(Archer.Data.HorizontalMoveAcceleration * Archer.InputReader.HorizontalMove, Archer.Data.HorizontalMoveSpeed);
            Archer.FlipByVelocity();
        }
    }

    public class ArcherJumpState : ArcherStateBase
    {
        private float _elapsedTime;

        public ArcherJumpState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Archer.IsGrounded && Archer.InputReader.Jump.Pressed;

        public override void OnEnter(ArcherStateBase previousState)
        {
            _elapsedTime = 0.0f;

            Archer.SwitchAnimation(ArcherCharacter.JumpAnimationKey);
            Archer.GravityScale = 0.0f;
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= Archer.Data.JumpDuration || Archer.IsOnCeiling())
                Machine.EnterDefaultState();
            else if (Archer.InputReader.Jump.Performed)
                Machine.EnterState(Machine.DoubleJumpState);
            else if (!Archer.InputReader.Jump.Pressed)
            {
                Archer.BodyVelocity.y = -Archer.Data.JumpStopVerticalSpeed;
                Machine.EnterDefaultState();
            }
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            float progress = _elapsedTime / Archer.Data.JumpDuration;
            float curveMultiplier = Mathf.Ease(progress, Archer.Data.JumpSpeedCurve);
            float verticalSpeed = Archer.Data.JumpVerticalSpeed * curveMultiplier;

            Archer.ApplyHorizontalAirMovement(Archer.Data.AirHorizontalAcceleration * Archer.InputReader.HorizontalMove, Archer.Data.AirHorizontalSpeed, Archer.Data.AirHorizontalAcceleration);
            Archer.BodyVelocity.y = -verticalSpeed;

            Archer.FlipByVelocity();
        }

        public override void OnExit()
        {
            Archer.GravityScale = 1.0f;
        }
    }

    public class ArcherDoubleJumpState : ArcherStateBase
    {
        private float _elapsedTime;

        public ArcherDoubleJumpState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Archer.InputReader.Jump.Pressed && Archer.LandedAfterDoubleJump;

        public override void OnEnter(ArcherStateBase previousState)
        {
            _elapsedTime = 0.0f;

            Archer.SwitchAnimation(ArcherCharacter.JumpAnimationKey);
            Archer.GravityScale = 0.0f;

            var gfx = Archer.Data.DoubleJumpGfxScene.Instance<Node2D>();
            gfx.Position = Archer.DoubleJumpGfxPosition.GlobalPosition;
            Archer.GetTree().CurrentScene.AddChild(gfx, true);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= Archer.Data.DoubleJumpDuration || Archer.IsOnCeiling())
                Machine.EnterDefaultState();
            else if (!Archer.InputReader.Jump.Pressed)
            {
                Archer.BodyVelocity.y = -Archer.Data.JumpStopVerticalSpeed;
                Machine.EnterDefaultState();
            }
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            float progress = _elapsedTime / Archer.Data.DoubleJumpDuration;
            float curveMultiplier = Mathf.Ease(progress, Archer.Data.DoubleJumpSpeedCurve);
            float verticalSpeed = Archer.Data.DoubleJumpVerticalSpeed * curveMultiplier;

            Archer.ApplyHorizontalAirMovement(Archer.Data.AirHorizontalAcceleration * Archer.InputReader.HorizontalMove, Archer.Data.AirHorizontalSpeed, Archer.Data.AirHorizontalAcceleration);
            Archer.BodyVelocity.y = -verticalSpeed;

            Archer.FlipByVelocity();
        }

        public override void OnExit()
        {
            Archer.LandedAfterDoubleJump = false;
            Archer.GravityScale = 1.0f;
        }
    }

    public class ArcherFallState : ArcherStateBase
    {
        private float _fallStartYPosition;

        public ArcherFallState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => !Archer.IsGrounded;

        public override void OnEnter(ArcherStateBase previousState)
        {
            _fallStartYPosition = Archer.Position.y;
            Archer.SwitchAnimation(ArcherCharacter.FallAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Archer.LandedAfterDoubleJump && Archer.InputReader.Jump.Performed)
                Machine.EnterState(Machine.DoubleJumpState);
            else if (Archer.IsGrounded)
                Machine.EnterDefaultState();
            else
                Machine.WallslideState.TryEnter();
        }

        public override void OnPhysicsProcess(float delta)
        {
            Archer.ApplyHorizontalAirMovement(Archer.Data.AirHorizontalAcceleration * Archer.InputReader.HorizontalMove, Archer.Data.AirHorizontalSpeed, Archer.Data.AirHorizontalAcceleration);
            Archer.FlipByVelocity();
        }
    }

    public class ArcherRollState : ArcherStateBase
    {
        private float _animDuration;
        private float _elapsedTime;
        private ulong _lastExitTime = ulong.MinValue;
        public bool IsInCooldown => OS.GetTicksMsec() - _lastExitTime < Archer.Data.RollCooldownInMS;

        public ArcherRollState(ArcherStateMachine machine) : base(machine)
        {
            _animDuration = Archer.PlayerAnimator.GetAnimation(ArcherCharacter.RollAnimationKey).Length;
        }

        public override bool CanEnter() => Archer.IsGrounded && Archer.InputReader.Roll.Pressed && !IsInCooldown;

        public override void OnEnter(ArcherStateBase previousState)
        {
            _elapsedTime = 0.0f;

            Archer.SwitchAnimation(ArcherCharacter.RollAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= _animDuration)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            float curveMultiplier = Archer.Data.RollSpeedCurve.Interpolate(_elapsedTime / _animDuration);
            Archer.BodyVelocity.x = Archer.Data.RollSpeed * curveMultiplier * Archer.FacingDirection;
        }

        public override void OnExit()
        {
            _lastExitTime = OS.GetTicksMsec();
        }
    }

    public class ArcherShootState : ArcherStateBase
    {
        private float _animDuration;
        private float _elapsedTime;

        public ArcherShootState(ArcherStateMachine machine) : base(machine)
        {
            _animDuration = Archer.PlayerAnimator.GetAnimation(ArcherCharacter.ShootAnimationKey).Length;
        }

        public override bool CanEnter() => Archer.IsGrounded && !Archer.IsShootObsolete && Archer.InputReader.Shoot.Pressed;

        public override void OnEnter(ArcherStateBase previousState)
        {
            _elapsedTime = 0.0f;
            Archer.BodyVelocity.x = 0.0f;

            Archer.SwitchAnimation(ArcherCharacter.ShootAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= _animDuration)
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;
        }
    }

    public class ArcherWallslideState : ArcherStateBase
    {
        public ArcherWallslideState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => !Archer.IsGrounded && Archer.IsTouchingWall && Archer.InputReader.HorizontalMove == Archer.FacingDirection;

        public override void OnEnter(ArcherStateBase previousState)
        {
            Archer.GravityScale = 0.0f;

            Archer.SwitchAnimation(ArcherCharacter.WallslideAnimationKey);
        }

        public override void DoTransitionLogic()
        {
            if (Archer.InputReader.Jump.Performed)
                Machine.EnterState(Machine.WalljumpState);
            else if (!CanEnter())
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            Archer.BodyVelocity.y = Archer.Data.WallslideSpeed;
        }

        public override void OnExit()
        {
            Archer.GravityScale = 1.0f;
        }
    }

    public class ArcherWalljumpState : ArcherStateBase
    {
        private float _elapsedTime;

        public ArcherWalljumpState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => Machine.CurrentState == Machine.WallslideState && Archer.InputReader.Jump.Pressed;

        public override void OnEnter(ArcherStateBase previousState)
        {
            _elapsedTime = 0.0f;
            Archer.GravityScale = 0.0f;

            Archer.SwitchAnimation(ArcherCharacter.JumpAnimationKey);
            Archer.FlipFacingDirection();
            Archer.BodyVelocity = new Vector2(Archer.Data.WalljumpForce.x * Archer.FacingDirection, Archer.Data.WalljumpForce.y);
        }

        public override void DoTransitionLogic()
        {
            if (_elapsedTime >= Archer.Data.WalljumpDuration || Archer.IsGrounded || Archer.IsOnCeiling())
                Machine.EnterDefaultState();
        }

        public override void OnPhysicsProcess(float delta)
        {
            _elapsedTime += delta;

            Archer.ApplyHorizontalFriction(Archer.Data.AirHorizontalAcceleration);
        }

        public override void OnExit()
        {
            Archer.GravityScale = 1.0f;
        }
    }

    public class ArcherHurtState : ArcherStateBase
    {
        public ArcherHurtState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;
    }

    public class ArcherDieState : ArcherStateBase
    {
        public ArcherDieState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;
    }

    public class ArcherValidationState : ArcherStateBase
    {
        public ArcherValidationState(ArcherStateMachine machine) : base(machine)
        {
        }

        public override bool CanEnter() => true;
    }
}