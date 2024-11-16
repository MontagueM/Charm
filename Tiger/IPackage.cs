using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Arithmic;
using ConcurrentCollections;

namespace Tiger;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "029D8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "029D8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C29E8080", 0x10)]
[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct SHash64Definition
{
    public ulong Hash64;
    public uint Hash32;
    public uint TagClass;
}

public interface IPackageHeader
{
    public ulong GetPackageGroup();
    public ushort GetPackageId();
    public uint GetTimestamp();
    public ushort GetPatchId();
    public uint GetFileCount();
    public List<D2FileEntry> GetFileEntries(TigerReader reader);
    public List<D2BlockEntry> GetBlockEntries(TigerReader reader);
    public List<SHash64Definition> GetHash64Definitions(TigerReader reader);

    public List<PackageActivityEntry> GetAllActivities(TigerReader reader);
}

public struct FileView
{
    public byte[] Data;
    public FileMetadata Metadata;
}

public interface IPackage
{
    PackageMetadata GetPackageMetadata();

    FileMetadata GetFileMetadata(ushort fileId);
    FileMetadata GetFileMetadata(FileHash fileId);
    List<FileMetadata> GetAllFileMetadata();
    public HashSet<FileView> GetAllFileData();

    // todo change these systems to be async producer/consumer, and do some smart calculations based on the queue
    // identify tasks that are sequential/from the same block so we can do all the processing in one go
    byte[] GetFileBytes(ushort fileIndex);
    Span<byte> GetFileSpan(ushort fileIndex);
    byte[] GetFileBytes(FileHash fileHash);
    HashSet<int> GetRequiredPatches();
    List<SHash64Definition> GetHash64List();

    public List<PackageActivityEntry> GetAllActivities();
}

public struct PackageActivityEntry
{
    public FileHash TagHash;
    public TagClassHash TagClassHash;
    public string Name;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "EC9E8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C59E8080", 0x10)]
public struct SD2PackageActivityEntry
{
    public FileHash TagHash;
    public TagClassHash TagClassHash;
    public StringPointer Name;
}

public abstract class Package : IPackage
{
    private static string PackageExtension = ".pkg";
    private List<D2FileEntry> FileEntries;
    private List<D2BlockEntry> BlockEntries;
    protected const int BlockSize = 0x40_000;
    public string PackagePath { get; }
    protected IPackageHeader Header;
    protected TigerStrategy PackageStrategy;

    private static readonly byte[] AesKey0 = { 0xD6, 0x2A, 0xB2, 0xC1, 0x0C, 0xC0, 0x1B, 0xC5, 0x35, 0xDB, 0x7B, 0x86, 0x55, 0xC7, 0xDC,
        0x3B };

    private static readonly byte[] AesKey1 = { 0x3A, 0x4A, 0x5D, 0x36, 0x73, 0xA6, 0x60, 0x58, 0x7E, 0x63, 0xE6, 0x76, 0xE4, 0x08, 0x92,
        0xB5 };

    public HashSet<int> GetRequiredPatches()
    {
        HashSet<int> requiredPatches = new HashSet<int>();
        foreach (D2FileEntry fileEntry in FileEntries)
        {
            int blockCount = GetBlockCount(fileEntry);
            List<D2BlockEntry> blocks = GetBlockEntries(fileEntry.StartingBlockIndex, blockCount);
            foreach (D2BlockEntry blockEntry in blocks)
            {
                requiredPatches.Add(blockEntry.PatchId);
            }
        }

        return requiredPatches;
    }

    public List<SHash64Definition> GetHash64List()
    {
        using TigerReader reader = GetReader();
        return Header.GetHash64Definitions(reader);
    }

    protected Package(string packagePath, TigerStrategy strategy)
    {
        PackageStrategy = strategy;
        CheckValidPackagePath(packagePath, PackageStrategy);
        PackagePath = SanitisePackagePath(packagePath);
        Initialise();
    }

