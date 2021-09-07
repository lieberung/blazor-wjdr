﻿namespace BlazorWjdr.Services
{
    using Models;
    using System.Collections.Generic;
    using System.Linq;

    public class LieuxService
    {
        private readonly  Dictionary<int, LieuTypeDto> _cacheLieuType;
        private readonly Dictionary<int, LieuDto> _cacheLieu;

        public LieuxService(Dictionary<int, LieuTypeDto> dataLieuxTypes, Dictionary<int, LieuDto> dataLieux)
        {
            _cacheLieu = dataLieux;
            _cacheLieuType = dataLieuxTypes;
        }
        
        protected Dictionary<int, LieuTypeDto> AllTypesDeLieu => _cacheLieuType;
        public List<LieuDto> AllLieux => _cacheLieu.Values.ToList();
        public LieuTypeDto GetTypeDeLieu(int id) => _cacheLieuType[id];

        public IEnumerable<LieuDto> GetLieux(IEnumerable<int> ids) => ids.Select(GetLieu).ToArray();
        public LieuDto GetLieu(int id) => _cacheLieu[id];
    }
}
