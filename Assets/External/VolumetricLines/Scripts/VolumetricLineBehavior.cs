using UnityEngine;
using System.Linq;

namespace VolumetricLines
{
	/// <summary>
	/// Render a single volumetric line
	/// 
	/// Based on the Volumetric lines algorithm by Sebastien Hillaire
	/// http://sebastien.hillaire.free.fr/index.php?option=com_content&view=article&id=57&Itemid=74
	/// 
	/// Thread in the Unity3D Forum:
	/// http://forum.unity3d.com/threads/181618-Volumetric-lines
	/// 
	/// Unity3D port by Johannes Unterguggenberger
	/// johannes.unterguggenberger@gmail.com
	/// 
	/// Thanks to Michael Probst for support during development.
	/// 
	/// Thanks for bugfixes and improvements to Unity Forum User "Mistale"
	/// http://forum.unity3d.com/members/102350-Mistale
    /// 
    /// Shader code optimization and cleanup by Lex Darlog (aka DRL)
    /// http://forum.unity3d.com/members/lex-drl.67487/
    /// 
	/// </summary>
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class VolumetricLineBehavior : MonoBehaviour 
	{
		#region private variables

		/// <summary>
		/// The start position relative to the GameObject's origin
		/// </summary>
		[SerializeField] 
		private Material m_material;

		/// <summary>
		/// The start position relative to the GameObject's origin
		/// </summary>
		[SerializeField] 
		private Vector3 m_startPos;
		
		/// <summary>
		/// The end position relative to the GameObject's origin
		/// </summary>
		[SerializeField] 
		private Vector3 m_endPos = new Vector3(0f, 0f, 100f);

		/// <summary>
		/// Line Color
		/// </summary>
		[SerializeField] [ColorUsage(true, true)]
		private Color m_lineColor;

		/// <summary>
		/// Line Color
		/// </summary>
		[SerializeField]
		private float m_lineWidth = 1;


		#endregion

		#region properties


		/// <summary>
		/// Get or set the start position of this volumetric line's mesh
		/// </summary>
		public Vector3 StartPos
		{
			get { return m_startPos; }
			set
			{
				m_startPos = value;
			UpdateMesh(m_startPos, m_endPos, m_lineWidth, m_lineColor);
			}
		}

		/// <summary>
		/// Get or set the end position of this volumetric line's mesh
		/// </summary>
		public Vector3 EndPos
		{
			get { return m_endPos; }
			set
			{
				m_endPos = value;
			UpdateMesh(m_startPos, m_endPos, m_lineWidth, m_lineColor);
			}
		}

		//TODO setter for color and width

		#endregion
		
		#region methods

		/// <summary>
		/// Retrieve the current line scale from the material
		/// </summary>
		private float LineRadius {
			get {
				return m_material == null ? 0f : m_material.GetFloat("_LineRadius");
			}
		}

		/// <summary>
		/// Retrieve the transform scale factor of the line
		/// </summary>
		private float LineScale {
			get {
				return (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
			}
		}

		/// <summary>
		/// Calculate the bounds of this line based on start and end points,
		/// the line width, and the scaling of the object.
		/// </summary>
		private Bounds CalculateBounds()
		{
			var scaledLineRadius = LineScale * LineRadius * m_lineWidth;

			var min = new Vector3(
				Mathf.Min(m_startPos.x, m_endPos.x) - scaledLineRadius,
				Mathf.Min(m_startPos.y, m_endPos.y) - scaledLineRadius,
				Mathf.Min(m_startPos.z, m_endPos.z) - scaledLineRadius
			);
			var max = new Vector3(
				Mathf.Max(m_startPos.x, m_endPos.x) + scaledLineRadius,
				Mathf.Max(m_startPos.y, m_endPos.y) + scaledLineRadius,
				Mathf.Max(m_startPos.z, m_endPos.z) + scaledLineRadius
			);
			
			return new Bounds
			{
				min = min,
				max = max
			};
		}

		/// <summary>
		/// Updates the bounds of this line according to the current properties, 
		/// which there are: start point, end point, line width, scaling of the object.
		/// </summary>
		public void UpdateBounds()
		{
			MeshFilter meshFilter = GetComponent<MeshFilter>();
			if (null != meshFilter)
			{
				var mesh = meshFilter.sharedMesh;
				Debug.Assert(null != mesh);
				if (null != mesh)
				{
					mesh.bounds = CalculateBounds();
				}
			}
		}

		/// <summary>
		/// Sets the start and end points - updates the data of the Mesh.
		/// </summary>
		public void UpdateMesh(Vector3 startPoint, Vector3 endPoint, float width, Color color)
		{
			m_startPos = startPoint;
			m_endPos = endPoint;

			Vector3[] vertexPositions = {
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
			};
			
			Vector3[] other = {
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
			};

			Color[] colors = Enumerable.Repeat(color, VolumetricLineVertexData.VertexCount).ToArray();

			Vector2[] offsets = VolumetricLineVertexData
				.VertexOffsets
				.Select(offset => offset * width)
				.ToArray();

			MeshFilter meshFilter = GetComponent<MeshFilter>();
			if (null != meshFilter)
			{
				var mesh = meshFilter.sharedMesh;
				
				if (mesh == null) {
					mesh = new Mesh();
					mesh.MarkDynamic();
					mesh.vertices = vertexPositions;
					mesh.normals = other;
					mesh.uv = VolumetricLineVertexData.TexCoords;
					mesh.uv2 = offsets;
					mesh.colors = colors;
					mesh.SetIndices(VolumetricLineVertexData.Indices, MeshTopology.Quads, 0);
					meshFilter.sharedMesh = mesh;
				} else {
					mesh.vertices = vertexPositions;
					mesh.normals = other;
					mesh.uv2 = offsets;
					mesh.colors = colors;
				}
				UpdateBounds();
			}
		}
		#endregion

		#region event functions
		void Start () 
		{
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = m_material;
			
			UpdateMesh(m_startPos, m_endPos, m_lineWidth, m_lineColor);
		}

		void OnDrawGizmos() {
			var start = transform.TransformPoint(m_startPos);
			var end = transform.TransformPoint(m_endPos);
			Gizmos.color = m_lineColor;
			Gizmos.DrawWireSphere(start, LineRadius * LineScale * m_lineWidth);
			Gizmos.DrawWireSphere(end, LineRadius * LineScale * m_lineWidth);
			Gizmos.DrawLine(start, end);
		}

		void OnDestroy()
		{
			MeshFilter meshFilter = GetComponent<MeshFilter>();
			if (null != meshFilter) 
			{
				if (Application.isPlaying) 
				{
					Mesh.Destroy(meshFilter.sharedMesh);
				}
				else // avoid "may not be called from edit mode" error
				{
					Mesh.DestroyImmediate(meshFilter.sharedMesh);
				}
				meshFilter.sharedMesh = null;
			}
		}
		
		void Update()
		{
			if (transform.hasChanged)
			{
				UpdateBounds();
			}
		}
		#endregion
	}
}