#version 330

// This shader code is used as a default, basic vertex shader

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexTexCoord;

uniform mat4 mvp;
uniform float time;
uniform float TAU = 6.283185307;

out vec3 fragPosition;
out vec3 fragNormal;
out vec2 fragTexCoord;

void main()
{
	fragTexCoord = vertexTexCoord;
	fragPosition = vertexPosition;
	fragNormal = vertexNormal;

	vec2 uvsCentered = fragTexCoord * 2 - vec2(1);
	float radialDistance = length(uvsCentered);

    float t = cos((radialDistance - time * 0.2) * TAU * 5);
    t *= (1-radialDistance);
	

	// float t = cos((vertexTexCoord.y - time * 0.1) * TAU * 5);

	vec3 pos = vec3(vertexPosition.x, t * 0.01, vertexPosition.z);

	gl_Position = mvp*vec4(pos, 1.0);
}