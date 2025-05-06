#version 330

in vec3 fragPosition;
in vec3 fragNormal;
in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float time;
uniform float closeUpIntensity;
uniform float TAU = 6.283185307;

out vec4 pixelColor;

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);

    vec2 uvsCentered = fragTexCoord * 2 - vec2(1);
    float radialDistance = length(uvsCentered);

    float t = cos((radialDistance - time * 0.2) * TAU * 5) * 0.5 + 0.5;
    t *= (1-radialDistance) * 2;

    pixelColor = mix(vec4(0.0), vec4(mix(1.0, 0.0, closeUpIntensity), 0.0, 0.0, mix(0.65, -0.1, closeUpIntensity)), t);
}