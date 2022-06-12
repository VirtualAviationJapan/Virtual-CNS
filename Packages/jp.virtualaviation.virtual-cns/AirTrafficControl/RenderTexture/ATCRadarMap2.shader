// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Virtual-CNS/ATCRadarMap2"
{
	Properties
	{
		[PerRendererData]_MainTex("Main Tex", 2D) = "white" {}
		_Contrast("Contrast", Float) = 1
		_Brightness("Brightness", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComparison("Stencil Comparison", Int) = 8
		_StencilID("Stencil ID", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOperation("Stencil Operation", Int) = 0
		_StencilWriteMask("Stencil Write Mask", Int) = 255
		_StencilReadMask("Stencil Read Mask", Int) = 255
		[Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask("Color Mask ", Int) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseAlphaClip("Use Alpha Clip", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZWrite Off
		Stencil
		{
			Ref [_StencilID]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilReadMask]
			Comp [_StencilComparison]
			Pass [_StencilOperation]
		}
		Blend SrcAlpha OneMinusSrcAlpha
		
		ColorMask [_ColorMask]
		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature_local UNITY_UI_ALPHACLIP
		#pragma surface surf Unlit keepalpha noshadow nodynlightmap nometa noforwardadd 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform int _StencilOperation;
		uniform int _StencilComparison;
		uniform int _StencilID;
		uniform int _StencilWriteMask;
		uniform int _StencilReadMask;
		uniform int _ColorMask;
		uniform float _Contrast;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Brightness;


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float grayscale4 = Luminance((tex2D( _MainTex, uv_MainTex )).rgb);
			float4 temp_cast_0 = (grayscale4).xxxx;
			o.Emission = ( (CalculateContrast(_Contrast,temp_cast_0)).rgb * _Brightness );
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
0;973;1675;1106;1622.566;574.4794;2.089735;True;False
Node;AmplifyShaderEditor.SamplerNode;1;-512,-128;Inherit;True;Property;_MainTex;Main Tex;0;1;[PerRendererData];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;2;-224,-128;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-32,-32;Inherit;False;Property;_Contrast;Contrast;2;0;Create;True;0;0;0;False;0;False;1;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;4;-32,-128;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;5;160,-128;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;7;352,-128;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;8;352,-32;Inherit;False;Property;_Brightness;Brightness;3;0;Create;True;0;0;0;False;0;False;1;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;544,-128;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.IntNode;12;-509.3276,439.9061;Inherit;False;Property;_StencilOperation;Stencil Operation;6;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.StencilOp;True;0;False;0;0;False;0;1;INT;0
Node;AmplifyShaderEditor.IntNode;11;-512,352;Inherit;False;Property;_StencilID;Stencil ID;5;0;Create;True;0;0;0;True;0;False;0;0;False;0;1;INT;0
Node;AmplifyShaderEditor.IntNode;10;-512,256;Inherit;False;Property;_StencilComparison;Stencil Comparison;4;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;8;8;False;0;1;INT;0
Node;AmplifyShaderEditor.IntNode;13;-512,544;Inherit;False;Property;_StencilWriteMask;Stencil Write Mask;7;0;Create;True;0;0;0;True;0;False;255;0;False;0;1;INT;0
Node;AmplifyShaderEditor.StaticSwitch;18;-480,848;Inherit;False;Property;_UseAlphaClip;Use Alpha Clip;10;0;Create;True;0;0;0;True;0;False;0;0;0;True;UNITY_UI_ALPHACLIP;Toggle;2;Key0;Key1;Create;True;False;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;14;-512,640;Inherit;False;Property;_StencilReadMask;Stencil Read Mask;8;0;Create;True;0;0;0;True;0;False;255;0;False;0;1;INT;0
Node;AmplifyShaderEditor.IntNode;15;-512,736;Inherit;False;Property;_ColorMask;Color Mask ;9;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.ColorWriteMask;True;0;False;15;0;False;0;1;INT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;672,-128;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Virtual-CNS/ATCRadarMap2;False;False;False;False;False;False;False;True;False;False;True;True;False;False;True;False;False;False;False;False;False;Back;2;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;True;15;True;0;True;11;255;True;14;255;True;14;7;True;10;1;True;12;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;0
WireConnection;4;0;2;0
WireConnection;5;1;4;0
WireConnection;5;0;6;0
WireConnection;7;0;5;0
WireConnection;9;0;7;0
WireConnection;9;1;8;0
WireConnection;0;2;9;0
ASEEND*/
//CHKSM=180CE245A462DD98AFC24D09421C127EB6EA0A3E