#version 330

// The following shader code is used for earth-globe UV correction

// Input attributes from vertex shader
in vec3 fragPosition;
in vec3 fragNormal;
in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform vec3 viewPos;
uniform vec4 lightColor;

out vec4 pixelColor;

// Main function of the fragment shader program
void main()
{
    // Map UVs
    vec2 uv = vec2(fragTexCoord.y, fragTexCoord.x);

    vec4 texelColor = texture(texture0, uv);

    pixelColor = texelColor;
}