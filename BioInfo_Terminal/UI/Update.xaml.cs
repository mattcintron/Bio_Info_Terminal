using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using BioInfo_Terminal.Methods.Updater;
using MessageBox = System.Windows.Forms.MessageBox;

namespace BioInfo_Terminal.UI
{
    /// <summary>
    ///     Interaction logic for Update.xaml
    /// </summary>
    public partial class Update
    {
        //pass bio info updater
        private readonly BioInfoUpdater _updater; // check for program updates 

        public Update(BioInfoUpdater bioInfoUpdater)
        {
            InitializeComponent();
            _updater = bioInfoUpdater;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusLabel.Content = _updater.Checking ? "Checking for updates..." : "Ready.";
                CheckUpdates();
                FillData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error: " + ex.Message);
            }
        }

        private void RepoStoreLoaded(object s, AsyncCompletedEventArgs args)
        {
            try
            {
                if (_updater.Enabled) FillData();
            }
            catch (Exception ex)
            {
                StatusLabel.Content = "Couldn't fetch updates.";
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _updater.RepoUpdated -= RepoStoreLoaded;
            }
        }

        private void CheckUpdates()
        {
            if (_updater.Checking)
            {
                StatusLabel.Content = "Already checking for updates.";
                return;
            }

            try
            {
                StatusLabel.Content = "Checking for updates.";
                _updater.RepoUpdated += RepoStoreLoaded;
                _updater.CheckRemoteForUpdates();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"There was an error while checking for updates. Details have been logged. Updater will not be usable. 
                    error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _updater.RepoUpdated -= RepoStoreLoaded;
            }
        }

        private void FillData()
        {
            var btnsEnabled = _updater.Enabled && _updater.UpdatesAvailable;
            if (!btnsEnabled) BtnUpdate.IsEnabled = false;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_updater.Enabled) return;
                if (!_updater.UpdatesAvailable) return;
                _updater.DownloadUpdate();
            }
            catch (Exception ex)
            {
                StatusLabel.Content = "Couldn't fetch updates.";
                MessageBox.Show(ex.Message);
            }
        }
    }
}