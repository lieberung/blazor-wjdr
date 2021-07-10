﻿namespace BlazorWjdr.Services
{
    using BlazorWjdr.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LieuxService
    {
        private Dictionary<int, LieuTypeDto>? _cacheLieuType = null;
        private Dictionary<int, LieuDto>? _cacheLieu = null;
        private List<LieuDto>? _allLieux = null;

        public LieuxService()
        {
        }

        protected Dictionary<int, LieuTypeDto> AllTypesDeLieu
        {
            get
            {
                if (_cacheLieuType == null)
                    Initialize();
#pragma warning disable CS8603 // Possible null Lieu return.
                return _cacheLieuType;
#pragma warning restore CS8603 // Possible null Lieu return.
            }
        }
        
        protected List<LieuDto> AllLieux
        {
            get
            {
                if (_allLieux == null)
                    Initialize();
#pragma warning disable CS8603 // Possible null Lieu return.
                return _allLieux;
#pragma warning restore CS8603 // Possible null Lieu return.
            }
        }
        
        public Task<LieuDto[]> Items()
        {
            return Task.FromResult(AllLieux.ToArray());
        }

        public LieuTypeDto GetTypeDeLieu(int id)
        {
            if (_cacheLieuType == null)
                Initialize();
#pragma warning disable CS8602 // DeLieu of a possibly null Lieu.
            return _cacheLieuType[id];
#pragma warning restore CS8602 // DeLieu of a possibly null Lieu.
        }

        public LieuDto GetLieu(int id)
        {
            if (_cacheLieu == null)
                Initialize();
#pragma warning disable CS8602 // DeLieu of a possibly null Lieu.
            return _cacheLieu[id];
#pragma warning restore CS8602 // DeLieu of a possibly null Lieu.
        }

        private void Initialize()
        {
            _cacheLieuType = DataSource.JsonLoader
                .GetRootLieuType()
                .items
                .Select(t => new LieuTypeDto
                {
                    Id = t.id,
                    Nom = t.libelle,
                    ParentId = t.parentid
                }).ToDictionary(k => k.Id, v => v);

            foreach (var lieuType in _cacheLieuType.Values.Where(t => t.ParentId.HasValue))
            {
                lieuType.Parent = _cacheLieuType[lieuType.ParentId!.Value];
            }
            
            _cacheLieu = DataSource.JsonLoader
                .GetRootLieu()
                .items
                .Select(c => new LieuDto
                {
                    Id = c.id,
                    Nom = c.nom,
                    ParentId = c.fk_parentid,
                    TypeDeLieu = _cacheLieuType[c.fk_typeid]
                })
                .ToDictionary(k => k.Id, v => v);
            
            _allLieux = _cacheLieu.Values.ToList();
            
            foreach (var lieu in _allLieux.Where(t => t.ParentId.HasValue))
            {
                lieu.Parent = _cacheLieu[lieu.ParentId!.Value];
            }
        }
    }
}
