// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MonacaAircraft/UV2 VideoScreen"
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset]_BumpMap("Normal", 2D) = "bump" {}
		[NoScaleOffset]_MetallicGlossMap("Metallic", 2D) = "black" {}
		[NoScaleOffset]_OcclusionMap("Ambient Occlusion", 2D) = "white" {}
		[NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,0)
		_VideoTexture("Video Texture", 2D) = "black" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
		};

		uniform sampler2D _BumpMap;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _VideoTexture;
		uniform float4 _VideoTexture_ST;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionColor;
		uniform sampler2D _MetallicGlossMap;
		uniform sampler2D _OcclusionMap;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap4 = i.uv_texcoord;
			o.Normal = UnpackNormal( tex2D( _BumpMap, uv_BumpMap4 ) );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv2_VideoTexture = i.uv2_texcoord2 * _VideoTexture_ST.xy + _VideoTexture_ST.zw;
			float temp_output_17_0 = step( 1.0 , max( uv2_VideoTexture.x , uv2_VideoTexture.y ) );
			float3 lerpResult23 = lerp( (tex2D( _MainTex, uv_MainTex )).rgb , float3( 0,0,0 ) , temp_output_17_0);
			o.Albedo = lerpResult23;
			float2 uv_EmissionMap5 = i.uv_texcoord;
			float3 lerpResult15 = lerp( ( (tex2D( _EmissionMap, uv_EmissionMap5 )).rgb * (_EmissionColor).rgb ) , (tex2D( _VideoTexture, frac( uv2_VideoTexture ) )).rgb , temp_output_17_0);
			o.Emission = lerpResult15;
			float2 uv_MetallicGlossMap10 = i.uv_texcoord;
			float4 tex2DNode10 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap10 );
			o.Metallic = tex2DNode10.r;
			o.Smoothness = tex2DNode10.g;
			float2 uv_OcclusionMap11 = i.uv_texcoord;
			o.Occlusion = tex2D( _OcclusionMap, uv_OcclusionMap11 ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
480;710;1920;1026;1456.314;-645.4557;1;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;16;-1094,1012.856;Inherit;False;1;12;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-512,480;Inherit;False;Property;_EmissionColor;Emission Color;5;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FractNode;24;-841.3143,1024.456;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;5;-512,288;Inherit;True;Property;_EmissionMap;Emission Map;4;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;2;None;d2d987b7efa10c64da720492ff1bf4bf;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-512,1056;Inherit;True;Property;_VideoTexture;Video Texture;6;0;Create;True;0;0;0;False;0;False;-1;None;ee58326bb94c3d04f9c41a47d8a920f6;True;3;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;8;-320,480;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;6;-224,288;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;2;-512,-128;Inherit;True;Property;_MainTex;Albedo;0;0;Create;False;0;0;0;False;0;False;-1;None;cf199e3bcae64314ea4dbf92f8544ea2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;19;-688.0288,1281.086;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;3;-192,-128;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StepOpNode;17;-512,1280;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;14;-224,1056;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-32,288;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;4;-512,80;Inherit;True;Property;_BumpMap;Normal;1;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;2;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-512,672;Inherit;True;Property;_MetallicGlossMap;Metallic;2;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;2;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;15;152.0984,419.3031;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;11;-512,864;Inherit;True;Property;_OcclusionMap;Ambient Occlusion;3;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;2;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;23;263.0069,-119.8777;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;852.5539,-136.8825;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;MonacaAircraft/UV2 VideoScreen;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;24;0;16;0
WireConnection;12;1;24;0
WireConnection;8;0;7;0
WireConnection;6;0;5;0
WireConnection;19;0;16;1
WireConnection;19;1;16;2
WireConnection;3;0;2;0
WireConnection;17;1;19;0
WireConnection;14;0;12;0
WireConnection;9;0;6;0
WireConnection;9;1;8;0
WireConnection;15;0;9;0
WireConnection;15;1;14;0
WireConnection;15;2;17;0
WireConnection;23;0;3;0
WireConnection;23;2;17;0
WireConnection;0;0;23;0
WireConnection;0;1;4;0
WireConnection;0;2;15;0
WireConnection;0;3;10;1
WireConnection;0;4;10;2
WireConnection;0;5;11;1
ASEEND*/
//CHKSM=73C0A2997E536E04FD671AEEC0E1AA66832483F5