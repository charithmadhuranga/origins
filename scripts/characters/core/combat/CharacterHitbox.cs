using Godot;
using OriginsGame.Entities;

namespace OriginsGame.Characters
{
    public class CharacterHitbox : Area2D
    {
        [Signal] public delegate void HitEntity(EntityHurtbox hurtbox);

        public CharacterBase Character { get; set; }

        public System.Func<CharacterHit> GetHit { get; set; }
    }
}