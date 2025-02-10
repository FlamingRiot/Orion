#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform vec2 resolution; // R�solution de l'�cran (doit �tre envoy�e en uniforme)

out vec4 pixelColor;

void main() {
    
    vec4 texelColor = texture(texture0, fragTexCoord);

    pixelColor = texelColor;
}