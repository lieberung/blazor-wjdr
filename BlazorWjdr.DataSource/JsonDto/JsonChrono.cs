// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
namespace BlazorWjdr.DataSource.JsonDto
{
    using System.Collections.Generic;

    public class JsonChrono
    {
        public int debut { get; set; }
        public int? fin { get; set; }
        public string resume { get; set; } = null!;
        public string? titre { get; set; }
        public string? comment { get; set; }
        public List<int> sources { get; set; } = null!;
        public List<int> domaines { get; set; } = null!;
    }

    public class RootChrono
    {
        public List<JsonDomaine> domaines { get; set; } = null!;
        public List<JsonChrono> items { get; set; } = null!;
    }
}
