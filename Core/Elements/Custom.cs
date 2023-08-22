using Core.Contracts.Elements;
using Core.Helpers;
using Core.Models;
using Core.Tools;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Runtime.InteropServices;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using CoreMaterial = Core.Models.ShaderStructures.Material;
using CoreMesh = Core.Models.Mesh;
using Program = Core.Tools.Program;

namespace Core.Elements;

public unsafe class Custom : BaseElement
{
    private readonly Assimp _assimp;
    private readonly string _directory;
    private readonly Dictionary<string, Texture2D> _cache;
    private readonly Dictionary<string, BoneData> _boneDatas;

    public override CoreMesh[] Meshes { get; }

    public Custom(GL gl, string path) : base(gl)
    {
        _assimp = Assimp.GetApi();
        _directory = Path.GetDirectoryName(path)!;
        _cache = new();
        _boneDatas = new();

        List<CoreMesh> meshes = new();

        Scene* scene = _assimp.ImportFile(path, (uint)(PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.PreTransformVertices));

        if (path.Contains("fbx"))
        {

        }

        ProcessNode(scene->MRootNode, scene, meshes);

        Meshes = meshes.ToArray();
    }

    public override void Draw(Program program)
    {
        bool anyMaterial = program.GetUniform(ShaderHelper.Lighting_MaterialUniform) >= 0;

        uint position = (uint)program.GetAttrib(ShaderHelper.MVP_PositionAttrib);
        uint normal = (uint)program.GetAttrib(ShaderHelper.MVP_NormalAttrib);
        uint texCoords = (uint)program.GetAttrib(ShaderHelper.MVP_TexCoordsAttrib);

        program.SetUniform(ShaderHelper.MVP_ModelUniform, Transform);

        foreach (CoreMesh mesh in Meshes)
        {
            _gl.ActiveTexture(GLEnum.Texture0);
            mesh.Diffuse2D!.Enable();

            if (anyMaterial)
            {
                _gl.ActiveTexture(GLEnum.Texture1);
                mesh.Specular2D!.Enable();

                CoreMaterial material = new()
                {
                    Diffuse = 0,
                    Specular = 1,
                    Shininess = 64.0f
                };

                program.SetUniform(ShaderHelper.Lighting_MaterialUniform, material);
            }
            else
            {
                program.SetUniform(ShaderHelper.Texture_TexUniform, 0);
            }

            mesh.Draw(position, normal, texCoords);
        }
    }

    private void ProcessNode(Node* node, Scene* scene, List<CoreMesh> meshes)
    {
        for (uint i = 0; i < node->MNumMeshes; i++)
        {
            AssimpMesh* mesh = scene->MMeshes[node->MMeshes[i]];
            meshes.Add(ProcessMesh(mesh, scene));
        }

        for (uint i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], scene, meshes);
        }
    }

    private CoreMesh ProcessMesh(AssimpMesh* mesh, Scene* scene)
    {
        List<Vertex> vertices = new();
        List<uint> indices = new();
        Texture2D? diffuse = null;
        Texture2D? specular = null;

        for (uint i = 0; i < mesh->MNumVertices; i++)
        {
            Vertex vertex = new()
            {
                Position = new Vector3D<float>(mesh->MVertices[i].X, mesh->MVertices[i].Y, mesh->MVertices[i].Z),
                Normal = new Vector3D<float>(mesh->MNormals[i].X, mesh->MNormals[i].Y, mesh->MNormals[i].Z)
            };

            if (mesh->MTextureCoords[0] != null)
            {
                vertex.TexCoords = new Vector2D<float>(mesh->MTextureCoords[0][i].X, mesh->MTextureCoords[0][i].Y);
            }

            vertices.Add(vertex);
        }

        for (uint i = 0; i < mesh->MNumBones; i++)
        {
            Bone* bone = mesh->MBones[i];

            string name = Marshal.PtrToStringAnsi((IntPtr)bone->MName.Data)!;

            if (!_boneDatas.TryGetValue(name, out BoneData boneData))
            {
                boneData = new BoneData(_boneDatas.Count, bone->MOffsetMatrix.Convert<float>());

                _boneDatas.Add(name, boneData);
            }

            for (uint j = 0; j < bone->MNumWeights; j++)
            {
                Vertex vertex = vertices[(int)bone->MWeights[j].MVertexId];

                for (int k = 0; k < 4; k++)
                {
                    if (vertex.BoneWeights[k] == 0.0f)
                    {
                        vertex.BoneIds[k] = boneData.Id;
                        vertex.BoneWeights[k] = bone->MWeights[j].MWeight;

                        break;
                    }
                }

                vertices[(int)bone->MWeights[j].MVertexId] = vertex;
            }
        }

        for (uint i = 0; i < mesh->MNumFaces; i++)
        {
            Face face = mesh->MFaces[i];

            for (uint j = 0; j < face.MNumIndices; j++)
            {
                indices.Add(face.MIndices[j]);
            }
        }

        if (mesh->MMaterialIndex >= 0)
        {
            Material* material = scene->MMaterials[mesh->MMaterialIndex];

            foreach (Texture2D texture in LoadMaterialTextures(material, TextureType.Diffuse))
            {
                diffuse = texture;
            }

            foreach (Texture2D texture in LoadMaterialTextures(material, TextureType.Specular))
            {
                specular = texture;
            }
        }

        if (diffuse == null)
        {
            diffuse = new Texture2D(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);
            diffuse.WriteColor(new Vector3D<byte>(0));
        }

        if (specular == null)
        {
            specular = new Texture2D(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);
            specular.WriteColor(new Vector3D<byte>(0));
        }

        return new CoreMesh(_gl, vertices.ToArray(), indices.ToArray(), diffuse, specular);
    }

    private List<Texture2D> LoadMaterialTextures(Material* mat, TextureType type)
    {
        List<Texture2D> materialTextures = new();

        uint textureCount = _assimp.GetMaterialTextureCount(mat, type);
        for (uint i = 0; i < textureCount; i++)
        {
            AssimpString path;
            _assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);

            if (!_cache.TryGetValue(path.ToString(), out Texture2D? texture))
            {
                texture = new(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);
                texture.WriteImage(Path.Combine(_directory, path.ToString()));

                _cache.Add(path.ToString(), texture);
            }

            materialTextures.Add(texture);
        }

        return materialTextures;
    }
}