#version 330

// The following shader code is used for earth-globe UV correction

// Input attributes from vertex shader
in vec3 fragPosition;
in vec3 fragNormal;
in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float time;
uniform vec3 viewPos;
uniform bool closeup = true;

out vec4 pixelColor;

// Main function of the fragment shader program
void main()
{
    // Map UVs
    vec2 uv = vec2(fragTexCoord.y, fragTexCoord.x);

    vec4 texelColor = texture(texture0, uv) * 1.9;

     //Convert to grayscale to simulate the hologram effect
    float grayscale = dot(texelColor.rgb, vec3(0.299, 0.587, 0.114));

    // Add a blue tint to the grayscale image
    vec3 hologramColor = vec3(0.0, 0.5, 1.0) * grayscale;

    //vec3 hologramColor = texelColor.rgb;

    // Add scanline effect
    float scanline = sin(fragTexCoord.x * 800.0) * 0.05; \
    hologramColor += scanline;

     //Add slight distortion for hologram effect
    vec2 distortion = vec2(sin(fragTexCoord.y + time) * 0.0005, 0.0); \
    hologramColor *= texture(texture0, uv + distortion).rgb; \
    
    pixelColor = vec4(hologramColor, texelColor.a);
}