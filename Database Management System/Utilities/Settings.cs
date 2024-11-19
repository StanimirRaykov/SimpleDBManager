using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System.Utilities
{
    public static class Settings
    {
        public const string BaseDirectory = @"C:\Users\Stanimir\Documents\TEST FILES";
        static Settings()
        {
            // Ensure the base directory exists
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
                Console.WriteLine($"Base directory created: {BaseDirectory}");
            }
        }
    }
}
