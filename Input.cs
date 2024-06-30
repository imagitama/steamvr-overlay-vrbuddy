using System;

namespace VRBuddy
{
    static class Input
    {
        public static string GetInput(string prompt, string existingValue, string defaultValue = "")
        {
            if ((existingValue == "" || existingValue == null) && defaultValue != "") {
                existingValue = defaultValue;
            }
            Console.WriteLine($"{prompt} {(defaultValue != "" ? $"(default: {defaultValue}):" : "")}");
            string input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? existingValue != "" ? existingValue : defaultValue : input;
        }

        public static float GetInputFloat(string prompt, float existingValue, float defaultValue)
        {
            Console.WriteLine($"{prompt} (default: {defaultValue.ToString("F1")}):");
            string input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? existingValue : float.Parse(input);
        }

        public static int GetInputInt(string prompt, int existingValue, int defaultValue)
        {
            Console.WriteLine($"{prompt} (default: {defaultValue}):");
            string input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? existingValue : int.Parse(input);
        }

        public static bool GetInputBool(string prompt, bool existingValue, bool defaultValue)
        {
            bool result = existingValue;

            while (true)
            {
                Console.WriteLine($"{prompt} (default: {(defaultValue ? "y" : "n")}) (y or n):");
                string input = Console.ReadLine().Trim().ToLower();

                if (string.IsNullOrWhiteSpace(input))
                {
                    return existingValue;
                }
                else if (input == "y")
                {
                    return true;
                }
                else if (input == "n")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
                }
            }
        }
    }
}