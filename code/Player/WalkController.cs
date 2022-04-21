using Sandbox;
using System;

namespace TTT;

public class Duck : Sandbox.Duck
{
	public Duck( BasePlayerController controller ) : base( controller ) { }

	public override float GetWishSpeed()
	{
		return IsActive ? 90.0f : -1;
	}
}

public class WalkController : Sandbox.WalkController
{
	public WalkController() : base()
	{
		Duck = new Duck( this );
		DefaultSpeed = 237.0f;
		WalkSpeed = 110.0f;
		StopSpeed = 150.0f;
	}

	private const float _fallDamageThreshold = 700f;
	private const float _fallDamageScale = 0.28f;

	public override void Simulate()
	{
		base.Simulate();

		var fallVelocity = -Pawn.Velocity.z;
		if ( GroundEntity is null || fallVelocity <= 0 )
			return;

		if ( fallVelocity > _fallDamageThreshold )
		{
			_ = new Sandbox.ScreenShake.Perlin( 1f, 0.2f, 2f );

			var damage = (MathF.Abs( fallVelocity ) - _fallDamageThreshold) * _fallDamageScale;
			Pawn.TakeDamage( new DamageInfo
			{
				Attacker = Pawn,
				Flags = DamageFlags.Fall,
				Force = Vector3.Down * Velocity.Length,
				Damage = damage,
			} );

			Pawn.PlaySound( Constants.Sounds.FallDamage ).SetVolume( (damage * 0.05f).Clamp( 0, 0.5f ) );
		}
	}

	public override float GetWishSpeed()
	{
		float ws = Duck.GetWishSpeed();

		if ( ws >= 0 )
			return ws;

		if ( Input.Down( InputButton.Run ) )
			return WalkSpeed;

		return DefaultSpeed;
	}
}
