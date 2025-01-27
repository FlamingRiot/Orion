#version 330

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexTexCoord;
in vec3 vertexTangent;

uniform mat4 mvp;
uniform mat4 matModel;

out vec3 fragPosition;
out vec3 fragNormal;
out vec2 fragTexCoord;
out mat3 TBN;

void main()
{
    // Compute binormal from vertex normal and tangent
    vec3 vertexBinormal = cross(vertexNormal, vertexTangent);
    
    // Compute fragment normal based on normal transformations
    mat3 normalMatrix = transpose(inverse(mat3(matModel)));
    
    // Compute fragment position based on model transformations
    fragPosition = vec3(matModel*vec4(vertexPosition, 1.0f));

    fragTexCoord = vertexTexCoord;
    fragNormal = normalize(normalMatrix*vertexNormal);
    vec3 fragTangent = normalize(normalMatrix*vertexTangent);
    fragTangent = normalize(fragTangent - dot(fragTangent, fragNormal)*fragNormal);
    vec3 fragBinormal = normalize(normalMatrix*vertexBinormal);
    fragBinormal = cross(fragNormal, fragTangent);

    TBN = transpose(mat3(fragTangent, fragBinormal, fragNormal));

    // Calculate final vertex position
    gl_Position = mvp*vec4(vertexPosition, 1.0);
}