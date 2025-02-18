#version 330

// The following shader code is used for the terminal screen texture

in vec2 fragTexCoord;
in vec3 fragPosition;
in vec3 fragNormal;

uniform sampler2D texture0;

out vec4 pixelColor;

void main()
{
    vec2 uv = vec2(fragTexCoord.y, fragTexCoord.x);

    vec4 texelColor = texture(texture0, -uv);

    pixelColor = texelColor;
}