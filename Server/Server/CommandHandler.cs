using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;

namespace CommandHandler
{
/*    class Test
    {
        static void Main(string[] args)
        {
            string Command;
            Command Cmd;
            CommandParser.InitializeCommandsDictionary();
            while (true)
            {
                Command = Console.ReadLine();
                Command = Regex.Unescape(Command);
                Cmd = CommandParser.ParseCommand(Command);
                Console.WriteLine(Cmd.Type);
                Console.WriteLine(Cmd.SourceID);
                Console.WriteLine(Cmd.DestinationID);
                Console.WriteLine(Cmd.Action_State);
                Console.WriteLine(Cmd.UserName);
                Console.WriteLine(Cmd.Password);
            }
        }
    }
 */
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
        public byte Action_State { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public Command(CommandType _Type, ushort _SourceID, ushort _DestinationID, byte _Action_State, string _UserName, string _Password)
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

        virtual public void Execute(Socket S) { }
    }
    class CommandParser
    {
        static Dictionary<Regex, Command> Dict = new Dictionary<Regex, Command>();

        static bool isInCorrectFormat(string Command)
        {
            Regex GeneralCommandFormat = new Regex(@"^(\d+(,)(.{2}|.{0})(,)(.{2}|.{0})(,)(.{1}|.{0})(,)\w*(,)\w*)", RegexOptions.Singleline);
            if (GeneralCommandFormat.IsMatch(Command))
                return true;
            else
                return false;
        }
        static Command GetCommandType(string Command)
        {
            foreach (var Element in Dict)
            {
                if (Element.Key.IsMatch(Command))
                    return Element.Value;
            }
            Command InvalidCommand = new Invalid();
            return InvalidCommand;
        }
        public static Command ParseCommand(string Command)
        {
            Command Cmd = new Invalid();

            if (isInCorrectFormat(Command))
            {
                Cmd = GetCommandType(Command);
                if (Cmd.Type == CommandType.Invalid)
                    return Cmd;
                else
                {
                    string[] SplittedCommand = Command.Split(',');

                    if (!String.IsNullOrEmpty(SplittedCommand[1]))
                        Cmd.SourceID = BitConverter.ToUInt16(Encoding.GetEncoding(437).GetBytes(SplittedCommand[1]), 0);
                    if (!String.IsNullOrEmpty(SplittedCommand[2]))
                        Cmd.DestinationID = BitConverter.ToUInt16(Encoding.GetEncoding(437).GetBytes(SplittedCommand[2]), 0);
                    if (!String.IsNullOrEmpty(SplittedCommand[3]))
                        Cmd.Action_State = Encoding.GetEncoding(437).GetBytes(SplittedCommand[3])[0];
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
            Dict.Add(new Regex(@"^((0,,,,,))", RegexOptions.Singleline), new Device_FirstConnection());
            Dict.Add(new Regex(@"^((1,).{2}(,,,,))", RegexOptions.Singleline), new Device_Reconnection());
            Dict.Add(new Regex(@"^((2,).{2}(,,,,))", RegexOptions.Singleline), new Device_WatchDog());
            Dict.Add(new Regex(@"^((3,).{2}(,).{2}(,).{1}(,,))", RegexOptions.Singleline), new Device_Acknowledgement());
            Dict.Add(new Regex(@"^((4,,,,)\w+(,)\w+\.)", RegexOptions.Singleline), new User_FirstConnection_SignIn());
            Dict.Add(new Regex(@"^((5,).{2}(,,,)\w+(,)\w+)", RegexOptions.Singleline), new User_Reconnection_SignIn());
            Dict.Add(new Regex(@"^((6,,,,)\w+(,)\w+)", RegexOptions.Singleline), new User_FirstConnection_SignUp());
            Dict.Add(new Regex(@"^((7,).{2}(,,,)\w+(,)\w+)", RegexOptions.Singleline), new User_Reconnection_SignUp());
            Dict.Add(new Regex(@"^((8,).{2}(,).{2}(,).{1}(,,))", RegexOptions.Singleline), new User_Action());
        }
    }
}
