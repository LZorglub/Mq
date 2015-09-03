using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mq
{
    class Program
    {

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args"></param>
		static void Main(string[] args)
        {

            if (args == null || args.Count() == 0)
            {
                HelpPage();
                return;

            }
            Arguments options = null;

            try
            {
                options = new Arguments(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                HelpPage();
                return;
            }

            string qname = GetQueueFormat(options.ComputerName, options.QueueName);

            if (options.Mode == ToolMode.Export)
            {
                // Export
                ExportMessage(qname, options.IsTransactional, options.Max);
            }
            else if (options.Mode == ToolMode.Import)
            {
                // Import
                ImportMessage(qname, options.IsTransactional, options.Files);
            }
        }

        /// <summary>
        /// Write the help page 
        /// </summary>
        static void HelpPage()
        {
            Console.WriteLine("Usage :");
            Console.WriteLine("mq -[ei] computer queue [-t] [files] [-m maxValue] ");
            Console.WriteLine("Options :");
            Console.WriteLine("\t-e : export from msqueue");
            Console.WriteLine("\t-i : import to msqueue");
            Console.WriteLine("\t-m : Number max of message to export");
            Console.WriteLine("\t[files] : Files to import into msqueue");
        }

        /// <summary>
        /// Import specified files into specified queue
        /// </summary>
        /// <param name="qname"></param>
        /// <param name="isTransactional"></param>
        /// <param name="files"></param>
        static void ImportMessage(string qname, bool isTransactional, IEnumerable<string> files)
        {
            MessageQueue queue = new MessageQueue(qname);
            int count = 0;

            foreach (string pattern in files)
            {
                string directory = Path.GetDirectoryName(pattern);
                string searchPattern = Path.GetFileName(pattern);

                if (string.IsNullOrEmpty(directory))
                {
                    directory = Directory.GetCurrentDirectory();
                }

                foreach (string file in Directory.GetFiles(directory, searchPattern))
                {
                    byte[] buffer = File.ReadAllBytes(file);

                    Message m = new Message();
                    m.BodyStream = new MemoryStream(buffer);
                    m.BodyType = 768;
                    if (!isTransactional)
                    {
                        queue.Send(m);
                    }
                    else
                    {
                        using (MessageQueueTransaction mqt = new MessageQueueTransaction())
                        {
                            mqt.Begin();
                            queue.Send(m, mqt);
                            mqt.Commit();
                        }
                    }
                    count++;

                    Console.WriteLine(file);
                }
            }
            Console.WriteLine("Send " + count + " message(s) into " + qname);
        }

        /// <summary>
        /// Export message from msqueue
        /// </summary>
        /// <param name="qname"></param>
        /// <param name="isTransactional"></param>
		static void ExportMessage(string qname, bool isTransactional, int maxValue)
        {
            string rep = Directory.GetCurrentDirectory();
            MessageQueue queue = new MessageQueue(qname);
            TimeSpan time = TimeSpan.FromMilliseconds(1000);

            int count = 0;
            try
            {
                do
                {
                    if (!isTransactional)
                    {
                        using (Message m = queue.Receive(time))
                        {

                            if (m.BodyStream != null)
                            {
                                string f = System.IO.Path.Combine(rep, Path.GetFileName(Path.GetTempFileName()));
                                using (FileStream sw = File.Create(f))
                                {
                                    int l = 0; byte[] buffer = new byte[500];
                                    do
                                    {
                                        l = m.BodyStream.Read(buffer, 0, 500);
                                        if (l == 0) break;
                                        sw.Write(buffer, 0, l);
                                    } while (true);
                                }
                            }
                            count++;
                        }
                    }
                    else
                    {
                        using (MessageQueueTransaction mqt = new MessageQueueTransaction())
                        {
                            mqt.Begin();
                            using (Message m = queue.Receive(time, mqt))
                            {

                                if (m.BodyStream != null)
                                {
                                    string f = System.IO.Path.Combine(rep, Path.GetFileName(Path.GetTempFileName()));
                                    using (FileStream sw = File.Create(f))
                                    {
                                        int l = 0; byte[] buffer = new byte[500];
                                        do
                                        {
                                            l = m.BodyStream.Read(buffer, 0, 500);
                                            if (l == 0) break;
                                            sw.Write(buffer, 0, l);
                                        } while (true);
                                    }
                                }
                                count++;
                                mqt.Commit();
                            }
                        }
                    }
                } while (count < maxValue);
            }
            catch (System.Messaging.MessageQueueException e)
            {
                if (e.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                {
                    Console.WriteLine(e.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Retrieve " + count + " message(s)");
        }

        /// <summary>
        /// Get the queue name formatted from machine and queue name specified
        /// </summary>
        /// <param name="machineName"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
		private static string GetQueueFormat(string machineName, string queueName)
        {
            Regex regIpAddress = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Compiled);

            string retour = null;

            if (!string.IsNullOrEmpty(machineName) && !string.IsNullOrEmpty(queueName))
            {
                if (machineName.Trim() == ".")
                    retour = string.Format(@"{0}\Private$\{1}", machineName, queueName);
                else if (regIpAddress.IsMatch(machineName))
                    retour = string.Format("Formatname:DIRECT=TCP:{0}\\Private$\\{1}", machineName, queueName);
                else
                    retour = string.Format("Formatname:DIRECT=OS:{0}\\Private$\\{1}", machineName, queueName);
            }

            return retour;
        }
    }
}
