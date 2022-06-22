﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TTT;

[GameResource( "Role", "role", "TTT role template.", Icon = "🎭" )]
public class RoleInfo : GameResource
{
	public Team Team { get; set; } = Team.None;

	[Description( "The amount of credits the player spawns with." )]
	public int DefaultCredits { get; set; } = 0;

	public List<string> ShopItems { get; set; } = new();

	public bool CanRetrieveCredits { get; set; } = false;

	public bool CanRoleChat { get; set; } = false;

	public bool CanAttachCorpses { get; set; } = false;

	[Category( "UI" )]
	public Color Color { get; set; }

	[JsonPropertyName( "icon" )]
	[Title( "Icon" ), Category( "UI" ), ResourceType( "png" )]
	public string IconPath { get; set; } = "ui/none.png";

	[HideInEditor]
	public HashSet<string> ShopItemClassNames { get; private set; } = new();

	[HideInEditor]
	[JsonPropertyName( "cached-icon" )]
	public Texture Icon { get; private set; }

	protected override void PostLoad()
	{
		base.PostLoad();

		ShopItemClassNames = new HashSet<string>( ShopItems );

		if ( Host.IsClient )
			Icon = Texture.Load( FileSystem.Mounted, IconPath );
	}
}

public abstract class BaseRole : IEquatable<BaseRole>, IEquatable<string>
{
	public RoleInfo Info { get; private set; }

	public Team Team => Info.Team;
	public Color Color => Info.Color;
	public HashSet<string> ShopItems => Info.ShopItemClassNames;
	public bool CanRetrieveCredits => Info.CanRetrieveCredits;
	public bool CanRoleChat => Info.CanRoleChat;
	public bool CanAttachCorpses => Info.CanAttachCorpses;
	public string Title => Info.Title;

	public BaseRole()
	{
		Info = GameResource.GetInfo<RoleInfo>( GetType() );
	}

	public virtual void OnSelect( Player player )
	{
		if ( player.IsLocalPawn )
		{
			if ( Info.ShopItems.Count > 0 )
				UI.RoleMenu.Instance.AddShopTab();

			Player.RoleButtons = GetRoleButtons();

			foreach ( var roleButton in Player.RoleButtons )
				Player.RoleButtonPoints.Add( new UI.RoleButtonPoint( roleButton ) );

			return;
		}

		if ( !Host.IsServer )
			return;

		player.Client?.SetInt( "team", (int)Team );
		player.Credits = Math.Max( Info.DefaultCredits, player.Credits );
		player.PurchasedLimitedShopItems.Clear();
	}

	public virtual void OnDeselect( Player player )
	{
		if ( !player.IsLocalPawn )
			return;

		player.ClearButtons();

		if ( Info.ShopItems.Count > 0 )
			UI.RoleMenu.Instance.RemoveTab( UI.RoleMenu.ShopTab );
	}

	protected List<RoleButton> GetRoleButtons()
	{
		Host.AssertClient();

		return Entity.All
				.OfType<RoleButton>()
				.Where( x => x.IsValid() && (x.RoleName == "All" || this == x.RoleName) )
				.ToList();
	}

	public static bool operator ==( BaseRole left, BaseRole right )
	{
		if ( left is null )
		{
			if ( right is null )
				return true;

			return false;
		}

		return left.Equals( right );
	}
	public static bool operator !=( BaseRole left, BaseRole right ) => !(left == right);

	public static bool operator ==( BaseRole left, string right )
	{
		if ( left is null || right is null )
			return false;

		return left.Equals( right );
	}
	public static bool operator !=( BaseRole left, string right ) => !(left == right);

	public bool Equals( BaseRole other )
	{
		if ( other is null )
			return false;

		if ( ReferenceEquals( this, other ) )
			return true;

		return Info.ResourceId == other.Info.ResourceId;
	}

	public bool Equals( string other )
	{
		if ( Info.Title.Equals( other, StringComparison.OrdinalIgnoreCase ) )
			return true;

		if ( Info.ClassName.Equals( other, StringComparison.OrdinalIgnoreCase ) )
			return true;

		return false;
	}

	public override bool Equals( object obj ) => Equals( obj as BaseRole );

	public override int GetHashCode() => Info.ResourceId.GetHashCode();

#if SANDBOX && DEBUG
	[Event.Hotload]
	private void OnHotload()
	{
		Info = GameResource.GetInfo<RoleInfo>( GetType() );
		Player.RoleButtons = GetRoleButtons();
	}
#endif
}
