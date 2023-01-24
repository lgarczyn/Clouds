using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumetricLines
{
	public static class VolumetricLineVertexData
	{
		public static readonly int VertexCount = 12;

		public static readonly Vector2[] TexCoords = {
			new Vector2(1.0f, 1.0f),
			new Vector2(1.0f, 0.0f),
			new Vector2(0f, 0.0f),
			new Vector2(0f, 1.0f),

			new Vector2(0.5f, 1.0f),
			new Vector2(0.5f, 0.0f),
			new Vector2(0.5f, 0.0f),
			new Vector2(0.5f, 1.0f),

			new Vector2(1f, 0.0f),
			new Vector2(1f, 1.0f),
			new Vector2(0.0f, 1.0f),
			new Vector2(0.0f, 0.0f),
		};


		public static readonly Vector2[] VertexOffsets = {
			 new Vector2(1.0f, 1.0f),
			 new Vector2(1.0f, -1.0f),
			 new Vector2(-1.0f, -1.0f),
			 new Vector2(-1.0f, 1.0f),

			 new Vector2(0f, 1.0f),
			 new Vector2(0f, -1.0f),
			 new Vector2(0f, 1.0f),
			 new Vector2(0f, -1.0f),

			 new Vector2(-1.0f, 1.0f),
			 new Vector2(-1.0f, -1.0f),
			 new Vector2(1.0f, -1.0f),
			 new Vector2(1.0f, 1.0f)
		};

		public static readonly int[] Indices =
		{
			0, 1, 2, 3,

			4, 5, 6, 7,

			8, 9, 10, 11
		};
	}
}
