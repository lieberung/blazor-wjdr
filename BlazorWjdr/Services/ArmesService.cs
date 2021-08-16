﻿namespace BlazorWjdr.Services
{
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ArmesService
    {
        private readonly CompetencesEtTalentsService _competencesEtTalentsService;
        
        private Dictionary<int, ArmeAttributDto>? _cacheArmeAttribut;
        private Dictionary<int, ArmeDto>? _cacheArme;
        private List<ArmeDto>? _allArmes;

        private Dictionary<string, List<ArmeDto>>? _armesDeContactPourTable;
        private Dictionary<string, List<ArmeDto>>? _armesADistancePourTable;

        public ArmesService(CompetencesEtTalentsService competencesEtTalentsService)
        {
            _competencesEtTalentsService = competencesEtTalentsService;
        }

        private Dictionary<int, ArmeAttributDto> AllAttributsDArme
        {
            get
            {
                if (_cacheArmeAttribut == null)
                    Initialize();
#pragma warning disable CS8603 // Possible null Arme return.
                return _cacheArmeAttribut;
#pragma warning restore CS8603 // Possible null Arme return.
            }
        }

        public List<ArmeAttributDto> AllGroupesDArmes => AllAttributsDArme.Values.Where(a => a.Type == "groupe").OrderBy(g => g.Nom).ToList();

        private List<ArmeDto> AllArmes
        {
            get
            {
                if (_allArmes == null)
                    Initialize();
#pragma warning disable CS8603 // Possible null Arme return.
                return _allArmes;
#pragma warning restore CS8603 // Possible null Arme return.
            }
        }
        
        public Task<ArmeDto[]> Items()
        {
            return Task.FromResult(AllArmes.ToArray());
        }

        private ArmeAttributDto GetAttributDArme(int id)
        {
            if (_cacheArmeAttribut == null)
                Initialize();
#pragma warning disable CS8602 // DeArme of a possibly null Arme.
            return _cacheArmeAttribut[id];
#pragma warning restore CS8602 // DeArme of a possibly null Arme.
        }

        public IEnumerable<ArmeDto> GetArmesDeMaitrise(TalentDto maitrise) =>
            AllArmes.Where(a => a.TalentsDeMaitrise.Contains(maitrise)).OrderBy(a => a.Nom).ToArray();

        public IEnumerable<ArmeDto> GetArmes(IEnumerable<int> ids) => ids.Select(GetArme).ToArray();

        private ArmeDto GetArme(int id)
        {
            if (_cacheArme == null)
                Initialize();
#pragma warning disable CS8602 // DeArme of a possibly null Arme.
            return _cacheArme[id];
#pragma warning restore CS8602 // DeArme of a possibly null Arme.
        }

        private void Initialize()
        {
            _cacheArmeAttribut = DataSource.JsonLoader
                .GetRootArmeAttribut()
                .items
                .Select(t => new ArmeAttributDto
                {
                    Id = t.id,
                    Type = t.type,
                    Nom = t.nom,
                    Description = t.description
                }).ToDictionary(k => k.Id, v => v);
            
            _cacheArme = DataSource.JsonLoader
                .GetRootArme()
                .items
                .Select(l => new ArmeDto
                {
                    Id = l.id,
                    Nom = l.nom,
                    Description = l.description ?? "",
                    Attributs = l.attributs.Select(id => _cacheArmeAttribut[id]).ToList(),
                    Degats = l.degats,
                    Disponibilite = l.dispo,
                    Encombrement = l.enc,
                    Groupes = l.groupes.Select(id => _cacheArmeAttribut[id]).ToList(),
                    Allonge = l.allonge ?? "",
                    Portee = l.portee ?? "",
                    Prix = l.prix,
                    Rechargement = l.rechargement ?? "",
                    TalentsDeMaitrise = l.talents.Select(id => _competencesEtTalentsService.GetTalent(id)).ToList()
                })
                .ToDictionary(k => k.Id, v => v);
            
            _allArmes = _cacheArme.Values.OrderBy(a => a.Nom).ToList();
        }

        #region Regroupements pour table

        // Au contact
        public ArmeAttributDto GroupeOrdinaires => GetAttributDArme(50);
        public ArmeAttributDto GroupeFléaux => GetAttributDArme(55);
        public ArmeAttributDto GroupeEscrime => GetAttributDArme(76);
        public ArmeAttributDto GroupeCavalerie => GetAttributDArme(77);
        public ArmeAttributDto GroupeParade => GetAttributDArme(56);
        public ArmeAttributDto GroupeBouclier => GetAttributDArme(74);
        public ArmeAttributDto GroupeParalysantes => GetAttributDArme(53);
        public ArmeAttributDto GroupeArmesDHast => GetAttributDArme(57);
        public ArmeAttributDto GroupeADeuxMains => GetAttributDArme(52);
        public ArmeAttributDto GroupeExotique => GetAttributDArme(100);
                
        public Dictionary<string, List<ArmeDto>> ArmesDeContactPourTable
        {
            get
            {
                if (_armesDeContactPourTable == null)
                    _armesDeContactPourTable = new Dictionary<string, List<ArmeDto>>
                    {
                        { "Ordinaires", AllArmes.Where(a => a.EstUneArmeDeCaC
                                                                && a.Groupes.Contains(GroupeOrdinaires)
                                                                && !a.Groupes.Contains(GroupeBouclier)
                                                    ).ToList() },
                        { "Armes lourdes", AllArmes.Where(a => a.Groupes.Contains(GroupeADeuxMains)
                                                                   && !a.Groupes.Contains(GroupeArmesDHast)
                                                                   && !a.Groupes.Contains(GroupeFléaux)
                                                    ).ToList() },
                        { "Armes d'hast", AllArmes.Where(a => a.Groupes.Contains(GroupeArmesDHast)).ToList() },
                        { "Fléaux", AllArmes.Where(a => a.Groupes.Contains(GroupeFléaux)).ToList() },
                        { "Escrime", AllArmes.Where(a => a.Groupes.Contains(GroupeEscrime)).ToList() },
                        { "Défense et parade", AllArmes.Where(a => a.Groupes.Contains(GroupeBouclier) || a.Groupes.Contains(GroupeParade)).ToList() },
                        { "Paralysantes", AllArmes.Where(a => a.Groupes.Contains(GroupeParalysantes)).ToList() },
                        { "Cavalerie", AllArmes.Where(a => a.Groupes.Contains(GroupeCavalerie)).ToList() },
                        { "Exotiques", AllArmes.Where(a => a.EstUneArmeDeCaC && a.Groupes.Contains(GroupeExotique)).ToList() }
                    };
                return _armesDeContactPourTable;
            }
        }
        
        // A distance
        public ArmeAttributDto GroupeArbalètes => GetAttributDArme(69);
        public ArmeAttributDto GroupeArcs => GetAttributDArme(70);
        public ArmeAttributDto GroupePoudreNoire => GetAttributDArme(61);
        public ArmeAttributDto GroupeMécaniques => GetAttributDArme(62);
        public ArmeAttributDto GroupeExplosifs => GetAttributDArme(60);
        public ArmeAttributDto GroupeArmesDeJet => GetAttributDArme(54);
        public ArmeAttributDto GroupeLancePierres => GetAttributDArme(58);
        
        public Dictionary<string, List<ArmeDto>> ArmesADistancePourTable
        {
            get
            {
                if (_armesADistancePourTable == null)
                    _armesADistancePourTable = new Dictionary<string, List<ArmeDto>>
                    {
                        { "Arbalètes", AllArmes.Where(a => a.Groupes.Contains(GroupeArbalètes)).ToList() },
                        { "Arcs", AllArmes.Where(a => a.Groupes.Contains(GroupeArcs)).ToList() },
                        { "Lance-pierres", AllArmes.Where(a => a.Groupes.Contains(GroupeLancePierres)).ToList() },
                        { "Poudre noire, armes mécaniques et explosifs", AllArmes.Where(a => 
                                        a.Groupes.Contains(GroupePoudreNoire) ||
                                        a.Groupes.Contains(GroupeMécaniques) ||
                                        a.Groupes.Contains(GroupeExplosifs)
                                    ).ToList() },
                        { "Armes de jet", AllArmes.Where(a => a.Groupes.Contains(GroupeArmesDeJet)).ToList() },
                        { "Exotiques", AllArmes.Where(a => a.EstUneArmeDeTir && a.Groupes.Contains(GroupeExotique)).ToList() }
                    };
                return _armesADistancePourTable;
            }
        }
        
        #endregion
    }
}