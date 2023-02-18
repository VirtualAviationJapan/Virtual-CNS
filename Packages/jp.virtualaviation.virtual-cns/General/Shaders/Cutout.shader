// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ZHI/Cutout"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Color("Color", Color) = (0.8,0.8,0.8,1)
		_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset]_MetallicGlossMap("Metallic", 2D) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smothness("Smothness", Range( 0 , 1)) = 0.5
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Int) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull [_CullMode]
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform int _CullMode;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _MetallicGlossMap;
		uniform float _Metallic;
		uniform float _Smothness;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 temp_output_5_0 = ( _Color * tex2D( _MainTex, uv_MainTex ) );
			o.Albedo = temp_output_5_0.rgb;
			float2 uv_MetallicGlossMap8 = i.uv_texcoord;
			float4 tex2DNode8 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap8 );
			o.Metallic = ( tex2DNode8.r * _Metallic );
			o.Smoothness = ( tex2DNode8.a * _Smothness );
			o.Alpha = 1;
			clip( temp_output_5_0.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
1284;986;1920;1014;1079.892;360.858;1;True;False
Node;AmplifyShaderEditor.ColorNode;2;-640,-368;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;0.8,0.8,0.8,1;0,0,0,0.1019608;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-640,-192;Inherit;True;Property;_MainTex;Albedo;2;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-321,-240;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-640,256;Inherit;False;Property;_Metallic;Metallic;4;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-640,352;Inherit;False;Property;_Smothness;Smothness;5;0;Create;True;0;0;0;False;0;False;0.5;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;-640,0;Inherit;True;Property;_MetallicGlossMap;Metallic;3;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;4;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-310,117;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-256,304;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;1;-640,448;Inherit;False;Property;_CullMode;Cull Mode;6;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;2;0;False;0;1;INT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;13;-197.892,-128.858;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;135,-242;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;ZHI/Cutout;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;ForwardOnly;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;True;1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;2;0
WireConnection;5;1;4;0
WireConnection;9;0;8;1
WireConnection;9;1;7;0
WireConnection;10;0;8;4
WireConnection;10;1;12;0
WireConnection;13;0;5;0
WireConnection;0;0;5;0
WireConnection;0;3;9;0
WireConnection;0;4;10;0
WireConnection;0;10;13;3
ASEEND*/
//CHKSM=90BBA13007609FD74DF8600C93E0CE8043FFAAD9