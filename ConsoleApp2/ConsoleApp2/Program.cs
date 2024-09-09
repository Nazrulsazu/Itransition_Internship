using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RockPaperScissors
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if arguments are valid
            if (args.Length < 3 || args.Length % 2 == 0)
            {
                Console.WriteLine("Error: Please provide an odd number (>= 3) of non-repeating moves as arguments.");
                Console.WriteLine("Example: rock paper scissors");
                Console.Read();
                return;
            }
            if (args.Distinct().Count() != args.Length)
            {
                Console.WriteLine("Error: Duplicate moves are not allowed. Please provide non-repeating moves.");
                Console.WriteLine("Example: rock paper scissors");
                Console.Read();
                return;
            }
            // Generate cryptographically strong random key
            KeyGenerator keyGen = new KeyGenerator();
            byte[] key = keyGen.GenerateKey(256);
            string keyString = BitConverter.ToString(key).Replace("-", "");

            // Computer makes a move
            Random random = new Random();
            int computerMoveIndex = random.Next(args.Length);
            string computerMove = args[computerMoveIndex];

            // Generate HMAC
            HMACGenerator hmacGen = new HMACGenerator();
            string hmac = hmacGen.GenerateHMAC(key, computerMove);

            // Show HMAC to the user
            Console.WriteLine($"HMAC: {hmac}");

            // Show menu to the user
            Console.WriteLine("Available moves:");
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine($"{i + 1} - {args[i]}");
            }
            Console.WriteLine("0 - exit");
            Console.WriteLine("? - help");

            int userMoveIndex;
            while (true)
            {
                Console.Write("Enter your move: ");
                string input = Console.ReadLine();

                if (input == "0")
                {
                    Console.WriteLine("Game exited.");
                    return;
                }
                else if (input == "?")
                {
                    // Show the help table
                    Console.WriteLine("\nHelp table:");
                    TableGenerator tableGen = new TableGenerator();
                    Rules rules = new Rules(args);
                    tableGen.GenerateTable(args, rules);
                }
                else if (int.TryParse(input, out userMoveIndex) && userMoveIndex > 0 && userMoveIndex <= args.Length)
                {
                    userMoveIndex--; // Adjust for zero-based index
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please try again.");
                }
            }

            string userMove = args[userMoveIndex];

            // Determine the winner
            Rules ruleSet = new Rules(args);
            string result = ruleSet.DetermineWinner(userMoveIndex, computerMoveIndex);

            // Show the result
            Console.WriteLine($"Your move: {userMove}");
            Console.WriteLine($"Computer move: {computerMove}");
            Console.WriteLine($"You {result}!");
            Console.WriteLine($"HMAC key: {keyString}");
            Console.Read();
        }
    }

    class KeyGenerator
    {
        public byte[] GenerateKey(int bits)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] key = new byte[bits / 8];
                rng.GetBytes(key);
                return key;
            }
        }
    }

    class HMACGenerator
    {
        public string GenerateHMAC(byte[] key, string message)
        {
            using (var hmac = new HMACSHA256(key))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] hash = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }

    class Rules
    {
        private string[] _moves;

        public Rules(string[] moves)
        {
            _moves = moves;
        }

        public string DetermineWinner(int userMoveIndex, int computerMoveIndex)
        {
            int half = _moves.Length / 2;
            if (userMoveIndex == computerMoveIndex)
                return "draw";

            if ((userMoveIndex > computerMoveIndex && userMoveIndex - computerMoveIndex <= half) ||
                (computerMoveIndex > userMoveIndex && computerMoveIndex - userMoveIndex > half))
                return "lose";
            else
                return "win"; 
        }
    }

    class TableGenerator
    {
        public void GenerateTable(string[] moves, Rules rules)
        {
            int size = moves.Length + 1;

            // Create header row
            Console.Write("+-------------");
            for (int i = 0; i < moves.Length; i++)
            {
                Console.Write("+----------");
            }
            Console.WriteLine("+");

            Console.Write("| v PC\\User > |");
            for (int i = 0; i < moves.Length; i++)
            {
                Console.Write($" {moves[i],-7}  |");
            }
            Console.WriteLine();
            Console.Write("+-------------");
            for (int i = 0; i < moves.Length; i++)
            {
                Console.Write("+----------");
            }
            Console.WriteLine("+");

            // Create table rows
            for (int i = 0; i < moves.Length; i++)
            {
                Console.Write($"| {moves[i],-11} |");
                for (int j = 0; j < moves.Length; j++)
                {
                    if (i == j)
                    {
                        Console.Write($" {"Draw",-7}  |");
                    }
                    else
                    {
                        string result = rules.DetermineWinner(i, j);
                        Console.Write($" {result,-7}  |");
                    }
                }
                Console.WriteLine();

                Console.Write("+-------------");
                for (int j = 0; j < moves.Length; j++)
                {
                    Console.Write("+----------");
                }
                Console.WriteLine("+");
            }
        }
    }

}
