#version 330

// This shader code is used as a default, basic vertex shader

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexTexCoord;

uniform mat4 mvp;

out vec3 fragPosition;
out vec3 fragNormal;
out vec2 fragTexCoord;

void main()
{
	fragTexCoord = vertexTexCoord;
	fragPosition = vertexPosition;
	fragNormal = vertexNormal;

	gl_Position = mvp*vec4(vertexPosition, 1.0);
}