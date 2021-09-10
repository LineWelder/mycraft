using OpenGL;
using System;

namespace Mycraft.Graphics
{
    public class TexturedShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 textureCoords;
uniform mat4 mvp;
out vec2 texCoords;

void main()
{
    texCoords = textureCoords;
    gl_Position = mvp * vec4(position, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform sampler2D tex;
in vec2 texCoords;

void main()
{
    gl_FragColor = texture(tex, texCoords);
}";

        public readonly int mvpLocation;
        public readonly int textureLocation;

        public TexturedShader()
            : base(VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            textureLocation = Gl.GetUniformLocation(glId, "tex");
            if (textureLocation < 0)
                throw new InvalidOperationException("tex variable not found");

            mvpLocation = Gl.GetUniformLocation(glId, "mvp");
            if (mvpLocation < 0)
                throw new InvalidOperationException("mvp variable not found");
        }
    }
}
