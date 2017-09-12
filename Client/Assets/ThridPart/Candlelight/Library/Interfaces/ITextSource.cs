// 
// ITextSource.cs
// 
// Copyright (c) 2015-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

namespace Candlelight
{
	/// <summary>
	/// A delegate type for an event on a <see cref="ITextSource"/>.
	/// </summary>
	public delegate void ITextSourceEvent(ITextSource textSource);

	/// <summary>
	/// An interface to specify an object is a source of text.
	/// </summary>
	public interface ITextSource
	{
		/// <summary>
		/// Gets a callback for whenever the text on this instance has changed.
		/// </summary>
		/// <value>A callback for whenever the text on this instance has changed.</value>
		event ITextSourceEvent OnBecameDirty;
		/// <summary>
		/// Gets the output text.
		/// </summary>
		/// <value>The output text.</value>
		string OutputText { get; }
	}
}