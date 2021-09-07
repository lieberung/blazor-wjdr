﻿// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
namespace BlazorWjdr.DataSource.JsonDto
{
    using System.Collections.Generic;

    public class JsonDomaine
    {
        public int id { get; set; }
        public string nom { get; set; } = null!;
        public string comment { get; set; } = null!;
    }

    public class RootDomaine
    {
        public List<JsonDomaine> items { get; set; } = null!;
    }
}
