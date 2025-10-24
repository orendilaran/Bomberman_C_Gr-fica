#version 460

in vec3 f_Position;
in vec3 f_Normal;
in vec2 f_UV;
        
uniform vec3 u_Color;
uniform sampler2D u_Texture;
uniform vec3 u_AmbientColor = vec3(0.1, 0.1, 0.2);
uniform vec3 u_LightDirection = vec3(0, -1, 0);
uniform vec3 u_LightColor = vec3(1.0);
uniform float u_SpecularIntensity = 0.5;
uniform vec3 u_ViewPosition;
                
out vec4 out_Color;

void main() {
    vec4 textureColor = texture(u_Texture, f_UV);

    vec3 normal = normalize(f_Normal);
    float lightValue = max(-dot(u_LightDirection, normal), 0.0);

    vec3 viewDirection = normalize(u_ViewPosition - f_Position);
    vec3 reflectDirection = reflect(u_LightDirection, normal);
    float specularValue = pow(max(dot(viewDirection, reflectDirection), 0.0), 32);

    vec3 ambient = u_AmbientColor;
    vec3 diffuse = lightValue * u_LightColor;
    vec3 specular = specularValue * u_SpecularIntensity * u_LightColor;

    vec3 baseColor = textureColor.rgb * u_Color;
    out_Color = vec4((ambient + diffuse) * baseColor + specular, 1.0);
}
