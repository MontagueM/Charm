using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tiger.Schema;
using Tiger.Schema.Entity;
using static Tiger.Exporters.MetadataScene;

namespace Tiger.Exporters;

public class GlobalExporter : AbstractExporter
{
    private GlobalExporterScene GlobalScene => Exporter.Get().GetGlobalScene();
    private ConfigSubsystem _config => ConfigSubsystem.Get();
    private string SavePath;

    public override void Export(Exporter.ExportEventArgs args)
    {
        SavePath = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, $"Maps");

        ExportAtmosphere();
        ExportLensFlares();
        ExportCubemaps();
        ExportLights();
        ExportDecals();
        ExportGlobalChannels();
    }

    private void ExportAtmosphere()
    {
        if (Exporter.Get().GetOrCreateGlobalScene().TryGetItem<SMapAtmosphere>(out SMapAtmosphere atmosphere))
        {
            List<Texture> AtmosTextures = new();
            if (atmosphere.Lookup0 != null)
                AtmosTextures.Add(atmosphere.Lookup0);
            if (atmosphere.Lookup1 != null)
                AtmosTextures.Add(atmosphere.Lookup1);
            if (atmosphere.Lookup2 != null)
                AtmosTextures.Add(atmosphere.Lookup2);
            if (atmosphere.Lookup3 != null)
                AtmosTextures.Add(atmosphere.Lookup3);
            if (atmosphere.Lookup4 != null)
                AtmosTextures.Add(atmosphere.Lookup4);

            //foreach (var tex in atmosphere.D1Lookup)
            //{
            //    tex.Load();
            //    AtmosTextures.Add(tex);
            //}

            string texSavePath = $"{SavePath}/Textures/Atmosphere";
            Directory.CreateDirectory(texSavePath);

            foreach (Texture tex in AtmosTextures)
            {
                // Not ideal but it works
                TextureExtractor.SaveTextureToFile($"{texSavePath}/{tex.Hash}", tex.IsVolume() ? Texture.FlattenVolume(tex.GetScratchImage(true)) : tex.GetScratchImage());
                if (_config.GetS2ShaderExportEnabled())
                    Source2Handler.SaveVTEX(tex, $"{texSavePath}", "Atmosphere");
            }

            string dataSavePath = $"{SavePath}/Rendering";
            Directory.CreateDirectory(dataSavePath);

            AtmosphereData data = new()
            {
                Texture0 = atmosphere.Lookup0?.Hash ?? "",
                Texture1 = atmosphere.Lookup1?.Hash ?? "",
                Texture2 = atmosphere.Lookup2?.Hash ?? "",
                Texture3 = atmosphere.Lookup3?.Hash ?? "",
                Texture4 = atmosphere.Lookup4?.Hash ?? "",

                UnkD8 = atmosphere.UnkD8,
                UnkE8 = atmosphere.UnkE8,
                UnkF8 = atmosphere.UnkF8,
                Unk108 = atmosphere.Unk108,
            };

            if (Exporter.Get().GetOrCreateGlobalScene().TryGetItem<S716A8080>(out S716A8080 dayCycle))
            {
                data.DayCycle = new()
                {
                    Seconds = dayCycle.Unk14,
                    Unk1 = dayCycle.Unk18,
                };

                if (dayCycle.Unk10 is not null && dayCycle.Unk10.TagData.Unk10 is not null)
                {
                    Tag<SC88A8080> cycles = dayCycle.Unk10.TagData.Unk10;
                    data.DayCycle.Unk2 = cycles.TagData.Unk08;
                    data.DayCycle.Unk3 = cycles.TagData.Unk0C;
                    data.DayCycle.Rotations = cycles.TagData.Unk30.Enumerate(cycles.GetReader()).Select(x => x.Vec).ToList();
                }
            }

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
            File.WriteAllText($"{dataSavePath}/Atmosphere.json", JsonConvert.SerializeObject(data, jsonSettings));
        }
    }

    private void ExportLensFlares()
    {
        if (GlobalScene.Any<LensFlare>())
        {
            ConcurrentDictionary<FileHash, LensFlareData> data = new();
            string dataSavePath = $"{SavePath}/Rendering";
            Directory.CreateDirectory(dataSavePath);

            foreach (LensFlare lensFlare in GlobalScene.GetAllOfType<LensFlare>())
            {
                LensFlareData lensFlareData = data.GetOrAdd(lensFlare.Hash, _ => new LensFlareData
                {
                    Instances = new(),
                    Materials = lensFlare.Materials.Select(x => x.ToString()).ToList()
                });

                lensFlareData.Instances.Add(new JsonInstance
                {
                    Translation = new[] { lensFlare.Transform.Translation.X, lensFlare.Transform.Translation.Y, lensFlare.Transform.Translation.Z },
                    Rotation = new[] { lensFlare.Transform.Rotation.X, lensFlare.Transform.Rotation.Y, lensFlare.Transform.Rotation.Z, lensFlare.Transform.Rotation.W },
                    Scale = new[] { lensFlare.Transform.Translation.W, lensFlare.Transform.Translation.W, lensFlare.Transform.Translation.W }
                });
            }

            File.WriteAllText($"{dataSavePath}/LensFlares.json", JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }

    private void ExportCubemaps()
    {
        if (GlobalScene.Any<Cubemap>())
        {
            ConcurrentDictionary<string, CubemapData> data = new();
            string dataSavePath = $"{SavePath}/Rendering";
            Directory.CreateDirectory(dataSavePath);

            List<Texture> textures = new();
            string texSavePath = $"{SavePath}/Textures/Cubemaps";
            Directory.CreateDirectory(texSavePath);

            foreach (Cubemap cubemapEntry in GlobalScene.GetAllOfType<Cubemap>())
            {
                Schema.Entity.SMapCubemapResource cubemap = cubemapEntry.CubemapEntry;
                string name = cubemap.CubemapName != null ? cubemap.CubemapName.Value : $"Cubemap_{cubemap.WorldID:X}";
                _ = data.GetOrAdd(name, _ => new CubemapData
                {
                    Transform = new JsonInstance
                    {
                        Translation = new[] { cubemap.CubemapPosition.X, cubemap.CubemapPosition.Y, cubemap.CubemapPosition.Z },
                        Rotation = new[] { cubemap.CubemapRotation.X, cubemap.CubemapRotation.Y, cubemap.CubemapRotation.Z, cubemap.CubemapRotation.W },
                        Scale = new[] { cubemap.CubemapSize.X, cubemap.CubemapSize.Y, cubemap.CubemapSize.Z, }
                    },
                    CubemapTexture = cubemap.CubemapTexture != null ? cubemap.CubemapTexture.Hash : "",
                    CubemapIBLTexture = cubemap.CubemapIBLTexture != null ? cubemap.CubemapIBLTexture.Hash : "",
                });

                if (cubemap.CubemapTexture != null)
                    textures.Add(cubemap.CubemapTexture);
                if (cubemap.CubemapIBLTexture != null)
                    textures.Add(cubemap.CubemapIBLTexture);
            }

            foreach (Texture tex in textures)
            {
                if (tex is null)
                    continue;

                TextureExtractor.SaveTextureToFile($"{texSavePath}/{tex.Hash}", tex.IsVolume() ? Texture.FlattenVolume(tex.GetScratchImage(true)) : Texture.FlattenCubemap(tex.GetScratchImage()));
                if (_config.GetS2ShaderExportEnabled())
                    Source2Handler.SaveVTEX(tex, $"{texSavePath}", "Cubemaps");
            }

            File.WriteAllText($"{dataSavePath}/Cubemaps.json", JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }

    private void ExportLights()
    {
        if (GlobalScene.Any<Lights.LightData>())
        {
            ConcurrentDictionary<string, LightData> data = new();
            string dataSavePath = $"{SavePath}/Rendering";
            Directory.CreateDirectory(dataSavePath);

            List<Texture> textures = new();
            string texSavePath = $"{SavePath}/Textures/Lights";
            Directory.CreateDirectory(texSavePath);

            foreach (Lights.LightData light in GlobalScene.GetAllOfType<Lights.LightData>())
            {
                // this is so stupid...but ensures theres no accidental overwrites with mismatches
                uint hash = Helpers.Fnv($"{light.LightType}{light.Material}{light.Hash}");
                LightData lightData = data.GetOrAdd($"{light.LightType}_{hash.ToString("X")}", _ => new LightData
                {
                    Type = light.LightType.ToString(),
                    Instances = new(),
                    Color = new[] { light.Color.X, light.Color.Y, light.Color.Z, light.Color.W },
                    Attenuation = light.Attenuation,
                    Cookie = light.Cookie ?? "",
                    Bytecode = light.Bytecode.ToList(),
                    BytecodeConstants = light.BytecodeConstants,
                    Hashes = new[] { $"{light.Hash}", $"{light.Material}" }
                });

                lightData.Instances.Add(new JsonInstance
                {
                    Translation = new[] { light.Transform.Position.X, light.Transform.Position.Y, light.Transform.Position.Z },
                    Rotation = new[] { light.Transform.Quaternion.X, light.Transform.Quaternion.Y, light.Transform.Quaternion.Z, light.Transform.Quaternion.W },
                    Scale = new[] { light.Size.X, light.Size.Y, light.Size.Z },
                });

                if (light.Cookie != null)
                    textures.Add(new Texture(light.Cookie));
            }

            foreach (Texture tex in textures)
            {
                if (tex is null)
                    continue;

                tex.SavetoFile($"{texSavePath}/{tex.Hash}");
                if (_config.GetS2ShaderExportEnabled())
                    Source2Handler.SaveVTEX(tex, $"{texSavePath}", "Lights");
            }
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
            File.WriteAllText($"{dataSavePath}/Lights.json", JsonConvert.SerializeObject(data, jsonSettings));
        }
    }

    private void ExportDecals()
    {
        if (GlobalScene.Any<Decals>())
        {
            ConcurrentDictionary<FileHash, DecalsData> data = new();
            string dataSavePath = $"{SavePath}/Rendering";
            Directory.CreateDirectory(dataSavePath);

            foreach (Decals decal in GlobalScene.GetAllOfType<Decals>())
            {
                List<Transform> transforms = decal.GetTransforms();
                foreach (S63698080 instance in decal.TagData.DecalResources.Enumerate(decal.GetReader()))
                {
                    for (int i = instance.StartIndex; i < instance.StartIndex + instance.Count; i++)
                    {
                        Vector3 loc = transforms[i].Position;
                        Vector4 rot = transforms[i].Quaternion;
                        Vector3 scale = transforms[i].Scale;

                        DecalsData decalData = data.GetOrAdd(instance.Material.Hash, _ => new DecalsData
                        {
                            Instances = new(),
                        });

                        decalData.Instances.Add(new JsonInstance
                        {
                            Translation = new[] { loc.X, loc.Y, loc.Z },
                            Rotation = new[] { rot.X, rot.Y, rot.Z, rot.W },
                            Scale = new[] { scale.X, scale.Y, scale.Z }
                        });
                    }
                }
            }

            File.WriteAllText($"{dataSavePath}/Decals.json", JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }

    private void ExportGlobalChannels()
    {
        if (GlobalScene.Any<EntityResource>())
        {
            Dictionary<string, GlobalChannelData> channels = new();
            string dataSavePath = $"{SavePath}/Rendering";
            Directory.CreateDirectory(dataSavePath);

            var defaults = Globals.Get().GlobalChannelDefaults;

            // Just gonna export all channels
            foreach (var defaultChannel in defaults)
            {
                channels.TryAdd(defaultChannel.Key, new GlobalChannelData
                {
                    Name = GlobalChannels.KnownChannelNames.TryGetValue(defaultChannel.Key.Hash32, out string name) ? name : defaultChannel.Key.ToString(),
                    Index = defaults.Keys.ToList().IndexOf(defaultChannel.Key),
                    Bytecode = new List<byte>(), // No bytecode for defaults
                    Constants = new List<Vector4> { defaultChannel.Value } // Default value
                });
            }

            // Overwrites defaults with any from the scene
            foreach (EntityResource resource in GlobalScene.GetAllOfType<EntityResource>())
            {
                switch (resource.TagData.Unk10.GetValue(resource.GetReader()))
                {
                    case S79948080:
                        var globals = ((S79818080)resource.TagData.Unk18.GetValue(resource.GetReader()));
                        DynamicArray<SF1918080> map = globals.Array1;
                        map.AddRange(globals.Array2);

                        foreach (SF1918080 entry in map)
                        {
                            if (entry.Unk10.GetValue(resource.GetReader()) is SD1918080 global)
                            {
                                var id = globals.Array3[global.ChannelIndex].ID;

                                if (channels.ContainsKey(id))
                                {
                                    channels[id] = new GlobalChannelData
                                    {
                                        Name = GlobalChannels.KnownChannelNames.TryGetValue(id.Hash32, out string name) ? name : id.ToString(),
                                        Index = Globals.Get().GlobalChannelDefaults.Keys.ToList().IndexOf(id),
                                        Bytecode = global.UnkBytecode.Select(x => x.Value).ToList(),
                                        Constants = global.Values.Select(x => x.Vec).ToList()
                                    };
                                }
                                else
                                {
                                    // Idk what to do with these, so just gonna skip for now
                                }
                            }
                            else if (entry.Unk10.GetValue(resource.GetReader()) is SCF918080 lut)
                            {
                                if (lut.Unk28 is null || lut.Unk28.TagData.LUT is null || channels.ContainsKey("LUT"))
                                    continue;

                                Directory.CreateDirectory($"{SavePath}/Textures/LUT");
                                lut.Unk28.TagData.LUT.SavetoFile($"{SavePath}/Textures/LUT/{lut.Unk28.TagData.LUT.Hash}");
                                Source2Handler.SaveVTEX(lut.Unk28.TagData.LUT, $"{SavePath}/Textures/LUT/", "LUT");

                                // TODO move out of global channels when theres more general stuff to export
                                channels.TryAdd("LUT", new GlobalChannelData
                                {
                                    Name = $"{lut.Unk28.TagData.LUT.Hash}",
                                    Index = -1
                                });
                            }
                        }
                        break;
                }

                //System.Console.WriteLine($"\t-{globals.Unk00}: Index {globals.ChannelIndex}");

                //TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(globals.UnkBytecode));
                //System.Console.WriteLine($"\t-Bytecode ops ({bytecode.Opcodes.Count})");
                //foreach (var op in bytecode.Opcodes)
                //{
                //    System.Console.WriteLine($"\t\t--{op.op}");
                //}
                //System.Console.WriteLine($"\t-Values ({globals.Values.Count})");
                //foreach (var value in globals.Values)
                //{
                //    System.Console.WriteLine($"\t\t--{value.Vec.ToString()}");
                //}
            }

            File.WriteAllText($"{dataSavePath}/GlobalChannels.json", JsonConvert.SerializeObject(channels, Formatting.Indented));
        }
    }

    private struct AtmosphereData
    {
        public string Texture0;
        public string Texture1;
        public string Texture2;
        public string Texture3;
        public string Texture4;

        public Vector4 UnkD8;
        public Vector4 UnkE8;
        public Vector4 UnkF8;
        public Vector4 Unk108;

        public DayCycleData DayCycle;
    }

    private struct DayCycleData
    {
        public float Seconds;
        public float Unk1;
        public float Unk2;
        public float Unk3;
        public List<Vector4> Rotations;
    }

    private struct DecalsData
    {
        public List<JsonInstance> Instances;
    }

    private struct LensFlareData
    {
        public List<JsonInstance> Instances;
        public List<string> Materials;
    }

    private struct CubemapData
    {
        public JsonInstance Transform;
        public string CubemapTexture;
        public string CubemapIBLTexture;
    }

    private struct LightData
    {
        public string Type;
        public List<JsonInstance> Instances;
        public float[] Color;
        public float Attenuation;
        public string Cookie;
        public List<byte> Bytecode;
        public Vector4[] BytecodeConstants;
        public string[] Hashes; // Stupid but could be useful for debugging/finding if needed
    }

    private struct GlobalChannelData
    {
        public string Name;
        public int Index;
        public List<byte> Bytecode;
        public List<Vector4> Constants;
    }
}
