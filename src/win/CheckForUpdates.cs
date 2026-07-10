using System;
using System.Net;
using System.Windows.Forms;

namespace MuteFmReloaded
{
	class CheckForUpdates
	{
		private const string GITHUB_REPO = "Krasen007/mutefmreloaded";
		private const string GITHUB_API_URL = "https://api.github.com/repos/" + GITHUB_REPO + "/releases/latest";

		public static bool Check()
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GITHUB_API_URL);
				request.UserAgent = "mute.fm-reloaded";
				request.Method = "GET";
				
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					if (response.StatusCode == HttpStatusCode.OK)
					{
						return true;
					}
				}
			}
			catch
			{
			}

			return false;
		}

		public static void Update()
		{
			if (MessageBox.Show("A new version of " + Constants.ProgramName + " is available. Would you like to download it?", Constants.ProgramName, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					string downloadUrl = "https://github.com/" + GITHUB_REPO + "/releases/latest";
					MessageBox.Show("The latest version will open in your browser. It is safe to install over the existing version.", Constants.ProgramName);
					System.Diagnostics.Process.Start(downloadUrl);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error opening browser: " + ex.Message, Constants.ProgramName);
				}
			}
		}
	}
}