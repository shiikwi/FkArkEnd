using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace IL2CPP
{
    public class GlobalMetadata
    {
        private const uint Sanity = 0xFAB11BAF;
        private const uint Version = 29;
        private const int OriHeaderSize = 0x100;
        private const int ModifiedHeaderSize = 0x108;
        private const int HeaderDelta = ModifiedHeaderSize - OriHeaderSize;

        public static void Fix(string infile)
        {
            var data = File.ReadAllBytes(infile);
            var metaHeader = Il2CppGlobalMetadataHeader.Read(data);
            if (metaHeader.Sanity != Sanity || metaHeader.Version != Version)
                throw new Exception("Not supported global-metadata");

            var output = new byte[data.Length - HeaderDelta];
            data[ModifiedHeaderSize..].CopyTo(output.AsSpan(OriHeaderSize));

            var fixedHeader = metaHeader.Shift(output.Length);
            fixedHeader.Write(output);
            File.WriteAllBytes(infile + ".fixed", output);
        }

        private readonly struct Il2CppGlobalMetadataHeader(uint sanity, int version, Il2CppMetadataSection[] sections)
        {
            public uint Sanity { get; } = sanity;
            public int Version { get; } = version;
            public Il2CppMetadataSection[] Sections { get; } = sections;
            public Il2CppMetadataSection StringLiteral => Sections[(int)MetadataSectionName.StringLiteral];
            public Il2CppMetadataSection ExportedTypeDefinitions => Sections[(int)MetadataSectionName.ExportedTypeDefinitions];

            public static Il2CppGlobalMetadataHeader Read(ReadOnlySpan<byte> data)
            {
                var sanity = BinaryPrimitives.ReadUInt32LittleEndian(data[0..4]);
                var version = BinaryPrimitives.ReadInt32LittleEndian(data[4..8]);
                var sections = new Il2CppMetadataSection[Il2CppMetadataSection.Count];

                for (var i = 0; i < sections.Length; i++)
                {
                    var entryOffset = 8 + i * 8;
                    sections[i] = new Il2CppMetadataSection(
                        BinaryPrimitives.ReadInt32LittleEndian(data.Slice(entryOffset, 4)),
                        BinaryPrimitives.ReadInt32LittleEndian(data.Slice(entryOffset + 4, 4)));
                }
                return new Il2CppGlobalMetadataHeader(sanity, version, sections);
            }

            public Il2CppGlobalMetadataHeader Shift(int outSize)
            {
                var fixedSections = new Il2CppMetadataSection[Il2CppMetadataSection.Count];
                for (var i = 0; i < fixedSections.Length; i++)
                    fixedSections[i] = Sections[i].ShiftOffset(-HeaderDelta);

                var exported = ExportedTypeDefinitions.ShiftOffset(-HeaderDelta);
                fixedSections[(int)MetadataSectionName.ExportedTypeDefinitions] = exported;

                var windowsRuntimeTypeNamesOffset = exported.Offset + exported.Size;
                fixedSections[(int)MetadataSectionName.WindowsRuntimeTypeNames] =
                    new Il2CppMetadataSection(windowsRuntimeTypeNamesOffset, outSize - windowsRuntimeTypeNamesOffset);

                fixedSections[(int)MetadataSectionName.WindowsRuntimeStrings] = default;

                fixedSections[(int)MetadataSectionName.StringLiteral] =
                    new Il2CppMetadataSection(OriHeaderSize, StringLiteral.Size);

                return new Il2CppGlobalMetadataHeader(Sanity, Version, fixedSections);
            }

            public void Write(Span<byte> data)
            {
                data[..OriHeaderSize].Clear();
                BinaryPrimitives.WriteUInt32LittleEndian(data[0..4], Sanity);
                BinaryPrimitives.WriteInt32LittleEndian(data[4..8], Version);

                for (var i = 0; i < Sections.Length; i++)
                {
                    var entryOffset = 8 + i * 8;
                    BinaryPrimitives.WriteInt32LittleEndian(data.Slice(entryOffset, 4), Sections[i].Offset);
                    BinaryPrimitives.WriteInt32LittleEndian(data.Slice(entryOffset + 4, 4), Sections[i].Size);
                }
            }
        }

        private readonly struct Il2CppMetadataSection(int offset, int size)
        {
            public const int Count = 31;

            public int Offset { get; } = offset;
            public int Size { get; } = size;

            public Il2CppMetadataSection ShiftOffset(int delta)
            {
                return Offset == 0 ? this : new Il2CppMetadataSection(Offset + delta, Size);
            }
        }

        private enum MetadataSectionName
        {
            StringLiteral,
            StringLiteralData,
            String,
            Events,
            Properties,
            Methods,
            ParameterDefaultValues,
            FieldDefaultValues,
            FieldAndParameterDefaultValueData,
            FieldMarshaledSizes,
            Parameters,
            Fields,
            GenericParameters,
            GenericParameterConstraints,
            GenericContainers,
            NestedTypes,
            Interfaces,
            VTableMethods,
            InterfaceOffsets,
            TypeDefinitions,
            Images,
            Assemblies,
            FieldRefs,
            ReferencedAssemblies,
            AttributeData,
            AttributeDataRange,
            UnresolvedVirtualCallParameterTypes,
            UnresolvedVirtualCallParameterRanges,
            WindowsRuntimeTypeNames,
            WindowsRuntimeStrings,
            ExportedTypeDefinitions
        }
    }
}
