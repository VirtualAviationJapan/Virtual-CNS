// Upgrade NOTE: upgraded instancing buffer 'VirtualCNSMarkerBeaconReceiver' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Virtual-CNS/MarkerBeaconReceiver"
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		[Toggle(_VERTEXCOLOR_ON)] _VertexColor("Vertex Color", Float) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		[NoScaleOffset][Normal]_BumpMap("Normal", 2D) = "bump" {}
		[NoScaleOffset]_EmissionMap("Emission", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,1)
		_OffsetUnits("Offset Units", Float) = 0
		_OffsetFactor("Offset Factor", Float) = 0
		[KeywordEnum(Texture,VertexColor)] _MaskSoruce("Mask Soruce", Float) = 0
		[NoScaleOffset]_MarkerMask("Marker Mask", 2D) = "white" {}
		[Header(Marker Settings)]_BlinkPerSeconds("Blink Per Seconds", Vector) = (2,1.33,4,0)
		_BlinkDT("Blink DT", Vector) = (0.8,0.1,0.5,0)
		_SecondaryOffset("Secondary Offset", Vector) = (0,0.3,0,0)
		[Header(Marker Status)]_Marker("Marker (O/M/I)", Vector) = (0,0,0,0)
		[KeywordEnum(Emission,Visibility)] _Mode("Mode", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		Offset  [_OffsetFactor] , [_OffsetUnits]
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature_local _MODE_EMISSION _MODE_VISIBILITY
		#pragma shader_feature_local _MASKSORUCE_TEXTURE _MASKSORUCE_VERTEXCOLOR
		#pragma shader_feature_local _VERTEXCOLOR_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _OffsetUnits;
		uniform float _OffsetFactor;
		uniform sampler2D _MarkerMask;
		uniform float3 _BlinkPerSeconds;
		uniform sampler2D _BumpMap;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionColor;
		uniform float _Metallic;
		uniform float _Smoothness;

		UNITY_INSTANCING_BUFFER_START(VirtualCNSMarkerBeaconReceiver)
			UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
#define _MainTex_ST_arr VirtualCNSMarkerBeaconReceiver
			UNITY_DEFINE_INSTANCED_PROP(float3, _Marker)
#define _Marker_arr VirtualCNSMarkerBeaconReceiver
			UNITY_DEFINE_INSTANCED_PROP(float3, _BlinkDT)
#define _BlinkDT_arr VirtualCNSMarkerBeaconReceiver
			UNITY_DEFINE_INSTANCED_PROP(float3, _SecondaryOffset)
#define _SecondaryOffset_arr VirtualCNSMarkerBeaconReceiver
		UNITY_INSTANCING_BUFFER_END(VirtualCNSMarkerBeaconReceiver)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float2 uv_MarkerMask24 = v.texcoord;
			#if defined(_MASKSORUCE_TEXTURE)
				float3 staticSwitch51 = (tex2Dlod( _MarkerMask, float4( uv_MarkerMask24, 0, 0.0) )).rgb;
			#elif defined(_MASKSORUCE_VERTEXCOLOR)
				float3 staticSwitch51 = (v.color).rgb;
			#else
				float3 staticSwitch51 = (tex2Dlod( _MarkerMask, float4( uv_MarkerMask24, 0, 0.0) )).rgb;
			#endif
			float3 _Marker_Instance = UNITY_ACCESS_INSTANCED_PROP(_Marker_arr, _Marker);
			float dotResult41 = dot( staticSwitch51 , _Marker_Instance );
			float3 temp_output_19_0 = ( _Marker_Instance * staticSwitch51 );
			float dotResult21 = dot( temp_output_19_0 , _BlinkPerSeconds );
			float mulTime18 = _Time.y * dotResult21;
			float3 _BlinkDT_Instance = UNITY_ACCESS_INSTANCED_PROP(_BlinkDT_arr, _BlinkDT);
			float dotResult34 = dot( temp_output_19_0 , _BlinkDT_Instance );
			float3 _SecondaryOffset_Instance = UNITY_ACCESS_INSTANCED_PROP(_SecondaryOffset_arr, _SecondaryOffset);
			float dotResult36 = dot( temp_output_19_0 , _SecondaryOffset_Instance );
			float temp_output_26_0 = saturate( ( dotResult41 * ( step( frac( mulTime18 ) , dotResult34 ) + step( frac( ( mulTime18 + dotResult36 ) ) , dotResult34 ) ) ) );
			#if defined(_MODE_EMISSION)
				float3 staticSwitch42 = ase_vertex3Pos;
			#elif defined(_MODE_VISIBILITY)
				float3 staticSwitch42 = ( ase_vertex3Pos * temp_output_26_0 );
			#else
				float3 staticSwitch42 = ase_vertex3Pos;
			#endif
			v.vertex.xyz = staticSwitch42;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap6 = i.uv_texcoord;
			o.Normal = UnpackNormal( tex2D( _BumpMap, uv_BumpMap6 ) );
			float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_MainTex_ST_arr, _MainTex_ST);
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
			float3 temp_output_11_0 = ( (_Color).rgb * (tex2D( _MainTex, uv_MainTex )).rgb );
			float3 gammaToLinear56 = GammaToLinearSpace( (i.vertexColor).rgb );
			#ifdef _VERTEXCOLOR_ON
				float3 staticSwitch57 = ( temp_output_11_0 * gammaToLinear56 );
			#else
				float3 staticSwitch57 = temp_output_11_0;
			#endif
			o.Albedo = staticSwitch57;
			float2 uv_EmissionMap5 = i.uv_texcoord;
			float3 temp_output_8_0 = (tex2D( _EmissionMap, uv_EmissionMap5 )).rgb;
			float3 temp_output_12_0 = (_EmissionColor).rgb;
			float2 uv_MarkerMask24 = i.uv_texcoord;
			#if defined(_MASKSORUCE_TEXTURE)
				float3 staticSwitch51 = (tex2D( _MarkerMask, uv_MarkerMask24 )).rgb;
			#elif defined(_MASKSORUCE_VERTEXCOLOR)
				float3 staticSwitch51 = (i.vertexColor).rgb;
			#else
				float3 staticSwitch51 = (tex2D( _MarkerMask, uv_MarkerMask24 )).rgb;
			#endif
			float3 _Marker_Instance = UNITY_ACCESS_INSTANCED_PROP(_Marker_arr, _Marker);
			float dotResult41 = dot( staticSwitch51 , _Marker_Instance );
			float3 temp_output_19_0 = ( _Marker_Instance * staticSwitch51 );
			float dotResult21 = dot( temp_output_19_0 , _BlinkPerSeconds );
			float mulTime18 = _Time.y * dotResult21;
			float3 _BlinkDT_Instance = UNITY_ACCESS_INSTANCED_PROP(_BlinkDT_arr, _BlinkDT);
			float dotResult34 = dot( temp_output_19_0 , _BlinkDT_Instance );
			float3 _SecondaryOffset_Instance = UNITY_ACCESS_INSTANCED_PROP(_SecondaryOffset_arr, _SecondaryOffset);
			float dotResult36 = dot( temp_output_19_0 , _SecondaryOffset_Instance );
			float temp_output_26_0 = saturate( ( dotResult41 * ( step( frac( mulTime18 ) , dotResult34 ) + step( frac( ( mulTime18 + dotResult36 ) ) , dotResult34 ) ) ) );
			#if defined(_MODE_EMISSION)
				float3 staticSwitch45 = ( temp_output_8_0 * temp_output_12_0 * temp_output_26_0 );
			#elif defined(_MODE_VISIBILITY)
				float3 staticSwitch45 = ( temp_output_8_0 * temp_output_12_0 );
			#else
				float3 staticSwitch45 = ( temp_output_8_0 * temp_output_12_0 * temp_output_26_0 );
			#endif
			o.Emission = staticSwitch45;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
