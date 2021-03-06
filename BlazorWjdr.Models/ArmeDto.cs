using System.Linq;

namespace BlazorWjdr.Models
{
    using System.Collections.Generic;

    public class ArmeDto
    {
        public int Id { get; init; }
        public int? ParentId { get; init; }
        public ArmeDto? Parent { get; set; }
        public List<AptitudeDto> CompetencesDeMaitrise { get; init; } = null!;
        public List<ArmeAttributDto> Attributs { get; init; } = null!;
        public List<ArmeAttributDto> Groupes { get; init; } = null!;
        public string Nom { get; init; } = null!;
        public string Degats { get; init; } = null!;
        public string Allonge { get; init; } = null!;
        public string Portee { get; init; } = null!;
        public string Rechargement { get; init; } = null!;
        public string Encombrement { get; init; } = null!;
        public string Prix { get; init; } = null!;
        public string Disponibilite { get; init; } = null!;
        public string Description { get; init; } = null!;

        public bool EstUneArmeDeCaC => Allonge != "";
        public bool EstUneArmeDeTir => Portee != "" && !EstUneMunition;
        public bool EstUneMunition => Groupes.Any(g => g.Nom == "Munitions");
    }
}
