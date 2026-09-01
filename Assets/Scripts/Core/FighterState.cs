namespace FightingGame.Core
{
    public enum FighterState
    {
        Idle,
        WalkForward,
        WalkBackward,
        Crouch,
        JumpNeutral,
        JumpForward,
        JumpBackward,
        DashForward,
        DashBackward,
        Attack,
        Hitstun,
        Block,
        Dead
    }

    public enum AttackType
    {
        None,
        LeftPunch,
        RightPunch,
        LeftKick,
        RightKick
    }
}
