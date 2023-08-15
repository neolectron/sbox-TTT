using Editor;
using Sandbox;

namespace TTT;

[Category( "Grenades" )]
[ClassName( "ttt_grenade_incendiary" )]
[EditorModel( "models/weapons/w_incendiary.vmdl" )]
[HammerEntity]
[Title( "Incendiary Grenade" )]
public class IncendiaryGrenade : Grenade
{
	private const string ExplodeSound = "incendiary_explode-1";
	private const string Particle = "particles/incendiarygrenade/explode.vpcf";

	protected override void OnExplode()
	{
		base.OnExplode();

		Particles.Create( Particle, Position );
		Sound.FromWorld( ExplodeSound, Position );
	}

	static IncendiaryGrenade()
	{
		Precache.Add( Particle );
	}
}
