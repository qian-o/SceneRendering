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

    public static string GetMVP_VertShader()
    {
        return @$"
#version 320 es

layout(location = 0) in vec3 {MVP_PositionAttrib};
layout(location = 1) in vec3 {MVP_NormalAttrib};
layout(location = 2) in vec2 {MVP_TexCoordsAttrib};

out vec3 Normal;
out vec3 FragPos;
out vec2 TexCoords;

uniform mat4 {MVP_ModelUniform};
uniform mat4 {MVP_ViewUniform};
uniform mat4 {MVP_ProjectionUniform};

void main() {{
   gl_Position = {MVP_ProjectionUniform} * {MVP_ViewUniform} * {MVP_ModelUniform} * vec4({MVP_PositionAttrib}, 1.0);
   Normal = mat3(transpose(inverse({MVP_ModelUniform}))) * {MVP_NormalAttrib};
   FragPos = vec3({MVP_ModelUniform} * vec4({MVP_PositionAttrib}, 1.0));
   TexCoords = {MVP_TexCoordsAttrib};
}}
";
    }

    public static string GetTexture_FragShader()
    {
        return @$"
#version 320 es

precision highp float;

in vec2 TexCoords;

out vec4 FragColor;

uniform sampler2D {Texture_TexUniform};

void main() {{
    FragColor = vec4(texture({Texture_TexUniform}, TexCoords));
}}
";
    }

    public static string GetLighting_FragShader(int pointLights)
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

uniform vec3 {Lighting_ViewPosUniform};
uniform Material {Lighting_MaterialUniform};
uniform DirLight {Lighting_DirLightUniform};
uniform PointLight {Lighting_PointLightsUniform}[NR_POINT_LIGHTS];
uniform SpotLight {Lighting_SpotLightUniform};

// function prototypes
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main() {{
   // properties
   vec3 norm = normalize(Normal);
   vec3 viewDir = normalize({Lighting_ViewPosUniform} - FragPos);

   vec3 result = CalcDirLight({Lighting_DirLightUniform}, norm, viewDir);

   for(int i = 0; i < NR_POINT_LIGHTS; i++) {{
      result += CalcPointLight({Lighting_PointLightsUniform}[i], norm, FragPos, viewDir);
   }}

   result += CalcSpotLight({Lighting_SpotLightUniform}, norm, FragPos, viewDir);

   FragColor = vec4(result, 1.0);
}}

// calculates the color when using a directional light.
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir) {{
   vec3 lightDir = normalize(-light.direction);

    // diffuse shading
   float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
   vec3 reflectDir = reflect(-lightDir, normal);
   float spec = pow(max(dot(viewDir, reflectDir), 0.0), {Lighting_MaterialUniform}.shininess);

    // combine results
   vec3 ambient = light.ambient * vec3(texture({Lighting_MaterialUniform}.diffuse, TexCoords));
   vec3 diffuse = light.diffuse * diff * vec3(texture({Lighting_MaterialUniform}.diffuse, TexCoords));
   vec3 specular = light.specular * spec * vec3(texture({Lighting_MaterialUniform}.specular, TexCoords));

   return (ambient + diffuse + specular);
}}

// calculates the color when using a point light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {{
   vec3 lightDir = normalize(light.position - fragPos);

    // diffuse shading
   float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
   vec3 reflectDir = reflect(-lightDir, normal);
   float spec = pow(max(dot(viewDir, reflectDir), 0.0), {Lighting_MaterialUniform}.shininess);

    // attenuation
   float distance = length(light.position - fragPos);
   float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    

    // combine results
   vec3 ambient = light.ambient * vec3(texture({Lighting_MaterialUniform}.diffuse, TexCoords));
   vec3 diffuse = light.diffuse * diff * vec3(texture({Lighting_MaterialUniform}.diffuse, TexCoords));
   vec3 specular = light.specular * spec * vec3(texture({Lighting_MaterialUniform}.specular, TexCoords));

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
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), {Lighting_MaterialUniform}.shininess);
    // attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    // spotlight intensity
    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    // combine results
    vec3 ambient = light.ambient * vec3(texture({Lighting_MaterialUniform}.diffuse, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture({Lighting_MaterialUniform}.diffuse, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture({Lighting_MaterialUniform}.specular, TexCoords));
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}}
";
    }

    public static string GetSkybox_VertShader()
    {
        return @$"
#version 320 es

layout(location = 0) in vec3 {Skybox_PositionAttrib};

out vec3 TexCoords;

uniform mat4 {Skybox_ViewUniform};
uniform mat4 {Skybox_ProjectionUniform};

void main() {{
   gl_Position = ({Skybox_ProjectionUniform} * {Skybox_ViewUniform} * vec4({Skybox_PositionAttrib}, 1.0));
   TexCoords = {Skybox_PositionAttrib};
}}
";
    }

    public static string GetSkybox_FragShader()
    {
        return @$"
#version 320 es

precision highp float;

in vec3 TexCoords;

out vec4 FragColor;

uniform samplerCube {Skybox_SkyboxUniform};

void main() {{
    FragColor = vec4(texture({Skybox_SkyboxUniform}, TexCoords));
}}
";
    }
}
