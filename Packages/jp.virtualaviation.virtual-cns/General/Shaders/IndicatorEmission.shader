// Upgrade NOTE: upgraded instancing buffer 'VirtualCNSIndicatorEmission' to new syntax.

// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Virtual-CNS/IndicatorEmission"
{
	Properties
	{
		_OffsetFactor("Offset Factor", Float) = 0
		_OffsetUnit("Offset Unit", Float) = 0
		_MainTex("Main Tex", 2D) = "white" {}
		[HDR]_Color("Color", Color) = (1,1,1,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		ZTest LEqual
		Offset  [_OffsetFactor] , [_OffsetUnit]
		Blend One One
		
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _OffsetUnit;
		uniform float _OffsetFactor;
		uniform sampler2D _MainTex;

		UNITY_INSTANCING_BUFFER_START(VirtualCNSIndicatorEmission)
			UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
#define _MainTex_ST_arr VirtualCNSIndicatorEmission
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
#define _Color_arr VirtualCNSIndicatorEmission
		UNITY_INSTANCING_BUFFER_END(VirtualCNSIndicatorEmission)

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_MainTex_ST_arr, _MainTex_ST);
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
			float4 _Color_Instance = UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			float4 temp_output_6_0 = ( tex2D( _MainTex, uv_MainTex ) * i.vertexColor * _Color_Instance );
			o.Emission = ( (temp_output_6_0).rgb * (temp_output_6_0).a );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.VertexColorNode;4;-640,-256;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-640,-64;Inherit;False;InstancedProperty;_Color;Color;4;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-307.2834,-383.3961;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;8;-48.71155,-148.4437;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;7;-72.33112,-321.2393;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-640,352;Inherit;False;Property;_OffsetUnit;Offset Unit;2;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-640,256;Inherit;False;Property;_OffsetFactor;Offset Factor;1;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;235.9662,-261.5689;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;192.6858,-68.37238;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Virtual-CNS/IndicatorEmission;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;3;False;;True;0;True;_OffsetFactor;0;True;_OffsetUnit;False;0;Custom;0.5;True;True;0;True;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;4;1;False;;1;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SamplerNode;5;-768,-464;Inherit;True;Property;_MainTex;Main Tex;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;6;0;5;0
WireConnection;6;1;4;0
WireConnection;6;2;3;0
WireConnection;8;0;6;0
WireConnection;7;0;6;0
WireConnection;9;0;7;0
WireConnection;9;1;8;0
WireConnection;0;2;9;0
ASEEND*/
//CHKSM=7F81252A69ED550FCB0F07BE1AB95D2BFD0C9DAB