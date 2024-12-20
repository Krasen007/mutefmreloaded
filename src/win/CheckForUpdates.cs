﻿using System.Net;

namespace MuteFmReloaded
{
	class CheckForUpdates
	{
		public static bool Check()
		{
			try
			{
				string url = "http://" + Constants.MuteFmDomain + "/patcher/update_" + Constants.VersionUnderscores + ".txt";
				WebRequest request = HttpWebRequest.Create(url);
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					return true;
				}
			}
			catch
			{
			}

			return false;
		}

		public static void Update()
		{
			//TODO: point to github for the updated app
			if (System.Windows.Forms.MessageBox.Show("A new version of " + Constants.ProgramName + " is available.  Would you like to download it?", Constants.ProgramName, System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
			{
				System.Windows.Forms.MessageBox.Show("To update " + Constants.ProgramName + ", please download and run the installer found at http://www.mutefm.com/prerelease/. \n\nIt is safe to install the new version on top of the older version.\n\nThe website will open in your browser after you click OK.", Constants.ProgramName);
				System.Diagnostics.Process.Start("http://www.mutefm.com/prerelease/");
			}
		}

		/*
        public static void Run()
        {
            // TODO: try to retrieve exe with appropriate name. Run it.  End application.


            try
            {
                string url = "http://" + Constants.MuteFmDomain + "/prerelease/update_" + Constants.VersionUnderscores + ".txt";
                WebRequest request = HttpWebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;

        }
         */

		// Issue: should it download the update automatically before asking user about running it???
	}
}
