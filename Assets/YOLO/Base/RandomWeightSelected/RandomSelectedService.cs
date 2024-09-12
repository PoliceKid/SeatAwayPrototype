using System;
using System.Collections;
using System.Collections.Generic;

public class RandomSelectedService
{
    private Random random;

    public RandomSelectedService()
    {
        random = new Random();
    }

    public T SelectRandom<T>(Dictionary<T, double> itemsWithWeights)
    {
        if (itemsWithWeights == null || itemsWithWeights.Count == 0)
        {
            throw new ArgumentException("The itemsWithWeights null or empty.");
        }

        double totalWeight = 0;
        foreach (var item in itemsWithWeights)
        {
            totalWeight += item.Value;
        }

        double randomValue = random.NextDouble() * totalWeight;

        foreach (var item in itemsWithWeights)
        {
            if (randomValue < item.Value)
            {
                return item.Key;
            }
            randomValue -= item.Value;
        }
        throw new InvalidOperationException(" error random item.");
    }
}
