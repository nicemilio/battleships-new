using System;
using battleships;

char choice = 'x';
const string UNDERLINE = "\x1B[4m";
const string RESET = "\x1B[0m";
while (choice != 's' && choice != 'c') {
    Console.WriteLine("Should this be a " + UNDERLINE + "s" + RESET + "erver or a " + UNDERLINE + "c" + RESET + "lient application?: ");
    choice = Console.ReadKey().KeyChar;
}
Console.WriteLine("");
if (choice == 's')
{
    Server myServer = new Server();
}
else
{
    board myBoard = new board(10, 10, true);
    //Console.WriteLine(myBoard.checkPosition(1, 9, 1, false));
    //myBoard.tryPlace(1, 7, 2, false);
    myBoard.PrintBoard();
}
