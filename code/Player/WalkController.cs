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

public partial class WalkController : Sandbox.WalkController
{
	public WalkController() : base()
	{
		Duck = new Duck( this );
		DefaultSpeed = 237f;
		WalkSpeed = 110f;
		GroundFriction = 7.0f;
	}

	public float FallVelocity { get; private set; } = 0;
	public float FallPunchThreshold = 350f;
	public float DamageForFallSpeed => 100.0f / (PlayerFatalFallSpeed - PlayerMaxSafeFallSpeed);
	public float PlayerMaxSafeFallSpeed => MathF.Sqrt( 2 * Gravity * 20 * 12 );
	public float PlayerFatalFallSpeed => MathF.Sqrt( 2 * Gravity * 60 * 12 );

	public override void Simulate()
	{
		FallVelocity = -Pawn.Velocity.z;

		base.Simulate();

		CheckFalling();
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

	private void CheckFalling()
	{
		if ( GroundEntity is null || FallVelocity <= 0 )
			return;

		float fallVelocity = FallVelocity;

		if ( Pawn.IsAlive() && fallVelocity >= FallPunchThreshold && Pawn.WaterLevel == 0f )
		{
			float punchStrength = fallVelocity.LerpInverse( FallPunchThreshold, FallPunchThreshold * 3 );
			_ = new Sandbox.ScreenShake.Perlin( 1, 1, punchStrength, 1 );

			if ( GroundEntity.Velocity.z < 0.0f )
			{
				FallVelocity += GroundEntity.Velocity.z;
				FallVelocity = MathF.Max( 0.1f, FallVelocity );
			}

			if ( FallVelocity > PlayerMaxSafeFallSpeed )
			{
				Pawn.TakeDamage( new DamageInfo
				{
					Flags = DamageFlags.Fall,
					Damage = (FallVelocity - PlayerMaxSafeFallSpeed) * DamageForFallSpeed,
					Attacker = Pawn,
				} );
			}
		}
	}
}
