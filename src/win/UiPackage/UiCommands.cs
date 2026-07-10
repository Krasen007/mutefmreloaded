using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace MuteFmReloaded.UiPackage
{
	public class UiCommands
	{
		public static PlayerForm mPlayerForm = null;
		private static KeyboardHook _hook = new KeyboardHook();
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		private static Operation _validOperation;
		private static bool _isVisible, _isRunning, _playerVisible;

		private static System.ComponentModel.BackgroundWorker _autoShowAfterPlayWorker = null;

		public static bool TrayLoaded = false;

		// Must be run within UI thread
		public static void InitUI(bool firstTime)
		{
			SetNotification(Constants.ProgramName + " started", false);

			if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys == null)
				MuteFmConfigUtil.LoadDefaultHotkeys(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);

			if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.SoundPollIntervalInS == 0)
				SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.SoundPollIntervalInS = MuteFmConfig.SoundPollIntervalDefault;

			RegisterHotkeys();

			UiPackage.UiCommands.UpdateUiForState(MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.GetValidOperation(), false, false, true);

			mPlayerForm = new PlayerForm();
			mPlayerForm.FormClosed += new FormClosedEventHandler(mPlayer_FormClosed);
			mPlayerForm.Init(false);

			if (firstTime)
			{
				System.ComponentModel.BackgroundWorker firstTimeWorker = new BackgroundWorker();
				firstTimeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(DoFirstTimeWork);
				firstTimeWorker.RunWorkerAsync();

				mPlayerForm.ToggleTopmost(true);
			}
			else
			{
				if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.PlayMusicOnStartup)
				{
					System.ComponentModel.BackgroundWorker firstTimeWorker = new BackgroundWorker();
					firstTimeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(delegate
					{
						SoundPlayerInfo playerInfo = (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GetActiveBgMusic());

						System.Threading.Thread.Sleep(2000);
						OnOperation(Operation.Play, playerInfo.AutoPlaysOnStartup, false);

						if (playerInfo.AutoPlaysOnStartup == false)
						{
							System.ComponentModel.BackgroundWorker firstTimeWorker2 = new BackgroundWorker();
							firstTimeWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(delegate
							{
								OnOperation(Operation.Play);
							});
							firstTimeWorker2.RunWorkerAsync();
						}
						System.Threading.Thread.Sleep(1000);
						OnOperation(Operation.Minimize);
					});
					firstTimeWorker.RunWorkerAsync();
				}
			}

			System.Windows.Forms.Application.Run(MuteFmReloaded.UiPackage.WinSoundServerSysTray.Instance);
		}

		public static void DoFirstTimeWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			System.Threading.Thread.Sleep(2000);
			UiPackage.UiCommands.ShowMixer();
			OnOperation(Operation.Play);
		}

		private static DateTime _prevNotifyDateTime = DateTime.MinValue;
		private static string _prevNotifyText = "";

		public static void SetNotification(string text, bool useBgMusicIcon)
		{
			try
			{
				// Don't send the same message multiple times over a short interval
				if ((text == _prevNotifyText) && (TimeSpan.Compare(DateTime.Now.Subtract(_prevNotifyDateTime).Duration(), new TimeSpan(0, 0, 0, 5, 0)) < 0))
				{
					return;
				}
				_prevNotifyDateTime = DateTime.Now;
				_prevNotifyText = text;

				if (mPlayerForm != null)
				{
					mPlayerForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
					{
						mPlayerForm.SetStatusText(text);
					});
				}

				if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.ShowBalloonNotifications)
				{
					MuteFmReloaded.UiPackage.WinSoundServerSysTray.Instance.ShowBalloonTip(3000, Constants.ProgramName, text, ToolTipIcon.Info);
				}
			}
			catch (Exception ex)
			{
				MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
			}
		}

		public static void ShowMixer()
		{
			MuteFmReloaded.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
			{
				mPlayerForm.Show();
				mPlayerForm.Visible = true;
				_playerVisible = true;
				mPlayerForm.WindowState = FormWindowState.Normal;
				mPlayerForm.Activate();
				UpdateUiForState();
			});
		}
		public static void HideMixer()
		{
			if (mPlayerForm == null)
				return;
			MuteFmReloaded.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
			{
				mPlayerForm.Hide();
				_playerVisible = false;
				UpdateUiForState();
			});
		}

		public static void UpdatePlayerVisibleState(bool playerVisible)
		{
			UpdateUiForState(_validOperation, _isVisible, _isRunning, playerVisible);
		}
		public static void UpdateUiForState()
		{
			UpdateUiForState(_validOperation, _isVisible, _isRunning, _playerVisible);
		}
		public static void UpdateUiForState(Operation validOperation, bool isVisible, bool isRunning)
		{
			UpdateUiForState(validOperation, isVisible, isRunning, _playerVisible);
		}
		public static void UpdateUiForState(Operation validOperation, bool isVisible, bool isRunning, bool playerVisible)
		{
			try
			{
				_validOperation = validOperation;
				_isVisible = isVisible;
				_isRunning = isRunning;
				_playerVisible = playerVisible;

				PlayerStateSendData playerState = new PlayerStateSendData(
					validOperation,
					isVisible,
					isRunning,
					MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.ActiveBgMusic,
					MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.FgMusics,
					SmartVolManagerPackage.BgMusicManager.BgMusicVolume,
					SmartVolManagerPackage.BgMusicManager.BgMusicMuted,
					SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic,
					SmartVolManagerPackage.BgMusicManager.AutoMuted,
					SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled,
					SmartVolManagerPackage.BgMusicManager.ForegroundSoundPlaying,
					SmartVolManagerPackage.BgMusicManager.MasterVol,
					SmartVolManagerPackage.BgMusicManager.MasterMuted
				);

				// Tray
				if (TrayLoaded)
				{
					try
					{
						WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
						{
							if (WinSoundServerSysTray.Instance.Visible)
								WinSoundServerSysTray.Instance.UpdateTrayMenu(validOperation, isVisible, isRunning, playerVisible);
						});
					}
					catch (Exception ex)
					{
						MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
					}
				}

				// Mixer
				if ((mPlayerForm != null) && (mPlayerForm.IsHandleCreated))
				{
					try
					{
						mPlayerForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
						{
							UiCommands.mPlayerForm.UpdateUI(playerState);
						});
					}
					catch (Exception ex)
					{
						MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
					}
				}
			}
			catch (Exception ex)
			{
				MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
			}
		}

		// Code that models how the user interacts with the application via the systray and player UIs.
		public static void OnOperation(Operation op)
		{
			OnOperation(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, op, null, false, true);
		}
		public static void OnOperation(Operation op, bool ignoreCommand, bool track)
		{
			OnOperation(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, op, null, ignoreCommand, track);
		}
		public static void OnOperation(Operation op, bool track)
		{
			OnOperation(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, op, null, false, track);
		}
		public static void OnOperation(long musicId, Operation op, string param)
		{
			OnOperation(musicId, op, param, false, true);
		}
		public static void OnOperation(long musicId, Operation op, string param, bool ignoreCommand, bool track)
		{
			if (MuteFmReloaded.UiPackage.WinSoundServerSysTray.Instance == null)
				return;

			MuteFmReloaded.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
			{
				if ((musicId == SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id) && ((op == Operation.Play) || (op == Operation.Unmute)))
				{
					MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.PerformOperation(musicId, Operation.ClearHistory, "", ignoreCommand);
				}

				if (op == Operation.Show)
					System.Threading.Thread.Sleep(250); // Ensure that window is shown after click sets focus to browser

				// Queue up background music if a foreground sound is active
				if ((musicId == SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id) &&
					(((op == Operation.Play) || (op == Operation.Unmute)) &&
					(SmartVolManagerPackage.BgMusicManager.EffectiveSilenceDateTime == DateTime.MaxValue) &&
					(!SmartVolManagerPackage.BgMusicManager.BgMusicHeard) &&
					(SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled)))
				{
					MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = true;
					MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.PerformOperation(musicId, Operation.AutoMutedPlay, param, ignoreCommand);
					return;
				}

				MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.PerformOperation(musicId, op, param, ignoreCommand);

				if ((musicId == SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id) || (op == Operation.ChangeMusic))
				{
					// Extra logic because we know user chose to perform the operation
					switch (op)
					{
						case Operation.Play:
							SmartVolManagerPackage.BgMusicManager.AutoMuted = false;
							MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = true;
							break;

						case Operation.ChangeMusic:
							SoundPlayerInfo playerInfo = SmartVolManagerPackage.BgMusicManager.FindPlayerInfo(musicId);
							if (playerInfo != null)
							{
								SmartVolManagerPackage.BgMusicManager.UserMustClickPlay = false; // reset it
								if (playerInfo.Id <= 0)
								{
									MuteFmConfigUtil.AddSoundPlayerInfo(playerInfo, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
								}

								SmartVolManagerPackage.BgMusicManager.AlbumArtFileName = "";
								SmartVolManagerPackage.BgMusicManager.TrackName = "";

								if (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.IsWeb)
									OnOperation(Operation.Stop);
								else
									OnOperation(Operation.Pause);

								SmartVolManagerPackage.BgMusicManager.ActiveBgMusic = playerInfo;
								SmartVolManagerPackage.BgMusicManager.BgMusicPids = new int[0];
								SmartVolManagerPackage.BgMusicManager.BgMusicVolInit = false;
								OnOperation(Operation.Play);
								UiPackage.UiCommands.UpdateUiForState();

								// If shows up as a fgmusic but not as a bgmusic, add to bgmusics and remove from fgmusic
								if (MuteFmConfigUtil.FindBgMusic(playerInfo.UrlOrCommandLine, SmartVolManagerPackage.BgMusicManager.MuteFmConfig) == null)
								{
									long tempId = playerInfo.Id;
									MuteFmConfigUtil.AddSoundPlayerInfo(playerInfo, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
									playerInfo.Id = tempId;

									var fgMusicList = new List<MuteFmReloaded.SoundPlayerInfo>(SmartVolManagerPackage.BgMusicManager.FgMusics);
									fgMusicList.Remove(playerInfo);
									SmartVolManagerPackage.BgMusicManager.FgMusics = fgMusicList.ToArray();
								}

								// Save current music as new default
								MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.MuteFmConfig.ActiveBgMusicId = musicId;
								MuteFmConfigUtil.Save(MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
							}
							break;

						case Operation.Stop:
							MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = false;
							if (_autoShowAfterPlayWorker != null)
							{
								_autoShowAfterPlayWorker.CancelAsync();
								_autoShowAfterPlayWorker = null;
							}
							break;
						case Operation.Pause:

						case Operation.Mute:
							MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = false;

							_autoShowAfterPlayWorker = new System.ComponentModel.BackgroundWorker();
							_autoShowAfterPlayWorker.WorkerSupportsCancellation = true;
							_autoShowAfterPlayWorker.DoWork += new DoWorkEventHandler(_isPausing_DoWork);
							_autoShowAfterPlayWorker.RunWorkerAsync();
							break;
						case Operation.Unmute:
							MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = true;
							break;
						case Operation.Exit:
							Exit();
							break;
					}
				}
			});
			if (op == Operation.Exit)
			{
				Exit();
			}
		}
		public static void ShowSite(string title, string url)
		{
			OnOperation(Operation.Stop);

			MuteFmReloaded.SoundPlayerInfo bgm = new MuteFmReloaded.SoundPlayerInfo();
			bgm.IsWeb = true;
			bgm.UrlOrCommandLine = url;
			bgm.Name = title;
			bgm.Id = -1;
			MuteFmConfigUtil.GenerateIconImage(bgm, false);

			SmartVolManagerPackage.BgMusicManager.ActiveBgMusic = bgm;
			SmartVolManagerPackage.BgMusicManager.BgMusicPids = new int[0];

			OnOperation(Operation.Show);
		}
		public static void Exit()
		{
			MuteFmReloaded.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
			{
				UnregisterHotkeys();

				// Signal the sound server thread to stop gracefully
				if (Program.SoundServerThread != null && !Program.SoundServerThread.Join(1000))
				{
					// Thread didn't stop gracefully, but we don't use Thread.Abort anymore
					// The thread will exit on its own when the application closes
				}
			});

			SmartVolManagerPackage.SoundEventLogger.LogMsg("Exiting...");
			SmartVolManagerPackage.SoundEventLogger.Close();
			try
			{
				UiPackage.WinSoundServerSysTray.Instance.Close();
				System.Windows.Forms.Application.Exit();
			}
			catch (Exception ex)
			{
				MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
			}
		}

		private static void _isPausing_DoWork(object sender, DoWorkEventArgs e)
		{
			// Prevent automute from seeing a brief pause as something that requires restoring again
			System.Threading.Thread.CurrentThread.Name = "IsPausing";
			MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.IsPausing = true;
			System.Threading.Thread.Sleep(750);
			MuteFmReloaded.SmartVolManagerPackage.BgMusicManager.IsPausing = false;
		}

		public static void UnregisterHotkeys()
		{
			_hook.UnregisterAllHotkeys();
		}

		public static void RegisterHotkeys()
		{
			_hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(Hook_KeyPressed);
			for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys.Length; i++)
			{
				if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Name == "Toggle muting music/videos")
					continue;

				if ((((Keys)SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Key) != Keys.None) && (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Enabled))
				{
					try
					{
						_hook.RegisterHotKey((Keys)SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Key);
					}
					catch (Exception ex)
					{
						MuteFmReloaded.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
					}
				}
			}
		}

		private static DateTime _lastKeyPress = DateTime.MinValue;

		private static void Hook_KeyPressed(object sender, KeyPressedEventArgs e)
		{
			if (DateTime.Now.Subtract(_lastKeyPress).TotalMilliseconds < 200) // Don't allow rapid keypresses
				return;

			_lastKeyPress = DateTime.Now;

			long key = (long)e.Key;
			if (0 != (e.Modifier & ModifierKeys.Alt))
				key |= (long)Keys.Alt;
			if (0 != (e.Modifier & ModifierKeys.Control))
				key |= (long)Keys.Control;
			if (0 != (e.Modifier & ModifierKeys.Shift))
				key |= (long)Keys.Shift;
			if (0 != (e.Modifier & ModifierKeys.Win))
				key |= (long)Keys.LWin;

			for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys.Length; i++)
			{
				Hotkey hotkey = SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i];
				if (hotkey.Key == key)
				{
					switch (hotkey.Name.ToLower())
					{
						case "play":
							UiCommands.OnOperation(Operation.Play);
							break;
						case "pause":
							UiCommands.OnOperation(Operation.Pause);
							break;
						case "stop":
							UiCommands.OnOperation(Operation.Stop);
							break;
						case "mute":
							UiCommands.OnOperation(Operation.Mute);
							break;
						case "unmute":
							UiCommands.OnOperation(Operation.Unmute);
							break;
						case "previous track":
							UiCommands.OnOperation(Operation.PrevTrack);
							break;
						case "next track":
							UiCommands.OnOperation(Operation.NextTrack);
							break;
						case "show":
							UiCommands.OnOperation(Operation.Show);
							break;
						case "toggle muting music/videos":
							SmartVolManagerPackage.BgMusicManager.ToggleFgMute();
							UiCommands.OnOperation(Operation.Restore);
							break;
						default:
							break;
					}
				}
			}
		}

		private static void mPlayer_FormClosed(object sender, FormClosedEventArgs e)
		{
			mPlayerForm = null;
		}
	}
}