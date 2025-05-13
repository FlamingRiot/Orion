#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform sampler2D cRender;
uniform vec2 resolution;
uniform float time;

out vec4 pixelColor;

void main()
{
    // Apply chromatic aberration
    float aberrationAmount = 0.05 + abs(0.01 / 8.0);
    vec2 distFromCenter = fragTexCoord - vec2(0.5);

    // Stronger aberration near the deges by raising to power 3
    vec2 aberrated = aberrationAmount * pow(distFromCenter, vec2(3.0, 3.0));

    vec2 uv = clamp(fragTexCoord, vec2(0.0, 0.2), vec2(1.0, 0.8));

    pixelColor = vec4(texture(texture0, uv - aberrated).r, texture(texture0, uv).g, texture(texture0, uv + aberrated).b, 1.0);

    // Add noise
    float noise = fract(sin(dot(uv.xy, vec2(12.9898, 78.233))) * 43758.5453 * time);
    pixelColor.rgb += noise * 0.1;
    // Gamma correction
    pixelColor = pow(pixelColor, vec4(1.2));

    // Manage compass rendering
    vec4 compassColor = texture(cRender, fragTexCoord);
    if (compassColor.rgb != vec3(0)){
        // Apply horizontal fading
        float distToCenter = clamp(abs(fragTexCoord.x - 0.5) * 2.0, 0.0, 0.4); // [0.0 to 1.0]
        float fade = 0.4 - distToCenter;
        compassColor *= fade*3.15;

        pixelColor = mix(pixelColor, compassColor, compassColor.r);
    }
}