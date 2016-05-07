using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCS_Manager
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AttachConsole(ATTACH_PARENT_PROCESS);

            //If no arguments are specified, launch windows form.
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //Launch login page and wait for it to close before
                //launching the main form with the connection string.
                LoginForm myLogin = new LoginForm();
                myLogin.ShowDialog();
                if (GlobalConnectionString.ConnectionString != null)
                {
                    Application.Run(new Form1(myLogin.username));
                    myLogin.Close();
                }
            }
            else
            {
                //Username, password, action, param
                if (args.Length == 4)
                {
                    LoginForm.login(args[0], args[1]);
                    if (!LoginForm.testConnection())
                    {
                        Console.WriteLine("Error: Invalid login information. Please try again.");
                    }
                    else
                    {
                        Console.WriteLine("Successfully connected to the database.");
                        int action = 0;
                        if (Int32.TryParse(args[2], out action))
                        {
                            if(action == 1)
                            {
                                //Parsed data
                                DCS_STORE myStore = new DCS_STORE(GlobalConnectionString.ConnectionString);
                                if (myStore.parseFile(args[3]) != null)
                                {
                                    if (myStore.storeData())
                                    {
                                        /*
                                        columns = new List<string>();
                                        setTableSelector();
                                        getColumns();
                                        */
                                        Console.WriteLine("Data successfully uploaded!");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Data was not uploaded successfully.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Data was not parsed successfully.");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Argument 3 must be an integer.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid argument count.");
                }
                Application.Exit();
            }
        }
    }
}
