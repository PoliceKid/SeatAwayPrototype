using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public static class GameHelper 
{
    public static Queue<Unit> ShuffleQueue(Queue<Unit> unitQueue)
    {
        Random rng = new Random();
        var unitList = unitQueue.ToList();
        int n = unitList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Unit value = unitList[k];
            unitList[k] = unitList[n];
            unitList[n] = value;
        }
        return new Queue<Unit>(unitList);
    }
    public static string GetStringBeforeCharacter(string text, char character)
    {
        if (string.IsNullOrEmpty(text)) return " ";
        string[] parts = text.Split(character);
        string extractedString = parts[0];
        return extractedString;
    }
}