    private void Initialise()
    {
        using TigerReader reader = GetReader();
        GetReader();
        ReadHeader(reader);
        FileEntries = Header.GetFileEntries(reader);
        BlockEntries = Header.GetBlockEntries(reader);
    }

    private static string SanitisePackagePath(string packagePath) { return packagePath.Replace("\\", "/"); }

    protected abstract void ReadHeader(TigerReader reader);

    public List<T> GetAllFiles<T>() where T : TigerFile
    {
        ConcurrentBag<T> tags = new();


        Type type = typeof(T);
        if (type.IsGenericType)
        {
            type = type.BaseType.GenericTypeArguments[0];
        }

        SchemaDeserializer.Get().TryGetSchemaTypeIdentifier(type, out TypeIdentifier typeIdentifier);

        if (typeIdentifier.HasClassHash())
        {
            Parallel.For(0, FileEntries.Count, i =>
            {
                if (FileEntries[i].Reference.Hash32 != typeIdentifier.ClassHash)
                {
                    return;
                }

                T tag = FileResourcer.Get().GetFile<T>(new FileHash(Header.GetPackageId(), (uint)i));
                tags.Add(tag);
            });
        }
        else
        {
            if (typeIdentifier.HasTypeSubType())
            {
                Parallel.For(0, FileEntries.Count, i =>
                {
                    if (FileEntries[i].NumType != typeIdentifier.Type ||
                        !typeIdentifier.SubTypes.Contains(FileEntries[i].NumSubType))
                    {
                        return;
                    }

                    T tag = FileResourcer.Get().GetFile<T>(new FileHash(Header.GetPackageId(), (uint)i));
                    tags.Add(tag);
                });
            }
            else
            {
                Log.Warning($"Tried to get all files of type {typeof(T).Name} but it has no schema or non-schema attribute.");
            }
        }


        return tags.ToList();
    }

    public List<TigerFile> GetAllFiles(Type fileType)
    {
        List<TigerFile> tags = new();

        SchemaStructAttribute attribute = GetAttribute<SchemaStructAttribute>(fileType.BaseType.GenericTypeArguments[0]);
        TigerHash referenceHash = new(attribute.ClassHash);

        Parallel.For(0, FileEntries.Count, i =>
        {
            if (!FileEntries[i].Reference.Equals(referenceHash))
            {
                return;
            }

            TigerFile tag = FileResourcer.Get().GetFile(fileType, new FileHash(Header.GetPackageId(), (uint)i));
            tags.Add(tag);
        });

        return tags;
    }

    // public HashSet<FileHash> GetAllHashesAsync<T>()
    // {
    //     return GetAllHashesAsync(typeof(T));
    // }

    public HashSet<FileHash> GetAllHashes()
    {
        ConcurrentBag<FileHash> hashes = new();
        Parallel.For(0, FileEntries.Count, i =>
        {
            hashes.Add(new FileHash(Header.GetPackageId(), (uint)i));
        });

        return hashes.ToHashSet();
    }

    public HashSet<FileHash> GetAllHashes(Type schemaType)
    {
        if (!SchemaDeserializer.Get().TryGetSchemaTypeIdentifier(schemaType, out TypeIdentifier typeIdentifier))
        {
            throw new ArgumentException($"Type {schemaType} is not a schema type so cannot get hashes for it");
        }

        ConcurrentBag<FileHash> hashes = new();
        Parallel.For(0, FileEntries.Count, i =>
        {
            if ((FileEntries[i].Reference.Hash32 == typeIdentifier.ClassHash && (typeIdentifier.ClassHash != TigerHash.InvalidHash32)) || (FileEntries[i].NumType == typeIdentifier.Type && typeIdentifier.SubTypes.Contains(FileEntries[i].NumSubType)))
            {
                hashes.Add(new FileHash(Header.GetPackageId(), (uint)i));
            }
        });

        return hashes.ToHashSet();
    }

