#version 330 core

in vec2 frag_texCoords;
out vec4 out_color;

uniform sampler2D uTexture;

void main()
{
    // out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0, 1.0);
    out_color = texture(uTexture, frag_texCoords);
}