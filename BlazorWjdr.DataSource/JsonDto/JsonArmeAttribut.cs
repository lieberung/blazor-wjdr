﻿// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
namespace BlazorWjdr.DataSource.JsonDto
{
    using System.Collections.Generic;

    public class JsonArmeAttribut
    {
        public int id { get; set; }
        public string type { get; set; } = null!;
        public string nom { get; set; } = null!;
        public string description { get; set; } = null!;
    }

    public class RootArmeAttribut
    {
        public List<JsonArmeAttribut> items { get; set; } = null!;
    }
}