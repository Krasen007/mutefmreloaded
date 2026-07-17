using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
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
						using (var reader = new StreamReader(response.GetResponseStream()))
						{
							string json = reader.ReadToEnd();
							
							// First try to extract from asset name (mute_fm_reloaded-v0.10.0.0.zip)
							// Pattern: "name":"mute_fm_reloaded-v0.10.0.0.zip"
							var assetMatch = Regex.Match(json, "\"name\":\"mute_fm_reloaded-v([\\d\\.]+)\\.zip\"");
							if (assetMatch.Success)
							{
								string versionStr = assetMatch.Groups[1].Value;
								Version remoteVersion = new Version(versionStr);
								Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;
								
								// Only return true if remote version is greater
								if (remoteVersion > localVersion)
								{
									return true;
								}
							}
							else
							{
								// Fallback: extract from tag_name
								var tagMatch = Regex.Match(json, "\"tag_name\":\"([^\"]+)\"");
								if (tagMatch.Success)
								{
									string tagName = tagMatch.Groups[1].Value;
									Version remoteVersion = ParseVersionFromTag(tagName);
									Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;
									
									if (remoteVersion != null && remoteVersion > localVersion)
									{
										return true;
									}
								}
							}
						}
					}
				}
			}
			catch
			{
			}

			return false;
		}

		private static Version ParseVersionFromTag(string tagName)
		{
			if (string.IsNullOrEmpty(tagName))
				return null;
			
			// Match version patterns like "v0.10.0.0" or "v0.10.0.0"
			var match = Regex.Match(tagName, @"(\d+\.\d+\.\d+\.\d+)");
			if (match.Success)
			{
				return new Version(match.Groups[1].Value);
			}
			return null;
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
