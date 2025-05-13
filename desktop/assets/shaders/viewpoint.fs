#version 330

in vec3 fragPosition;
in vec3 fragNormal;
in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float time;
uniform float TAU = 6.283185307;

out vec4 pixelColor;

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);

    vec2 uvsCentered = fragTexCoord * 2 - vec2(1);
    float radialDistance = length(uvsCentered);

    float t = cos((radialDistance - time * 0.2) * TAU * 5) * 0.5 + 0.5;
    t *= (1-radialDistance) * 2;

    pixelColor = mix(vec4(0.0), vec4(1.0, 0.0, 0.0, 0.65), t);
}