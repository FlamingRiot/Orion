#version 330

// The following shader code is used for the terminal screen texture

in vec2 fragTexCoord;
in vec3 fragPosition;
in vec3 fragNormal;

uniform sampler2D texture0;
uniform float time;

out vec4 pixelColor;

void main()
{
    // Map UVs
    vec2 uv = vec2(fragTexCoord.y, fragTexCoord.x);

    vec4 texelColor = texture(texture0, -uv);

    // Convert to grayscale to simulate the hologram effect
    float grayscale = dot(texelColor.rgb, vec3(0.299, 0.587, 0.114));

    // Add a blue tint to the grayscale image
    vec3 hologramColor = vec3(0.0, 0.5, 1.0) * grayscale;

    // Add scanline effect
    float scanline = sin(fragTexCoord.x * 600.0 + time * 10.0) * 0.05; 
    hologramColor += scanline;

    // Add slight distortion for hologram effect
    vec2 distortion = vec2(sin(fragTexCoord.y * 5.0 + time) * 0.0005, 0.0); \
    hologramColor *= texture(texture0, -uv + distortion).rgb; \

    // Vignetting
    uv *= 1.0 - uv.yx;
    float vig = uv.x*uv.y * 25.0;
    vig = pow(vig, 0.25);

    pixelColor = vec4(hologramColor, texelColor.a);
    pixelColor *= vig;
}