#version 460
                
layout(location = 0)in vec3 v_Position;
layout(location = 1)in vec3 v_Normal;
layout(location = 2)in vec2 v_UV;

uniform mat4 u_Model;
uniform mat4 u_Rotation;
uniform mat4 u_View;
uniform mat4 u_Projection;

out vec3 f_Position;
out vec3 f_Normal;
out vec2 f_UV;

void main() {
    f_Position = (vec4(v_Position, 1.0) * u_Model).xyz;
    f_Normal = (vec4(v_Normal, 1.0) * u_Rotation).xyz;
    f_UV = v_UV;

    gl_Position = vec4(v_Position, 1.0) * u_Model * u_View * u_Projection;
}
