using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TTT;

public abstract class Asset : GameResource
{
	private static readonly Dictionary<string, Asset> Collection = new( StringComparer.OrdinalIgnoreCase );

	[Property( "libraryname", "The name you define in the Library Attribute in code." ), Category( "Important" )]
	public string LibraryName { get; set; }

	public string Title { get; private set; }

	public static T GetInfo<T>( string name ) where T : Asset
	{
		if ( string.IsNullOrEmpty( name ) || !Collection.ContainsKey( name ) )
			return null;

		return Collection[name] as T;
	}

	protected override void PostLoad()
	{
		base.PostLoad();

		if ( string.IsNullOrWhiteSpace( LibraryName ) )
			return;

		var attribute = Library.GetAttribute( LibraryName );

		if ( attribute is null )
			return;

		Title = attribute.Title;
		Collection[LibraryName] = this;

		if ( !string.IsNullOrWhiteSpace( Title ) )
			Collection[Title] = this;
	}
}
