using Godot;
using GrayGame.Entities;

namespace GrayGame.Characters
{
    public class CharacterHitbox : Area2D
    {
        [Signal] public delegate void HitEntity(EntityHurtbox hurtbox);

        public CharacterBase Character { get; set; }

        public CharacterHit CurrentHit { get; set; }
    }
}