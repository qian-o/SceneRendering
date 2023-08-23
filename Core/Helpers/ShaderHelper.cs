namespace Core.Helpers;

public static class ShaderHelper
{
    // mvp.vert
    public const string MVP_PositionAttrib = "position";
    public const string MVP_NormalAttrib = "normal";
    public const string MVP_TexCoordsAttrib = "texCoords";
    public const string MVP_ModelUniform = "model";
    public const string MVP_ViewUniform = "view";
    public const string MVP_ProjectionUniform = "projection";

    // texture.frag
    public const string Texture_TexUniform = "tex";

    // lighting.frag
    public const string Lighting_ViewPosUniform = "viewPos";
    public const string Lighting_MaterialUniform = "material";
    public const string Lighting_DirLightUniform = "dirLight";
    public const string Lighting_PointLightsUniform = "pointLights";
    public const string Lighting_SpotLightUniform = "spotLight";

    // skybox.vert
    public const string Skybox_PositionAttrib = "position";
    public const string Skybox_ViewUniform = "view";
    public const string Skybox_ProjectionUniform = "projection";

    // skybox.frag
    public const string Skybox_SkyboxUniform = "skybox";

    // texture-gaussian_blur.frag
    public const string GaussianBlur_RadiusUniform = "radius";
    public const string GaussianBlur_DeviationUniform = "deviation";
    public const string GaussianBlur_NoiseIntensityUniform = "noiseIntensity";

    // mvp-bone.vert
    public const int Max_Bones = 200;
    public const string Bone_BoneIdsAttrib = "boneIds";
    public const string Bone_WeightsAttrib = "weights";
    public const string Bone_BoneTransformsUniform = "boneTransforms";

    public static string GetMVP_VertexShader()
    {
        return @$"
#version 320 es

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoords;

out vec3 Normal;
out vec3 FragPos;
out vec2 TexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {{
   gl_Position = projection * view * model * vec4(position, 1.0);
   Normal = mat3(transpose(inverse(model))) * normal;
   FragPos = vec3(model * vec4(position, 1.0));
   TexCoords = texCoords;
}}
";
    }

    public static string GetTexture_FragmentShader()
    {
        return @$"
#version 320 es

precision highp float;

in vec2 TexCoords;

out vec4 FragColor;

uniform sampler2D tex;

void main() {{
    FragColor = vec4(texture(tex, TexCoords));
}}
";
    }

    public static string GetLighting_FragmentShader(uint pointLights)
    {
        return @$"
#version 320 es

#define NR_POINT_LIGHTS {pointLights}

precision highp float;

struct Material {{
   sampler2D diffuse;
   sampler2D specular;
   float shininess;
}};

struct DirLight {{
   vec3 direction;

   vec3 ambient;
   vec3 diffuse;
   vec3 specular;
}};

struct PointLight {{
   vec3 position;

   float constant;
   float linear;
   float quadratic;

   vec3 ambient;
   vec3 diffuse;
   vec3 specular;
}};

struct SpotLight {{
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
  
    float constant;
    float linear;
    float quadratic;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;       
}};

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

out vec4 FragColor;

uniform vec3 viewPos;
uniform Material material;
uniform DirLight dirLight;
uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform SpotLight spotLight;

// function prototypes
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main() {{
   // properties
   vec3 norm = normalize(Normal);
   vec3 viewDir = normalize(viewPos - FragPos);

   vec3 result = CalcDirLight(dirLight, norm, viewDir);

   for(int i = 0; i < NR_POINT_LIGHTS; i++) {{
      result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);
   }}

   result += CalcSpotLight(spotLight, norm, FragPos, viewDir);

   FragColor = vec4(result, 1.0);
}}

// calculates the color when using a directional light.
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir) {{
   vec3 lightDir = normalize(-light.direction);

    // diffuse shading
   float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
   vec3 reflectDir = reflect(-lightDir, normal);
   float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    // combine results
   vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
   vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));
   vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));

   return (ambient + diffuse + specular);
}}

// calculates the color when using a point light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {{
   vec3 lightDir = normalize(light.position - fragPos);

    // diffuse shading
   float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
   vec3 reflectDir = reflect(-lightDir, normal);
   float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    // attenuation
   float distance = length(light.position - fragPos);
   float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    

    // combine results
   vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
   vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));
   vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));

   ambient *= attenuation;
   diffuse *= attenuation;
   specular *= attenuation;

   return (ambient + diffuse + specular);
}}

