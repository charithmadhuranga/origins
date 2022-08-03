namespace GrayGame.Characters.Archer
{
    public class ArcherStateMachine
    {
        public ArcherCharacter Archer { get; }

        public ArcherStateBase CurrentState { get; private set; }

        public ArcherIdleState IdleState { get; private set; }
        public ArcherRunState RunState { get; private set; }
        public ArcherJumpState JumpState { get; private set; }
        public ArcherDoubleJumpState DoubleJumpState { get; private set; }
        public ArcherFallState FallState { get; private set; }
        public ArcherRollState RollState { get; private set; }
        public ArcherShootState ShootState { get; private set; }
        public ArcherWallslideState WallslideState { get; private set; }
        public ArcherWalljumpState WalljumpState { get; private set; }
        public ArcherHurtState HurtState { get; private set; }
        public ArcherDieState DieState { get; private set; }

        public ArcherStateMachine(ArcherCharacter archer)
        {
            Archer = archer;

            IdleState = new ArcherIdleState(this);
            RunState = new ArcherRunState(this);
            JumpState = new ArcherJumpState(this);
            DoubleJumpState = new ArcherDoubleJumpState(this);
            FallState = new ArcherFallState(this);
            RollState = new ArcherRollState(this);
            ShootState = new ArcherShootState(this);
            WallslideState = new ArcherWallslideState(this);
            WalljumpState = new ArcherWalljumpState(this);
            HurtState = new ArcherHurtState(this);
            DieState = new ArcherDieState(this);

            EnterState(new ArcherValidationState(this));
            EnterState(IdleState);
        }

        public void EnterState(ArcherStateBase state)
        {
            var previousState = CurrentState;
            CurrentState = state;
            previousState?.OnExit();
            CurrentState.OnEnter(previousState);
        }

        public void EnterDefaultState()
        {
            if (FallState.TryEnter() ||
                WallslideState.TryEnter())
                return;

            EnterState(Archer.InputReader.HorizontalMove == 0 ? (ArcherStateBase)IdleState : (ArcherStateBase)RunState);
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
    }
}