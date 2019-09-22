using System;
using System.Collections.Generic;
using System.Linq;

namespace Zicore.Neat
{
    public class SpeciesCollection
    {
        public SpeciesCollection(NeatEvaluator evaluator)
        {
            Evaluator = evaluator;
        }

        public NeatEvaluator Evaluator { get; }
        public List<Species> SpeciesItems { get; private set; } = new List<Species>();

        //public Dictionary<Genome, Species> NextGeneration { get; private set; } = new Dictionary<Genome, Species>();

        public void ClearSpecies()
        {
            foreach (var species in SpeciesItems)
            {
                if (Evaluator.NeatConfig.UseSpeciesControl)
                {
                    if (species.Genomes.Count > Evaluator.NeatConfig.SpeciesControlTarget)
                        species.CompatibilityThresholdModifier -= Evaluator.NeatConfig.SpeciesControlRate;
                    if (species.Genomes.Count < Evaluator.NeatConfig.SpeciesControlTarget) 
                        species.CompatibilityThresholdModifier += Evaluator.NeatConfig.SpeciesControlRate;
                }

                species.Genomes.Clear();
                species.ClearFitness();
            }
        }

        public void ClearEmptySpecies()
        {
            var species = SpeciesItems.Where(x => x.Genomes.Count == 0).Select(x=>x).ToList();
            foreach (var s in species)
            {
                SpeciesItems.Remove(s);
            }
        }

        public void Speciate(Genome genome)
        {
            float deltaT = Evaluator.NeatConfig.CompatibilityThresholdDeltaT;
            float c1 = Evaluator.NeatConfig.DisjointCoefficientC1;
            float c2 = Evaluator.NeatConfig.ExcessCoefficientC2;
            float c3 = Evaluator.NeatConfig.WeightCoefficientC3;
            
            bool createNewSpecies = false;
            bool genomeAdded = false;

            if (SpeciesItems.Count == 0)
            {
                createNewSpecies = true;
            }
            else
            {
                foreach (var species in SpeciesItems)
                {
                    var rs = Genome.CompareGenomes(genome, species.Representative);
                    float delta = Genome.CalculateCompatibilityDistance(rs, c1, c2, c3);
                    if (delta < deltaT + species.CompatibilityThresholdModifier)
                    {
                        species.Add(genome);
                        genomeAdded = true;
                        break;
                    }
                }
            }

            if (createNewSpecies || !genomeAdded)
            {
                var species = new Species(Evaluator);
                species.Add(genome);
                species.SelectRepresentative();
                SpeciesItems.Add(species);
            }
        }

