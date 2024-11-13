using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System
{
    public class CommandParser
    {
        public void ParseCommand(string command)
        {

            var tokens = Utilities.Functions.StringSplit(command, ' ');
            var operation = tokens[0].ToUpper();

            switch (operation)
            {
                case "CREATE":
                    if (tokens[1] == "TABLE")
                    {
                        //TODO: Imlement logic -> CreateTable(command);
                    }
                    else if (tokens[1] == "INDEX")
                    {
                        //TODO: Imlement logic -> CreateIndex(command)
                    }
                    break;

                case "DROP":
                    if (tokens[1] == "TABLE")
                    {
                        //TODO: Imlement logic -> DropTable(tokens[2]);
                    }
                    else if (tokens[1] == "INDEX")
                    {
                        //TODO: Imlement logic -> DropIndex(tokens[2])
                    }
                    break;

                case "INSERT":
                    //TODO: Imlement logic -> InsertInto(command);
                    break;

                case "DELETE":
                    //TODO: Imlement logic -> DeleteFrom(command);
                    break;

                case "SELECT":
                    //TODO: Imlement logic -> Select(Command);
                    break;

                default:
                    Console.WriteLine("Invalid Command!");
                    break;
            }

        }
    }
}
