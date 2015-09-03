using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mq
{
    public enum ToolMode
    {
        NoSet,
        Import,
        Export
    }

    /// <summary>
    /// Represents arguments of command
    /// </summary>
    class Arguments
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="args"></param>
        public Arguments(string[] args)
        {
            CheckArguments(args);
        }

        /// <summary>
        /// Checks validity of arguments
        /// </summary>
        /// <param name="args"></param>
        private void CheckArguments(string[] args)
        {
            this.Max = Int32.MaxValue;
            List<string> files = new List<string>();

            for (int index = 0; index < args.Count(); index++)
            {
                string argument = args[index];

                switch (argument)
                {
                    // Transactional
                    case "-t":
                        IsTransactional = true;
                        break;
                    case "-e":
                        if (index + 2 >= args.Count())
                        {
                            throw new ArgumentException("Unexpected number of arguments");
                        }
                        else
                        {
                            if (Mode != ToolMode.NoSet)
                            {
                                throw new ArgumentException(string.Format("Mode is already defined. Argument : {0}", argument));
                            }
                            Mode = ToolMode.Export;
                            ComputerName = args[index + 1];
                            QueueName = args[index + 2];
                            index += 2;
                        }
                        break;
                    case "-i":
                        if (index + 2 >= args.Count())
                        {
                            throw new ArgumentException("Unexpected number of arguments");
                        }
                        else
                        {
                            if (Mode != ToolMode.NoSet)
                            {
                                throw new ArgumentException(string.Format("Mode is already defined. Argument : {0}", argument));
                            }
                            Mode = ToolMode.Import;
                            ComputerName = args[index + 1];
                            QueueName = args[index + 2];
                            index += 2;
                        }
                        break;
                    case "-m":
                    case "-max":
                        {
                            if (index + 1 >= args.Count())
                            {
                                throw new ArgumentException("Unexpected number of arguments for max value");
                            }
                            else
                            {
                                int maximum;
                                if (!Int32.TryParse(args[index + 1], out maximum))
                                {
                                    throw new ArgumentException("Unexpected value for max value");
                                }
                                this.Max = maximum;
                                index++;
                            }
                        }
                        break;
                    default:
                        if (argument.StartsWith("-"))
                            throw new ArgumentException(string.Format("Invalid argument : {0}", argument));
                        else
                            files.Add(argument);
                        break;
                }
            }

            if (this.Mode == ToolMode.NoSet)
                throw new ArgumentException("Mode is unspecified");

            this.Files = files.ToArray();
        }

        /// <summary>
        /// Gets if transactional option is set
        /// </summary>
        public bool IsTransactional { get; private set; }

        /// <summary>
        /// Gets the computer name 
        /// </summary>
        public string ComputerName { get; private set; }

        /// <summary>
        /// Gets the queue name
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// Gets the mode selected (Import/Export)
        /// </summary>
        public ToolMode Mode { get; private set; }

        /// <summary>
        /// Gets the maximum number of messages to dequeue
        /// </summary>
        public int Max { get; private set; }

        /// <summary>
        /// Gets the list of files to import into msqueue
        /// </summary>
        public string[] Files { get; private set; }
    }
}
