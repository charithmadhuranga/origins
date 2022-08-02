using Godot;
using GrayGame.Characters;

namespace GrayGame.Entities
{
	public class EntityHurtbox : Area2D
	{
		[Signal] public delegate void TakeHit(CharacterHitbox hitbox);

		public void OnHurtboxAreaEntered(Area2D area)
		{
			if (area is CharacterHitbox hitbox)
				EmitSignal(nameof(TakeHit), hitbox);
		}
	}
}
