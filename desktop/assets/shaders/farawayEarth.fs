#version 330

in vec3 fragPosition;
in vec3 fragNormal;
in vec2 fragTexCoord;

uniform sampler2D texture0;

out vec4 fragColor;

vec4 lighting()
{
    vec4 ambient = vec4(0.2);
    vec3 viewPos = vec3(0);
    vec3 lightColor = vec3(1, 0.914, 0.482);

    vec3 view = normalize(viewPos - fragPosition);
    vec3 light = normalize(-fragPosition);

    float incidentAngle = max(dot(fragNormal, light), 0.0);
    vec4 diffuse = vec4(lightColor * incidentAngle * 2, 1.0);

    return mix(-ambient, diffuse, incidentAngle);
}

void main()
{
    vec2 uv = vec2(fragTexCoord.y, fragTexCoord.x);
    vec4 texelColor = texture(texture0, uv);

    fragColor = mix(lighting(), texelColor, 1);
}