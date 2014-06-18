﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CommandHandler
{
    //class Test
    //{
    //    static void Main(string[] args)
    //    {
    //        string Command;
    //        Command Cmd;
    //        CommandParser.InitializeCommandsDictionary();
    //        while (true)
    //        {
    //            Command = Console.ReadLine();
    //            Cmd = CommandParser.ParseCommand(Command);
    //            Console.WriteLine(Cmd.Type);
    //            Console.WriteLine(Cmd.SourceID);
    //            Console.WriteLine(Cmd.DestinationID);
    //            Console.WriteLine(Cmd.Action_State);
    //            Console.WriteLine(Cmd.UserName);
    //            Console.WriteLine(Cmd.Password);
    //        }
    //    }
    //}
    public enum CommandType
    {
        Device_FirstConnection,
        Device_Reconnection,
        Device_WatchDog,
        Device_Acknowledgement,
        User_FirstConnection_SignIn,
        User_Reconnection_SignIn,
        User_FirstConnection_SignUp,
        User_Reconnection_SignUp,
        User_Action,
        User_Locate,
        Invalid
    }
    class Command
    {
        public CommandType Type { get; set; }
        public ushort SourceID { get; set; }
        public ushort DestinationID { get; set; }
        public ushort Action_State { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public Command(CommandType _Type, ushort _SourceID, ushort _DestinationID, ushort _Action_State, string _UserName, string _Password)
        {
            Type = _Type;
            SourceID = _SourceID;
            DestinationID = _DestinationID;
            Action_State = _Action_State;
            UserName = _UserName;
            Password = _Password;
        }

        public Command()
        {
        }
    }
    class CommandParser
    {
        static Dictionary<Regex, CommandType> Dict = new Dictionary<Regex, CommandType>();

        static bool isInCorrectFormat(string Command)
        {
            Regex GeneralCommandFormat = new Regex(@"^(\d+(,)\d*(,)\d*(,)\d*(,)\w*(,)\w*\.)");
            if (GeneralCommandFormat.IsMatch(Command))
                return true;
            else
                return false;
        }

        static CommandType GetCommandType(string Command)
        {
            foreach (var Element in Dict)
            {
                if (Element.Key.IsMatch(Command))
                    return Element.Value;
            }
            return CommandType.Invalid;
        }

        public static Command ParseCommand(string Command)
        {
            Command Cmd = new Command(CommandType.Invalid, 0, 0, 0, "x", "x");

            if (isInCorrectFormat(Command))
            {
                Cmd.Type = GetCommandType(Command);
                if (Cmd.Type == CommandType.Invalid)
                    return Cmd;
                else
                {
                    Command = Command.Remove(Command.Length - 1);           //Remove delimiter
                    string[] SplittedCommand = Command.Split(',');
                    if (!String.IsNullOrEmpty(SplittedCommand[1]))
                        Cmd.SourceID = Convert.ToUInt16(SplittedCommand[1]);
                    if (!String.IsNullOrEmpty(SplittedCommand[2]))
                        Cmd.DestinationID = Convert.ToUInt16(SplittedCommand[2]);
                    if (!String.IsNullOrEmpty(SplittedCommand[3]))
                        Cmd.Action_State = Convert.ToUInt16(SplittedCommand[3]);
                    Cmd.UserName = SplittedCommand[4];
                    Cmd.Password = SplittedCommand[5];
                    return Cmd;
                }
            }
            else
                return Cmd;
        }

        public static void InitializeCommandsDictionary()
        {
            Dict.Add(new Regex(@"^((0,,,,,)\.)"), CommandType.Device_FirstConnection);
            Dict.Add(new Regex(@"^((1,)\d+(,,,,)\.)"), CommandType.Device_Reconnection);
            Dict.Add(new Regex(@"^((2,)\d+(,,,,)\.)"), CommandType.Device_WatchDog);
            Dict.Add(new Regex(@"^((3,)\d+(,)\d+(,)\d+(,,)\.)"), CommandType.Device_Acknowledgement);
            Dict.Add(new Regex(@"^((4,,,,)\w+(,)\w+\.)"), CommandType.User_FirstConnection_SignIn);
            Dict.Add(new Regex(@"^((5,)\d+(,,,)\w+(,)\w+\.)"), CommandType.User_Reconnection_SignIn);
            Dict.Add(new Regex(@"^((6,,,,)\w+(,)\w+\.)"), CommandType.User_FirstConnection_SignUp);
            Dict.Add(new Regex(@"^((7,)\d+(,,,)\w+(,)\w+\.)"), CommandType.User_Reconnection_SignUp);
            Dict.Add(new Regex(@"^((8,)\d+(,)\d+(,)\d+(,,)\.)"), CommandType.User_Action);
        }
    }
}
