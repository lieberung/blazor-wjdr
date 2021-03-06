using BlazorWjdr.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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

            var startTime = DateTime.Now;
            var dataUsers = InitializeUsers(data.Campagne!.users);

            var dataAptitudes = InitializeAptitudes(data.Aptitudes!.items);
            var dataReferences = InitializeReferences(data.References!.items);
            var listChrono = InitializeChronologie(data.Chrono!.items, dataReferences);
            var dataLieuxTypes = InitializeLieuxTypes(data.Lieux!.types);
            var dataLieux = InitializeLieux(data.Lieux!.items, dataLieuxTypes);
            var dataEquipements = InitializeEquipements(data.Equipements!.items, dataLieux, dataLieuxTypes);
            var dataDieux = InitializeDieux(data.Dieux!.items, dataAptitudes, dataLieux);
            var dataTables = InitializeTables(data.Tables!.items);
            var dataArmesAttributs = InitializeArmesAttributs(data.Armes!.attributs);
            var dataArmes = InitializeArmes(data.Armes!.armes, dataArmesAttributs, dataAptitudes);
            var dataArmures = InitializeArmures(data.Armes!.armures, dataArmesAttributs);
            var dataSortileges = InitializeSortileges(data.Sortileges!.sortileges, dataAptitudes);
            var dataRaces = InitializeRaces(data.Races!.items, dataAptitudes, dataLieux);
            //dataRaces.Add(0,dataRaces[4]);
            var dataCarrieres = InitializeCarrieres(data.Carrieres!.items, dataRaces, dataAptitudes, dataReferences);
            var dataTablesCarrInit = InitializeTablesCarrieresInitiales(dataCarrieres);
            var dataBestioles = InitializeCreatures(data.Creatures!.items, dataRaces, dataAptitudes, dataLieux, dataCarrieres, dataArmes, dataArmures, dataEquipements, dataSortileges, dataUsers);
            var dataRegles = InitializeRegles(data.Regles!.items, dataTables, dataBestioles, dataAptitudes, dataLieux, dataCarrieres);

            var dataScenarios = InitializeScenarios(data.Scenarios!.scenarios, dataLieux, dataLieuxTypes);
            var dataTeams = InitializeTeams(data.Campagne!.teams);
            var dataCampagnes = InitializeCampagnes(dataUsers, dataTeams, data.Campagne!.campagnes, dataScenarios, dataBestioles, dataCarrieres, dataLieux);

            Console.WriteLine($"Initializing data... {DateTime.Now.Subtract(startTime).TotalSeconds}sec.");
            
            builder.Services.AddSingleton(_ => new AptitudesService(dataAptitudes));
            builder.Services.AddSingleton(_ => new LieuxService(dataLieuxTypes, dataLieux));
            builder.Services.AddSingleton(_ => new DieuxService(dataDieux));
            builder.Services.AddSingleton(_ => new ReferencesService(dataReferences));
            builder.Services.AddSingleton(_ => new TablesService(dataTables));
            builder.Services.AddSingleton(_ => new ChronologieService(listChrono));
            builder.Services.AddSingleton(_ => new ArmesService(dataArmesAttributs, dataArmes, dataArmures, dataEquipements));
            builder.Services.AddSingleton(_ => new CarrieresService(dataCarrieres));
            builder.Services.AddSingleton(_ => new RacesService(dataRaces));
            builder.Services.AddSingleton(_ => new TableDesCarrieresInitialesService(dataTablesCarrInit));
            builder.Services.AddSingleton(_ => new BestiolesService(dataBestioles));
            builder.Services.AddSingleton(_ => new ReglesService(dataRegles));
            builder.Services.AddSingleton(_ => new SortilegesService(dataSortileges));
            builder.Services.AddSingleton(_ => new CampagnesService(dataCampagnes));
            builder.Services.AddSingleton(_ => new ScenariosService(dataScenarios));
            
            builder.Services.AddSingleton(_ => new AppState());

            builder.RootComponents.Add<App>("#app");
            
            data.Dispose();

            await builder.Build().RunAsync();
        }

        private static ScenarioDto[] InitializeScenarios(IEnumerable<JsonScenario> items,
            IReadOnlyDictionary<int, LieuDto> lieux,
            IReadOnlyDictionary<int, LieuTypeDto> lieuxTypes)
        {
            return items.Select(s => new ScenarioDto
            {
                Difficulte = s.difficulte ?? "",
                Duree = s.duree ?? "",
                Lieux = (s.lieux ?? Array.Empty<int>()).Select(id => lieux[id]).ToArray(),
                LieuxTypes = (s.lieuxtypes ?? Array.Empty<int>()).Select(id => lieuxTypes[id]).ToArray(),
                Nom = s.nom,
                Lien = s.lien ?? "",
                Note = s.note,
                Resume = s.resume ?? "",
                Source = s.source ?? "",
                Styles = s.styles ?? Array.Empty<string>(),
                DejaJoue = s.deja_joue ?? ""
            }).ToArray();
        }

        private static CampagneDto[] InitializeCampagnes(
            IReadOnlyDictionary<int, UserDto> users, 
            IReadOnlyDictionary<int, TeamDto> teams,
            IEnumerable<JsonCampagne> campagnes,
            IEnumerable<ScenarioDto> scenarios,
            IReadOnlyDictionary<int, BestioleDto> bestioles,
            IReadOnlyDictionary<int, CarriereDto> carrieres,
            IReadOnlyDictionary<int, LieuDto> lieux)
        {
            return campagnes.Select(c => new CampagneDto()
            {
                Id = c.id,
                Mj = users[c.mj],
                Seances = (c.seances ?? Array.Empty<JsonSeance>()).Select(s => GetSeanceDtoFromJson(s, bestioles, scenarios, lieux)).ToArray(),
                Contacts = (c.contacts ?? Array.Empty<JsonContactDeCampagne>()).Select(s => GetContactDeCampagneDtoFromJson(s, bestioles, carrieres, lieux)).ToArray(),
                Team = teams[c.team],
                Titre = c.titre
            }).ToArray();
        }

        private static SeanceDto GetSeanceDtoFromJson(JsonSeance s,
            IReadOnlyDictionary<int, BestioleDto> bestioles,
            IEnumerable<ScenarioDto> scenarios,
            IReadOnlyDictionary<int, LieuDto> lieux)
        {
            return new SeanceDto
            {
                Acte = s.acte,
                Debut = s.debut ?? "",
                Duree = s.duree,
                Lieux = (s.lieux ?? Array.Empty<int>()).Select(id => lieux[id]).ToArray(),
                Facts = (s.facts ?? Array.Empty<JsonFact>()).Select(f => new FactDto {
                    Fact = f.fact,
                    Pjs = (f.pjs ?? Array.Empty<int>()).Select(id => bestioles[id]).ToArray(),
                    Tri = f.tri
                }).ToArray(),
                Pjs = (s.pjs ?? Array.Empty<int>()).Select(id => bestioles[id]).ToArray(),
                Quand = s.quand,
                Resume = s.resume ?? "",
                Secret = s.secret,
                Titre = s.titre,
                Scenario = s.scenario == null ? null : scenarios.SingleOrDefault(sc => sc.Nom.ToLower() == s.scenario.ToLower()),
                Xp = s.xp,
                XpComment = s.xp_comment,
                Rencontres = (s.rencontres ?? Array.Empty<JsonRencontre>())
                    .Select(r => GetRencontreDtoFromJson(r, bestioles)).ToArray()
            };
        }

        private static ContactDeCampagneDto GetContactDeCampagneDtoFromJson(JsonContactDeCampagne s,
            IReadOnlyDictionary<int, BestioleDto> bestioles,
            IReadOnlyDictionary<int, CarriereDto> carrieres,
            IReadOnlyDictionary<int, LieuDto> lieux)
        {
            return new ContactDeCampagneDto
            {
                Description = s.description ?? "",
                Notes = s.notes ?? Array.Empty<string>(),
                Pnj = bestioles[s.pnj],
                LieuDeRencontre = lieux[s.lieu_de_rencontre],
                LieuDeResidence = s.lieu_de_residence == 0 ? null : lieux[s.lieu_de_residence],
                ProposeLesCarrieres = (s.employeur ?? Array.Empty<int>()).Select(id => carrieres[id]).ToArray()
            };
        }

        private static RencontreDto GetRencontreDtoFromJson(JsonRencontre r,
            IReadOnlyDictionary<int, BestioleDto> bestioles)
        {
            return new RencontreDto
            {
                Groupe = r.groupe,
                Allies = (r.allies ?? Array.Empty<JsonCombattant>()).Select(a => GetRencontreCombattantDtoFromJson(a, false, bestioles)).ToArray(),
                Ennemis = (r.ennemis ?? Array.Empty<JsonCombattant>()).Select(a => GetRencontreCombattantDtoFromJson(a, true, bestioles)).ToArray()
            };
        }

        private static CombattantDto GetRencontreCombattantDtoFromJson(JsonCombattant c, bool ennemi,
            IReadOnlyDictionary<int, BestioleDto> bestioles)
        {
            return new CombattantDto
            {
                Combattant = bestioles[c.id],
                Nom = c.nom ?? bestioles[c.id].Nom,
                Ennemi = ennemi
            };
        }
        
        private static Dictionary<int, TeamDto> InitializeTeams(IEnumerable<JsonTeam> teams)
        {
            return teams.Select(t => new TeamDto { Id = t.id, Nom = t.nom }).ToDictionary(k => k.Id);
        }

        private static Dictionary<int, UserDto> InitializeUsers(IEnumerable<JsonUser> users)
        {
            return users.Select(t => new UserDto { Id = t.id, Pseudo = t.pseudo, Email = t.email }).ToDictionary(k => k.Id);
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
                    Ingredient = i.ingredient,
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
            IReadOnlyDictionary<int, AptitudeDto> aptitudes,
            IReadOnlyDictionary<int, LieuDto> lieux,
            IReadOnlyDictionary<int, CarriereDto> carrieres,
            IReadOnlyDictionary<int, ArmeDto> armes,
            IReadOnlyDictionary<int, ArmureDto> armures,
            IReadOnlyDictionary<int, EquipementDto> equipement,
            IReadOnlyDictionary<int, SortilegeDto> sorts,
            IReadOnlyDictionary<int, UserDto> users)
        {
            var bestioles = items
                .Select(c => new BestioleDto
                {
                    Id = c.id,
                    Userid = c.user ?? 0,
                    MembreDe = c.membrede ?? Array.Empty<string>(),
                    Age = c.age,
                    DateDeCreation = c.date_creation ?? "",
                    Notes = c.notes ?? string.Empty,
                    Histoire = c.histoire ?? string.Empty,
                    Ambitions = c.ambitions ?? Array.Empty<string>(),
                    Description = c.description ?? string.Empty,
                    Nom = c.nom,
                    NomCourt = c.nom_court ?? c.nom,
                    Poids = c.poids,
                    Psychologie = c.psycho ?? "",
                    Race = races[c.race],
                    Sexe = c.sexe,
                    Taille = c.taille,
                    Masquer = c.masquer,
                    ProfilActuel = GetProfilDtoFromJson(c.profil_actuel),
                    ProfilInitial = c.profil_initial != null ? GetProfilDtoFromJson(c.profil_initial) : null,
                    AptitudesAcquises = AptitudeAcquiseDto.GetList(
                        (c.aptitudes ?? Array.Empty<int>()).Select(id => aptitudes[id]).ToArray(),
                        GetProfilDtoFromJson(c.profil_actuel)),
                    AptitudesOptionnels = AptitudeAcquiseDto.GetList(
                        (c.aptitudes_facultatives ?? Array.Empty<int>()).Select(id => aptitudes[id]).ToArray(),
                        GetProfilDtoFromJson(c.profil_actuel)),
                    Origines = (c.origines ?? Array.Empty<int>()).Select(id => lieux[id]).ToArray(),
                    Armes = (c.armes ?? Array.Empty<int>()).Select(id => armes[id]).ToArray(),
                    Armures = (c.armures ?? Array.Empty<int>()).Select(id => armures[id]).ToArray(),
                    Equipement = (c.equipement ?? Array.Empty<int>()).Select(id => equipement[id]).ToArray(),
                    Sorts = (c.sorts ?? Array.Empty<int>()).Select(id => sorts[id]).ToArray(),
                    SigneAstralId = c.fk_signeastralid,
                    Cheveux = c.cheveux,
                    Yeux = c.yeux,
                    FreresEtSoeurs = c.freres_et_soeurs,
                    MainDirectrice = c.main_directrice ?? -1,
                    Mort = c.mort,
                    CarriereDuPere = c.carriere_du_pere.HasValue ? carrieres[c.carriere_du_pere.Value] : null,
                    CarriereDeLaMere = c.carriere_de_la_mere.HasValue ? carrieres[c.carriere_de_la_mere.Value] : null,
                    Joueur = c.user != null ? users[c.user.Value].Pseudo : "",
                    CheminementPro = c.cheminement != null ? c.cheminement!.Select(id => carrieres[id]).ToArray() : Array.Empty<CarriereDto>(),
                    XpActuel = c.xp_actuel ?? 0,
                    XpTotal = c.xp_total ?? 0
                })
                .ToDictionary(k => k.Id);

            foreach (var bestiole in bestioles.Values)
            {
                bestiole.Gabarit = BestiolesService.GetGabarit(bestiole) ?? aptitudes[AptitudesService.TraitGabaritMoyenId];
                bestiole.Blessures = bestiole.ProfilActuel.B == 0 ? BestiolesService.CalculBlessures(bestiole.Gabarit, bestiole.ProfilActuel,
                    bestiole.AptitudesAcquises.Any(aa => aa.Aptitude.Id == AptitudesService.TraitDurACuirId)) : bestiole.ProfilActuel.B;
                bestiole.BlessuresDetailDuCalcul = BestiolesService.GetBlessuresDetailDuCalcul(bestiole.Gabarit, bestiole.ProfilActuel,
                    bestiole.AptitudesAcquises.Any(aa => aa.Aptitude.Id == AptitudesService.TraitDurACuirId));
                bestiole.BlessuresFormuleDeCalcul = BestiolesService.GetBlessuresFormuleDeCalcul(bestiole.Gabarit,
                    bestiole.AptitudesAcquises.Any(aa => aa.Aptitude.Id == AptitudesService.TraitDurACuirId));
            }

            return bestioles;
        }

        private static Dictionary<int, List<LigneDeCarriereInitialeDto>> InitializeTablesCarrieresInitiales(
            IReadOnlyDictionary<int, CarriereDto> carrieres)
        {
            var result = new Dictionary<int, List<LigneDeCarriereInitialeDto>>
            {
                { 1, new List<LigneDeCarriereInitialeDto>() },
                { 4, new List<LigneDeCarriereInitialeDto>() },
                { 7, new List<LigneDeCarriereInitialeDto>() },
                { 22, new List<LigneDeCarriereInitialeDto>() },
                { 23, new List<LigneDeCarriereInitialeDto>() },
                { 26, new List<LigneDeCarriereInitialeDto>() },
                { 27, new List<LigneDeCarriereInitialeDto>() },
                { 28, new List<LigneDeCarriereInitialeDto>() },
                { 63, new List<LigneDeCarriereInitialeDto>() },
                { 65, new List<LigneDeCarriereInitialeDto>() },
                { 66, new List<LigneDeCarriereInitialeDto>() }
            };
            
            var carrieresInitiales = carrieres.Values
                .Where(c => c.TirageInitial.Any());

            foreach (var ci in carrieresInitiales)
            {
                foreach (var ti in ci.TirageInitial)
                {
                    result[ti.Race.Id].Add(new LigneDeCarriereInitialeDto { Carriere = ci, Facteur = ti.Facteur });
                }
            }

            foreach (var key in result.Keys)
                result[key] = result[key]
                    .OrderBy(l => l.Carriere.Groupe)
                    .ThenBy(l => l.Carriere.Nom)
                    .ToList();

            return result;
        }

        private static Dictionary<int, RaceDto> InitializeRaces(IEnumerable<JsonRace> items,
            IReadOnlyDictionary<int, AptitudeDto> aptitudes,
            IReadOnlyDictionary<int, LieuDto> lieux)
        {
            var cache = items
                .Select(r => new RaceDto
                {
                    Id = r.id,
                    Description = r.description??"",
                    Aptitudes = (r.aptitudes ?? Array.Empty<int>()).Select(id => aptitudes[id]).OrderBy(a => a.NomComplet).ToArray(),
                    Lieux = (r.lieux ?? Array.Empty<int>()).Select(id => lieux[id]).OrderBy(l => l.Nom).ToArray(),
                    GroupOnly = r.group_only,
                    NomFeminin = r.nom_feminin??"",
                    NomMasculin = r.nom_masculin,
                    Autochtones = r.nom_autoch??"",
                    ParentId = r.parent,
                    PourPj = r.pj,
                    Opinions = (r.opinions ?? Array.Empty<JsonOpinion>())
                        .Select(o => new OpinionDto {RaceId = o.race, Ambiance = o.ambiance}).ToList(),
                    Infos = (r.infos ?? Array.Empty<JsonInfo>())
                        .Select(i => new RaceInfoDto {Titre = i.titre, Detail = i.detail}).ToList()
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
            IReadOnlyDictionary<int, ArmeAttributDto> attributs)
        {
            var result = items
                .Select(a => new ArmureDto
                {
                    Id = a.id,
                    ParentId = a.parent, 
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
            
            foreach (var arme in result.Values.Where(a => a.ParentId.HasValue))
            {
                arme.Parent = result[arme.ParentId!.Value];
            }
            
            return result;
        }

        private static Dictionary<int, EquipementDto> InitializeEquipements(
            IEnumerable<JsonEquipement> items,
            IReadOnlyDictionary<int, LieuDto> lieux,
            IReadOnlyDictionary<int, LieuTypeDto> lieuxtypes)
        {
            var result = items
                .Select(t => new EquipementDto
                {
                    Id = t.id,
                    ParentId = t.parent,
                    Dispo = t.dispo,
                    Nom = t.nom,
                    Description = t.description,
                    Enc = t.enc ?? "-",
                    Groupes = (t.groupes ?? Array.Empty<string>()).ToArray(),
                    Prix = t.prix,
                    Contenance = t.contenance,
                    Addiction = t.addiction,
                    Lieux = (t.lieux ?? Array.Empty<int>()).Select(id => lieux[id]).ToArray(),
                    LieuxTypes = (t.lieuxtypes ?? Array.Empty<int>()).Select(id => lieuxtypes[id]).ToArray(),
                    Potion = t.potion == null ? null : GetPotionDtoFromJson(t.potion)  
                })
                .ToDictionary(k => k.Id);
            
            foreach (var equipement in result.Values.Where(e => e.ParentId.HasValue))
            {
                equipement.Parent = result[equipement.ParentId!.Value];
            }
            return result;
        }

        private static PotionDto GetPotionDtoFromJson(JsonPotion item)
        {
            return new PotionDto
            {
                Creation = new CreationDto { Difficulte = item.creation.difficulte, Temps = item.creation.temps },
                Ingredients = new IngredientsDto
                {
                    Localisation = item.ingredients.localisation, Prix = item.ingredients.prix,
                    Difficulte = item.ingredients.difficulte
                },
                Instabilite = item.instabilite,
                Reaction = item.reaction
            };
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
            var result = items
                .Select(l => new ArmeDto
                {
                    Id = l.id,
                    ParentId = l.parent,
                    Nom = l.nom,
                    Description = l.description ?? "",
                    Attributs = (l.attributs ?? Array.Empty<int>()).Select(id => cacheAttributs[id]).ToList(),
                    Degats = l.degats,
                    Disponibilite = l.dispo,
                    Encombrement = l.enc,
                    Groupes = (l.groupes ?? Array.Empty<int>()).Select(id => cacheAttributs[id]).ToList(),
                    Allonge = l.allonge ?? "",
                    Portee = l.portee ?? "",
                    Prix = l.prix,
                    Rechargement = l.rechargement ?? "",
                    CompetencesDeMaitrise = l.competences.Select(id => cacheCompetences[id]).ToList()
                })
                .ToDictionary(k => k.Id);

            foreach (var arme in result.Values.Where(a => a.ParentId.HasValue))
            {
                arme.Parent = result[arme.ParentId!.Value];
            }
            
            return result;
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
                    Ordres = (c.ordres ?? Array.Empty<JsonOrdre>()).Select(jc => new CulteDto
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
                    Population = l.population ?? "",
                    Allegeance = l.allegeance ?? "",
                    Industrie = l.industrie ?? "",
                    Description = l.description ?? "",
                    ParentId = l.parent,
                    TypeDeLieu = cacheTypesDeLieu[l.type],
                    Ignorer = l.ignorer
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
            
            foreach (var lieuId in (parents))
            {
                result[lieuId].SousElements = result[lieuId].SousElements.OrderBy(c => c.Nom).ToList();
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
                    Martial = c.martial,
                    Nom = $"{c.nom}{(!string.IsNullOrWhiteSpace(c.spe) ? $" : {c.spe}" : "")}",
                    NomEn = c.nom_en,
                    Spe = c.spe ?? "",
                    Max = c.max,
                    Tests = c.tests ?? "",
                    Categ = c.categ,
                    CategSpe = c.categ_spe ?? "",
                    Resume = c.resume ?? "",
                    Description = c.description ?? "",
                    CaracteristiqueAssociee = c.carac ?? "",
                    AptitudeMereId = c.parent,
                    Contagieux = c.contagieux,
                    Guerison = c.guerison ?? "",
                    Severite = c.severite ?? 0,
                    IncompatiblesIds = c.incompatibles ?? new List<int>(),
                    AptitudesLieesIds = c.aptitudes ?? new List<int>(),
                })
                .ToDictionary(k => k.Id, v => v);

            foreach (var apt in result.Values)
            {
                apt.NomPourRecherche = GenericService.NettoyerPourRecherche(apt.Nom);
                apt.MotsClefDeRecherche = GenericService.MotsClefsDeRecherche(apt.NomPourRecherche);
                //apt.SetResume();
                //apt.SetDescription();
                apt.AptitudesLiees = apt.AptitudesLieesIds.Select(id => result[id]).OrderBy(a => a.NomComplet).ToList();
                apt.Incompatibles = apt.IncompatiblesIds.Select(id => result[id]).OrderBy(a => a.NomComplet).ToList();
            }
            
            // Compl??ter ToDo: voir si utile
            foreach (var apt in result.Values)
            {
                foreach (var aptLiee in apt.AptitudesLiees.Where(aptLiee => !aptLiee.AptitudesLiees.Contains(apt)))
                    aptLiee.AptitudesLiees.Add(apt);
                foreach (var aptInc in apt.Incompatibles.Where(aptInc => !aptInc.Incompatibles.Contains(apt)))
                    aptInc.Incompatibles.Add(apt);
            }

            foreach (var aptitude in result.Values.Where(c => c.AptitudeMereId.HasValue))
            {
                aptitude.Parent = result[aptitude.AptitudeMereId!.Value];
                if (aptitude.Description == "")
                    aptitude.Description = aptitude.Parent.Description;
                if (aptitude.Resume == "")
                    aptitude.Resume = aptitude.Parent.Resume;
                if (aptitude.CaracteristiqueAssociee == "" && aptitude.Parent.CaracteristiqueAssociee != "")
                    aptitude.CaracteristiqueAssociee = aptitude.Parent.CaracteristiqueAssociee;
                if (aptitude.Tests == "" && aptitude.Parent.Tests != "")
                    aptitude.Tests = aptitude.Parent.Tests;
                if (!aptitude.AptitudesLiees.Any())
                    aptitude.AptitudesLiees = aptitude.Parent.AptitudesLiees;
            }
            
            foreach (var aptitude in result.Values.Where(t => t.Parent != null).Select(t => t.Parent).Distinct())
            {
                aptitude!.SousElements.AddRange(result.Values
                    .Where(c => c.Parent == aptitude)
                    .OrderBy(c => c.Nom));
            }

            return result;
        }
        
        private static ProfilDto GetProfilDtoFromJson(JsonProfil p)
        {
            return new ProfilDto {
                Ag = p.ag,
                Cc = p.cc,
                Ct = p.ct,
                Dex = p.dex,
                E = p.e,
                F = p.f,
                Fm = p.fm,
                I = p.i,
                Int = p.intel,
                Soc = p.soc,
                M = p.m,
                B = p.b
            };
        }

        private static IEnumerable<CarriereDto> GetCarrieres(IEnumerable<int>? ids,
            IReadOnlyDictionary<int, CarriereDto> cache)
            => (ids ?? Array.Empty<int>()).Select(id => cache[id]).OrderBy(c => c.Nom);

        private static Dictionary<int, CarriereDto> InitializeCarrieres(
            IEnumerable<JsonCarriere> carrieres,
            IReadOnlyDictionary<int, RaceDto> cacheRaces,
            IReadOnlyDictionary<int, AptitudeDto> cacheAptitudes,
            IReadOnlyDictionary<int, ReferenceDto> cacheReferences)
        {
            var cacheCarrieres = carrieres
                .Select(c => new CarriereDto
                {
                    Id = c.id,
                    Groupe = c.groupe ?? "",
                    Statut = c.revenu ?? "",
                    Nom = c.nom,
                    NomAnglais = c.nom_en ?? "",
                    NiveauSpecifie = c.niveau,
                    MotsClefDeRecherche = GenericService.MotsClefsDeRecherche(GenericService.NettoyerPourRecherche(c.nom)),
                    Description = c.description,
                    Ambiance = c.ambiance ?? Array.Empty<string>(),
                    CarriereMereId = c.parent,
                    DebouchesIds = c.debouch ?? Array.Empty<int>(),
                    AvancementsIds = c.avancements ?? Array.Empty<int>(),
                    TirageInitial = (c.tirage ?? Array.Empty<JsonCarriereInitiale>())
                        .Select(ti => new TirageDto { Facteur = ti.f, Race = cacheRaces[ti.r]}).ToArray(),
                    Dotations = c.dotations,
                    Image = $"images/careers/{c.id}.png",
                    PlanDeCarriere = GetProfilDtoFromJson(c.profil),
                    Restriction = c.restriction ?? "",
                    Source = GetSourceFromJsonToDto(c.source, cacheReferences),
                    Leitmotiv = c.leitmotiv ?? "",
                    CompetenceDeMetier = c.metier == null ? null : cacheAptitudes[c.metier.Value],
                    Aptitudes = GetAptitudes(c.aptitudes, cacheAptitudes),
                    AptitudesChoix = (c.aptitudes_choix ?? Array.Empty<int[]>()).Select(choix => GetAptitudes(choix, cacheAptitudes).ToArray()).ToList(),
                })
                .ToDictionary(k => k.Id, v => v);

            foreach (var carriere in cacheCarrieres.Values.Where(c => c.CarriereMereId.HasValue))
            {
                carriere.Parent = cacheCarrieres[carriere.CarriereMereId!.Value];
                if (carriere.Description == "")
                    carriere.Description = carriere.Parent.Description;
                if (carriere.Restriction == "")
                    carriere.Restriction = carriere.Parent.Restriction;
                if (carriere.Leitmotiv == "")
                    carriere.Leitmotiv = carriere.Parent.Leitmotiv;
                if (!carriere.Ambiance.Any())
                    carriere.Ambiance = carriere.Parent.Ambiance;
                if (carriere.Groupe == "")
                    carriere.Groupe = carriere.Parent.Groupe;
                if (carriere.Statut == "")
                    carriere.Statut = carriere.Parent.Statut;
                carriere.CompetenceDeMetier ??= carriere.Parent.CompetenceDeMetier;
            }

            foreach (var carriere in cacheCarrieres.Values.Where(c => c.DebouchesIds.Any()))
                carriere.Debouches = GetCarrieres(carriere.DebouchesIds, cacheCarrieres).ToList();
            
            foreach (var carriere in cacheCarrieres.Values.Where(c => c.AvancementsIds.Any()))
                carriere.DebouchesDAvancements = GetCarrieres(carriere.AvancementsIds, cacheCarrieres).ToList();

            var tousLesAvancements = cacheCarrieres.Values.SelectMany(c => c.DebouchesDAvancements).Distinct().ToArray();
            foreach (var carriere in tousLesAvancements)
            {
                carriere.FilieresDAvancement = cacheCarrieres.Values.Where(c => c.DebouchesDAvancements.Contains(carriere)).OrderBy(c => c.Nom).ToList();
                if (carriere.Description == "")
                {
                    var first = carriere.FilieresDAvancement.First();
                    if (first.Description == "" && first.FilieresDAvancement.Any())
                        first = first.FilieresDAvancement.First();
                    if (first.Description == "" && first.FilieresDAvancement.Any())
                        first = first.FilieresDAvancement.First();
                    carriere.Description = first.Description;
                }
            }

            foreach (var carriere in cacheCarrieres.Values.Where(c => c.Parent != null).Select(c => c.Parent).Distinct())
                carriere!.SousElements.AddRange(cacheCarrieres.Values
                    .Where(c => c.Parent == carriere)
                    .OrderBy(c => c.Nom));

            foreach (var carriere in cacheCarrieres.Values)
                carriere.Filieres = cacheCarrieres.Values
                    .Where(c => c.Debouches.Contains(carriere))
                    .OrderBy(c => c.Nom)
                    .ToList();

            return cacheCarrieres;
        }

        private static SourceDto? GetSourceFromJsonToDto(JsonSource? source, IReadOnlyDictionary<int, ReferenceDto> references)
        {
            if (source?.id == null) return null;
            return new SourceDto
            {
                Book = references[source.id!.Value],
                Info = source.info ?? ""
            };
        }

        private static List<AptitudeDto> GetAptitudes(int[]? argAptitudes, IReadOnlyDictionary<int, AptitudeDto> cacheAptitudes)
        {
            return argAptitudes == null ? new List<AptitudeDto>()
                : argAptitudes.Select(id => cacheAptitudes[id]).OrderBy(a => a.NomComplet).ToList();
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