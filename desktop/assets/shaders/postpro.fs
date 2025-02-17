#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform sampler2D bRender;

out vec4 pixelColor;

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);
    vec4 holoColor = texture(bRender, fragTexCoord);
    
    pixelColor = texelColor * holoColor;
}