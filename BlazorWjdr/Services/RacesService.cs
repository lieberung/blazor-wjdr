﻿namespace BlazorWjdr.Services
{
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class RacesService
    {
        private readonly LieuxService _lieuxService;
        private readonly ProfilsService _profilsService;
        
        private Dictionary<int, RaceDto>? _cacheRace;
        private List<RaceDto>? _allRaces;

        public RacesService(LieuxService lieuxService, ProfilsService profilsService)
        {
            _lieuxService = lieuxService;
            _profilsService = profilsService;
        }

        private List<RaceDto> AllRaces
        {
            get
            {
                if (_allRaces == null)
                    Initialize();
#pragma warning disable CS8603 // Possible null Race return.
                return _allRaces;
#pragma warning restore CS8603 // Possible null Race return.
            }
        }
        
        public Task<RaceDto[]> Items()
        {
            return Task.FromResult(AllRaces.ToArray());
        }

        public RaceDto Elfes => GetRace(25);
        public RaceDto Humains => GetRace(1);
        public RaceDto HumainsMutants => GetRace(64);
        public RaceDto Halfelings => GetRace(26);
        public RaceDto Nains => GetRace(27);
        public RaceDto Gnomes => GetRace(63);
        
        public RaceDto GetRace(int id)
        {
            if (_cacheRace == null)
                Initialize();
#pragma warning disable CS8602 // DeRace of a possibly null Race.
            return _cacheRace[id];
#pragma warning restore CS8602 // DeRace of a possibly null Race.
        }

        private void Initialize()
        {
            _cacheRace = DataSource.JsonLoader
                .GetRootRace()
                .items
                .Select(r => new RaceDto
                {
                    Id = r.id,
                    Description = r.description,
                    Lieux = (r.lieux_ids ?? System.Array.Empty<int>()).Select(id => _lieuxService.GetLieu(id)).ToArray(),
                    Profil = r.profil_id.HasValue ? _profilsService.GetProfil(r.profil_id.Value) : null,
                    GroupOnly = r.group_only,
                    NomFeminin = r.nom_feminin,
                    NomMasculin = r.nom_masculin,
                    ParentId = r.parent_id,
                    PourPj = r.pj
                })
                .ToDictionary(k => k.Id, v => v);
            
            _allRaces = _cacheRace.Values.ToList();
            
            foreach (var race in _allRaces.Where(d => d.ParentId.HasValue))
            {
                race.Parent = _cacheRace[race.ParentId!.Value];
            }
            
            foreach (var lieu in _allRaces)
            {
                lieu.SousElements.AddRange(_allRaces
                    .Where(c=>c.Parent == lieu)
                    .OrderBy(c => c.NomMasculin));                
            }
        }
    }
}
