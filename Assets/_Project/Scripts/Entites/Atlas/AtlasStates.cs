public enum StateType
{
    Unset,

#region Broom    
    su_Broom,
    CompleteLoop,
    PD_Falling,
    PD_Rising,
    PD_HoldBack,
    PU_Falling,
    PU_Rising,
    PU_Full,
    RollOver,
    Straight,
    HoldBack,
    TurnAround,
    UpsideDown,
    UD_Falling,
    UD_PUFalling,
    UD_PURising,
    UD_Rising,
    BroomStart,
#endregion

#region Attacks
    UpAir,
    DownAir,
    FallingNair,
    RisingNair,
    UpTilt,
    DownTilt,
    Jab1,
    Jab2,
    Jab3,
#endregion

#region Hurt
    Hurt,
    Bonk,
#endregion
    
#region Jump
    Jump,
    SpinJump,
    
    #region WallJump
    WallJump,
    WallSlide,
    #endregion
#endregion

#region Movement    
    Fall,
    Walk,
    Crouch,
    Slide,
    Slip,
#endregion

    Dash,
    
    Wait,
}