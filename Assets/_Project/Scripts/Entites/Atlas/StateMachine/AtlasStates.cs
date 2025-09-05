using static StateType;
using System.Collections.Generic;
using System;

public enum StateType
{
    Unset,
    su_Any,

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
    su_Attack,
    su_ArialAttack,
    su_GroundedAttack,
    #endregion

    #region Hurt
    Hurt,
    Bonk,
    #endregion

    #region Jump
    su_Jump,
    Jump,
    SpinJump,

    #region WallJump
    WallJump,
    WallSlide,
    #endregion
    #endregion

    #region Movement 
    su_Movement,   
    Fall,
    Walk,
    Crouch,
    Slide,
    Slip,
    #endregion

    Dash,

    Wait,
}