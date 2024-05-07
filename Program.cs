using NLog;
using Helper;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWContext();
    char option;
    // Update this array with each menu option added
    char[] optionArray = new char[] {'1', '2', '3', '4'};
    do
    {
        // Main user menu
        Console.WriteLine("Enter an option:");
        Console.WriteLine("1) Add a record");
        Console.WriteLine("2) Edit a record");
        Console.WriteLine("3) Display records");
        Console.WriteLine("4) Delete a record");
        Console.WriteLine("Enter anything else to quit.");
        option = Inputs.GetChar("> ", optionArray);
        logger.Info("User choice: {option}", option);

        switch(option) {

            // Add records
            // Choose whether to add a Product or Category
            case '1':
            break;

            // Edit records
            // Choose whether to edit Products or Categories
            case '2':
            break;

            // Display records
            // Add options to search or display all records or search by category
            // Ask if user wants to search through discontinued products as well
            // Color discontinued products red
            case '3':
            break; 

            // Delete records
            // Delete from Product or Category table
            // Make sure to account for any orphaned records on other tables
            case '4':
            break;

        }
    } while (optionArray.Contains(option));
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");
