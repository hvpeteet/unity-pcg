using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public delegate void OnStatusUpdate(float percent_complete, string update_message);

public class RuinGenerator {

    public int num_rounds = 100;
    public int num_elite = 0;
    public int pop_size = 100;
    public int num_survivors = 0;

    public OnStatusUpdate on_status_update = (x, y) => {};
    public System.Random rng = new System.Random();

    public int dim_x = 10;
    public int dim_y = 10;
    public int dim_z = 10;

    private Blueprint[] population;

    // Creates a collection of empty blueprints.
    private Blueprint[] CreateEmptyPopulation()
    {
        Blueprint[] pop = new Blueprint[pop_size];
        for (int i = 0; i < pop.Length; i++)
        {
            pop[i] = new Blueprint(dim_x, dim_y, dim_z);
        }
        return pop;
    }

    int CompareBlueprints(Blueprint a, Blueprint b)
    {
        return CalculateScore(a).CompareTo(CalculateScore(b));
    }

    int CountBlocks(Blueprint blueprint)
    {
        int total = 0;
        foreach (int i in blueprint.GetBlocks())
        {
            total += i;
        }
        return total;
    }

    // Calculates the amount of volume that has a roof over it.
    int CalcCoveredVolume(Blueprint blueprint)
    {
        int total = 0;
        int[,,] blocks = blueprint.GetBlocks();
        for (int x = 0; x < blueprint.GetDims().x; x++)
        {
            for (int z = 0; z < blueprint.GetDims().z; z++)
            {
                bool has_cover = false;
                for (int y = blueprint.GetDims().y - 1; y >= 0; y--)
                {
                    if (blocks[x, y, z] > 0)
                    {
                        has_cover = true;
                    } else if (has_cover)
                    {
                        total++;
                    }
                }
            }

        }
        return total;
    }

    int CalculateScore(Blueprint blueprint)
    {
        int running_total = 0;
        // running_total += CountBlocks(blueprint);
        running_total += CalcCoveredVolume(blueprint);
        // TODO: Calculate scores/heuristics here
        return running_total;
    }

    public Blueprint GenerateRuin()
    {
        this.population = CreateEmptyPopulation();
        int init_count = 0;
        foreach (Blueprint b in population)
        {
            on_status_update(
                ((float)init_count) / ((float)(pop_size + num_rounds)), 
                string.Format("Initializing population {0} / {1}", init_count, pop_size));
            init_count++;
            b.RandomizeInto(b);
        }

        for (int round = 0; round < num_rounds; round++)
        {

            System.Array.Sort(population, (a, b) => CompareBlueprints(b, a));

            int[] scores = new int[population.Length];

            // Record scores for the current generation
            for (int i = 0; i < population.Length; i++)
            {
                scores[i] = CalculateScore(population[i]);
            }

            double[] cdf = Utils.CalcCDF(scores);

            // Setup the next generation            
            Blueprint[] next_generation = CreateEmptyPopulation();

            // Select elite
            for (int i = 0; i < num_elite; i++)
            {
                population[i].CopyInto(next_generation[i]);
            }

            // Select survivors
            for (int i = num_elite; i < num_survivors + num_elite; i++)
            {
                population[Utils.WeightedRandomIndex(cdf)].CopyInto(next_generation[i]);
            }

            // Select everyone else and mutate them
            for (int i = num_survivors + num_elite; i < next_generation.Length; i++)
            {
                population[Utils.WeightedRandomIndex(cdf)].MutateInto(next_generation[i]);
            }
            population = next_generation;
            on_status_update(
                ((float)init_count + round) / ((float)(pop_size + num_rounds)),
                string.Format("Completed {0} / {1} rounds of evolution", round, num_rounds));
            Debug.Log(string.Format("finished gen, best score {0}", scores.Max()));
        }
        System.Array.Sort(population, (a, b) => CompareBlueprints(b, a));
        return population[0];
    }


}
