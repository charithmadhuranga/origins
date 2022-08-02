using Godot;

namespace GrayGame.Entities
{
    public struct EntityHit
    {
        public int damage;
        public float strength;
        public Vector2 angle;

        public EntityHit(int damage, float strength, Vector2 angle)
        {
            this.damage = damage;
            this.strength = strength;
            this.angle = angle;
        }
    }
}