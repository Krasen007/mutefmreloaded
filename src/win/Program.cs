using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace MuteFmReloaded
{
	static public class Program
	{
		public static bool FirstTime = false;

		public static Thread SoundServerThread;
		public static Thread DoPeriodicTasksThread;

		public static bool Installed = true;
		private static string _identity = "";

		public static string Identity
		{
			get
			{
				if (_identity == "")
				{
					Random rnd = new Random();
					int r = rnd.Next(1, int.MaxValue);

					_identity = "user" + r.ToString();
				}
				return _identity;
			}

			set
			{
				_identity = value;
			}
		}

		public static bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}

		[STAThread]
		static void Main(string[] args)
		{
			MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogMsg(Constants.ProgramName + " loaded!");

			if (System.Environment.CommandLine.ToUpper().Contains("FIRSTTIME"))
				FirstTime = true;

			string appGuid = ((GuidAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

			string mutexId = string.Format("Global\\{{{0}}}", appGuid);

			using (var mutex = new Mutex(false, mutexId))
			{
				var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
				var securitySettings = new MutexSecurity();
				securitySettings.AddAccessRule(allowEveryoneRule);
				mutex.SetAccessControl(securitySettings);

				var hasHandle = false;
				try
				{
					try
					{
						hasHandle = mutex.WaitOne(500, false);

						if (hasHandle == false)
						{
							MuteFmReloaded.UiPackage.PlayerForm playerForm = new UiPackage.PlayerForm();
							playerForm.Show();
							playerForm.Init(true);
							Application.Run(playerForm);
							Environment.Exit(0);
						}
					}
					catch (AbandonedMutexException)
					{
						hasHandle = true;
					}

					Init(args);
				}
				finally
				{
					if (hasHandle)
						mutex.ReleaseMutex();
				}

				Environment.Exit(0);
			}
		}

		public static void Init(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			int verTimesTen = (System.Environment.OSVersion.Version.Major * 10) + (System.Environment.OSVersion.Version.Minor);

			if (verTimesTen < 61)
			{
				MessageBox.Show("This program requires Windows 7 or higher.");
				return;
			}

			DoPeriodicTasksThread = new Thread(new ThreadStart(DoPeriodicTasks));
			DoPeriodicTasksThread.Name = "DoPeriodicTasks";
			DoPeriodicTasksThread.Start();

			MuteFmReloaded.SmartVolManagerPackage.SoundServer.OnChange = MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.OnUpdateSoundSourceInfos;
			MuteFmReloaded.SmartVolManagerPackage.SoundServer.OnManualVolumeChange = MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.OnManualVolumeChange;
			MuteFmReloaded.SmartVolManagerPackage.SoundServer.OnMasterVolumeChange = MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.OnMasterVolumeChange;
			InitExtensions();

			SoundServerThread = new Thread(new ThreadStart(MuteFmReloaded.SmartVolManagerPackage.SoundServer.Init));
			SoundServerThread.Name = "SoundServer";

			UiPackage.UiCommands.InitUI(FirstTime);
		}

		public static void DoPeriodicTasks()
		{
			// Check for updates only on startup (once)
			try
			{
				if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.NotifyAboutUpdates == true && CheckForUpdates.Check())
				{
					CheckForUpdates.Update();
				}
			}
			catch (Exception ex)
			{
				MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
			}

			DateTime prevTime = DateTime.MinValue;
			while (true)
			{
				try
				{
					WebServer.ClearOldEntries(prevTime);
					prevTime = DateTime.Now;
				}
				catch (Exception ex)
				{
					MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
				}

				System.Threading.Thread.Sleep(new TimeSpan(4, 0, 0));
			}
		}

		public static void InitExtensions()
		{
			// Chrome extension integration placeholder
		}

		public static void ParseCommandLine()
		{
			// TODO: allow performing operations (play, pause, stop, mute, unmute, setvol, switchbgmusic)
		}
	}
}