    /// <summary>
    /// Since we know we need every file, we can figure out what blocks to extract and read them into files
    /// Will read blocks serially, but after the block reads we can do it multithreaded.
    /// </summary>
    /// <returns></returns>
    // public HashSet<FileView> GetAllFileData()
    // {
    //     var a = FileEntries.Select(e => e.StartingBlockIndex).Distinct().Select(i => new KeyValuePair<int, ConcurrentHashSet<(ushort, int)>>(i, new ConcurrentHashSet<(ushort, int)>()));
    //     // Maps a block index -> a set of entry index offsets to split into separate files
    //     ConcurrentDictionary<int, ConcurrentHashSet<(ushort, int)>> blockEntryMap = new(a);
    //     Parallel.For(0, FileEntries.Count, entryIndex =>
    //     {
    //         blockEntryMap[FileEntries[entryIndex].StartingBlockIndex].Add(((ushort)entryIndex, FileEntries[entryIndex].StartingBlockOffset));
    //     });
    //
    //     // Span<byte> test = stackalloc byte[100];
    //
    //     // todo maybe sorting the blockOffsets can reduce random access times?
    //
    //     // at most there can be 1 file that stretches across block boundaries
    //
    //     ConcurrentDictionary<ushort, FileView> fileViews = new();
    //     // int crossBlockSizeAlreadyRead = -1;
    //     // short crossBlockFileIndex = -1;
    //     // byte[] crossBlockData = default;
    //     HashSet<int> unseenBlockIndices = Enumerable.Range(0, BlockEntries.Count).ToHashSet();
    //     foreach ((int blockIndex, ConcurrentHashSet<(ushort, int)> blockOffsets) in blockEntryMap)
    //     {
    //         unseenBlockIndices.Remove(blockIndex);
    //         Span<byte> finalBytes;
    //         D2BlockEntry blockEntry = BlockEntries[blockIndex];
    //         using (TigerReader packageHandle = GetPackageHandle(blockEntry.PatchId))
    //         {
    //             finalBytes = ReadBlockBufferSpan(packageHandle, blockEntry);
    //         }
    //
    //         // if (crossBlockFileIndex != -1)
    //         // {
    //         //     // todo this doesnt work for multi-cross-blocks rn
    //         //     FileMetadata fileMetadata = GetFileMetadata((ushort)crossBlockFileIndex);
    //         //     if (fileMetadata.Size - crossBlockSizeAlreadyRead > BlockSize)
    //         //     {
    //         //         Array.Copy(finalBytes.ToArray(), 0, crossBlockData, crossBlockSizeAlreadyRead, BlockSize);
    //         //         crossBlockSizeAlreadyRead += BlockSize;
    //         //         continue;
    //         //     }
    //         //     Span<byte> secondBlockData = finalBytes.Slice(0, fileMetadata.Size - crossBlockSizeAlreadyRead);
    //         //     byte[] crossBlockFinalBytes = new byte[fileMetadata.Size];
    //         //     crossBlockData.CopyTo(crossBlockFinalBytes, 0);
    //         //     Array.Copy(secondBlockData.ToArray(), 0, crossBlockFinalBytes, crossBlockSizeAlreadyRead, secondBlockData.Length);
    //         //
    //         //
    //         //     fileViews.TryAdd((ushort) crossBlockFileIndex, new FileView {Data = crossBlockData, Metadata = fileMetadata});
    //         //     crossBlockFileIndex = -1;
    //         //     crossBlockSizeAlreadyRead = -1;
    //         // }
    //
    //         foreach (var indexAndOffset in blockOffsets)
    //         {
    //             FileMetadata fileMetadata = GetFileMetadata(indexAndOffset.Item1);
    //             if (indexAndOffset.Item2 + fileMetadata.Size > BlockSize)
    //             {
    //                 // Debug.Assert(crossBlockFileIndex == -1);
    //                 // crossBlockFileIndex = (short)fileMetadata.FileIndex;
    //                 byte[] crossBlockData = new byte[fileMetadata.Size];
    //                 Array.Copy(finalBytes.Slice(indexAndOffset.Item2).ToArray(), crossBlockData, BlockSize - indexAndOffset.Item2);
    //                 int crossBlockSizeAlreadyRead = BlockSize - indexAndOffset.Item2;
    //
    //                 int blocksRequired = GetBlockCount(FileEntries[fileMetadata.FileIndex]) - 1;
    //
    //                 // we can assume all the same patch
    //                 using (TigerReader packageHandle = GetPackageHandle(blockEntry.PatchId))
    //                 {
    //                     for (int i = blockIndex; i < blockIndex + blocksRequired; i++)
    //                     {
    //                         unseenBlockIndices.Remove(i);
    //                         D2BlockEntry blockEntryRec = BlockEntries[i];
    //                         Span<byte> finalBytesRec = ReadBlockBufferSpan(packageHandle, blockEntryRec);
    //                         // Span<byte> finalBytesRec = DecryptAndDecompressBlockBufferIfRequired(bytes, blockEntry);
    //                         int lengthToRead = i == blockIndex + blocksRequired - 1 ? fileMetadata.Size - crossBlockSizeAlreadyRead : BlockSize;
    //                         Array.Copy(finalBytesRec.ToArray(), 0, crossBlockData, crossBlockSizeAlreadyRead, lengthToRead);
    //                         crossBlockSizeAlreadyRead += BlockSize;
    //                     }
    //                 }
    //                 fileViews.TryAdd(fileMetadata.FileIndex, new FileView {Data = crossBlockData, Metadata = fileMetadata});
    //
    //                 continue;
    //             }
    //             ReadOnlySpan<byte> aa = finalBytes.Slice(indexAndOffset.Item2, fileMetadata.Size);
    //             // for now skip the large ones
    //
    //             fileViews.TryAdd(fileMetadata.FileIndex, new FileView {Data = aa.ToArray(), Metadata = fileMetadata});
    //         }
    //     }
    //
    //     // would be faster to make a consumer queue that runs async to the block reads as a single producer
    //     // this can then be generalised to multiple packages
    //     return fileViews.Values.ToHashSet();
    // }

