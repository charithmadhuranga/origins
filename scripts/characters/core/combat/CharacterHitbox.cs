using Godot;

namespace GrayGame.Characters
{
    public class CharacterHitbox : Area2D
    {
        public CharacterBase Character { get; set; }

        public CharacterHit CurrentHit { get; set; }
    }
}