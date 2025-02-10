#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform vec2 resolution; // Résolution de l'écran (doit être envoyée en uniforme)

out vec4 pixelColor;

void main() {
    
    vec4 texelColor = texture(texture0, fragTexCoord);

    pixelColor = texelColor;
}