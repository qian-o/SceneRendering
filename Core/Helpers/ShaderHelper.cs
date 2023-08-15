namespace Core.Helpers;

public static class ShaderHelper
{
    // mvp.vert
    public const string PositionAttrib = "position";
    public const string NormalAttrib = "normal";
    public const string TexCoordsAttrib = "texCoords";
    public const string ModelUniform = "model";
    public const string ViewUniform = "view";
    public const string ProjectionUniform = "projection";

    // texture.frag
    public const string TexUniform = "tex";

    // lighting.frag
    public const string ViewPosUniform = "viewPos";
    public const string MaterialUniform = "material";
    public const string DirLightUniform = "dirLight";
    public const string PointLightsUniform = "pointLights";
    public const string SpotLightUniform = "spotLight";

    public static string GetModelViewProjectionShader()
    {
        return @$"
#version 320 es

layout(location = 0) in vec3 {PositionAttrib};
layout(location = 1) in vec3 {NormalAttrib};
layout(location = 2) in vec2 {TexCoordsAttrib};

out vec3 Normal;
out vec3 FragPos;
out vec2 TexCoords;

uniform mat4 {ModelUniform};
uniform mat4 {ViewUniform};
uniform mat4 {ProjectionUniform};

void main() {{
   gl_Position = {ProjectionUniform} * {ViewUniform} * {ModelUniform} * vec4({PositionAttrib}, 1.0);
   Normal = mat3(transpose(inverse({ModelUniform}))) * {NormalAttrib};
   FragPos = vec3({ModelUniform} * vec4({PositionAttrib}, 1.0));
   TexCoords = {TexCoordsAttrib};
}}
";
    }

    public static string GetTextureShader()
    {
        return @$"
#version 320 es

precision highp float;

in vec2 TexCoords;

out vec4 FragColor;

uniform sampler2D {TexUniform};

void main() {{
    FragColor = vec4(texture({TexUniform}, TexCoords));
}}
";
    }

    public static string GetLightingShader(int pointLights)
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

uniform vec3 {ViewPosUniform};
uniform Material {MaterialUniform};
uniform DirLight {DirLightUniform};
uniform PointLight {PointLightsUniform}[NR_POINT_LIGHTS];
uniform SpotLight {SpotLightUniform};

// function prototypes
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main() {{
   // properties
   vec3 norm = normalize(Normal);
   vec3 viewDir = normalize({ViewPosUniform} - FragPos);

   vec3 result = CalcDirLight({DirLightUniform}, norm, viewDir);

   for(int i = 0; i < NR_POINT_LIGHTS; i++) {{
      result += CalcPointLight({PointLightsUniform}[i], norm, FragPos, viewDir);
   }}

   result += CalcSpotLight({SpotLightUniform}, norm, FragPos, viewDir);

   FragColor = vec4(result, 1.0);
}}

// calculates the color when using a directional light.
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir) {{
   vec3 lightDir = normalize(-light.direction);

    // diffuse shading
   float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
   vec3 reflectDir = reflect(-lightDir, normal);
   float spec = pow(max(dot(viewDir, reflectDir), 0.0), {MaterialUniform}.shininess);

    // combine results
   vec3 ambient = light.ambient * vec3(texture({MaterialUniform}.diffuse, TexCoords));
   vec3 diffuse = light.diffuse * diff * vec3(texture({MaterialUniform}.diffuse, TexCoords));
   vec3 specular = light.specular * spec * vec3(texture({MaterialUniform}.specular, TexCoords));

   return (ambient + diffuse + specular);
}}

// calculates the color when using a point light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {{
   vec3 lightDir = normalize(light.position - fragPos);

    // diffuse shading
   float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
   vec3 reflectDir = reflect(-lightDir, normal);
   float spec = pow(max(dot(viewDir, reflectDir), 0.0), {MaterialUniform}.shininess);

    // attenuation
   float distance = length(light.position - fragPos);
   float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    

    // combine results
   vec3 ambient = light.ambient * vec3(texture({MaterialUniform}.diffuse, TexCoords));
   vec3 diffuse = light.diffuse * diff * vec3(texture({MaterialUniform}.diffuse, TexCoords));
   vec3 specular = light.specular * spec * vec3(texture({MaterialUniform}.specular, TexCoords));

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
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), {MaterialUniform}.shininess);
    // attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    // spotlight intensity
    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    // combine results
    vec3 ambient = light.ambient * vec3(texture({MaterialUniform}.diffuse, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture({MaterialUniform}.diffuse, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture({MaterialUniform}.specular, TexCoords));
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}}
";
    }
}
