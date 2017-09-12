// 
// HashPool.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// A generic class for storing a pool of hash sets.
	/// </summary>
	/// <typeparam name="T">The hash set element type.</typeparam>
	internal static class HashPool<T>
	{
		/// <summary>
		/// An underlying <see cref="ObjectPool{T}"/>.
		/// </summary>
		private static readonly ObjectPool<HashSet<T>> s_Pool = new ObjectPool<HashSet<T>>(null, l => l.Clear());

		/// <summary>
		/// Gets an available hash set from the pool.
		/// </summary>
		public static HashSet<T> Get()
		{
			return s_Pool.Get();
		}

		/// <summary>
		/// Releases a hash set to the pool.
		/// </summary>
		/// <param name="hashSet">Hash set to release.</param>
		public static void Release(HashSet<T> hashSet)
		{
			s_Pool.Release(hashSet);
		}
	}
}
