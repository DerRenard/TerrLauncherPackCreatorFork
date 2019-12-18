﻿using System;
using Newtonsoft.Json;

namespace TerrLauncherPackCreator.Code.Json
{
    public class PackSettings
    {
        [JsonProperty("terrariaStructureVersion")]
        public int TerrariaStructureVersion { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("descriptionEnglish")]
        public string DescriptionEnglish { get; set; }
        
        [JsonProperty("descriptionRussian")]
        public string DescriptionRussian { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }
        
        [JsonProperty("guid")]
        public Guid Guid { get; set; }
    }
}