// calculates the color when using a spot light.
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{{
    vec3 lightDir = normalize(light.position - fragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    // attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    // spotlight intensity
    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    // combine results
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}}
";
    }

    public static string GetSkybox_VertexShader()
    {
        return @$"
#version 320 es

layout(location = 0) in vec3 position;

out vec3 TexCoords;

uniform mat4 view;
uniform mat4 projection;

void main() {{
   gl_Position = (projection * view * vec4(position, 1.0)).xyww;
   TexCoords = position;
}}
";
    }

    public static string GetSkybox_FragmentShader()
    {
        return @$"
#version 320 es

precision highp float;

in vec3 TexCoords;

out vec4 FragColor;

uniform samplerCube skybox;

void main() {{
    FragColor = vec4(texture(skybox, TexCoords));
}}
";
    }

    public static string GetGaussianBlur_FragmentShader()
    {
        return @$"
#version 320 es

precision highp float;

in vec2 TexCoords;

out vec4 FragColor;

uniform sampler2D tex;
uniform int radius;
uniform vec4 deviation;
uniform float noiseIntensity;

const float PI = 3.1415926;

float GetWeight(int i);

void main() {{
    if(radius <= 1) {{
        FragColor = vec4(texture(tex, TexCoords));

        return;
    }}

    ivec2 tex_offset_i = textureSize(tex, 0);
    vec2 tex_offset = vec2(float(tex_offset_i.x), float(tex_offset_i.y));

    vec4 result = vec4(0.0);
    float totalWeight = 0.0;

    for(int i = -radius; i <= radius; i++) {{
        for(int j = -radius; j <= radius; j++) {{
            float weight = GetWeight(i);
            totalWeight += weight;

            vec2 offset = vec2(1.0 / tex_offset.x * float(i), 1.0 / tex_offset.y * float(j));

            vec4 noise = vec4(0.0, 0.0, 0.0, 1.0);
            if(noiseIntensity > 0.0) {{
                noise.r = fract(sin(dot(TexCoords + offset, vec2(12.9898, 78.233))) * 43758.5453);
                noise.g = fract(sin(dot(TexCoords + offset, vec2(4.898, 7.23))) * 23421.631);
                noise.b = fract(sin(dot(TexCoords + offset, vec2(53.23, 9.654))) * 12371.421);
            }}
            noise *= noiseIntensity;

            result += (texture(tex, TexCoords + offset) + noise) * weight;
        }}
    }}

    FragColor = result / totalWeight + deviation;
}}

float GetWeight(int i) {{
    float sigma = float(radius) / 3.0;

    return (1.0 / sqrt(2.0 * PI * sigma * sigma)) * exp(-float(i * i) / (2.0 * sigma * sigma));
}}
";
    }

    public static string GetBone_VertexShader()
    {
        return @$"
#version 320 es

#define MAX_Bones {Max_Bones}

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoords;
layout(location = 3) in ivec4 boneIds;
layout(location = 4) in vec4 weights;

out vec3 Normal;
out vec3 FragPos;
out vec2 TexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 boneTransforms[MAX_Bones];

void main() {{
    mat4 boneTransform = boneTransforms[boneIds[0]] * weights[0];
    boneTransform += boneTransforms[boneIds[1]] * weights[1];
    boneTransform += boneTransforms[boneIds[2]] * weights[2];
    boneTransform += boneTransforms[boneIds[3]] * weights[3];

    if(weights[0] == 0.0) {{
        boneTransform = mat4(1.0);
    }}

    gl_Position = projection * view * model * boneTransform * vec4(position, 1.0);
    Normal = mat3(transpose(inverse(model * boneTransform))) * normal;
    FragPos = vec3(model * boneTransform * vec4(position, 1.0));
    TexCoords = texCoords;
}}
";
    }

    public static float GetTotalWeight(int radius)
    {
        float totalWeight = 0.0f;

        float sigma = radius / 3.0f;

        for (int i = 0; i < radius; i++)
        {
            totalWeight += 1.0f / (float)Math.Sqrt(2.0f * Math.PI * sigma * sigma) * (float)Math.Exp(-(float)(i * i) / (2.0f * sigma * sigma));
        }

        return totalWeight;
    }
}