0;1441;1809;638;336.766;579.3061;1;True;False
Node;AmplifyShaderEditor.SamplerNode;24;-896,384;Inherit;True;Property;_MarkerMask;Marker Mask;11;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;4;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;52;-896,576;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;25;-608,384;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;53;-608,576;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;51;-416.2046,450.4153;Inherit;False;Property;_MaskSoruce;Mask Soruce;10;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Texture;VertexColor;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;17;-336,608;Inherit;False;InstancedProperty;_Marker;Marker (O/M/I);15;1;[Header];Create;False;1;Marker Status;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;1;-336,768;Inherit;False;Property;_BlinkPerSeconds;Blink Per Seconds;12;1;[Header];Create;True;1;Marker Settings;0;0;False;0;False;2,1.33,4;0.5,0.33,0.25;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-66.01442,629.981;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;21;128,768;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;35;-256,1152;Inherit;False;InstancedProperty;_SecondaryOffset;Secondary Offset;14;0;Create;True;0;0;0;False;0;False;0,0.3,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;18;256,768;Inherit;True;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;36;128,1120;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;602.9443,871.1445;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;32;-450.274,953.3467;Inherit;False;InstancedProperty;_BlinkDT;Blink DT;13;0;Create;True;0;0;0;False;0;False;0.8,0.1,0.5;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FractNode;38;675.796,1095.495;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;34;128,960;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;22;448,640;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;27;699.0557,734.4738;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;39;852.6214,922.9088;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-226,-226;Inherit;True;Property;_MainTex;Albedo;0;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-128,-416;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;54;-256,-576;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;40;1108.221,912.6024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;41;463.1353,490.5617;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-224,160;Inherit;True;Property;_EmissionMap;Emission;6;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;4;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;10;64,-416;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;7;64,-224;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;734.5573,496.0027;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-144,352;Inherit;False;Property;_EmissionColor;Emission Color;7;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;55;-96,-576;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;413,-309;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;8;64,160;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;47;624,240;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;12;48,352;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;26;916.3484,482.5713;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GammaToLinearNode;56;96,-576;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;368,144;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;566,-425;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;368,240;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;822.9479,335.8882;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;57;752,-464;Inherit;False;Property;_VertexColor;Vertex Color;2;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Emission;Visibility;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;6;-224,-32;Inherit;True;Property;_BumpMap;Normal;5;2;[NoScaleOffset];[Normal];Create;False;0;0;0;False;0;False;4;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;16;321,65;Inherit;False;Property;_Smoothness;Smoothness;4;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;45;816,48;Inherit;False;Property;_Keyword0;Keyword 0;16;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;42;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;15;324,-8;Inherit;False;Property;_Metallic;Metallic;3;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-1152.096,865.9004;Inherit;False;Property;_OffsetFactor;Offset Factor;9;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-1146.519,750.6467;Inherit;False;Property;_OffsetUnits;Offset Units;8;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;42;1054.858,250.8607;Inherit;False;Property;_Mode;Mode;16;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Emission;Visibility;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1253.21,-263.578;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Virtual-CNS/MarkerBeaconReceiver;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;True;0;True;50;0;True;49;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Absolute;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;24;0
WireConnection;53;0;52;0
WireConnection;51;1;25;0
WireConnection;51;0;53;0
WireConnection;19;0;17;0
WireConnection;19;1;51;0
WireConnection;21;0;19;0
WireConnection;21;1;1;0
WireConnection;18;0;21;0
WireConnection;36;0;19;0
WireConnection;36;1;35;0
WireConnection;37;0;18;0
WireConnection;37;1;36;0
WireConnection;38;0;37;0
WireConnection;34;0;19;0
WireConnection;34;1;32;0
WireConnection;22;0;18;0
WireConnection;27;0;22;0
WireConnection;27;1;34;0
WireConnection;39;0;38;0
WireConnection;39;1;34;0
WireConnection;40;0;27;0
WireConnection;40;1;39;0
WireConnection;41;0;51;0
WireConnection;41;1;17;0
WireConnection;10;0;9;0
WireConnection;7;0;4;0
WireConnection;31;0;41;0
WireConnection;31;1;40;0
WireConnection;55;0;54;0
WireConnection;11;0;10;0
WireConnection;11;1;7;0
WireConnection;8;0;5;0
WireConnection;12;0;13;0
WireConnection;26;0;31;0
WireConnection;56;0;55;0
WireConnection;43;0;8;0
WireConnection;43;1;12;0
WireConnection;58;0;11;0
WireConnection;58;1;56;0
WireConnection;14;0;8;0
WireConnection;14;1;12;0
WireConnection;14;2;26;0
WireConnection;48;0;47;0
WireConnection;48;1;26;0
WireConnection;57;1;11;0
WireConnection;57;0;58;0
WireConnection;45;1;14;0
WireConnection;45;0;43;0
WireConnection;42;1;47;0
WireConnection;42;0;48;0
WireConnection;0;0;57;0
WireConnection;0;1;6;0
WireConnection;0;2;45;0
WireConnection;0;3;15;0
WireConnection;0;4;16;0
WireConnection;0;11;42;0
ASEEND*/
//CHKSM=4879BFBC0F6322356EDFF714E034C7EC4C19EC86