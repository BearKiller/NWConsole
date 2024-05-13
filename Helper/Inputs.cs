using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Helper
{
    
    internal class Inputs
    {

        public static int GetInt(string prompt) {

            string path = Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}nlog.config";
            var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
            bool isSuccess = false;
            int returnValue = 0;
            
            do {
                Console.Write(prompt);
                string userInput = Console.ReadLine();

                isSuccess = Int32.TryParse(userInput, out returnValue);

                if (!isSuccess) {
                    logger.Error("Input must be an integer.");
                }
                
            } while (!isSuccess);
            return returnValue;

        }

        public static decimal GetDecimal(string prompt) {

            string path = Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}nlog.config";
            var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
            bool isSuccess = false;
            decimal returnValue = 0;
            
            do {
                Console.Write(prompt);
                string userInput = Console.ReadLine();

                isSuccess = Decimal.TryParse(userInput, out returnValue);

                if (!isSuccess) {
                    logger.Error("Input must be a decimal.");
                }
                
            } while (!isSuccess);
            return returnValue;
        }

        public static short GetShort(string prompt) {
            string path = Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}nlog.config";
            var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
            bool isSuccess = false;
            short returnValue = 0;

            do {
                Console.Write(prompt);
                string userInput = Console.ReadLine();

                isSuccess = Int16.TryParse(userInput, out returnValue);

                if (!isSuccess) {
                    logger.Error("Input must be a 16 bit integer");
                }
            } while (!isSuccess);
            return returnValue;
        }

        public static bool GetBool(string prompt) {
            string path = Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}nlog.config";
            var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
            bool isSuccess = false;
            bool returnValue = false;

            do {
                Console.Write(prompt);
                string userInput = Console.ReadLine();

                isSuccess = Boolean.TryParse(userInput, out returnValue);

                if (!isSuccess) {
                    logger.Error("Please enter either True or False");
                }

            } while (!isSuccess);

            return returnValue;      
        }

        public static string GetString(string prompt) {

            char[] illegalChars = new char[] {'\\', '|', '\'', '"', '/', '*', '+', '^', ';' };
            string path = Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}nlog.config";
            var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
            bool isSuccess = false;
            bool containsIllegalChar = true;
            string returnValue = null;

            do {
                Console.Write(prompt);
                string userInput = Console.ReadLine();

                if (!String.IsNullOrEmpty(userInput)) {
                    isSuccess = true;
                    containsIllegalChar = userInput.Any(x => illegalChars.Contains(x));

                } else {
                    logger.Error("Input can not be null.");
                }

                if (!containsIllegalChar) {
                    isSuccess = true;
                } else {
                    isSuccess = false;
                    logger.Error("Input must not contain illegal characters (\\|/\'\"*+^;')");
                }

                returnValue = userInput;
                
            } while (!isSuccess);

            return returnValue;
        }

        public static char GetChar(string prompt, char[] possibleAnswers) {

            string path = Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}nlog.config";
            var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();

            while (true) {
                Console.Write(prompt);
                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput)) {
                    logger.Error("Input can not be null.");
                    continue;
                }
                else {
                    if (userInput.Length > 1) {
                        logger.Error("Multiple characters entered. Please enter a single character.");
                        continue;
                    }
                }

                if (possibleAnswers.Contains(userInput[0])) {
                    return userInput[0];
                } else {
                    logger.Error("Input out of bounds.");
                    Console.WriteLine("Only the following characters are allowed: "
                        + String.Join(", ", possibleAnswers));
                    continue;
                }
            }
        }
    }
}