// Upgrade NOTE: upgraded instancing buffer 'BeaconLight' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BeaconLight"
{
	Properties
	{
		_Color("Color", Color) = (0.1,0.1,0.1,0)
		_Intensity("Intensity", Float) = 8
		_InflateOffset("Inflate Offset", Float) = 0
		_Inflate("Inflate", Float) = 1
		_PositionNoiseScale("Position Noise Scale", Float) = 5
		_PositionNoiseIntensity("Position Noise Intensity", Float) = 5
		_InflateDistance("Inflate Distance", Float) = 1
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		_OnTime("On Time", Range( 0 , 1)) = 0.2
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float4 vertexColor : COLOR;
		};

		uniform float _InflateDistance;
		uniform float _InflateOffset;
		uniform float _Inflate;
		uniform float4 _Color;
		uniform float _PositionNoiseScale;
		uniform float _PositionNoiseIntensity;
		uniform float _OnTime;
		uniform float _Smoothness;

		UNITY_INSTANCING_BUFFER_START(BeaconLight)
			UNITY_DEFINE_INSTANCED_PROP(float, _Intensity)
#define _Intensity_arr BeaconLight
		UNITY_INSTANCING_BUFFER_END(BeaconLight)


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 temp_output_36_0_g3 = ase_vertexNormal;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 worldToObjDir33_g3 = mul( unity_WorldToObject, float4( ase_worldViewDir, 0 ) ).xyz;
			float3 temp_output_6_0_g4 = worldToObjDir33_g3;
			float dotResult1_g4 = dot( temp_output_36_0_g3 , temp_output_6_0_g4 );
			float dotResult2_g4 = dot( temp_output_6_0_g4 , temp_output_6_0_g4 );
			float3 normalizeResult34_g3 = normalize( ( temp_output_36_0_g3 - ( ( dotResult1_g4 / dotResult2_g4 ) * temp_output_6_0_g4 ) ) );
			float temp_output_30_0_g3 = _InflateOffset;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float cameraDepthFade14_g3 = (( -UnityObjectToViewPos( ase_vertex3Pos ).z -_ProjectionParams.y - temp_output_30_0_g3 ) / ( _InflateDistance - temp_output_30_0_g3 ));
			float localunity_CameraInvProjection_m1146_g3 = ( unity_CameraInvProjection._m11 );
			v.vertex.xyz += ( normalizeResult34_g3 * saturate( cameraDepthFade14_g3 ) * _Inflate * saturate( ( 1.0 * localunity_CameraInvProjection_m1146_g3 ) ) );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = (_Color).rgb;
			float _Intensity_Instance = UNITY_ACCESS_INSTANCED_PROP(_Intensity_arr, _Intensity);
			float mulTime23 = _Time.y * i.vertexColor.a;
			float3 objToWorld30 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float simplePerlin3D31 = snoise( objToWorld30*_PositionNoiseScale );
			simplePerlin3D31 = simplePerlin3D31*0.5 + 0.5;
			float lerpResult22 = lerp( step( frac( ( mulTime23 + ( simplePerlin3D31 * _PositionNoiseIntensity ) ) ) , ( _OnTime * i.vertexColor.a ) ) , 1.0 , step( i.vertexColor.a , 0.0 ));
			o.Emission = ( (i.vertexColor).rgb * _Intensity_Instance * lerpResult22 );
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
635;833;1920;1020;2825.961;175.8648;1.487587;True;False
Node;AmplifyShaderEditor.TransformPositionNode;30;-1667.841,641.7436;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;32;-1634.157,815.9597;Inherit;False;Property;_PositionNoiseScale;Position Noise Scale;4;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1;-1539.552,-10.00065;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;31;-1399.428,696.8265;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-1628.454,991.891;Inherit;False;Property;_PositionNoiseIntensity;Position Noise Intensity;5;0;Create;True;0;0;0;False;0;False;5;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;23;-1362.756,546.5507;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-1046.807,834.2067;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-1246.684,324.66;Inherit;False;Property;_OnTime;On Time;8;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-1127.324,635.3503;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-944.7786,410.9202;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;24;-1011.888,568.4803;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;25;-780.0641,471.3646;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;21;-902.2412,173.7529;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-352,768;Inherit;False;Property;_InflateOffset;Inflate Offset;2;0;Create;True;0;0;0;False;0;False;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-349,681;Inherit;False;Property;_InflateDistance;Inflate Distance;6;0;Create;True;0;0;0;False;0;False;1;500;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-505.0813,939.1433;Inherit;False;Property;_Inflate;Inflate;3;0;Create;True;0;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-326,375;Inherit;False;InstancedProperty;_Intensity;Intensity;1;0;Create;True;0;0;0;False;0;False;8;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;6;-443,-175;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;0.1,0.1,0.1,0;0.1999999,0.1971697,0.1971697,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;3;-283,207;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;22;-524.744,317.8597;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-39,303;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;18;605,423;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;7;-197,-119;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;10;-384,512;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;17;215,964;Inherit;False;World;Object;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;19;305.4672,668.6754;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-352,54;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;0;False;0;False;0.5;0.8;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;15;523,619;Inherit;False;Projection;-1;;5;3249e2c8638c9ef4bbd1902a2d38a67c;0;2;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;16;44,991;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FunctionNode;29;-31.04939,849.6054;Inherit;False;EAA_DistanceScale;-1;;3;b989b4d69a2967c49bf3692bcfd7a862;0;5;29;FLOAT;1;False;37;FLOAT;1;False;30;FLOAT;100;False;31;FLOAT;400;False;36;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;11;0,480;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CameraDepthFade;9;0,640;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;281,395;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;762,-118;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;BeaconLight;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;31;0;30;0
WireConnection;31;1;32;0
WireConnection;23;0;1;4
WireConnection;34;0;31;0
WireConnection;34;1;35;0
WireConnection;33;0;23;0
WireConnection;33;1;34;0
WireConnection;28;0;26;0
WireConnection;28;1;1;4
WireConnection;24;0;33;0
WireConnection;25;0;24;0
WireConnection;25;1;28;0
WireConnection;21;0;1;4
WireConnection;3;0;1;0
WireConnection;22;0;25;0
WireConnection;22;2;21;0
WireConnection;5;0;3;0
WireConnection;5;1;4;0
WireConnection;5;2;22;0
WireConnection;18;0;12;0
WireConnection;18;1;15;0
WireConnection;7;0;6;0
WireConnection;17;0;16;0
WireConnection;19;0;9;0
WireConnection;15;5;12;0
WireConnection;15;6;17;0
WireConnection;29;29;20;0
WireConnection;29;30;13;0
WireConnection;29;31;14;0
WireConnection;9;2;10;0
WireConnection;9;0;14;0
WireConnection;9;1;13;0
WireConnection;12;0;11;0
WireConnection;12;1;19;0
WireConnection;12;2;20;0
WireConnection;0;0;7;0
WireConnection;0;2;5;0
WireConnection;0;4;8;0
WireConnection;0;11;29;0
ASEEND*/
//CHKSM=FA14EE0E048C415B2679F0C0E3837897E172F74D