    public HashSet<FileView> GetAllFileData()
    {
        // We know that basically all blocks are read, so we read then serially inside of each patch file but in parallel across patch files

        // Maps block index to a set of block (index, offset, size) to read
        ConcurrentDictionary<ushort, ConcurrentHashSet<(int, D2BlockEntry)>> patchBlockEntryMap = new();
        for (ushort i = 0; i < Header.GetPatchId() + 1; i++)
        {
            patchBlockEntryMap.TryAdd(i, new ConcurrentHashSet<(int, D2BlockEntry)>());
        }
        Parallel.For(0, BlockEntries.Count, blockIndex =>
        {
            patchBlockEntryMap[BlockEntries[blockIndex].PatchId].Add((blockIndex, BlockEntries[blockIndex]));
        });

        // remove empty patch files
        foreach (var pair in patchBlockEntryMap)
        {
            if (pair.Value.Count == 0)
            {
                patchBlockEntryMap.TryRemove(pair.Key, out _);
            }
        }

        ConcurrentDictionary<int, byte[]> blockMap = new();

        Parallel.ForEach(patchBlockEntryMap, pair =>
        {
            using TigerReader packageHandle = GetPackageHandle(pair.Key);
            List<(int, D2BlockEntry)> sortedBlockIndices = pair.Value.OrderBy(x => x.Item2.Offset).ToList();
            foreach (var block in sortedBlockIndices)
            {
                packageHandle.Seek(block.Item2.Offset, SeekOrigin.Begin);
                blockMap.TryAdd(block.Item1, packageHandle.ReadBytes((int)block.Item2.Size));
            }
        });

        // parallel process the blocks into their actual data
        Parallel.ForEach(blockMap.Keys, blockIndex =>
        {
            blockMap[blockIndex] = DecryptAndDecompressBlockBufferIfRequired(blockMap[blockIndex], BlockEntries[blockIndex]);
        });

        ConcurrentDictionary<ushort, FileView> fileViews = new();
        Parallel.For(0, FileEntries.Count, fileIndex =>
        {
            D2FileEntry fileEntry = FileEntries[fileIndex];
            byte[] finalFileBuffer = new byte[fileEntry.FileSize];
            int blockCount = GetBlockCount(fileEntry);
            int currentBufferOffset = 0;

            for (int currentBlockId = 0; currentBlockId < blockCount; currentBlockId++)
            {
                bool isFirstBlock = currentBlockId == 0;
                bool isLastBlock = currentBlockId == blockCount - 1;
                bool isOnlyOneBlock = blockCount == 1;
                if (isOnlyOneBlock)
                {
                    int copySize = fileEntry.FileSize;
                    Array.Copy(blockMap[fileEntry.StartingBlockIndex + currentBlockId], fileEntry.StartingBlockOffset, finalFileBuffer, 0, copySize);
                }
                else if (isFirstBlock)
                {
                    int copySize = BlockSize - fileEntry.StartingBlockOffset;
                    Array.Copy(blockMap[fileEntry.StartingBlockIndex + currentBlockId], fileEntry.StartingBlockOffset, finalFileBuffer, 0, copySize);
                    currentBufferOffset += copySize;
                }
                else if (isLastBlock)
                {
                    int copySize = fileEntry.FileSize - currentBufferOffset;
                    Array.Copy(blockMap[fileEntry.StartingBlockIndex + currentBlockId], 0, finalFileBuffer, currentBufferOffset, copySize);
                }
                else
                {
                    const int copySize = BlockSize;
                    Array.Copy(blockMap[fileEntry.StartingBlockIndex + currentBlockId], 0, finalFileBuffer, currentBufferOffset, copySize);
                    currentBufferOffset += BlockSize;
                }
            }
        });

        return fileViews.Values.ToHashSet();
    }

