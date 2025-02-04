#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform sampler2D prevRender;
uniform float blurAmount;

out vec4 pixelColor;

void main()
{
    // Retrieve color data from renders
    vec4 texelColor = texture(texture0, fragTexCoord);
    vec4 prevColor = texture(prevRender, fragTexCoord);

    // Define blur weight
    float currentWeight = 1.0 / (blurAmount + 1.0);
    float previousWeight = blurAmount / (blurAmount + 1.0);

    // Define blurred color
    vec4 blur = (currentWeight * texelColor) + (previousWeight * prevColor);

    pixelColor = blur;
}