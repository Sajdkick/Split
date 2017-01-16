Shader "SpikeShader"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex wfiVertCol
			#pragma fragment passThrough

			#include "UnityCG.cginc"

			struct VertOut
			{
				float4 position : SV_POSITION;
				float4 color : COLOR;
			};
			struct VertIn
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			VertOut wfiVertCol(VertIn input)
			{
				VertOut output;
				output.position = mul(UNITY_MATRIX_MVP,input.vertex);
				output.color = input.color;
				return output;
			}
			struct FragOut
			{
				float4 color : COLOR;
			};
			FragOut passThrough(float4 color : COLOR)
			{
				FragOut output;
				output.color = color;
				return output;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
