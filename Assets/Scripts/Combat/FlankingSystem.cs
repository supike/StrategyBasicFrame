using UnityEngine;

namespace Combat
{
    public class FlankingSystem : MonoBehaviour
    {
        public enum AttackDirection
        {
            Front,
            Side,
            Back
        }
    
        public static AttackDirection GetAttackDirection(Unit attacker, Unit defender)
        {
            // 방어자가 바라보는 방향
            Vector2Int defenderFacing = defender.GetFacingDirection();
        
            // 공격자의 상대적 위치
            Vector2Int attackVector = new Vector2Int(
                attacker.CurrentTile.X - defender.CurrentTile.X,
                attacker.CurrentTile.Y - defender.CurrentTile.Y
            );
        
            // 내적으로 방향 판정
            float dot = Vector2.Dot(defenderFacing.normalized, attackVector.normalized);
        
            if (dot > 0.5f)
                return AttackDirection.Front;
            else if (dot < -0.5f)
                return AttackDirection.Back;
            else
                return AttackDirection.Side;
        }
    
        public static float GetFlankingModifier(AttackDirection direction)
        {
            switch (direction)
            {
                case AttackDirection.Front:
                    return 1f;      // 정면: 보너스 없음
                case AttackDirection.Side:
                    return 1.25f;   // 측면: +25% 데미지
                case AttackDirection.Back:
                    return 1.5f;    // 배후: +50% 데미지
                default:
                    return 1f;
            }
        }
    }
}