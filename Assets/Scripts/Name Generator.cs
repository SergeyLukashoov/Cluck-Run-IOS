using UnityEngine;
using System.Text;

public class NameGenerator
{
    private static string[] firstParts = { "Dark", "Shadow", "Blood", "Iron", "Storm", "Night", "Wolf", "Dragon", "Fire", "Ice" };
    private static string[] secondParts = { "Hunter", "Walker", "Slayer", "Guardian", "Killer", "Master", "Warrior", "Rider", "Breaker", "Creator" };
    private static string[] thirdParts = { "X", "Z", "99", "07", "42", "JR", "SR", "01", "TheBest", "Pro" };

    public static string GenerateName()
    {
        string first = firstParts[Random.Range(0, firstParts.Length)];
        string second = secondParts[Random.Range(0, secondParts.Length)];
        string third = thirdParts[Random.Range(0, thirdParts.Length)];
        
        return first + second + third;
    }
}