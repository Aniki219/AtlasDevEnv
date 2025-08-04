using UnityEngine;

public class ToLedgeGrab : StateTransition
{
    [SerializeField] CircleCollider2D hand;
    [SerializeField] CircleCollider2D aboveHand;
    [SerializeField] CircleCollider2D foot;
    [SerializeField] BoxCollider2D stand;
    [SerializeField] BoxCollider2D crouch;

    public override bool CheckCondition()
    {
        if (input.Down(ButtonState.DOWN)) return false;

        bool checkAbove = Physics2D.OverlapCircle(aboveHand.bounds.center, aboveHand.radius, body.collisionMask);
        bool checkHand = Physics2D.OverlapCircle(hand.bounds.center, hand.radius, body.collisionMask);
        bool checkFoot = Physics2D.OverlapCircle(foot.bounds.center, foot.radius, body.collisionMask);

        return checkHand && !checkAbove && checkFoot;
    }
}