        public Dictionary<Genome, Species> Selection(int population)
        {
            if(SpeciesItems.Count == 0)
                throw new InvalidOperationException("No species found!?");

            Dictionary<Genome, Species> nextGeneration = new Dictionary<Genome, Species>();

            float sumOfAllAdjustedFitnesses = SpeciesItems.Sum(x => x.CalcuateAdjustedFitnessSum());
            
            var speciesByFitness = SpeciesItems.OrderByDescending(x => x.AdjustedFitnessSum).ToList();
            int rank = 1;
            
            float totalSum = sumOfAllAdjustedFitnesses;
            int offspringToDistribute = population;
            foreach (var species in speciesByFitness)
            {
                species.SetRank(rank);
                rank++;

                var fitnessSum = species.AdjustedFitnessSum;
                float offspringPerSpecies = fitnessSum / totalSum;
                totalSum -= fitnessSum;
                int offspring = (int)Math.Ceiling(offspringToDistribute * offspringPerSpecies);

                if (offspring > offspringToDistribute)
                    offspring = offspringToDistribute;

                int offspringToUse = offspring;
                int offspringAssigned = 0;

                var champ = species.Champion.Copy();
                if (champ != null && species.Genomes.Count > Evaluator.NeatConfig.ChampCopyThreshold)
                {
                    nextGeneration.Add(champ, species);
                    offspringAssigned++;
                    offspringToUse--;
                }

                if (Evaluator.NeatConfig.UseStagnation)
                {
                    float stagnationEqualTolerance = 0.001f;
                    float difference = species.AdjustedFitnessSum - species.LastFitness;

                    if (difference < stagnationEqualTolerance)
                    {
                        species.StagnationCounter++;
                        if (species.StagnationCounter > Evaluator.NeatConfig.StagnationThreshold)
                        {
                            offspringToUse = 0;
                            species.Stagnated = true;
                        }
                    }
                    else
                    {
                        species.StagnationCounter = 0;
                    }
                } 

                species.LastFitness = species.AdjustedFitnessSum;

                if (offspringToUse <= 0)
                {
                    continue; // skip this species, because it received no population at all
                }

                int offspringToMate = (int)Math.Floor(offspringToUse * Evaluator.NeatConfig.CrossoverOffspringRate);
                int offspringToClone = offspringToUse - offspringToMate;
                
                var selection = species.Selection();
                var adjustedFitnessGenomesLookup = selection.ToDictionary(x => x, x => (x.Fitness / species.Genomes.Count));
                
                for (int i = 0; i < offspringToMate; i++)
                {
                    // only mate interspecies if the rate matches and 
                    var doInterspeciesCrossover =
                        speciesByFitness.Count > 1 && Evaluator.GetRandomFloat() < Evaluator.NeatConfig.InterspeciesMatingRate;

                    if (doInterspeciesCrossover)
                    {
                        var p1 = GetRandomGenomeBiasedAdjustedFitness(adjustedFitnessGenomesLookup);

                        var intermateSpeciesList = speciesByFitness.Where(x => x != species).ToList();

                        var intermateSpeciesRndIndex = Evaluator.Random.Next(0, intermateSpeciesList.Count);
                        var intermateSpecies = intermateSpeciesList[intermateSpeciesRndIndex];

                        if (intermateSpecies.Genomes.Count > 0)
                        {
                            var intermateGenomeIndex = Evaluator.Random.Next(0, intermateSpecies.Genomes.Count);
                            var intermateGenome = intermateSpecies.Genomes.Items[intermateGenomeIndex];

                            var child = Genome.Crossover(Evaluator, p1, intermateGenome);
                            child.Mutate();
                            nextGeneration.Add(child, species);
                            offspringAssigned++;
                        }
                        else
                        {
                            doInterspeciesCrossover = false; // fallback when somehow there were no genomes in the other species
                        }
                    }
                    
                    if(!doInterspeciesCrossover)
                    {
                        var p1 = GetRandomGenomeBiasedAdjustedFitness(adjustedFitnessGenomesLookup);
                        var p2 = GetRandomGenomeBiasedAdjustedFitness(adjustedFitnessGenomesLookup);

                        var child = Genome.Crossover(Evaluator, p1, p2);
                        child.Mutate();
                        nextGeneration.Add(child, species);
                        offspringAssigned++;
                    }
                }

                for (int i = 0; i < offspringToClone; i++)
                {
                    var p1 = GetRandomGenomeBiasedAdjustedFitness(adjustedFitnessGenomesLookup);
                    var clone = p1.Copy();
                    clone.Mutate();
                    nextGeneration.Add(clone, species);
                    offspringAssigned++;
                }
                offspringToDistribute -= offspringAssigned;
            }

            return nextGeneration;
        }

        public Genome GetRandomGenomeBiasedAdjustedFitness(Dictionary<Genome, float> selection)
        {
            double completeWeight = selection.Sum(x => System.Math.Abs(x.Value));
            double r = Evaluator.Random.NextDouble() * completeWeight;
            double countWeight = 0.0;
            foreach (var fg in selection)
            {
                countWeight += System.Math.Abs(fg.Value);
                if (countWeight >= r)
                {
                    return fg.Key;
                }
            }
            throw new InvalidOperationException("Couldnt find a genome...");
        }
    }
}
