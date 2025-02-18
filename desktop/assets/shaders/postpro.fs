#version 330

// This shader code is used for interpolating the two main renders of the scene and thus giving a proper transparency effect
// (couldn't manage to setup a depth buffer test, lack of knowledge. ugh.)

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform sampler2D bRender;

out vec4 pixelColor;

void main()
{
    vec4 baseColor = texture(bRender, fragTexCoord);
    vec4 maskColor = texture(texture0, fragTexCoord);

    if (maskColor.rgb != vec3(0)){
        pixelColor = mix(baseColor, maskColor, maskColor.g);
    }
    else{
        pixelColor = baseColor;
    }
}