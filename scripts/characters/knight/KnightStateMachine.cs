namespace GrayGame.Characters.Knight
{
    public class KnightStateMachine
    {
        public KnightCharacter Knight { get; }

        public KnightStateBase CurrentState { get; private set; }

        public KnightIdleState IdleState { get; private set; }
        public KnightRunState RunState { get; private set; }
        public KnightJumpState JumpState { get; private set; }
        public KnightFallState FallState { get; private set; }
        public KnightCrouchTransitionState CrouchTransitionState { get; private set; }
        public KnightCrouchIdleState CrouchIdleState { get; private set; }
        public KnightCrouchWalkState CrouchWalkState { get; private set; }
        public KnightRollState RollState { get; private set; }
        public KnightSlideState SlideState { get; private set; }
        public KnightWallslideState WallslideState { get; private set; }
        public KnightWalljumpState WallJumpState { get; private set; }
        public KnightAttackState FirstAttackState { get; private set; }
        public KnightAttackState SecondAttackState { get; private set; }
        public KnightCrouchAttackState CrouchAttackState { get; private set; }
        public KnightHurtState HurtState { get; private set; }
        public KnightDieState DieState { get; private set; }
        public KnightAutoMoveState AutoMoveState { get; private set; }

        public KnightStateMachine(KnightCharacter Character)
        {
            this.Knight = Character;

            IdleState = new KnightIdleState(this);
            RunState = new KnightRunState(this);
            JumpState = new KnightJumpState(this);
            FallState = new KnightFallState(this);
            CrouchTransitionState = new KnightCrouchTransitionState(this);
            CrouchIdleState = new KnightCrouchIdleState(this);
            CrouchWalkState = new KnightCrouchWalkState(this);
            RollState = new KnightRollState(this);
            SlideState = new KnightSlideState(this);
            WallslideState = new KnightWallslideState(this);
            WallJumpState = new KnightWalljumpState(this);
            FirstAttackState = new KnightAttackState(this);
            SecondAttackState = new KnightAttackState(this);
            CrouchAttackState = new KnightCrouchAttackState(this);
            HurtState = new KnightHurtState(this);
            DieState = new KnightDieState(this);
            AutoMoveState = new KnightAutoMoveState(this);

            FirstAttackState.NextAttackState = SecondAttackState;
            FirstAttackState.SetupData(KnightCharacter.FirstAttackAnimationKey, KnightCollisionShape.Stand);
            SecondAttackState.NextAttackState = FirstAttackState;
            SecondAttackState.SetupData(KnightCharacter.SecondAttackAnimationKey, KnightCollisionShape.Stand);

            EnterState(new KnightValidationState(this));
            EnterState(IdleState);
        }

        public void Process(float delta)
        {
            CurrentState.OnProcess(delta);
        }

        public void PhysicsProcess(float delta)
        {
            CurrentState.DoTransitionLogic();
            CurrentState.OnPhysicsProcess(delta);
        }

        public void EnterState(KnightStateBase state)
        {
            var previousState = CurrentState;
            CurrentState = state;
            previousState?.OnExit();
            CurrentState.OnEnter(previousState);
        }

        public void EnterDefaultState()
        {
            if (FallState.TryEnter() ||
                CrouchIdleState.TryEnter() ||
                CrouchWalkState.TryEnter() ||
                WallslideState.TryEnter())
                return;

            EnterState(Knight.InputReader.HorizontalMove == 0 ? (KnightStateBase)IdleState : (KnightStateBase)RunState);
        }

        public bool TryEnterAttackState()
        {
            if (CrouchAttackState.TryEnter())
                return true;

            int oldAttack = KnightAttackState.LastStandAttack;
            int attack = KnightAttackState.StepAttack(Knight.Data.AttackComboMaxDelayInMs);

            if ((attack == 1 && FirstAttackState.TryEnter()) || (attack == 2 && SecondAttackState.TryEnter()))
                return true;

            KnightAttackState.LastStandAttack = oldAttack;
            return false;
        }
    }
}