    private T? GetAttribute<T>(ICustomAttributeProvider var) where T : StrategyAttribute
    {
        T[] attributes = var.GetCustomAttributes(typeof(T), false).Cast<T>().ToArray();
        if (!attributes.Any())
        {
            // we still want to be able to get size of non-schema types
            return null;
        }
        if (attributes.Length == 1)
        {
            return attributes.First();
        }
        T? attribute = (T?)var.GetCustomAttributes(typeof(T), false).FirstOrDefault(a => ((T)a).Strategy == PackageStrategy);
        if (attribute == null)
        {
            throw new Exception($"Failed to get schema struct size for type {var} as it has multiple schema struct attributes but none match the current strategy {PackageStrategy}");
        }
        return attribute;
    }

    public static void CheckValidPackagePath(string packagePath, TigerStrategy strategy)
    {
        CheckPackagePathExists(packagePath);
        CheckPackagePathValidPrefix(packagePath, strategy);
        CheckPackagePathValidExtension(packagePath);
    }

    private static void CheckPackagePathExists(string packagePath)
    {
        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException($"The package path '{packagePath}' does not exist");
        }
    }

    private static void CheckPackagePathValidPrefix(string packagePath, TigerStrategy strategy)
    {
        string prefix = Strategy.GetStrategyPackagePrefix(strategy);
        if (!packagePath.Contains(prefix + "_"))
        {
            throw new ArgumentException($"The package path '{packagePath}' does not have the correct prefix '{prefix}'");
        }
    }

    private static void CheckPackagePathValidExtension(string packagePath)
    {
        if (!packagePath.EndsWith(PackageExtension))
        {
            throw new ArgumentException($"The package path '{packagePath}' does not have the extension {PackageExtension}.");
        }
    }

    private TigerReader GetReader() { return new TigerReader(File.Open(PackagePath, FileMode.Open, FileAccess.Read, FileShare.Read)); }

    public byte[] GetFileBytes(FileHash fileHash)
    {
        return GetFileBytes(fileHash.FileIndex);
    }

    public Span<byte> GetFileSpan(ushort fileIndex)
    {
        return GetFileBytes(fileIndex).AsSpan();
    }

    /// <summary>
    /// Find what blocks the file is made out of. For most small files this is a single block since blocks are
    /// 262144 bytes long, but larger files will span multiple blocks.
    /// ( = block start, [ = file start, ] = file end, ) = block end, - = data
    /// If a file is made of a single block, we just need to identify which patch file the block is located in and copy from the given block
    /// offset -> offset + fileSize:
    /// (----[--]--)
    ///
    /// If a file is made of multiple blocks, it often looks like this:
    /// (----[-----) (----------) (---]------)
    /// The first block is be copied from offset -> end of block.
    /// The middling blocks are copied entirely.
    /// The final block is copied from start -> fileSize - bytes already copied
    /// </summary>
    public byte[] GetFileBytes(ushort fileIndex)
    {
        D2FileEntry fileEntry = FileEntries[fileIndex];
        byte[] finalFileBuffer = new byte[fileEntry.FileSize];
        int blockCount = GetBlockCount(fileEntry);
        int currentBufferOffset = 0;
        int currentBlockId = 0;

        List<D2BlockEntry> blocks = GetBlockEntries(fileEntry.StartingBlockIndex, blockCount);
        foreach (D2BlockEntry blockEntry in blocks)
        {
            //if ((blockEntry.BitFlag & 0x8) == 8)
            //{
            //    return new byte[fileEntry.FileSize];
            //}

            // TigerReader packageHandle = GetPackageHandle(blockEntry.PatchId);
            // todo use spans
            byte[] blockBuffer;
            using (TigerReader packageHandle = GetPackageHandle(blockEntry.PatchId))
            {
                blockBuffer = ReadBlockBuffer(packageHandle, blockEntry).ToArray();
            }

            blockBuffer = DecryptAndDecompressBlockBufferIfRequired(blockBuffer, blockEntry);

            bool isFirstBlock = currentBlockId == 0;
            bool isLastBlock = currentBlockId == blockCount - 1;
            bool isOnlyOneBlock = blockCount == 1;
            if (isOnlyOneBlock)
            {
                int copySize = fileEntry.FileSize;
                Array.Copy(blockBuffer, fileEntry.StartingBlockOffset, finalFileBuffer, 0, copySize);
            }
            else if (isFirstBlock)
            {
                int copySize = BlockSize - fileEntry.StartingBlockOffset;
                Array.Copy(blockBuffer, fileEntry.StartingBlockOffset, finalFileBuffer, 0, copySize);
                currentBufferOffset += copySize;
            }
            else if (isLastBlock)
            {
                int copySize = fileEntry.FileSize - currentBufferOffset;
                Array.Copy(blockBuffer, 0, finalFileBuffer, currentBufferOffset, copySize);
            }
            else
            {
                const int copySize = BlockSize;
                Array.Copy(blockBuffer, 0, finalFileBuffer, currentBufferOffset, copySize);
                currentBufferOffset += BlockSize;
            }

            currentBlockId++;
        }

        return finalFileBuffer;
    }

    public PackageMetadata GetPackageMetadata()
    {
        PackageMetadata packageMetadata = new PackageMetadata();
        packageMetadata.Path = PackagePath;
        packageMetadata.Name = PackagePath.Split('/').Last();
        packageMetadata.Id = Header.GetPackageId();
        packageMetadata.PatchId = Header.GetPatchId();
        packageMetadata.Timestamp = Header.GetTimestamp();
        packageMetadata.FileCount = Header.GetFileCount();
        packageMetadata.PackageGroup = Header.GetPackageGroup();
        return packageMetadata;
    }

    public FileMetadata GetFileMetadata(FileHash fileHash)
    {
        if (fileHash.PackageId != Header.GetPackageId())
        {
            throw new ArgumentException($"The provided file hash '{fileHash}' has an invalid package id: {fileHash.PackageId}");
        }
        return GetFileMetadata(fileHash.FileIndex);
    }

    public FileMetadata GetFileMetadata(ushort fileIndex)
    {
        if (fileIndex >= FileEntries.Count)
        {
            throw new ArgumentOutOfRangeException($"The provided file hash has an out-of-range file index {fileIndex} >= {FileEntries.Count}");
        }
        return new FileMetadata(new FileHash(Header.GetPackageId(), fileIndex), FileEntries[fileIndex]);
    }

    private List<D2BlockEntry> GetBlockEntries(int blockIndex, int blockCount) { return BlockEntries.GetRange(blockIndex, blockCount); }

    private int GetBlockCount(D2FileEntry fileEntry)
    {
        return 1 + (int)Math.Floor((double)(fileEntry.StartingBlockOffset + fileEntry.FileSize - 1) / BlockSize);
    }

    private TigerReader GetPackageHandle(ushort patchId)
    {
        // if (!_packageHandles.TryGetValue(patchId, out TigerReader packageHandle))
        // {
        //     packageHandle =
        //         new TigerReader(new FileStream(GetSpecificPackagePatchPath(patchId), FileMode.Open, FileAccess.Read, FileShare.Read));
        //     _packageHandles.Add(patchId, packageHandle);
        // }
        return new TigerReader(new FileStream(GetSpecificPackagePatchPath(patchId), FileMode.Open, FileAccess.Read, FileShare.Read));
        // return packageHandle;
    }

    // This only supports patchIds that are from 0-9.
    private string GetSpecificPackagePatchPath(ushort patchId)
    {
        string packagePatchAndExtension = "0.pkg";
        string pathWithNoPatchAndExtension = PackagePath.Substring(0, PackagePath.Length - packagePatchAndExtension.Length);

        return Path.Combine(pathWithNoPatchAndExtension + patchId.ToString("D") + ".pkg");
    }

    private byte[] ReadBlockBuffer(TigerReader packageHandle, D2BlockEntry blockEntry)
    {
        packageHandle.Seek(blockEntry.Offset, SeekOrigin.Begin);
        byte[] blockBuffer = packageHandle.ReadBytes((int)blockEntry.Size);
        return blockBuffer;
    }

    [DllImport("ThirdParty/oo2core_9_win64.dll", EntryPoint = "OodleLZ_Decompress")]
    public static extern bool OodleLZ_Decompress(byte[] buffer, int bufferSize, byte[] outputBuffer, int outputBufferSize, int a, int b,
        int c, IntPtr d, IntPtr e, IntPtr f, IntPtr g, IntPtr h, IntPtr i, int threadModule);

    private byte[] DecryptAndDecompressBlockBufferIfRequired(byte[] blockBuffer, D2BlockEntry blockEntry)
    {
        byte[] decryptedBuffer;
        if ((blockEntry.BitFlag & 0x8) != 0)
        {
            decryptedBuffer = DecryptBuffer(blockBuffer, blockEntry, true);
            //return new byte[blockBuffer.Length];
        }
        else if ((blockEntry.BitFlag & 0x2) == 2)
        {
            decryptedBuffer = DecryptBuffer(blockBuffer, blockEntry);
        }
        else
        {
            decryptedBuffer = blockBuffer;
        }

        byte[] decompressedBuffer;
        if ((blockEntry.BitFlag & 0x1) == 1)
        {
            decompressedBuffer = DecompressBuffer(decryptedBuffer, blockEntry);
        }
        else
        {
            decompressedBuffer = decryptedBuffer;
        }

        return decompressedBuffer;
    }

    private unsafe byte[] DecryptBuffer(byte[] buffer, D2BlockEntry block, bool redacted = false)
    {
        byte[] decryptedBuffer = new byte[buffer.Length];
        byte[] key;
        byte[] iv = GenerateNonce();

        if (redacted) // (block.BitFlag & 0x4) == 0 
        {
            var kvp = PackageResourcer.Get().Keys;
            if (kvp.ContainsKey(GetPackageMetadata().PackageGroup))
            {
                key = kvp[GetPackageMetadata().PackageGroup].First().Key;
                iv = kvp[GetPackageMetadata().PackageGroup].First().Value;
            }
            else
                return decryptedBuffer;
        }
        else if ((block.BitFlag & 0x4) == 4)
        {
            key = AesKey1;
        }
        else
        {
            key = AesKey0;
        }

        using var aes = new AesGcm(key);
        byte[] tag = new byte[0x10];
        Marshal.Copy((IntPtr)block.GCMTag, tag, 0, 0x10);
        aes.Decrypt(iv, buffer, tag, decryptedBuffer);
        return decryptedBuffer;
    }

    protected abstract byte[] GenerateNonce();

    private byte[] DecompressBuffer(byte[] buffer, D2BlockEntry block)
    {
        return OodleDecompress(buffer, (int)block.Size);
    }

    protected abstract byte[] OodleDecompress(byte[] buffer, int blockSize);

    public List<FileMetadata> GetAllFileMetadata()
    {
        List<FileMetadata> fileMetadataList = new List<FileMetadata>();
        for (ushort fileIndex = 0; fileIndex < FileEntries.Count; fileIndex++)
        {
            fileMetadataList.Add(GetFileMetadata(fileIndex));
        }
        return fileMetadataList;
    }

    public List<PackageActivityEntry> GetAllActivities()
    {
        using TigerReader reader = GetReader();
        return Header.GetAllActivities(reader);
    }
}

