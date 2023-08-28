using Core.Contracts.Elements;
using Core.Helpers;
using Core.Models;
using Core.Tools;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Runtime.InteropServices;
using AssimpBone = Silk.NET.Assimp.Bone;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using CoreAnimation = Core.Models.Animation;
using CoreMaterial = Core.Models.ShaderStructures.Material;
using CoreMesh = Core.Models.Mesh;
using Program = Core.Tools.Program;

namespace Core.Elements;

public unsafe class Custom : BaseElement
{
    private readonly Assimp _assimp;
    private readonly string _directory;
    private readonly Dictionary<string, Texture2D> _cache;

    public override CoreMesh[] Meshes { get; }

    public Dictionary<string, BoneInfo> BoneMapping { get; } = new Dictionary<string, BoneInfo>();

    public List<CoreAnimation> Animations { get; } = new List<CoreAnimation>();

    public Animator Animator { get; } = new Animator();

    public Custom(GL gl, string path, bool isAnimation = false) : base(gl)
    {
        _assimp = Assimp.GetApi();
        _directory = Path.GetDirectoryName(path)!;
        _cache = new();

        PostProcessSteps flags = PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.FlipUVs;

        if (!isAnimation)
        {
            flags |= PostProcessSteps.PreTransformVertices;
        }

        Scene* scene = _assimp.ImportFile(path, (uint)flags);

        if (scene != null)
        {
            List<CoreMesh> meshes = new();

            ProcessNode(scene->MRootNode, scene, meshes);

            Meshes = meshes.ToArray();

            for (int i = 0; i < scene->MNumAnimations; i++)
            {
                Animations.Add(new CoreAnimation(scene, this, i));
            }
        }
        else
        {
            Meshes = Array.Empty<CoreMesh>();
        }

        _assimp.Dispose();
    }

    public override void Draw(Program program)
    {
        bool anyMaterial = program.GetUniform(ShaderHelper.Lighting_MaterialUniform) >= 0;
        bool anyBones = program.GetUniform(ShaderHelper.Bone_FinalBonesMatricesUniform) >= 0;

        uint position = (uint)program.GetAttrib(ShaderHelper.MVP_PositionAttrib);
        uint normal = (uint)program.GetAttrib(ShaderHelper.MVP_NormalAttrib);
        uint texCoords = (uint)program.GetAttrib(ShaderHelper.MVP_TexCoordsAttrib);
        uint? boneIds = null;
        uint? weights = null;

        program.SetUniform(ShaderHelper.MVP_ModelUniform, Transform);

        if (anyBones)
        {
            boneIds = (uint)program.GetAttrib(ShaderHelper.Bone_BoneIdsAttrib);
            weights = (uint)program.GetAttrib(ShaderHelper.Bone_WeightsAttrib);

            program.SetUniform(ShaderHelper.Bone_FinalBonesMatricesUniform, Animator.FinalBoneMatrices);
        }

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

            mesh.Draw(position, normal, texCoords, boneIds, weights);
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
        Vertex[] vertices = new Vertex[mesh->MNumVertices];
        List<uint> indices = new();
        Texture2D? diffuse = null;
        Texture2D? specular = null;

        for (uint i = 0; i < mesh->MNumVertices; i++)
        {
            Vertex vertex = new()
            {
                Position = mesh->MVertices[i].ToGeneric(),
                Normal = mesh->MNormals[i].ToGeneric()
            };

            if (mesh->MTextureCoords[0] != null)
            {
                vertex.TexCoords = new Vector2D<float>(mesh->MTextureCoords[0][i].X, mesh->MTextureCoords[0][i].Y);
            }

            vertices[i] = vertex;
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
            diffuse.WriteLinearColor(new Color[] { Color.Blue, Color.Red }, new PointF(0.0f, 0.0f), new PointF(1.0f, 1.0f));
        }

        if (specular == null)
        {
            specular = new Texture2D(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);
            specular.WriteColor(new Vector3D<byte>(0));
        }

        ExtractBoneWeightForVertices(vertices, mesh, scene);

        return new CoreMesh(_gl, vertices, indices.ToArray(), diffuse, specular);
    }

    private List<Texture2D> LoadMaterialTextures(Material* mat, TextureType type)
    {
        List<Texture2D> materialTextures = new();

        uint textureCount = _assimp.GetMaterialTextureCount(mat, type);
        for (uint i = 0; i < textureCount; i++)
        {
            AssimpString path;
            _assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);

            if (!_cache.TryGetValue(path.AsString, out Texture2D? texture))
            {
                texture = new(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);
                texture.WriteImage(Path.Combine(_directory, path.ToString()));

                _cache.Add(path.ToString(), texture);
            }

            materialTextures.Add(texture);
        }

        return materialTextures;
    }

    private void ExtractBoneWeightForVertices(Vertex[] vertices, AssimpMesh* mesh, Scene* scene)
    {
        for (uint i = 0; i < mesh->MNumBones; i++)
        {
            AssimpBone* bone = mesh->MBones[i];

            string name = Marshal.PtrToStringAnsi((IntPtr)bone->MName.Data)!;

            if (!BoneMapping.TryGetValue(name, out BoneInfo boneInfo))
            {
                boneInfo = new BoneInfo(BoneMapping.Count, Matrix4X4.Transpose(bone->MOffsetMatrix.ToGeneric()));

                BoneMapping.Add(name, boneInfo);
            }

            for (uint j = 0; j < bone->MNumWeights; j++)
            {
                SetVertexBoneData(ref vertices[(int)bone->MWeights[j].MVertexId], boneInfo.Id, bone->MWeights[j].MWeight);
            }
        }
    }

    private void SetVertexBoneData(ref Vertex vertex, int boneId, float weight)
    {
        for (int i = 0; i < ShaderHelper.MAX_BONE_INFLUENCE; i++)
        {
            if (vertex.BoneIds[i] == -1)
            {
                vertex.BoneIds[i] = boneId;
                vertex.BoneWeights[i] = weight;

                break;
            }
        }
    }
}