﻿using Hi3Helper.Preset;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static Hi3Helper.Data.ConverterTool;

namespace Hi3Helper.Shared.ClassStruct
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FileType { Blocks, Generic, Audio }
    public class FilePropertiesRemote
    {
        // public long BlkS() => BlkC.Sum(x => x.BlockSize);
        public string N { get; set; }
        public string RN { get; set; }
        public string CRC { get; set; }
        public string M { get; set; }
        public FileType FT { get; set; }
        public List<XMFBlockList> BlkC { get; set; }
        public long S { get; set; }
    }

    public class FileProperties
    {
        public string FileName { get; set; }
        public FileType DataType { get; set; }
        public string FileSource { get; set; }
        public long FileSize { get; set; }
        public string FileSizeStr => SummarizeSizeSimple(FileSize);
        // public long Offset { get; set; }
        public string ExpctCRC { get; set; }
        public string CurrCRC { get; set; }
    }
}
