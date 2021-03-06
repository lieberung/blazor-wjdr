// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace BlazorWjdr.DataSource.JsonDto
{
    using System.Collections.Generic;

    public class JsonRace
    {
        public int id { get; set; }
        public int? parent { get; set; }
        public int[]? lieux { get; set; }
        public int[]? aptitudes { get; set; }
        public bool pj { get; set; }
        public bool group_only { get; set; }
        public string nom_masculin { get; init; } = null!;
        public string? nom_feminin { get; init; } = null!;
        public string? nom_autoch { get; init; } = null!;
        public string? description { get; set; }
        public JsonOpinion[]? opinions { get; set; }
        public JsonInfo[]? infos { get; set; }
    }

    public class JsonOpinion
    {
        public int race { get; set; }
        public string ambiance { get; init; } = null!;
    }

    public class JsonInfo
    {
        public string? titre { get; set; }
        public string detail { get; init; } = null!;
    }

    public class RootRace
    {
        public List<JsonRace> items { get; set; } = null!;
    }

}
