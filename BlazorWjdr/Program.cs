using BlazorWjdr.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using BlazorWjdr.DataSource.JsonDto;
using BlazorWjdr.Models;

namespace BlazorWjdr
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            var data = new ADataClassToRuleThemAllService(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            await data.InitializeDataAsync();
            Debug.WriteLine("await data.InitializeDataAsync();");

            var dataAptitudes = InitializeAptitudes(data.Aptitudes!.items);
            var dataProfils = InitializeProfils(data.Profils!.items);
            var dataReferences = InitializeReferences(data.References!.items);
            var dataCarrieres = InitializeCarrieres(data.Carrieres!.items, dataProfils, dataAptitudes, dataReferences);
            var dataChrono = InitializeChronologie(data.Chrono!.items, dataReferences);
            var dataLieuxTypes = InitializeLieuxTypes(data.LieuxTypes!.items);
            var dataLieux = InitializeLieux(data.Lieux!.items, dataLieuxTypes);
            var dataEquipements = InitializeEquipements(data.Equipements!.items, dataLieux);
            var dataDieux = InitializeDieux(data.Dieux!.items, dataAptitudes, dataLieux);
            var dataTables = InitializeTables(data.Tables!.items);
            var dataArmesAttributs = InitializeArmesAttributs(data.ArmesAttributs!.items);
            var dataArmes = InitializeArmes(data.Armes!.items, dataArmesAttributs, dataAptitudes);
            var dataArmures = InitializeArmures(data.Armures!.items, dataArmesAttributs);
            var dataSortileges = InitializeSortileges(data.Sortileges!.items, dataAptitudes);
            var dataRaces = InitializeRaces(data.Races!.items, dataLieux);
            var dataTablesCarrInit = InitializeTablesCarrieresInitiales(data.CarrieresInitiales!.items, dataRaces, dataCarrieres);
            var dataBestioles = InitializeCreatures(data.Creatures!.items, dataRaces, dataProfils, dataAptitudes, dataLieux, dataCarrieres);
            var dataRegles = InitializeRegles(data.Regles!.items, dataTables, dataBestioles, dataAptitudes, dataLieux, dataCarrieres);
            
            builder.Services.AddSingleton(_ => new AptitudesService(dataAptitudes));
            builder.Services.AddSingleton(_ => new LieuxService(dataLieuxTypes, dataLieux));
            builder.Services.AddSingleton(_ => new DieuxService(dataDieux));
            builder.Services.AddSingleton(_ => new ReferencesService(dataReferences));
            builder.Services.AddSingleton(_ => new ProfilsService(dataProfils));
            builder.Services.AddSingleton(_ => new TablesService(dataTables));
            builder.Services.AddSingleton(_ => new ChronologieService(dataChrono));
            builder.Services.AddSingleton(_ => new ArmesService(dataArmesAttributs, dataArmes, dataArmures, dataEquipements));
            builder.Services.AddSingleton(_ => new CarrieresService(dataCarrieres));
            builder.Services.AddSingleton(_ => new RacesService(dataRaces));
            builder.Services.AddSingleton(_ => new TableDesCarrieresInitialesService(dataTablesCarrInit));
            builder.Services.AddSingleton(_ => new BestiolesService(dataBestioles));
            builder.Services.AddSingleton(_ => new ReglesService(dataRegles));
            builder.Services.AddSingleton(_ => new SortilegesService(dataSortileges));

            builder.RootComponents.Add<App>("#app");

            await builder.Build().RunAsync();
        }

        private static Dictionary<int, SortilegeDto> InitializeSortileges(IEnumerable<JsonSortilege> items, IReadOnlyDictionary<int, AptitudeDto> dataAptitudes)
        {
            return items
                .Select(i => new SortilegeDto
                {
                    Id = i.id,
                    Aptitudes = (i.aptitudes ?? Array.Empty<int>()).Select(id => dataAptitudes[id]).ToArray(),
                    Cible = i.cible,
                    Distance = i.distance,
                    Duree = i.duree,
                    Effet = i.effet,
                    Nom = i.nom,
                    Type = i.type,
                    NS = i.ns
                })
                .ToDictionary(k => k.Id);
        }

        #region Initialize

        private static Dictionary<int, RegleDto> InitializeRegles(
            IEnumerable<JsonRegle> items,
            IReadOnlyDictionary<int, TableDto> tables,
            IReadOnlyDictionary<int, BestioleDto> bestioles,
            IReadOnlyDictionary<int, AptitudeDto> aptitudes,
            IReadOnlyDictionary<int, LieuDto> lieux,
            IReadOnlyDictionary<int, CarriereDto> carrieres)
        {
            var cacheRegle = items
                .Select(r => new RegleDto
                {
                    Id = r.id,
                    Html = r.html,
                    Titre = r.titre,
                    ReglesId = r.regles ?? Array.Empty<int>(),
                    Carrieres = (r.carrieres ?? Array.Empty<int>()).Select(id => carrieres[id]).ToArray(),
                    Aptitudes = GetAptitudes(r.aptitudes, aptitudes),
                    AptitudesChoix = r.aptitudes_choix != null
                        ? r.aptitudes_choix.Select(choix => GetAptitudes(choix, aptitudes).ToArray()).ToList()
                        : new List<AptitudeDto[]>(),
                    Bestioles = (r.bestioles ?? Array.Empty<int>()).Select(id => bestioles[id]).ToArray(),
                    Tables = (r.tables ?? Array.Empty<int>()).Select(id => tables[id]).ToArray(),
                    Lieux = (r.lieux ?? Array.Empty<int>()).Select(id => lieux[id]).ToArray(),
                    Regle = r.regle
                })
                .ToDictionary(k => k.Id, v => v);

            foreach (var regle in cacheRegle.Values)
            {
                regle.SousRegles = regle.ReglesId.Select(id => cacheRegle[id]).ToArray();
            }

            return cacheRegle;
        }

        private static Dictionary<int, BestioleDto> InitializeCreatures(
            IEnumerable<JsonCreature> items,
            IReadOnlyDictionary<int, RaceDto> races,
            IReadOnlyDictionary<int, ProfilDto> profils,
            IReadOnlyDictionary<int, AptitudeDto> aptitudes,
            IReadOnlyDictionary<int, LieuDto> lieux,
            IReadOnlyDictionary<int, CarriereDto> carrieres)
        {
            return items
                .Select(c => new BestioleDto
                {
                    Id = c.id,
                    EstUnPersonnage = (c.fk_cheminprofess ?? Array.Empty<int>()).Any(),
                    EstUnPersonnageJoueur = !string.IsNullOrWhiteSpace(c.nom_joueur),
                    Userid = c.user,
                    MembreDe = c.membrede,
                    Age = c.age,
                    Commentaire = c.description ?? string.Empty,
                    Histoire = c.histoire ?? string.Empty,
                    Nom = c.nom,
                    Poids = c.poids,
                    Psychologie = c.psycho ?? "",
                    Race = races[c.race],
                    Sexe = c.sexe ?? -1,
                    Taille = c.taille,
                    ProfilActuel = profils[c.profil_actuel],
                    AptitudesAcquises = AptitudeAcquise.GetList(c.aptitudes.Select(id => aptitudes[id]).ToArray()),
                    Origines = (c.origines ?? Array.Empty<int>()).Select(id => lieux[id]).ToArray(),
                    // Personnage
                    SigneAstralId = c.fk_signeastralid,
                    Cheveux = c.cheveux,
                    Yeux = c.yeux,
                    FreresEtSoeurs = c.freres_et_soeurs,
                    MainDirectrice = c.main_directrice ?? -1,
                    Mort = c.mort,
                    CarriereDuPere = c.fk_carrierepereid.HasValue ? carrieres[c.fk_carrierepereid.Value] : null,
                    CarriereDeLaMere = c.fk_carrieremereid.HasValue ? carrieres[c.fk_carrieremereid.Value] : null,
                    // PJ
                    Joueur = c.nom_joueur ?? "",
                    CheminementPro = c.fk_cheminprofess != null ? c.fk_cheminprofess!.Select(id => carrieres[id]).ToArray() : Array.Empty<CarriereDto>(),

                    ProfilInitial = c.fk_profilinitialid.HasValue ? profils[c.fk_profilinitialid.Value] : null,
                    XpActuel = c.xp_actuel,
                    XpTotal = c.xp_total
                })
                .ToDictionary(k => k.Id);
        }

        private static Dictionary<int, List<LigneDeCarriereInitialeDto>> InitializeTablesCarrieresInitiales(
            IEnumerable<JsonTableCarriereInitiale> items,
            IReadOnlyDictionary<int, RaceDto> races,
            IReadOnlyDictionary<int, CarriereDto> carrieres)
        {
            var lignesEnVrac = items
                .Select(l => new LigneDeCarriereInitialeDto
                {
                    Carriere = carrieres[l.carriere],
                    Facteur = l.facteur,
                    Race = races[l.race]
                })
                .ToList();

            var allLignes = lignesEnVrac
                .Select(l => l.Race.Id)
                .Distinct()
                .ToDictionary(k => k, _ => new List<LigneDeCarriereInitialeDto>());

            foreach (var raceId in allLignes.Keys)
            {
                allLignes[raceId].AddRange(lignesEnVrac
                    .Where(l => l.Race.Id == raceId)
                    .OrderBy(l => l.Carriere.Groupe)
                    .ThenBy(l => l.Carriere.Nom)
                );
            }

            return allLignes;
        }

        private static Dictionary<int, RaceDto> InitializeRaces(IEnumerable<JsonRace> items,
            IReadOnlyDictionary<int, LieuDto> lieux)
        {
            var cache = items
                .Select(r => new RaceDto
                {
                    Id = r.id,
                    Description = r.description,
                    Lieux = (r.lieux ?? Array.Empty<int>()).Select(id => lieux[id]).ToArray(),
                    GroupOnly = r.group_only,
                    NomFeminin = r.nom_feminin??"",
                    NomMasculin = r.nom_masculin,
                    Autochtones = r.nom_autoch??"",
                    ParentId = r.parent,
                    PourPj = r.pj,
                    Opinions = (r.opinions ?? Array.Empty<JsonOpinion>())
                        .Select(o => new OpinionDto { RaceId = o.race, Ambiance = o.ambiance }).ToList()
                })
                .ToDictionary(k => k.Id);

            foreach (var race in cache.Values.Where(d => d.ParentId.HasValue))
            {
                race.Parent = cache[race.ParentId!.Value];
            }

            foreach (var race in cache.Values)
            {
                race.SousElements.AddRange(cache.Values
                    .Where(c => c.Parent == race)
                    .OrderBy(c => c.NomMasculin));
                foreach (var opinion in race.Opinions)
                {
                    if (opinion.RaceId != 0)
                        opinion.Race = cache[opinion.RaceId];
                }
            }

            return cache;
        }

        private static Dictionary<int, ArmureDto> InitializeArmures(
            IEnumerable<JsonArmure> items,
            Dictionary<int, ArmeAttributDto> attributs)
        {
            return items
                .Select(a => new ArmureDto
                {
                    Id = a.id,
                    Type = a.type,
                    Nom = a.nom,
                    Description = a.description,
                    Disponibilite = a.dispo,
                    Enc = a.enc,
                    Pa = a.pa,
                    Prix = a.prix,
                    Zones = a.zones,
                    Attributs = (a.attributs ?? Array.Empty<int>()).Select(id => attributs[id]).OrderBy(at => at.Nom).ToArray()
                }).ToDictionary(k => k.Id);
        }

        private static Dictionary<int, EquipementDto> InitializeEquipements(
            IEnumerable<JsonEquipement> items,
            IReadOnlyDictionary<int, LieuDto> lieux)
        {
            return items
                .Select(t => new EquipementDto
                {
                    Id = t.id,
                    Dispo = t.dispo,
                    Nom = t.nom,
                    Description = t.description,
                    Enc = t.enc,
                    Groupes = (t.groupes ?? Array.Empty<string>()).ToArray(),
                    Prix = t.prix,
                    Contenance = t.contenance,
                    Addiction = t.addiction,
                    Lieux = (t.lieux ?? Array.Empty<int>()).Select(id => lieux[id]).ToArray()
                }).ToDictionary(k => k.Id);
        }

        private static Dictionary<int, ArmeAttributDto> InitializeArmesAttributs(IEnumerable<JsonArmeAttribut> items)
        {
            return items
                .Select(t => new ArmeAttributDto
                {
                    Id = t.id,
                    Type = t.type,
                    Nom = t.nom,
                    Description = t.description
                }).ToDictionary(k => k.Id);
        }

        private static Dictionary<int, ArmeDto> InitializeArmes(
            IEnumerable<JsonArme> items,
            IReadOnlyDictionary<int, ArmeAttributDto> cacheAttributs,
            IReadOnlyDictionary<int, AptitudeDto> cacheCompetences)
        {
            return items
                .Select(l => new ArmeDto
                {
                    Id = l.id,
                    Nom = l.nom,
                    Description = l.description ?? "",
                    Attributs = l.attributs.Select(id => cacheAttributs[id]).ToList(),
                    Degats = l.degats,
                    Disponibilite = l.dispo,
                    Encombrement = l.enc,
                    Groupes = l.groupes.Select(id => cacheAttributs[id]).ToList(),
                    Allonge = l.allonge ?? "",
                    Portee = l.portee ?? "",
                    Prix = l.prix,
                    Rechargement = l.rechargement ?? "",
                    CompetencesDeMaitrise = l.competences.Select(id => cacheCompetences[id]).ToList()
                })
                .ToDictionary(k => k.Id);
        }

        private static Dictionary<int, TableDto> InitializeTables(IEnumerable<JsonTable> items)
        {
            return items
                .Select(c => new TableDto
                {
                    Id = c.id,
                    Titre = c.titre,
                    Description = c.description,
                    StylesHeader = c.styles_th ?? Array.Empty<string>(),
                    StylesRows = c.styles_td ?? Array.Empty<string>(),
                    Lignes = c.lignes
                })
                .ToDictionary(k => k.Id, v => v);
        }

        private static Dictionary<int, DieuDto> InitializeDieux(
            IEnumerable<JsonDieu> items,
            IReadOnlyDictionary<int, AptitudeDto> aptitudes,
            IReadOnlyDictionary<int, LieuDto> lieux)
        {
            var cache = items
                .Select(c => new DieuDto
                {
                    Id = c.id,
                    Nom = c.nom,
                    Commentaire = c.comment ?? "",
                    Domaines = c.domaines ?? "",
                    Fideles = c.fideles ?? "",
                    Histoire = c.histoire ?? "",
                    Symboles = c.symboles ?? "",
                    PatronId = c.patron,
                    Ambiance = (c.ambiance ?? Array.Empty<string>()).ToList(),
                    Aptitudes = c.aptitudes != null ? new AptitudesAssocieesDto {
                        Inities = (c.aptitudes.initie ?? Array.Empty<int>())
                            .Select(id => aptitudes[id]).OrderBy(a => a.NomComplet).ToList(),
                        PretesSansOrdre = (c.aptitudes.pretre_sans_ordre ?? Array.Empty<int>())
                            .Select(id => aptitudes[id]).OrderBy(a => a.NomComplet).ToList()
                    } : new AptitudesAssocieesDto { Inities = new List<AptitudeDto>(), PretesSansOrdre = new List<AptitudeDto>() },
                    Chef = c.chef ?? "",
                    Commandements = (c.commandements ?? Array.Empty<string>()).ToList(),
                    Culte = c.culte ?? "",
                    Cultistes = c.cultistes ?? "",
                    Dogme = c.dogme ?? "",
                    Fetes = c.fetes ?? "",
                    Initiation = c.initiation ?? "",
                    Intro = c.intro ?? "",
                    Penitences = c.penitences ?? "",
                    Personnalites = (c.personnalites ?? Array.Empty<JsonPersonnalite>()).Select(p => new PersonnaliteDto { Nom = p.nom ?? "", Description = p.description ?? "" }).ToList(),
                    Pretrise = c.pretrise ?? "",
                    Regles = (c.regles ?? Array.Empty<JsonRegleAssociee>()).Select(r => new RegleAssocieeDto { Titre = r.titre ?? "", Description = r.description ?? "" }).ToList(),
                    Sectes = (c.sectes ?? Array.Empty<JsonSecte>()).Select(p => new SecteDto { Nom = p.nom ?? "", Description = p.description ?? "" }).ToList(),
                    Siege = c.siege.HasValue ? lieux[c.siege.Value] : null,
                    Structure = c.structure ?? "",
                    Temples = (c.temples ?? Array.Empty<JsonTemple>()).Select(t => new TempleDto { Nom = t.nom ?? "", Description = t.description ?? "" }).ToList(),
                    LivresSaints = c.livres ?? "aucun.",
                    Ordres = (c.cultes ?? Array.Empty<JsonCulte>()).Select(jc => new CulteDto
                    {
                        Ambiance = jc.ambiance ?? "",
                        Aptitudes = (jc.aptitudes ?? Array.Empty<int>()).Select(id => aptitudes[id]).ToList(),
                        Description = jc.description ?? "",
                        Id = jc.id,
                        Mineur = jc.mineur,
                        Nom = jc.nom ?? ""
                    }).ToList()
                })
                .ToDictionary(k => k.Id, v => v);

            foreach (var dieu in cache.Values.Where(d => d.PatronId.HasValue))
            {
                dieu.Patron = cache[dieu.PatronId!.Value];
            }

            return cache;
        }

        private static Dictionary<int, LieuTypeDto> InitializeLieuxTypes(IEnumerable<JsonLieuType> items)
        {
            var result = items
                .Select(t => new LieuTypeDto
                {
                    Id = t.id,
                    Nom = t.libelle,
                    ParentId = t.parentid
                }).ToDictionary(k => k.Id, v => v);

            foreach (var lieuType in result.Values.Where(t => t.ParentId.HasValue))
            {
                lieuType.Parent = result[lieuType.ParentId!.Value];
            }

            return result;
        }

        private static Dictionary<int, LieuDto> InitializeLieux(IEnumerable<JsonLieu> items,
            IReadOnlyDictionary<int, LieuTypeDto> cacheTypesDeLieu)
        {
            var result = items
                .Select(l => new LieuDto
                {
                    Id = l.id,
                    Nom = l.nom,
                    Description = l.description ?? "",
                    ParentId = l.fk_parentid,
                    TypeDeLieu = cacheTypesDeLieu[l.fk_typeid]
                })
                .ToDictionary(k => k.Id);

            var parents = new List<int>();
            foreach (var lieu in result.Values.Where(t => t.ParentId.HasValue))
            {
                var parentId = lieu.ParentId!.Value;
                if (!parents.Contains(parentId))
                    parents.Add(parentId);
                lieu.Parent = result[parentId];
                lieu.Parent.SousElements.Add(lieu);
            }

            //foreach (var lieu in result.Values.Where(l => l.ParentId == null))
            foreach (var lieuId in (parents))
            {
                result[lieuId].SousElements = result[lieuId].SousElements.OrderBy(c => c.Nom).ToList();
                /*
                .AddRange(result.Values
                .Where(c=>c.Parent == lieu)
                .OrderBy(c => c.Nom));
                */
            }

            return result;
        }

        private static Dictionary<int, ReferenceDto> InitializeReferences(IEnumerable<JsonReference> items)
        {
            return items
                .Select(c => new ReferenceDto
                {
                    Id = c.id,
                    AnneeDePublication = c.publishyear ?? 6666,
                    Code = c.code,
                    Titre = c.titre,
                    Version = c.version
                })
                .ToDictionary(k => k.Id, v => v);
        }
        
        private static Dictionary<int, AptitudeDto> InitializeAptitudes(IEnumerable<JsonAptitude> aptitudes)
        {
            var result = aptitudes
                .Select(c => new AptitudeDto
                {
                    Id = c.id,
                    Ignore = c.ignorer,
                    Nom = $"{c.nom}{(!string.IsNullOrWhiteSpace(c.spe) ? $" : {c.spe}" : "")}",
                    Spe = c.spe ?? "",
                    Categ = c.categ,
                    CategSpe = c.categ_spe ?? "",
                    Resume = c.resume,
                    Description = c.description,
                    CaracteristiqueAssociee = c.carac ?? "",
                    AptitudeMereId = c.parent,
                    Contagieux = c.contagieux,
                    Guerison = c.guerison ?? "",
                    Severite = c.severite ?? 0,
                    IncompatiblesIds = c.incompatibles ?? new List<int>(),
                    AptitudesLieesIds = c.aptitudes ?? new List<int>(),
                })
                .ToDictionary(k => k.Id, v => v);

            foreach (var aptitude in result.Values.Where(c => c.AptitudeMereId.HasValue))
            {
                aptitude.Parent = result[aptitude.AptitudeMereId!.Value];
            }

            foreach (var apt in result.Values)
            {
                apt.NomPourRecherche = GenericService.ConvertirCaracteres(apt.Nom);
                apt.MotsClefDeRecherche = GenericService.MotsClefsDeRecherche(apt.NomPourRecherche);
                apt.SetResume();
                apt.SetDescription();
                apt.AptitudesLiees = apt.AptitudesLieesIds.Select(id => result[id]).OrderBy(a => a.NomComplet).ToList();
                apt.Incompatibles = apt.IncompatiblesIds.Select(id => result[id]).OrderBy(a => a.NomComplet).ToList();
            }
            
            // Compléter ToDo: voir si utile
            foreach (var apt in result.Values)
            {
                foreach (var aptLiee in apt.AptitudesLiees.Where(aptLiee => !aptLiee.AptitudesLiees.Contains(apt)))
                {
                    aptLiee.AptitudesLiees.Add(apt);
                }
                foreach (var aptInc in apt.Incompatibles.Where(aptInc => !aptInc.Incompatibles.Contains(apt)))
                {
                    aptInc.Incompatibles.Add(apt);
                }
            }
            
            foreach (var aptitude in result.Values.Where(t => t.Parent != null).Select(t => t.Parent).Distinct())
            {
                aptitude!.SousElements.AddRange(result.Values
                    .Where(c => c.Parent == aptitude)
                    .OrderBy(c => c.Nom));
            }

            return result;
        }

        private static Dictionary<int, ProfilDto> InitializeProfils(IEnumerable<JsonProfil> profils)
        {
            return profils
                .Select(c => new ProfilDto
                {
                    Id = c.id,
                    Ag = c.ag,
                    Cc = c.cc,
                    Ct = c.ct,
                    Dex = c.dex,
                    E = c.e,
                    F = c.f,
                    Fm = c.fm,
                    I = c.i,
                    Int = c.intel,
                    Soc = c.soc
                })
                .ToDictionary(k => k.Id, v => v);
        }

        private static IEnumerable<CarriereDto> GetCarrieres(IEnumerable<int>? ids,
            IReadOnlyDictionary<int, CarriereDto> cache)
            => (ids ?? Array.Empty<int>()).Select(id => cache[id]).OrderBy(c => c.Nom);

        private static Dictionary<int, CarriereDto> InitializeCarrieres(
            IEnumerable<JsonCarriere> carrieres,
            IReadOnlyDictionary<int, ProfilDto> cacheProfils,
            IReadOnlyDictionary<int, AptitudeDto> cacheAptitudes,
            IReadOnlyDictionary<int, ReferenceDto> cacheReferences)
        {
            var cacheCarrieres = carrieres
                .Select(c => new CarriereDto
                {
                    Id = c.id,
                    Groupe = c.groupe ?? "",
                    Nom = c.nom,
                    MotsClefDeRecherche = GenericService.MotsClefsDeRecherche(GenericService.ConvertirCaracteres(c.nom)),
                    Description = c.description,
                    Ambiance = c.ambiance ?? Array.Empty<string>(),
                    CarriereMereId = c.parent,
                    DebouchesIds = c.debouch ?? Array.Empty<int>(),
                    AvancementsIds = c.avancements ?? Array.Empty<int>(),
                    Dotations = c.dotations,
                    EstUneCarriereAvancee = c.avancee,
                    Image = $"images/careers/{c.id}.png",
                    PlanDeCarriere = cacheProfils[c.plan],
                    Restriction = c.restriction ?? "",
                    Source = c.source_page ?? "",
                    SourceLivre = c.source_livre == null ? null : cacheReferences[c.source_livre.Value],
                    Aptitudes = GetAptitudes(c.aptitudes, cacheAptitudes),
                    AptitudesChoix = c.aptitudes_choix != null
                        ? c.aptitudes_choix.Select(choix => GetAptitudes(choix, cacheAptitudes).ToArray()).ToList()
                        : new List<AptitudeDto[]>(),
                })
                .ToDictionary(k => k.Id, v => v);

            foreach (var carriere in cacheCarrieres.Values.Where(c => c.CarriereMereId.HasValue))
            {
                carriere.Parent = cacheCarrieres[carriere.CarriereMereId!.Value];
            }

            foreach (var carriere in cacheCarrieres.Values.Where(c => c.DebouchesIds.Any()))
                carriere.Debouches = GetCarrieres(carriere.DebouchesIds, cacheCarrieres).ToList();
            foreach (var carriere in cacheCarrieres.Values.Where(c => c.AvancementsIds.Any()))
                carriere.Avancements = GetCarrieres(carriere.AvancementsIds, cacheCarrieres).ToList();

            foreach (var carriere in cacheCarrieres.Values.Where(c => c.Parent != null).Select(c => c.Parent)
                .Distinct())
            {
                carriere!.SousElements.AddRange(cacheCarrieres.Values
                    .Where(c => c.Parent == carriere)
                    .OrderBy(c => c.Nom));
            }

            foreach (var carriere in cacheCarrieres.Values)
            {
                carriere.Filieres = cacheCarrieres.Values
                    .Where(c => c.Debouches.Contains(carriere))
                    .OrderBy(c => c.Nom)
                    .ToList();
                carriere.Origines = cacheCarrieres.Values
                    .Where(c => c.Avancements.Contains(carriere))
                    .OrderBy(c => c.Nom)
                    .ToList();
            }

            return cacheCarrieres;
        }

        private static List<AptitudeDto> GetAptitudes(int[]? argAptitudes, IReadOnlyDictionary<int, AptitudeDto> cacheAptitudes)
        {
            return argAptitudes == null ? new List<AptitudeDto>() : argAptitudes.Select(id => cacheAptitudes[id]).ToList();
        }

        private static IEnumerable<ChronologieDto> InitializeChronologie(IEnumerable<JsonChrono> chrono,
            IReadOnlyDictionary<int, ReferenceDto> cache)
        {
            return chrono
                .Select(c => new ChronologieDto(
                    c.debut,
                    c.fin,
                    c.resume,
                    c.titre ?? "",
                    c.comment ?? "",
                    c.sources.Select(id => cache[id]).ToArray()
                ))
                .OrderBy(c => c.Debut).ThenBy(c => c.Fin)
                .ToArray();
        }

        #endregion
    }
}