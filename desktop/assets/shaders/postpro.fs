#version 330

// This shader code is used for interpolating the two main renders of the scene and thus giving a proper transparency effect
// (couldn't manage to setup a depth buffer test, lack of knowledge. ugh.)

in vec2 fragTexCoord;

uniform float closeUpIntensity; // Defines the mix between hologram color and texture-albedo color
uniform sampler2D texture0;
uniform sampler2D bRender;
uniform sampler2D cRender;
uniform vec2 resolution; // Screen resolution

out vec4 pixelColor;

void main()
{
    vec4 compassColor = texture(cRender, fragTexCoord);
    vec4 baseColor = texture(bRender, fragTexCoord);
    vec4 maskColor = texture(texture0, fragTexCoord);

    if (maskColor.rgb != vec3(0)){
        vec4 mixed = mix(baseColor, maskColor, maskColor.g * 3);
        pixelColor = vec4(maskColor.r, mixed.g, mixed.b * 1.2, 1.0);
    }
    else{
        pixelColor = baseColor;
    }

    pixelColor = mix(pixelColor, maskColor, closeUpIntensity + 0.05);

    if (compassColor.rgb != vec3(0)){
        // Apply horizontal fading
        float distToCenter = clamp(abs(fragTexCoord.x - 0.5) * 2.0, 0.0, 0.4); // [0.0 to 1.0]
        float fade = 0.4 - distToCenter;
        compassColor *= fade*3.15;

        pixelColor = mix(pixelColor, compassColor, compassColor.r);
    }
}