﻿namespace BlazorWjdr.Services
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class CompetencesEtTalentsService
    {
        private List<CompetenceDto>? _allCompetences;
        private Dictionary<int, CompetenceDto>? _cacheCompetences;

        private List<TalentDto>? _allTalents;
        private Dictionary<int, TalentDto>? _cacheTalents;

        public IEnumerable<CompetenceDto> GetCompetences(IEnumerable<int> ids) => ids.Select(GetCompetence).OrderBy(c => c.Nom).ToArray();
        public CompetenceDto GetCompetence(int id)
        {
            if (_cacheCompetences == null)
                Initialize();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return _cacheCompetences[id];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public Task<CompetenceDto[]> ItemsCompetences()
        {
            if (_allCompetences == null)
                Initialize();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return Task.FromResult(_allCompetences.ToArray());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        
        private IEnumerable<CompetenceDto> AllCompetences
        {
            get
            {
                if (_allCompetences == null)
                    Initialize();
#pragma warning disable CS8603 // Possible null reference return.
                return _allCompetences;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        public IEnumerable<TalentDto> GetTalents(IEnumerable<int> ids) => ids.Select(GetTalent).OrderBy(t => t.Nom).ToArray();
        public TalentDto GetTalent(int id)
        {
            if (_cacheTalents == null)
                Initialize();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return _cacheTalents[id];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public Task<TalentDto[]> ItemsTalents()
        {
            if (_allTalents == null)
                Initialize();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return Task.FromResult(_allTalents.ToArray());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        
        private IEnumerable<TalentDto> AllTalents
        {
            get
            {
                if (_allTalents == null)
                    Initialize();
#pragma warning disable CS8603 // Possible null reference return.
                return _allTalents;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        private void Initialize()
        {
            _allTalents = DataSource.JsonLoader
                .GetRootTalent()
                .items
                .Select(t => new TalentDto
                {
                    Id = t.id,
                    Nom = $"{t.nom}{(!string.IsNullOrWhiteSpace(t.specialisation) ? $" : {t.specialisation}" : "")}",
                    Description = t.description,
                    Ignore = t.ignorer,
                    Resume = t.resume,
                    Specialisation = t.specialisation,
                    TalentParentId = t.parent_id,
                    Trait = t.trait
                })
                .OrderBy(t => t.Nom)
                .ToList();

            _cacheTalents = _allTalents.ToDictionary(k => k.Id, v => v);

            foreach (var talent in _allTalents.Where(t => t.TalentParentId.HasValue))
#pragma warning disable CS8629 // Nullable value type may be null.
                talent.Parent = GetTalent(talent.TalentParentId.Value);
#pragma warning restore CS8629 // Nullable value type may be null.
            foreach (var talent in _allTalents)
                talent.SousElements.AddRange(_allTalents
                    .Where(c=>c.Parent == talent)
                    .OrderBy(c => c.Nom));

            _allCompetences = DataSource.JsonLoader
                .GetRootCompetence()
                .items
                .Select(c => new CompetenceDto
                {
                    Id = c.id,
                    Ignore = c.ignorer,
                    Nom = $"{c.nom}{(!string.IsNullOrWhiteSpace(c.specialisation) ? $" : {c.specialisation}" : "")}",
                    Resume = c.resume,
                    Specialisation = c.specialisation,
                    CaracteristiqueAssociee = c.carac,
                    EstUneCompetenceDeBase = c.de_base,
                    CompetenceMereId = c.fk_competencemereid,
                    TalentsLies = (c.fk_talentslies ?? Array.Empty<int>())
                        .Select(id => _cacheTalents[id])
                        .OrderBy(t => t.Nom)
                        .ToList()
                })
                .OrderBy(t => t.Nom)
                .ToList();

            _cacheCompetences = _allCompetences.ToDictionary(k => k.Id, v => v);

            foreach (var competence in _allCompetences.Where(c => c.CompetenceMereId.HasValue))
#pragma warning disable CS8629 // Nullable value type may be null.
                competence.Parent = GetCompetence(competence.CompetenceMereId.Value);
#pragma warning restore CS8629 // Nullable value type may be null.

            foreach (var competence in _allCompetences)
            {
                competence.NomPourRecherche = GenericService.ConvertirCaracteres(competence.Nom);
                competence.MotsClefDeRecherche = GenericService.MotsClefsDeRecherche(competence.NomPourRecherche);
                competence.SousElements.AddRange(_allCompetences
                    .Where(c=>c.Parent == competence)
                    .OrderBy(c => c.Nom));
                competence.SetResume();
            }

            foreach (var talent in _allTalents)
            {
                talent.NomPourRecherche = GenericService.ConvertirCaracteres(talent.Nom);
                talent.MotsClefDeRecherche = GenericService.MotsClefsDeRecherche(talent.NomPourRecherche);
                talent.CompetencesLiees = _allCompetences
                    .Where(c => c.TalentsLies.Contains(talent))
                    .ToList();
            }
        }

        #region Competences & Talents
        
        // Caractéristiques
        public TalentDto TalentGuerrierNe => GetTalent(34);
        public TalentDto TalentTireurDElite => GetTalent(90);
        public TalentDto TalentForceAccrue => GetTalent(29);
        public TalentDto TalentResistanceAccrue => GetTalent(69);
        public TalentDto TalentReflexesEclairs => GetTalent(80);
        public TalentDto TalentIntelligent => GetTalent(39);
        public TalentDto TalentSangFroid => GetTalent(76);
        public TalentDto TalentSociable => GetTalent(83);

        // Académique
        public CompetenceDto CompetenceGroupeConnaissancesAcademiques => GetCompetence(13);
        public CompetenceDto CompetenceGroupeConnaissancesGenerales => GetCompetence(14);
        public CompetenceDto CompetenceGroupeLangue => GetCompetence(39);
        public CompetenceDto CompetenceConnaissancesAcademiquesDeuxAuChoix => GetCompetence(169);
        public CompetenceDto CompetenceConnaissancesAcademiquesTroisAuChoix => GetCompetence(166);
        public CompetenceDto CompetenceLireEcrire => GetCompetence(42);
        public TalentDto TalentCalculMental => GetTalent(7);
        public TalentDto TalentLinguistique => GetTalent(42);

        // Arcanique
        public CompetenceDto CompetenceConnaissanceAcademiqueEsprits => GetCompetence(141);
        public CompetenceDto CompetenceConnaissanceAcademiqueMagie => GetCompetence(109);
        public CompetenceDto CompetenceConnaissanceAcademiqueNecromancie => GetCompetence(110);
        public CompetenceDto CompetenceFocalisation => GetCompetence(31);
        public CompetenceDto CompetenceLangageMystique => GetCompetence(36);
        public CompetenceDto CompetenceLangageMystiqueMagick => GetCompetence(162);
        public CompetenceDto CompetenceLangageMystiqueDemoniaque => GetCompetence(170);
        public CompetenceDto CompetenceLangageMystiqueElfeMystique => GetCompetence(171);
        public CompetenceDto CompetenceLangueClassique => GetCompetence(142);
        public CompetenceDto CompetenceSensDeLaMagie => GetCompetence(52);
        public TalentDto TalentHarmonieAethyrique => GetTalent(35);
        public TalentDto TalentMainsAgiles => GetTalent(48);
        public TalentDto TalentMeditation => GetTalent(63);
        public TalentDto TalentProjectilePuissant => GetTalent(66);
        public TalentDto TalentGroupeMagieCommune => GetTalent(44);
        public TalentDto TalentGroupeMagieMineure => GetTalent(45);
        public TalentDto TalentMagieNoire => GetTalent(46);
        public TalentDto TalentMagieVulgaire => GetTalent(47);

        // Martial
        public CompetenceDto CompetenceLangSecretBataille => GetCompetence(148);
        public TalentDto TalentAmbidextrie => GetTalent(5);
        public TalentDto TalentCoupsPrécis => GetTalent(20);
        public TalentDto TalentGroupeMaitrise => GetTalent(50);
        public TalentDto TalentSurSesGardes => GetTalent(85);
        public TalentDto TalentTroublant => GetTalent(91);
        public TalentDto TalentMaitriseUneAuChoix => GetTalent(153);
        public TalentDto TalentMaitriseDeuxAuChoix => GetTalent(152);
        
        // Martial CaC
        public CompetenceDto CompetenceEsquive => GetCompetence(26);
        public TalentDto TalentCombatADeuxArmes => GetTalent(155);
        public TalentDto TalentCombatDeRue => GetTalent(14);
        public TalentDto TalentCombattantVirevoltant => GetTalent(15);
        public TalentDto TalentCoupsAuBut => GetTalent(18);
        public TalentDto TalentCoupsPuissants => GetTalent(21);
        public TalentDto TalentCoupsAssomants => GetTalent(19);
        public TalentDto TalentDesarmement => GetTalent(23);
        public TalentDto TalentDurACuir => GetTalent(24);
        public TalentDto TalentFrenesie => GetTalent(30);
        public TalentDto TalentParadeEclair => GetTalent(65);
        public TalentDto TalentLutte => GetTalent(43);
        public TalentDto TalentEffrayant => GetTalent(26);
        public TalentDto TalentRobuste => GetTalent(74);
        public TalentDto TalentValeureux => GetTalent(92);
        public TalentDto TalentGroupeVertu => GetTalent(206);
        public TalentDto TalentMaitriseArmesDEscrime => GetTalent(57);
        public TalentDto TalentMaitriseArmesDeCavalerie => GetTalent(54);
        public TalentDto TalentMaitriseArmesDeParade => GetTalent(56);
        public TalentDto TalentMaitriseArmesLourdes => GetTalent(58);
        public TalentDto TalentMaitriseArmesParalisantes => GetTalent(60);
        public TalentDto TalentMaitriseFléaux => GetTalent(61);
        public List<TalentDto> TalentsMaitriseAuContact => new() {TalentMaitriseArmesDEscrime,TalentMaitriseArmesDeCavalerie,TalentMaitriseArmesDeParade,TalentMaitriseArmesLourdes,TalentMaitriseArmesParalisantes,TalentMaitriseFléaux}; 
        
        // Martial Distance
        public CompetenceDto CompetenceMetierArquebusier => GetCompetence(59);
        public TalentDto TalentAdresseAuTir => GetTalent(4);
        public TalentDto TalentRechergementRapide => GetTalent(67);
        public TalentDto TalentTirDePrecision => GetTalent(88);
        public TalentDto TalentTirEnPuissance => GetTalent(89);
        public TalentDto TalentMaitreArtilleur => GetTalent(49);
        public TalentDto TalentMaitriseArbaletes => GetTalent(51);
        public TalentDto TalentMaitriseArcsLongs => GetTalent(52);
        public TalentDto TalentMaitriseArmesAFeu => GetTalent(53);
        public TalentDto TalentMaitriseArmesDeJet => GetTalent(55);
        public TalentDto TalentMaitriseArmesMecaniques => GetTalent(59);
        public TalentDto TalentMaitriseLancePierres => GetTalent(62);
        
        // De l'ombre
        public CompetenceDto CompetenceAlphSecretVoleurs => GetCompetence(89);
        public CompetenceDto CompetenceLangSecretVoleurs => GetCompetence(147);
        public CompetenceDto CompetenceDeplacementSilencieux => GetCompetence(19);
        public CompetenceDto CompetenceDissimulation => GetCompetence(20);
        public CompetenceDto CompetenceFouille => GetCompetence(32);
        public CompetenceDto CompetencePerception => GetCompetence(48);
        public CompetenceDto CompetenceEscalade => GetCompetence(24);
        public CompetenceDto CompetenceCrochetage => GetCompetence(16);
        public CompetenceDto CompetenceDeguisement => GetCompetence(18);
        public CompetenceDto CompetenceEscamotage => GetCompetence(25);
        public CompetenceDto CompetencePreparationDePoisons => GetCompetence(50);
        public TalentDto TalentConnaissanceDesPieges => GetTalent(16);
        public TalentDto TalentCamouflageRural => GetTalent(8);
        public TalentDto TalentCamouflageSouterrain => GetTalent(9);
        public TalentDto TalentCamouflageUrbain => GetTalent(10);
        public TalentDto TalentCodeDeLaRue => GetTalent(13);
        public TalentDto TalentImitation => GetTalent(36);
        public TalentDto TalentSensAiguises => GetTalent(80);
        public TalentDto TalentAccuiteAuditive => GetTalent(2);
        public TalentDto TalentAccuiteVisuelle => GetTalent(3);
        public TalentDto TalentFilature => GetTalent(30);
        public TalentDto TalentLectureSurLesLevres => GetTalent(41);
        public TalentDto TalentPistage => GetTalent(49);
        
        // Sociales ( + TalentCodeDeLaRue)
        public CompetenceDto CompetenceBaratin => GetCompetence(4);
        public CompetenceDto CompetenceCharisme => GetCompetence(8);
        public CompetenceDto CompetenceCommandement => GetCompetence(9);
        public CompetenceDto CompetenceCommérage => GetCompetence(10);
        public CompetenceDto CompetenceIntimidation => GetCompetence(34);
        public TalentDto TalentEloquence => GetTalent(27);
        public TalentDto TalentOrateurNe => GetTalent(64);
        public TalentDto TalentPolitique => GetTalent(174);
        public TalentDto TalentEtiquette => GetTalent(28);
        public TalentDto TalentIntriguant => GetTalent(40);
        
        // Commerce  + TalentCalculMental
        public CompetenceDto CompetenceMarchandage => GetCompetence(43);
        public CompetenceDto CompetenceMetierMarchand => GetCompetence(78);
        public CompetenceDto CompetenceEvaluation => GetCompetence(179);
        public CompetenceDto CompetenceExpressionArtistiqueConteur => GetCompetence(123);
        public TalentDto TalentDurEnAffaires => GetTalent(25);

        // Cavalerie  + CompetenceMetierVendeurDeChevaux, TalentMaitriseArmesDeCavalerie
        public CompetenceDto CompetenceEquitation => GetCompetence(23);
        public CompetenceDto CompetenceEquitationCochonDeGuerre => GetCompetence(152);
        public CompetenceDto CompetenceExpressionArtistiqueAcrobatEquestre => GetCompetence(153);
        public CompetenceDto CompetenceEmpriseSurLesAnimaux => GetCompetence(22);
        public CompetenceDto CompetenceSoinsDesAnimaux => GetCompetence(54);
        public CompetenceDto CompetenceMetierGarconDEcurie => GetCompetence(180);
        public CompetenceDto CompetenceDressage => GetCompetence(21);
        public TalentDto TalentAcrobateEquestre => GetTalent(1);

        // Artisanat  + CompetencePreparationDePoisons, CompetenceEvaluation
        public CompetenceDto CompetenceLangageSecretGuilde => GetCompetence(158);
        public CompetenceDto CompetenceGroupeMetier => GetCompetence(44);
        public CompetenceDto CompetenceMetierDeuxAuChoix => GetCompetence(159);
        public CompetenceDto CompetenceMetierTroisAuChoix => GetCompetence(172);
        public CompetenceDto CompetenceConnaissancesAcademiquesArts => GetCompetence(102);
        public CompetenceDto CompetenceConnaissancesAcademiquesIngénierie => GetCompetence(108);
        public CompetenceDto CompetenceConnaissancesAcademiquesRunes => GetCompetence(112);
        public CompetenceDto CompetenceConnaissancesAcademiquesSciences => GetCompetence(113);
        public CompetenceDto CompetenceLangageMystiqueNain => GetCompetence(3);
        public TalentDto TalentRuneDeuxAuChoix => GetTalent(170);
        public TalentDto TalentRuneSixAuChoix => GetTalent(171);
        public TalentDto TalentRuneDixAuChoix => GetTalent(173);
        public TalentDto TalentRuneMajeureDeuxAuChoix => GetTalent(172);
        public CompetenceDto CompetenceCreationDeRunes => GetCompetence(6);
        public TalentDto TalentSavoirFaireNain => GetTalent(78);
        public TalentDto TalentTalentArtistique => GetTalent(86);
        
        // Rôdeurs  + CompetenceDeplacementSilencieux, CompetenceDissimulation, CompetenceEmpriseSurLesAnimaux, CompetenceGroupeLangue
        //          , CompetencePerception, CompetenceFouille, CompetenceEscalade, TalentLinguistique, TalentConnaissanceDesPieges
        public CompetenceDto CompetenceBraconnage => GetCompetence(5);
        public CompetenceDto CompetenceAlphabetSecretPisteurs => GetCompetence(86);
        public CompetenceDto CompetenceAlphabetSecretDeuxAuChoix => GetCompetence(2);
        public CompetenceDto CompetenceLangageSecretRodeurs => GetCompetence(149);
        public CompetenceDto CompetenceLangageSecretDeuxAuChoix => GetCompetence(38);
        public CompetenceDto CompetenceOrientation => GetCompetence(47);
        public CompetenceDto CompetenceMetierCartographe => GetCompetence(63);
        public CompetenceDto CompetenceSurvie => GetCompetence(55);
        public CompetenceDto CompetencePistage => GetCompetence(49);
        public TalentDto TalentSensDeLOrientation => GetTalent(81);
        public TalentDto TalentGrandVoyageur => GetTalent(33);
        public TalentDto TalentSixiemeSens => GetTalent(82);
        public TalentDto TalentCourseAPied => GetTalent(22);

        // Maritimes  + CompetenceOrientation, CompetenceMetierCartographe, TalentGrandVoyageur
        public CompetenceDto CompetenceCanotage => GetCompetence(7);
        public CompetenceDto CompetenceNatation => GetCompetence(45);
        public CompetenceDto CompetenceNavigation => GetCompetence(46);
        public CompetenceDto CompetenceConnaissancesAcademiquesAstronomie => GetCompetence(103);
        public CompetenceDto CompetenceConnaissancesAcademiquesPotamologie => GetCompetence(189);
        public CompetenceDto CompetenceMetierCharpentierNaval => GetCompetence(65);
        
        // Poudre noire  + CompetenceMetierArquebusier, TalentMaitriseArmesAFeu

        // Ami des bêtes  + CompetenceMetierGarconDEcurie
        public CompetenceDto CompetenceMetierVendeurDeChevaux => GetCompetence(177);
        public CompetenceDto CompetenceMetierMaitreChien => GetCompetence(178);
        public CompetenceDto CompetenceMetierFauconnerie => GetCompetence(179);
        public CompetenceDto CompetenceConnaissancesAcademiquesZoologie => GetCompetence(188);
        public CompetenceDto CompetenceMetierFermier => GetCompetence(74);
        public CompetenceDto CompetenceConduiteDAttelage => GetCompetence(12);
        
        #endregion
        
        public CompetenceDto[] RechercheCompetences(string searchText)
        {
            searchText = GenericService.ConvertirCaracteres(searchText);
            var motsClefRecherches = GenericService.MotsClefsDeRecherche(searchText);

            return AllCompetences
                .Where(c => c.NomPourRecherche.Contains(searchText)
                            || c.MotsClefDeRecherche.Intersect(motsClefRecherches).Any())
                .ToArray();
        }
        
        public TalentDto[] RechercheTalents(string searchText)
        {
            searchText = GenericService.ConvertirCaracteres(searchText);
            var motsClefRecherches = GenericService.MotsClefsDeRecherche(searchText);

            return AllTalents
                .Where(t => t.NomPourRecherche.Contains(searchText)
                            || t.MotsClefDeRecherche.Intersect(motsClefRecherches).Any())
                .ToArray();
        }
    }
}