public struct PackageMetadata
{
    public string Path;
    public string Name;
    public ushort Id;
    public ushort PatchId;
    public uint Timestamp;
    public uint FileCount;
    public ulong PackageGroup;
}

public struct FileMetadata
{
    public ushort FileIndex;
    public FileHash Hash;
    public TigerHash Reference;
    public int Size;
    public sbyte Type;
    public sbyte SubType;

    public FileMetadata(FileHash fileHash, D2FileEntry fileEntry)
    {
        FileIndex = fileHash.FileIndex;
        Hash = fileHash;
        Reference = fileEntry.Reference;
        Size = fileEntry.FileSize;
        Type = fileEntry.NumType;
        SubType = fileEntry.NumSubType;
    }
}

public struct D2FileEntry
{
    public TigerHash Reference;
    public sbyte NumType;
    public sbyte NumSubType;
    public int StartingBlockIndex;
    public int StartingBlockOffset;
    public int FileSize;

    public D2FileEntry(D2FileEntryBitpacked entryBitpacked)
    {
        // EntryA
        Reference = new TigerHash(entryBitpacked.Reference);

        // EntryB
        NumType = (sbyte)((entryBitpacked.EntryB >> 9) & 0x7F);
        NumSubType = (sbyte)((entryBitpacked.EntryB >> 6) & 0x7);

        // EntryC
        StartingBlockIndex = (int)(entryBitpacked.EntryC & 0x3FFF);
        StartingBlockOffset = (int)(((entryBitpacked.EntryC >> 14) & 0x3FFF) << 4);

        // EntryD
        FileSize = (int)((entryBitpacked.EntryD & 0x3FFFFFF) << 4 | (entryBitpacked.EntryC >> 28) & 0xF);
    }
};

[StructLayout(LayoutKind.Sequential)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F39E8080", 0x10)]
public struct D2FileEntryBitpacked
{
    public uint Reference;
    public uint EntryB;
    public uint EntryC;
    public uint EntryD;

    public D2FileEntryBitpacked(uint reference, uint entryB, uint entryC, uint entryD)
    {
        Reference = reference;
        EntryB = entryB;
        EntryC = entryC;
        EntryD = entryD;
    }
}

[StructLayout(LayoutKind.Sequential)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "EE9E8080", 0x30)]
public unsafe struct D2BlockEntry
{
    public uint Offset;
    public uint Size;
    public ushort PatchId;
    public ushort BitFlag;
    public fixed byte SHA1[0x14];
    public fixed byte GCMTag[0x10];
};
