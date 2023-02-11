// Upgrade NOTE: upgraded instancing buffer 'VirtualCNSContour' to new syntax.

// Made with Amplify Shader Editor v1.9.1.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Virtual-CNS/Contour"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[HDR]_Emission("Emission", Color) = (1,1,1,1)
		_MainTex("Main Tex", 2D) = "white" {}
		_Scale("Scale", Float) = 100
		_Offset("Offset", Range( 0 , 1)) = 0
		_Contrast("Contrast", Range( 0 , 1)) = 0.5
		_Luminance("Luminance", Range( 0 , 1)) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
		
		Stencil
		{
			Ref [_Stencil]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
			Comp [_StencilComp]
			Pass [_StencilOp]
		}


		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		
		Pass
		{
			Name "Default"
		CGPROGRAM
			
			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			#pragma multi_compile_instancing

			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};
			
			uniform fixed4 _Color;
			uniform fixed4 _TextureSampleAdd;
			uniform float4 _ClipRect;
			uniform sampler2D _MainTex;
			uniform float4 _Emission;
			UNITY_INSTANCING_BUFFER_START(VirtualCNSContour)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
#define _MainTex_ST_arr VirtualCNSContour
				UNITY_DEFINE_INSTANCED_PROP(float, _Luminance)
#define _Luminance_arr VirtualCNSContour
				UNITY_DEFINE_INSTANCED_PROP(float, _Contrast)
#define _Contrast_arr VirtualCNSContour
				UNITY_DEFINE_INSTANCED_PROP(float, _Offset)
#define _Offset_arr VirtualCNSContour
				UNITY_DEFINE_INSTANCED_PROP(float, _Scale)
#define _Scale_arr VirtualCNSContour
			UNITY_INSTANCING_BUFFER_END(VirtualCNSContour)

			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID( IN );
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				OUT.worldPosition = IN.vertex;
				
				
				OUT.worldPosition.xyz +=  float3( 0, 0, 0 ) ;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float _Luminance_Instance = UNITY_ACCESS_INSTANCED_PROP(_Luminance_arr, _Luminance);
				float temp_output_85_0 = ( 1.0 - _Luminance_Instance );
				float _Contrast_Instance = UNITY_ACCESS_INSTANCED_PROP(_Contrast_arr, _Contrast);
				float temp_output_81_0 = ( temp_output_85_0 / ( 1.0 - _Contrast_Instance ) );
				float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_MainTex_ST_arr, _MainTex_ST);
				float2 uv_MainTex = IN.texcoord.xy * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
				float4 tex2DNode34 = tex2D( _MainTex, uv_MainTex );
				float _Offset_Instance = UNITY_ACCESS_INSTANCED_PROP(_Offset_arr, _Offset);
				float _Scale_Instance = UNITY_ACCESS_INSTANCED_PROP(_Scale_arr, _Scale);
				float smoothstepResult18 = smoothstep( ( temp_output_85_0 - temp_output_81_0 ) , ( temp_output_85_0 + temp_output_81_0 ) , frac( ( ( abs( ddx( tex2DNode34.r ) ) + abs( ddy( tex2DNode34.r ) ) + _Offset_Instance ) * _Scale_Instance ) ));
				
				half4 color = ( _Emission * smoothstepResult18 );
				
				#ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19103
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-28.70328,-204.7675;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DdxOpNode;66;-1536,-384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;70;-1408,-384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;34;-2176,-384;Inherit;True;Property;_MainTex;Main Tex;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;12;-896,-640;Inherit;False;Property;_Emission;Emission;0;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;0.2,0.2,0.2,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-896,-384;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;18;-317.9461,-57.97352;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;76;222.844,-309.4858;Float;False;True;-1;2;ASEMaterialInspector;0;3;Virtual-CNS/Contour;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;False;True;2;5;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;True;0;True;_ColorMask;False;False;False;False;False;False;False;True;True;0;True;_Stencil;255;True;_StencilReadMask;255;True;_StencilWriteMask;0;True;_StencilComp;0;True;_StencilOp;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;0;True;unity_GUIZTestMode;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.DdyOpNode;68;-1536,-256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;71;-1408,-256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-1280,-384;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;75;-640,-256;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-1280,384;Inherit;False;InstancedProperty;_Luminance;Luminance;5;0;Create;True;0;0;0;False;0;False;0.5;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-1280,480;Inherit;False;InstancedProperty;_Contrast;Contrast;4;0;Create;True;0;0;0;False;0;False;0.5;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;82;-704,240;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;83;-704,144;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;81;-881.7,404.8;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;85;-1001.7,5.473877;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;86;-941.9001,530.6739;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-1573.9,-119.7;Inherit;False;InstancedProperty;_Offset;Offset;3;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-1278.3,-48.7;Inherit;False;InstancedProperty;_Scale;Scale;2;0;Create;True;0;0;0;False;0;False;100;100;0;0;0;1;FLOAT;0
WireConnection;15;0;12;0
WireConnection;15;1;18;0
WireConnection;66;0;34;1
WireConnection;70;0;66;0
WireConnection;73;0;72;0
WireConnection;73;1;74;0
WireConnection;18;0;75;0
WireConnection;18;1;83;0
WireConnection;18;2;82;0
WireConnection;76;0;15;0
WireConnection;68;0;34;1
WireConnection;71;0;68;0
WireConnection;72;0;70;0
WireConnection;72;1;71;0
WireConnection;72;2;77;0
WireConnection;75;0;73;0
WireConnection;82;0;85;0
WireConnection;82;1;81;0
WireConnection;83;0;85;0
WireConnection;83;1;81;0
WireConnection;81;0;85;0
WireConnection;81;1;86;0
WireConnection;85;0;80;0
WireConnection;86;0;79;0
ASEEND*/
//CHKSM=C7EA42F897BA376090754BF3245951F32C8DF0EC