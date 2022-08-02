using Godot;

namespace GrayGame.Characters
{
    public struct CharacterHit
    {
        public int damage;
        public Vector2 angle;
        public float strength;
        public int direction;

        public CharacterHit(int damage, Vector2 angle, float strength, int direction)
        {
            this.damage = damage;
            this.angle = angle;
            this.strength = strength;
            this.direction = direction;
        }
    }
}