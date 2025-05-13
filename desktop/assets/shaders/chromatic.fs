#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform sampler2D cRender;
uniform vec2 resolution;

out vec4 pixelColor;

void main()
{
    // Apply chromatic aberration
    float aberrationAmount = 0.05 + abs(0.01 / 8.0);
    vec2 distFromCenter = fragTexCoord - vec2(0.5);

    // Stronger aberration near the deges by raising to power 3
    vec2 aberrated = aberrationAmount * pow(distFromCenter, vec2(7, 3.0));

    pixelColor = vec4(texture(texture0, fragTexCoord - aberrated).r, texture(texture0, fragTexCoord).g, texture(texture0, fragTexCoord + aberrated).b, 1.0);

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